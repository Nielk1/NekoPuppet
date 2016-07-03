using EmoteEngineNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NekoPuppet
{
    class CharacterListView : System.Windows.Forms.ListView
    {
        enum CurrentSort
        {
            NameDown,
            NameUp,
            OriginDown,
            OriginUp
        }

        private CurrentSort sort1 = CurrentSort.OriginDown;
        private CurrentSort sort2 = CurrentSort.NameDown;

        public CharacterListView()
        {
            // This call is required by the Windows.Forms Form Designer.
            //InitializeComponent();

            base.RetrieveVirtualItem += CharacterListView_RetrieveVirtualItem;
            base.ColumnClick += CharacterListView_ColumnClick;

            //base.SelectedIndexChanged += new EventHandler(
            //                   MyListView_SelectedIndexChanged);
            //base.ColumnClick += new ColumnClickEventHandler(
            //                   MyListView_ColumnClick);
        }

        private void CharacterListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == 0)
            {
                switch (sort1)
                {
                    case CurrentSort.NameDown:
                        sort2 = CurrentSort.NameUp;
                        sort1 = CurrentSort.NameUp;
                        break;
                    case CurrentSort.NameUp:
                        sort2 = CurrentSort.NameDown;
                        sort1 = CurrentSort.NameDown;
                        break;
                    case CurrentSort.OriginDown:
                        sort2 = sort1;
                        sort1 = CurrentSort.NameDown;
                        break;
                    case CurrentSort.OriginUp:
                        sort2 = sort1;
                        sort1 = CurrentSort.NameDown;
                        break;
                }
            }
            else if (e.Column == 1)
            {
                switch (sort1)
                {
                    case CurrentSort.NameDown:
                        sort2 = sort1;
                        sort1 = CurrentSort.OriginDown;
                        break;
                    case CurrentSort.NameUp:
                        sort2 = sort1;
                        sort1 = CurrentSort.OriginDown;
                        break;
                    case CurrentSort.OriginDown:
                        sort2 = CurrentSort.OriginUp;
                        sort1 = CurrentSort.OriginUp;
                        break;
                    case CurrentSort.OriginUp:
                        sort2 = CurrentSort.OriginDown;
                        sort1 = CurrentSort.OriginDown;
                        break;
                }
            }

            //Console.WriteLine("----------");
            //Console.WriteLine(sort1);
            //Console.WriteLine(sort2);

            this.Refresh();
        }

        private void ApplySort()
        {
            source.Sort((a, b) =>
            {
                int val = 0;
                switch (sort1)
                {
                    case CurrentSort.NameDown: val = a.Name.CompareTo(b.Name); break;
                    case CurrentSort.NameUp:   val = b.Name.CompareTo(a.Name); break;
                    case CurrentSort.OriginDown: val = a.Origin.CompareTo(b.Origin); break;
                    case CurrentSort.OriginUp:   val = b.Origin.CompareTo(a.Origin); break;
                }
                if (val != 0 || sort1 == sort2) return val;
                switch (sort2)
                {
                    case CurrentSort.NameDown: return a.Name.CompareTo(b.Name);
                    case CurrentSort.NameUp:   return b.Name.CompareTo(a.Name);
                    case CurrentSort.OriginDown: return a.Origin.CompareTo(b.Origin);
                    case CurrentSort.OriginUp:   return b.Origin.CompareTo(a.Origin);
                }
                return 0;
            });
        }

        public override void Refresh()
        {
            ApplySort();
            base.Refresh();
        }

        private void CharacterListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            ICharacterListViewItem item = source[e.ItemIndex];

            if (item.ListViewItemCache != null)
            {
                e.Item = item.ListViewItemCache;
                return;
            }

            if (item.LargeIcon != null)
            {
                if (!LargeImageList.Images.ContainsKey(item.UniqueStringID))
                {
                    LargeImageList.Images.Add(item.UniqueStringID, item.LargeIcon);
                }
            }
            if (item.SmallIcon != null)
            {
                if (!SmallImageList.Images.ContainsKey(item.UniqueStringID))
                {
                    SmallImageList.Images.Add(item.UniqueStringID, item.SmallIcon);
                }
            }

            ListViewItem lvi = new ListViewItem(item.Name, LargeImageList.Images.IndexOfKey(item.UniqueStringID));
            lvi.Tag = item;
            lvi.SubItems.Add(item.Origin);
            lvi.SubItems.Add(item.Key);
            e.Item = lvi;

            item.ListViewItemCache = lvi;
        }

        private List<ICharacterListViewItem> source;

        [Bindable(true)]
        [TypeConverter("System.Windows.Forms.Design.DataSourceConverter, System.Design")]
        [Category("Data")]
        public List<ICharacterListViewItem> DataSource
        {
            get
            {
                return source;
            }
            set
            {
                source = value;
                bind();
            }
        }

        /*[Browsable(false)]
        public new SortOrder Sorting
        {
            get
            {
                return base.Sorting;
            }
            set
            {
                base.Sorting = value;
            }
        }*/

        private void bind()
        {
            //this.BeginUpdate();
            //Clear the existing list
            VirtualListSize = 0;
            Items.Clear();
            Columns.Clear();
            LargeImageList = new ImageList();
            LargeImageList.ImageSize = new Size(128, 128);
            SmallImageList = new ImageList();
            SmallImageList.ImageSize = new Size(16, 16);
            if (source != null)
            {
                Columns.Add("Name", "Name", 250);
                Columns.Add("Origin", "Origin", 200);

                VirtualListSize = source.Count;

                /*int imageIndex = 0;
                foreach(ILinqListViesItem item in source)
                {
                    ListViewItem lvi = new ListViewItem(item.Name, item.IconKey);
                    lvi.Tag = item;
                    Items.Add(lvi);

                    if (item.Icon != null)
                    {
                        newImages.Images.Add(item.IconKey, item.Icon);
                    }

                    imageIndex++;
                }*/
            }
            else
            {
                //If no source is defined, Currency Manager is null  
                //cm = null;
            }
            this.EndUpdate();
        }
    }

    public interface ICharacterListViewItem
    {
        string UniqueStringID { get; }
        string Name { get; }
        string Origin { get; }
        string Key { get; }
        ColorType ColorMode { get; }
        Image LargeIcon { get; }
        Image SmallIcon { get; }
        ListViewItem ListViewItemCache { get; set; }
        Stream GetDataStream();
    }

}
