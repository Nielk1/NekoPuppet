using EmoteEngineNet;
using LoaderGoGoNippon;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NekoPuppet.Plugins.Loaders.GoGoNipponMyFirstTripToJapan
{
    public class GoGoNipponCharacterSource : ICharacterLoader
    {
        string goGoNipponDataPath = Path.Combine(Directory.GetCurrentDirectory(), @"assets", @"GoGoNippon", @"resources.assets");

        public class FileData
        {
            public string Filename;
            public UInt64 offset;
            public UInt64 size;
        }

        public List<ICharacterListViewItem> GetCharacters()
        {
            try
            {
                if(File.Exists(goGoNipponDataPath))
                {
                    List<FileData> knownCharacters = ParseUnityArchive();

                    return knownCharacters.Select(dr => (ICharacterListViewItem)new GoGoNipponCharacterData(dr.Filename, dr.offset, dr.size, this)).ToList();
                }
                return null;
            }
            catch (DirectoryNotFoundException)
            {
                return null;
            }
        }

        private List<FileData> ParseUnityArchive()
        {
            int GUESS_NAMES = 1;
            List<FileData> retVal = new List<FileData>();

            byte[] buffer = new byte[4];

            using (FileStream fs = File.OpenRead(goGoNipponDataPath))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                UInt32 HEADER_SIZE;
                UInt32 FULL_SIZE;
                UInt32 VERSION;
                UInt32 BASE_OFF;

                // big endian start
                reader.Read(buffer, 0, 4); HEADER_SIZE = BitConverter.ToUInt32(buffer.Reverse().ToArray(), 0);
                reader.Read(buffer, 0, 4); FULL_SIZE = BitConverter.ToUInt32(buffer.Reverse().ToArray(), 0);
                reader.Read(buffer, 0, 4); VERSION = BitConverter.ToUInt32(buffer.Reverse().ToArray(), 0);
                reader.Read(buffer, 0, 4); BASE_OFF = BitConverter.ToUInt32(buffer.Reverse().ToArray(), 0);

                long OFFSET;
                UInt32 ZERO_GUESS = 0;
                if (VERSION <= 8)
                {
                    OFFSET = FULL_SIZE - HEADER_SIZE;
                    reader.BaseStream.Seek(OFFSET, SeekOrigin.Begin);
                    reader.ReadByte();// get DUMMY byte
                }
                else
                {
                    reader.Read(buffer, 0, 4); ZERO_GUESS = BitConverter.ToUInt32(buffer.Reverse().ToArray(), 0);
                }

                //little endian start
                //endian guess ZERO_GUESS // TODO: find how to implement this BMS fragment if needed

                string VERSION_STRING = string.Empty;
                if (VERSION >= 8)
                {
                    byte tmp;
                    while ((tmp = reader.ReadByte()) != 0x0)
                    {
                        VERSION_STRING += (char)tmp;
                    }
                    //Console.WriteLine(VERSION_STRING);
                    reader.ReadUInt32(); // read DUMMY long
                }
                if (VERSION >= 0xe)
                {
                    byte ZERO = reader.ReadByte();
                    UInt32 BASES = reader.ReadUInt32();
                    for (UInt32 i = 0; i < BASES; i++)
                    {
                        Int32 DUMMY = reader.ReadInt32();
                        if (DUMMY < 0)
                        {
                            string DUMMYStr = new string(reader.ReadBytes(0x20).Select(dr => (char)dr).ToArray());
                            //Console.WriteLine(DUMMYStr);
                        }
                        else
                        {
                            string DUMMYStr = new string(reader.ReadBytes(0x10).Select(dr => (char)dr).ToArray());
                            //Console.WriteLine(DUMMYStr);
                        }
                    }
                }
                /*else
                {
                    UInt32 BASES = reader.ReadUInt32();
                    for (UInt32 BASE = 0; BASE < BASES; BASE++)
                    {
                        Int32 DUMMY = reader.ReadInt32();
                        UInt32 SUB_ELEMENTS = 1;
                        PARSE_TYPES(reader, SUB_ELEMENTS);
                    }
                }*/

                UInt32 ADDITIONAL_FIELD = 0;
                if (VERSION >= 7)
                {
                    if (VERSION < 0xe)
                    {
                        ADDITIONAL_FIELD = reader.ReadUInt32();
                    }
                }
                UInt32 FILES = reader.ReadUInt32();

                if (VERSION >= 0xe)
                {
                    if ((reader.BaseStream.Position % 4) > 0)//padding 4
                    {
                        reader.BaseStream.Position = (reader.BaseStream.Position / 4 * 4) + 4;
                    }
                }

                UInt32 SIZE_RESS = 0;
                UInt32 FIRST = 1;
                Int32 a = -1;
                for (int i = 0; i < FILES; i++)
                {
                    UInt32 INDEX;
                    UInt32 ZERO;
                    UInt32 SIZE;
                    Int32 TYPE;
                    UInt32 XTYPE;

                    if (VERSION >= 0xe)
                    {
                        reader.Read(buffer, 0, 4); INDEX = BitConverter.ToUInt32(buffer, 0);
                        reader.Read(buffer, 0, 4); ZERO = BitConverter.ToUInt32(buffer, 0);
                        reader.Read(buffer, 0, 4); OFFSET = BitConverter.ToUInt32(buffer, 0);
                        reader.Read(buffer, 0, 4); SIZE = BitConverter.ToUInt32(buffer, 0);
                        reader.Read(buffer, 0, 4); TYPE = BitConverter.ToInt32(buffer, 0);
                        reader.Read(buffer, 0, 2); XTYPE = BitConverter.ToUInt16(buffer, 0); // same as TYPE
                        Int16 DUMMY = reader.ReadInt16(); // -1
                        if (VERSION >= 0xf)
                        {
                            ZERO = reader.ReadUInt32();
                        }
                    }
                    else
                    {
                        reader.Read(buffer, 0, 4); INDEX = BitConverter.ToUInt32(buffer, 0);
                        if (ADDITIONAL_FIELD != 0)
                        {
                            UInt32 _ZERO = reader.ReadUInt32();
                        }
                        reader.Read(buffer, 0, 4); OFFSET = BitConverter.ToUInt32(buffer, 0);
                        reader.Read(buffer, 0, 4); SIZE = BitConverter.ToUInt32(buffer, 0);
                        reader.Read(buffer, 0, 4); TYPE = BitConverter.ToInt32(buffer, 0);
                        reader.Read(buffer, 0, 4); XTYPE = BitConverter.ToUInt32(buffer, 0);  // same as TYPE
                    }

                    long TMP_OFF = reader.BaseStream.Position;
                    long OFFSET_TMP = OFFSET + BASE_OFF;
                    reader.BaseStream.Position = OFFSET_TMP;

                    string NAME = string.Empty;
                    UInt32 GET_FILENAMES = 0;
                    if (GUESS_NAMES != 0)
                    {
                        UInt32 NAMESZ = reader.ReadUInt32();
                        if (NAMESZ < 128)
                        {
                            NAME = new string(reader.ReadBytes((int)NAMESZ).Select(dr => (char)dr).ToArray());
                            int _TMP = NAME.Length;
                            if (_TMP == NAMESZ)
                            {
                                GET_FILENAMES = 1;
                            }
                        }
                        reader.BaseStream.Position = OFFSET_TMP;
                    }

                    NAME = string.Empty;
                    if (GET_FILENAMES != 0)
                    {
                        UInt32 NAMESZ = reader.ReadUInt32();
                        NAME = new string(reader.ReadBytes((int)NAMESZ).Select(dr => (char)dr).ToArray());
                        int _TMP = NAME.Length;
                        if (_TMP == NAMESZ)
                        {
                            GET_FILENAMES = 1;
                        }
                        if ((reader.BaseStream.Position % 4) > 0)//padding 4
                        {
                            reader.BaseStream.Position = (reader.BaseStream.Position / 4 * 4) + 4;
                        }
                    }
                    OFFSET = reader.BaseStream.Position;
                    reader.BaseStream.Position = TMP_OFF;

                    long TMP = SIZE - (OFFSET - OFFSET_TMP);
                    if (TMP < 0)
                    {
                        OFFSET = OFFSET_TMP;
                    }
                    else
                    {
                        SIZE = (UInt32)TMP;
                    }

                    string BNAME = Path.GetFileNameWithoutExtension(goGoNipponDataPath);

                    if (NAME.Length == 0 || NAME[0] <= 0x20)
                    {
                        NAME = string.Format("{0}_{1}", BNAME, i);
                    }

                    string EXT = "." + TYPE;
                    {
                        long disttype_offset = reader.BaseStream.Position;// backup offset
                        reader.BaseStream.Position = OFFSET;             // maybe useful

                        switch (TYPE)
                        {
                            case -2:
                                EXT = ".txt";
                                break;
                            case -3:
                                EXT = ".txt";
                                break;
                            case -4:
                                EXT = ".txt";
                                break;
                            case -5:
                                EXT = ".txt";
                                break;
                            case -6:
                                EXT = ".txt";
                                break;
                            case -7:
                                EXT = ".txt";
                                break;
                            case -8:
                                EXT = ".txt";
                                break;
                            case -9:
                                EXT = ".txt";
                                break;
                            case -14:
                                EXT = ".txt";
                                break;
                            case -15:
                                EXT = ".txt";
                                break;
                            case 21:
                                EXT = ".mat";
                                break;
                            case 28:
                                EXT = ".tex";
                                break;
                            case 48:
                                EXT = ".shader";
                                break;
                            case 49:
                                {
                                    reader.ReadUInt32();//get DUMMY long
                                    string TEST = new string(reader.ReadBytes(4).Select(dr => (char)dr).ToArray());//getDstring TEST 4
                                    if (TEST == "PSB\0")
                                    {
                                        OFFSET += 4;
                                        SIZE -= 4;
                                        EXT = ".psb";
                                    }
                                    else if (TEST == "<?xm")
                                    {
                                        OFFSET += 4;
                                        SIZE -= 4;
                                        EXT = ".xml";
                                    }
                                }
                                break;
                            case 74:
                                EXT = ".ani";
                                break;
                            case 83:
                                {
                                    EXT = ".snd";
                                    OFFSET += 0x14;
                                    SIZE -= 0x14;
                                    reader.BaseStream.Position = OFFSET;
                                    string TEMP = new string(reader.ReadBytes(4).Select(dr => (char)dr).ToArray());//getDstring TEMP 4
                                    if (TEMP == "OggS")
                                    {
                                        EXT = ".ogg";
                                    }
                                    else if (TEMP == "RIFF")
                                    {
                                        EXT = ".wav";
                                    }
                                    else if (TEMP == "FORM")
                                    {
                                        EXT = ".aif";
                                    }
                                    else
                                    {
                                        reader.BaseStream.Position = OFFSET;
                                        byte bTEMP = reader.ReadByte();
                                        if (bTEMP == 0xff)
                                        {
                                            EXT = ".mp3";
                                        }
                                        else if (bTEMP == 0x49)
                                        {
                                            reader.BaseStream.Position = OFFSET;
                                            TEMP = new string(reader.ReadBytes(3).Select(dr => (char)dr).ToArray());//getDstring TEMP 3;
                                            if (TEMP == "ID3")
                                            {
                                                EXT = ".mp3";
                                            }
                                            else
                                            {
                                                OFFSET -= 0x14;
                                                SIZE += 0x14;
                                            }
                                        }
                                        else // roll back changes
                                        {
                                            OFFSET -= 0x14;
                                            SIZE += 0x14;
                                        }
                                    }
                                }
                                break;
                            case 115:
                                EXT = ".script";
                                break;
                            case 128:
                                EXT = ".ttf";
                                break;
                            case 150:
                                EXT = ".bin";
                                break;
                            case 152:
                                OFFSET += 0x10;
                                SIZE -= 0x10;
                                EXT = ".ogm";
                                break;
                            case 156:
                                EXT = ".ter"; ;
                                break;
                                break;
                            case 184:
                                EXT = ".sbam";
                                break;
                            case 194:
                                EXT = ".tes";
                                break;
                        }

                        reader.BaseStream.Position = disttype_offset;
                    }
                    string FNAME = string.Format("{0}~{1}{2}", BNAME, NAME, EXT);

                    //log FNAME OFFSET SIZE 0
                    //Console.WriteLine("{0:X8} {1}\t{2}", OFFSET, SIZE, FNAME);
                    if (EXT == ".psb")
                    {
                        retVal.Add(new FileData()
                        {
                            Filename = FNAME,
                            offset = (ulong)OFFSET,
                            size = SIZE
                        });
                    }
                }
            }
            return retVal;
        }

        public Stream GetFileStream(string filename, UInt64 offset, UInt64 size)
        {
            byte[] internalData = null;
            using (FileStream fs = File.OpenRead(goGoNipponDataPath))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                reader.BaseStream.Seek((long)offset, SeekOrigin.Begin);

                internalData = reader.ReadBytes((int)size);
            }

            if (internalData == null || internalData.Length == 0) return null;

            return new MemoryStream(internalData);
        }
    }

    public class GoGoNipponCharacterData : ICharacterListViewItem
    {
        GoGoNipponCharacterSource intr;

        private static Image ggnIcon = Resource.thumb_small_gogonippon;

        public GoGoNipponCharacterData(string filename, UInt64 offset, UInt64 size, GoGoNipponCharacterSource intr)
        {
            this.filename = filename;
            this.offset = offset;
            this.size = size;
            this.intr = intr;
        }

        private string filename;
        private UInt64 offset;
        private UInt64 size;

        public string UniqueStringID
        {
            get
            {
                return "GoGoNippon-" + filename;
            }
        }

        public string Key { get { return "1014557619"; } }
        public ColorType ColorMode { get { return ColorType.BGRA; } }

        public Image LargeIcon { get { return new Bitmap(256, 256); } }
        public Image SmallIcon { get { return ggnIcon; } }

        public ListViewItem ListViewItemCache { get; set; }

        public string Name
        {
            get
            {
                return (filename != null ? Path.GetFileNameWithoutExtension(filename) : null);
            }
        }

        public string Origin { get { return "Go! Go! Nippon! ~My First Trip to Japan~"; } }

        public Stream GetDataStream()
        {
            return intr.GetFileStream(filename, offset, size);
        }
    }
}
