using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace NekoPuppet.Plugins.Loaders.Nekopara
{
    public abstract class NekoparaVolBase
    {
        public class FileData
        {
            public string Filename;
            public bool zip;
            public UInt64 offset;
            public UInt64 size;
            public UInt64 zsize;
        }

        protected virtual string nekoparaVolDataPath { get; set; }

        protected byte[] GetInternalData()
        {
            if (!File.Exists(nekoparaVolDataPath))
                throw new FileNotFoundException("Data file not found", nekoparaVolDataPath);

            using (FileStream fs = File.OpenRead(nekoparaVolDataPath))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                if (reader.ReadByte() == 'X' &&
                   reader.ReadByte() == 'P' &&
                   reader.ReadByte() == '3')
                {
                    reader.BaseStream.Seek(0x20, SeekOrigin.Begin);
                    UInt32 offset = reader.ReadUInt32();

                    reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                    byte zip = reader.ReadByte();
                    UInt64 zsize = reader.ReadUInt64();
                    UInt64 size = reader.ReadUInt64();
                    offset = (UInt32)reader.BaseStream.Position;

                    if (zip == 0)
                    {
                        return reader.ReadBytes((int)zsize);
                    }
                    else
                    {
                        reader.ReadByte(); reader.ReadByte(); // need to read these for .net's deflate implementation, these are zlib header bytes
                        using (DeflateStream keepProc = new DeflateStream(reader.BaseStream, CompressionMode.Decompress))
                        using (BinaryReader reader2 = new BinaryReader(keepProc))
                        {
                            return reader2.ReadBytes((int)size);
                        }
                    }
                }
            }
            return null;
        }

        protected void ReadData(BinaryReader reader, List<FileData> fileData)
        {
            string BlockType = new String(reader.ReadChars(4));
            UInt64 blockSize = reader.ReadUInt64();
            switch (BlockType)
            {
                case "File":
                    ReadData(reader, fileData);
                    break;
                case "adlr":
                    {
                        UInt32 CRC = reader.ReadUInt32();
                    }
                    break;
                case "segm":
                    {
                        UInt32 zip = reader.ReadUInt32();
                        UInt64 offset = reader.ReadUInt64();
                        UInt64 size = reader.ReadUInt64();
                        UInt64 zsize = reader.ReadUInt64();
                        FileData last = fileData.LastOrDefault();
                        if (last != null && last.size == 0)
                        {
                            last.zip = zip != 0;
                            last.offset = offset;
                            last.size = size;
                            last.zsize = zsize;
                        }
                    }
                    break;
                case "neko":
                case "eliF":
                    {
                        UInt32 unknown = reader.ReadUInt32();
                        UInt16 namez = reader.ReadUInt16();
                        if (namez <= 0x100)
                        {
                            namez *= 2;
                            string name = new string(Encoding.Unicode.GetChars(reader.ReadBytes(namez)));
                            reader.ReadBytes(2); // pull off 2 nuls?
                            //Console.WriteLine(name);
                            fileData.Add(new FileData() { Filename = name });
                        }
                    }
                    break;
                case "time":
                    {
                        UInt64 timestamps = reader.ReadUInt64();
                    }
                    break;
                case "info":
                default:
                    reader.BaseStream.Seek((long)blockSize, SeekOrigin.Current);
                    break;
            }
        }

        public Stream GetFileStream(bool zip, ulong offset, ulong size, ulong zsize)
        {
            byte[] internalData = null;
            using (FileStream fs = File.OpenRead(nekoparaVolDataPath))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                reader.BaseStream.Seek((long)offset, SeekOrigin.Begin);

                if (zip)
                {
                    reader.ReadByte(); reader.ReadByte(); // need to read these for .net's deflate implementation, these are zlib header bytes
                    using (DeflateStream keepProc = new DeflateStream(reader.BaseStream, CompressionMode.Decompress))
                    using (BinaryReader reader2 = new BinaryReader(keepProc))
                    {
                        internalData = reader2.ReadBytes((int)size);
                    }
                }
                else
                {
                    internalData = reader.ReadBytes((int)zsize);
                }
            }

            if (internalData == null || internalData.Length == 0) return null;

            /// this is fit specificly for PSB files
            byte TMP = internalData[1];
            TMP ^= 0x53;
            byte TMP0 = TMP;
            TMP0 ^= 0x50;
            internalData[0] = TMP0;
            ///

            for (int x = 0; x < internalData.Length; x++)
            {
                { internalData[x] ^= TMP; }
            }

            return new MemoryStream(internalData);
        }
    }
}
