﻿using LoaderPluginNekopara;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NekoPuppet.Plugins.Loaders.Nekopara
{
    public class NekoparaVol0CharacterSource : NekoparaVolBase, ICharacterLoader
    {
        protected override string nekoparaVolDataPath { get { return Path.Combine(Directory.GetCurrentDirectory(), @"assets", @"NekoparaVol0", @"emotewin.xp3"); } }

        public List<ICharacterListViewItem> GetCharacters()
        {
            try
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

                return fileData.Where(dr => dr.Filename.EndsWith(".psb")).Select(dr => (ICharacterListViewItem)new NekoparaVol0CharacterData(dr, this)).ToList();
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

    public class NekoparaVol0CharacterData : ICharacterListViewItem
    {
        private bool zip;
        private UInt64 offset;
        private UInt64 size;
        private UInt64 zsize;

        NekoparaVol0CharacterSource intr;

        private static Image nekoIcon = Resource.thumb_small_nekopara;

        public NekoparaVol0CharacterData(NekoparaVol0CharacterSource.FileData data, NekoparaVol0CharacterSource intr)
        {
            this.filename = data.Filename;
            this.offset = data.offset;
            this.size = data.size;
            this.zip = data.zip;
            this.zsize = data.zsize;
            this.intr = intr;
        }

        private string filename;

        public string UniqueStringID { get { return "NekoVol0-" + filename; } }
        public string Key { get { return "742877301"; } }
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

        public string Origin { get { return "NEKOPARA Vol.0"; } }

        public Stream GetDataStream()
        {
            return intr.GetFileStream(zip, offset, size, zsize);
        }
    }
}
