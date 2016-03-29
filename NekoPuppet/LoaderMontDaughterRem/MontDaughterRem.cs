using IonicCustomMod.Zip;
using LoaderMontDaughterRem;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NekoPuppet.Plugins.Loaders.MontDaughterRem
{
    public class MontDaughterRemCharacterSource : ICharacterLoader
    {
        string montDaughterRemDataPath = Path.Combine(Directory.GetCurrentDirectory(), @"assets", @"jp.furyu.moefan", @"files", @"DAT", @"Resources");

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
                List<string> characters = Directory.GetFiles(montDaughterRemDataPath, "emote_cmp.bytes", SearchOption.AllDirectories).AsEnumerable().ToList(); ;
                return characters.Select(dr => (ICharacterListViewItem)new MontDaughterRemCharacterData(dr, dr.Remove(0, montDaughterRemDataPath.Length + 1), this)).ToList();
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
                            file.ExtractWithPassword(fstream, 0x7393d246, 0x829dc765, 0x19176ecc);
                        }
                        return new MemoryStream(data);
                    }
                }
            }
            return null;
        }
    }

    public class MontDaughterRemCharacterData : ICharacterListViewItem
    {
        MontDaughterRemCharacterSource intr;

        private static Image montIcon = Resource.thumb_small_mont_daughter_rem;

        public MontDaughterRemCharacterData(string filename, string shortpath, MontDaughterRemCharacterSource intr)
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
                return "MontDaughterRem-" + shortpath;
            }
        }

        public string Key { get { return "180425132"; } }


        public Image LargeIcon { get { return new Bitmap(256, 256); } }
        public Image SmallIcon { get { return montIcon; } }

        public ListViewItem ListViewItemCache { get; set; }

        public string Name
        {
            get
            {
                return shortpath != null ? Path.GetDirectoryName(shortpath) : null;
            }
        }

        public string Origin { get { return "Mont daughter - Rem"; } }

        public Stream GetDataStream()
        {
            return intr.GetFileStream(filename);
        }
    }
}
