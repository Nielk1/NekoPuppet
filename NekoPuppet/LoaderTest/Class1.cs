using EmoteEngineNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NekoPuppet.Plugins.Loaders.LoaderTest
{
    public class TestCharacterSource : ICharacterLoader
    {
        string testDataPath = Path.Combine(Directory.GetCurrentDirectory(), @"assets", @"Test");

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
                List<string> characters = Directory.GetFiles(testDataPath, "*.psb", SearchOption.AllDirectories).AsEnumerable().ToList(); ;
                return characters.Select(dr => (ICharacterListViewItem)new TestCharacterData(dr, Path.GetFileName(dr), this)).ToList();
            }
            catch (DirectoryNotFoundException)
            {
                return null;
            }
        }

        public Stream GetFileStream(string filepath)
        {
            return File.OpenRead(filepath);
        }
    }

    public class TestCharacterData : ICharacterListViewItem
    {
        TestCharacterSource intr;

        private static Image testIcon = new Bitmap(16, 16);//Resource.thumb_small_mont_daughter_rem;

        public TestCharacterData(string filename, string name, TestCharacterSource intr)
        {
            this.filename = filename;
            this.name = name;
            this.intr = intr;
        }

        private string filename;
        private string name;

        public string UniqueStringID
        {
            get
            {
                return "Test-" + Path.GetFileName(filename);
            }
        }

        //public string Key { get { return "192918854"; } }
        public string Key { get { return "000000000"; } }
        public ColorType ColorMode { get { return ColorType.BGRA; } }

        public Image LargeIcon { get { return new Bitmap(256, 256); } }
        public Image SmallIcon { get { return testIcon; } }

        public ListViewItem ListViewItemCache { get; set; }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public string Origin { get { return "Test"; } }

        public Stream GetDataStream()
        {
            return intr.GetFileStream(filename);
        }
    }
}
