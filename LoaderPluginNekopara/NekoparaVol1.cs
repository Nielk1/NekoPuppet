using LoaderPluginNekopara;
using NekoPuppet.Plugins;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NekoPuppet.CharacterData
{
    public class NekoparaVol1CHaracterSource : NekoparaVolBase, ICharacterLoader
    {
        protected override string nekoparaVolDataPath { get { return Path.Combine(Directory.GetCurrentDirectory(), @"assets", @"NekoparaVol1", @"emotewin.xp3"); } }

        public List<ICharacterListViewItem> GetCharacters()
        {
            byte[] data = GetInternalData();

            List<FileData> fileData = new List<FileData>();

            using (MemoryStream memStream = new MemoryStream(data))
            using (BinaryReader reader2 = new BinaryReader(memStream))
            {
                while (reader2.BaseStream.Position < reader2.BaseStream.Length)
                {
                    ReadData(reader2, fileData);
                }
            }

            return fileData.Where(dr => dr.Filename.EndsWith(".psb")).Select(dr => (ICharacterListViewItem)new NekoparaVol1CharacterData(dr, this)).ToList();
        }
    }

    public class NekoparaVol1CharacterData : ICharacterListViewItem
    {
        private bool zip;
        private UInt64 offset;
        private UInt64 size;
        private UInt64 zsize;

        NekoparaVol1CHaracterSource intr;

        private static Image nekoIcon = Resource.thumb_small_nekopara;

        public NekoparaVol1CharacterData(NekoparaVol0CHaracterSource.FileData data, NekoparaVol1CHaracterSource intr)
        {
            this.filename = data.Filename;
            this.offset = data.offset;
            this.size = data.size;
            this.zip = data.zip;
            this.zsize = data.zsize;
            this.intr = intr;
        }

        private string filename;

        public string UniqueStringID { get { return "NekoVol1-" + filename; } }
        public string Key { get { return "742877301"; } }
        public Image LargeIcon { get { return new Bitmap(256, 256); } }
        public Image SmallIcon { get { return nekoIcon; } }

        public ListViewItem ListViewItemCache { get; set; }

        public string Name
        {
            get
            {
                return filename != null ? Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename)) : null;
            }
        }

        public string Origin { get { return "NEKOPARA Vol.1"; } }

        public Stream GetDataStream()
        {
            return intr.GetFileStream(zip, offset, size, zsize);
        }
    }
}
