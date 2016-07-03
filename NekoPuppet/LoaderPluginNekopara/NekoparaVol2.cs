using EmoteEngineNet;
using LoaderPluginNekopara;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NekoPuppet.Plugins.Loaders.Nekopara
{
    public class NekoparaVol2CHaracterSource : NekoparaVolBase, ICharacterLoader
    {
        protected override string nekoparaVolDataPath { get { return Path.Combine(Directory.GetCurrentDirectory(), @"assets", @"NekoparaVol2", @"emotewin.xp3"); } }

        public List<ICharacterListViewItem> GetCharacters()
        {
            try
            {
                byte[] data = GetInternalData();

                List<FileData> fileData = new List<FileData>();
                Queue<FileData> fileDataQueue = new Queue<FileData>();
                bool ReadFirstFile = false;

                using (MemoryStream memStream = new MemoryStream(data))
                using (BinaryReader reader2 = new BinaryReader(memStream))
                {
                    while (reader2.BaseStream.Position < reader2.BaseStream.Length)
                    {
                        ReadData(reader2, fileData, fileDataQueue, ref ReadFirstFile);
                    }
                }

                return fileData.Where(dr => dr.Filename.EndsWith(".psb")).Select(dr => (ICharacterListViewItem)new NekoparaVol2CharacterData(dr, this)).ToList();
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (DirectoryNotFoundException)
            {
                return null;
            }
        }

    }

    public class NekoparaVol2CharacterData : ICharacterListViewItem
    {
        private bool zip;
        private UInt64 offset;
        private UInt64 size;
        private UInt64 zsize;

        NekoparaVol2CHaracterSource intr;

        private static Image nekoIcon = Resource.thumb_small_nekopara;

        public NekoparaVol2CharacterData(NekoparaVol2CHaracterSource.FileData data, NekoparaVol2CHaracterSource intr)
        {
            this.filename = data.Filename;
            this.offset = data.offset;
            this.size = data.size;
            this.zip = data.zip;
            this.zsize = data.zsize;
            this.intr = intr;
        }

        private string filename;

        public string UniqueStringID { get { return "NekoVol2-" + filename; } }
        public string Key { get { return "742877301"; } }
        public ColorType ColorMode { get { return ColorType.BGRA; } }

        public Image LargeIcon { get { return new Bitmap(256,256); } }
        public Image SmallIcon { get { return nekoIcon; } }

        public ListViewItem ListViewItemCache { get; set; }

        public string Name
        {
            get
            {
                return filename != null ? Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename)) : null;
            }
        }

        public string Origin { get { return "NEKOPARA Vol.2"; } }

        public Stream GetDataStream()
        {
            return intr.GetFileStream(zip, offset, size, zsize);
        }
    }
}
