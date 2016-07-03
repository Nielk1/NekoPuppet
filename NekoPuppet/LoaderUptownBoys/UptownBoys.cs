using EmoteEngineNet;
using Ionic.Zip;
using LoaderUptownBoys;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NekoPuppet.Plugins.Loaders.LoaderUptownBoys
{
    public class UptownBoysCharacterSource : ICharacterLoader
    {
        string uptownBoysDataPath = Path.Combine(Directory.GetCurrentDirectory(), @"assets", @"jp.co.eitarosoft.yd", @"files", @"fc", @"resources");

        public class FileData
        {
            public string Filename;
            public bool zip;
            public UInt64 offset;
            public UInt64 size;
            public UInt64 zsize;
        }

        public List<ICharacterListViewItem> GetCharacters()
        {
            try
            {
                List<string> characters = Directory.GetFiles(uptownBoysDataPath, "*.psb", SearchOption.AllDirectories).AsEnumerable().ToList();
                return characters.Select(dr => (ICharacterListViewItem)new UptownBoysCharacterData(dr, dr.Remove(0, uptownBoysDataPath.Length + 1), this)).ToList();
            }
            catch (DirectoryNotFoundException)
            {
                return null;
            }
        }

        public Stream GetFileStream(string filepath)
        {
            byte[] data;
            ReadOptions options = new ReadOptions();
            using (ZipFile zip = ZipFile.Read(filepath, options))
            {
                if (zip.Entries.Count > 0)
                {
                    ZipEntry file = zip.Entries.FirstOrDefault();
                    if (file != null)
                    {
                        data = new byte[file.UncompressedSize];
                        using (MemoryStream fstream = new MemoryStream(data))
                        {
                            file.Extract(fstream);
                        }
                        return new MemoryStream(data);
                    }
                }
            }
            return null;
        }
    }

    public class UptownBoysCharacterData : ICharacterListViewItem
    {
        UptownBoysCharacterSource intr;

        private static Image boysIcon = Resource.thumb_small_uptown_boys;

        public UptownBoysCharacterData(string filename, string shortpath, UptownBoysCharacterSource intr)
        {
            this.filename = filename;
            this.shortpath = shortpath;
            this.intr = intr;
        }

        private string filename;
        private string shortpath;

        public string UniqueStringID
        {
            get
            {
                return "UptownBoys-" + shortpath;
            }
        }

        public string Key { get { return "601512504"; } }
        public ColorType ColorMode { get { return ColorType.RGBA; } }

        public Image LargeIcon { get { return new Bitmap(256, 256); } }
        public Image SmallIcon { get { return boysIcon; } }

        public ListViewItem ListViewItemCache { get; set; }

        public string Name
        {
            get
            {
                return (shortpath != null ? Path.GetDirectoryName(shortpath) : null) + "/" + (filename != null ? Path.GetFileNameWithoutExtension(filename) : null);
            }
        }

        public string Origin { get { return "Uptown Boys"; } }

        public Stream GetDataStream()
        {
            return intr.GetFileStream(filename);
        }
    }
}
