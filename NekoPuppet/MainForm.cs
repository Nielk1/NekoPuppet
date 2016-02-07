using EmoteEngineNet;
using NekoPuppet.Plugins;
using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NekoPuppet
{
    public partial class MainForm : Form
    {
        BindingList<EmoteModuleMetadata> emoteLibs = new BindingList<EmoteModuleMetadata>();

        private EnhancedRenderForm renderForm;
        private Emote emote;
        private Device _device;
        private List<EmotePlayer> players = new List<EmotePlayer>();

        const double REFRESH = 1.0 / 50.0;
        private double elaspedTime;
        private PreciseTimer _timer = new PreciseTimer();
        private bool rendering = false;

        private FunctionGraphForm functionGraph;

        List<ICharacterLoader> CharacterLoaders = new List<ICharacterLoader>();
        List<ICharacterListViewItem> files;

        //[DllImport("kernel32.dll", SetLastError = true)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //static extern bool AllocConsole();

        //[DllImport("kernel32.dll")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //static extern bool FreeConsole();

        public MainForm()
        {
            InitializeComponent();
        }

        private ICharacterLoader[] LoadAssembly(string assemblyPath)
        {
            string assembly = Path.GetFullPath(assemblyPath);
            Assembly ptrAssembly = Assembly.LoadFile(assembly);
            try
            {
                return ptrAssembly.GetTypes().AsEnumerable()
                    .Where(item => item.IsClass)
                    .Where(item => item.GetInterfaces().Contains(typeof(ICharacterLoader)))
                    .ToList()
                    .Select(item => (ICharacterLoader)Activator.CreateInstance(item))
                    .ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new ICharacterLoader[0];
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //AllocConsole();

            EmoteModuleMetadata meta = new EmoteModuleMetadata()
            {
                Text = @"NEKOPARA Vol.0",
                Path = @"neko0\emotedriver.dll",
                Key = "742877301",
                Ver = InterfaceVersion.NEKO0,
            };
            //EmoteModuleMetadata meta = new EmoteModuleMetadata()
            //{
            //    Text = @"NEKOPARA Vol.1",
            //    Path = @"neko1\emotedriver.dll",
            //    Key = "742877301",
            //    Ver = InterfaceVersion.NEKO1,
            //};
            emoteLibs.Add(meta);



            {
                string pluginsDir = Path.Combine(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath), "plugins");
                if (!Directory.Exists(pluginsDir)) Directory.CreateDirectory(pluginsDir);
                List<string> plugins = Directory.GetFiles(pluginsDir, "*.dll", SearchOption.TopDirectoryOnly).ToList();
                plugins.ForEach(dr =>
                {
                    try
                    {
                        ICharacterLoader[] pluginInstances = LoadAssembly(dr);
                        foreach (ICharacterLoader plugin in pluginInstances)
                        {
                            CharacterLoaders.Add(plugin);
                        }
                    }
                    catch { }
                });
            }






            renderForm = new EnhancedRenderForm();
            //renderForm.AutoScaleMode = AutoScaleMode.None;
            renderForm.FormBorderStyle = FormBorderStyle.None;
            renderForm.Width = 512;
            renderForm.Height = 512;
            renderForm.StartPosition = FormStartPosition.Manual;
            renderForm.Location = new Point(this.Location.X + this.Width, this.Location.Y);
            renderForm.Show();
            emote = new Emote(renderForm.Handle, 512, 512, false);



            emote.LoadEmoteEngine(Path.Combine(Directory.GetCurrentDirectory(), @"engines", meta.Path), meta.Ver);
            emote.EmoteInit();

            _device = new Device(new IntPtr(emote.D3Device));

            functionGraph = new FunctionGraphForm();
            functionGraph.Width = this.Width;
            functionGraph.Height = renderForm.Height - this.Height;
            functionGraph.StartPosition = FormStartPosition.Manual;
            functionGraph.Location = new Point(this.Location.X, this.Location.Y + this.Height);
            functionGraph.Show();

            rendering = true;
            Task.Run(() =>
            {
                while (rendering)
                {
                    Render();
                }
            }).ContinueWith((task) => { });

            //timer1.Start();

            LoadCharacters();
        }

        private void LoadCharacters()
        {
            tsbRefresh.Enabled = false;
            listView1.DataSource = null;
            listView1.Enabled = false;
            //progressBar1.Value = 0;
            /*IProgress<int> SetMax = new Progress<int>(value =>
            {
                progressBar1.Maximum = value;
            });
            IProgress<int> progress = new Progress<int>(value =>
            {
                progressBar1.Value = value;
            });*/

            Task.Run(() => {
                //string[] apps = Directory.GetFiles(path, @"*.jar", SearchOption.AllDirectories).Union(Directory.GetFiles(path2, @"*.jar", SearchOption.AllDirectories)).ToArray();

                //SetMax.Report(apps.Length - 1);
                List<ICharacterListViewItem> tmpList = new List<ICharacterListViewItem>();

                CharacterLoaders.ForEach(dr =>
                {
                    tmpList.AddRange(dr.GetCharacters());
                });

                //for (int x = 0; x < apps.Length; x++)
                //{
                //    if (new FileInfo(apps[x]).Length > 0)
                //    {
                //        tmpList.Add(new FileData(apps[x]));
                //    }
                //    //progress.Report(x);
                //}
                //tmpList.Sort((a, b) => {
                //    ////int comp = a.Name.CompareTo(b.Name);
                //    ////if (comp != 0) return comp;
                //    ////if (a.Version == null) return -1;
                //    ////if (b.Version == null) return 1;
                //    ////return a.Version.CompareTo(b.Version);
                //    //int comp = 0;
                //    //if (a.WebName != null && b.WebName != null) comp = a.WebName.CompareTo(b.WebName);
                //    //if (comp != 0) return comp;
                //    ////comp = a.Name.CompareTo(b.Name);
                //    ////if (comp != 0) return comp;
                //    //if (a.Version == null) return -1;
                //    //if (b.Version == null) return 1;
                //    //return a.Version.CompareTo(b.Version);
                //});
                return tmpList;
            }).ContinueWith((task) =>
            {
                //tsbRefresh.Enabled = true;

                if (task.Exception != null)
                {
                    throw task.Exception;
                }

                //progressBar1.Maximum = 100;
                //progressBar1.Value = 0;

                files = task.Result;
                listView1.DataSource = files;
                listView1.Refresh();
                listView1.Enabled = true;
                tsbRefresh.Enabled = true;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void Render()
        {
            elaspedTime += _timer.GetElaspedTime();
            if (elaspedTime < REFRESH)
            {
                Thread.Sleep(1);
                return;
            }

            lock (emote)
            {
                emote.Update((float)REFRESH * 1000);

                //_device.Clear(ClearFlags.Target, Color.Transparent, 1.0f, 0);
                //_device.Clear(ClearFlags.Target, new SharpDX.Mathematics.Interop.RawColorBGRA(0, 0, 0, 0), 1.0f, 0);
                _device.Clear(ClearFlags.Target, new SharpDX.Mathematics.Interop.RawColorBGRA(0, 255, 0, 255), 1.0f, 0);

                _device.BeginScene();
                //device.UpdateSurface(device.GetBackBuffer(0,0),new Surface(new IntPtr(e.D3DSurface)));
                emote.Draw();
                _device.EndScene();
                try
                {
                    _device.Present();
                }
                catch (SharpDXException exception)
                {
                    if (exception.ResultCode == SharpDX.Direct3D9.ResultCode.DeviceLost)
                    {
                        Console.WriteLine("Device Lost Detected");
                        emote.OnDeviceLost();
                        Result r;
                        while ((r = _device.TestCooperativeLevel()) == SharpDX.Direct3D9.ResultCode.DeviceLost)
                        {
                            Thread.Sleep(5);
                        }
                        r = _device.TestCooperativeLevel();
                        if (r == SharpDX.Direct3D9.ResultCode.DeviceNotReset)
                        {
                            emote.D3DReset();
                            emote.OnDeviceReset();
                            //e.D3DInitRenderState();
                        }
                        else
                        {
                            Console.WriteLine(r);
                        }
                        //r = _device.TestCooperativeLevel();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            //if (EnableTransparentWindow)
            //{
            //    emote.D3DAfterPresentSetTransparentWindow();
            //}

            elaspedTime = 0;
        }

        private void tsbRefresh_Click(object sender, EventArgs e)
        {
            LoadCharacters();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            toolStripButton1.Checked = true;
            toolStripButton2.Checked = false;
            toolStripButton3.Checked = false;
            listView1.View = View.LargeIcon;
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            toolStripButton1.Checked = false;
            toolStripButton2.Checked = false;
            toolStripButton3.Checked = true;
            listView1.View = View.List;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            toolStripButton1.Checked = false;
            toolStripButton2.Checked = true;
            toolStripButton3.Checked = false;
            listView1.View = View.Details;
        }

        private void nudWW_ValueChanged(object sender, EventArgs e)
        {
            renderForm.Width = (int)nudWW.Value;
        }

        private void nudWH_ValueChanged(object sender, EventArgs e)
        {
            renderForm.Height = (int)nudWH.Value;
        }

        private void nudRW_ValueChanged(object sender, EventArgs e)
        {
            ResizeRenderArea((int)nudRW.Value, (int)nudRH.Value);
        }

        private void nudRH_ValueChanged(object sender, EventArgs e)
        {
            ResizeRenderArea((int)nudRW.Value, (int)nudRH.Value);
        }

        public void ResizeRenderArea(int w, int h)
        {
            //?
        }

        private void LoadEmotePlayer()
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                lock (emote)
                {
                    //lock (emote)
                    {
                        players.ForEach(dr => emote.DeletePlayer(dr));
                        players.Clear();
                    }
                    foreach (int x in listView1.SelectedIndices)
                    {
                        //lock (emote)
                        {
                            ICharacterListViewItem item = (ICharacterListViewItem)listView1.Items[x].Tag;

                            EmotePlayer player = emote.CreatePlayer(item.Name, TransCryptCharacter(item.GetDataStream(), item.Key, emoteLibs.First().Key));
                            player.Show();

                            player.SetScale(0.4f, 0, 0);
                            player.SetCoord(0, 50, 0, 0);
                            player.StartWind(0f, 1f, 0.8f, 0.5f, 0.8f);
                            player.SetSmoothing(true);

                            players.Add(player);
                        }
                    }
                }
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            LoadEmotePlayer();
        }

        private Stream TransCryptCharacter(Stream stream, string keyIn, string keyOut)
        {
            if (keyIn == keyOut)
                return stream;

            return CharacterCrypto(CharacterCrypto(stream, keyIn), keyOut);
        }

        private Stream CharacterCrypto(Stream stream, string passKey)
        {
            UInt32 _ecx_0x10_ = 0;
            byte unkIter = 0x0A; // might be string length?
            for (byte itr = 0; itr < passKey.Length; itr++)
            {
                _ecx_0x10_ = _ecx_0x10_ * unkIter + ((uint)passKey[itr] - 0x30);
            }

            //UInt32 _ecx_0x00_ = 0x0F1F38B8;
            UInt32 _ecx_0x04_ = 0x075BCD15; // fixed
            UInt32 _ecx_0x08_ = 0x159A55E5; // fixed
            UInt32 _ecx_0x0C_ = 0x1F123BB5; // fixed
            UInt32 _ecx_0x14_ = 0x00000000; // fixed
            UInt32 _ecx_0x18_ = 0x00000000; // fixed

            UInt32 eax;
            UInt32 esi;
            UInt32 edx;

            byte[] retData = new byte[stream.Length];

            using (MemoryStream fsOut = new MemoryStream(retData))
            using (BinaryWriter writer = new BinaryWriter(fsOut))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    UInt32 FileMarker = reader.ReadUInt32(); // PSB\0
                    writer.Write(FileMarker);

                    UInt32 UnkVer = reader.ReadUInt32(); // Perhapse version?
                    writer.Write(UnkVer);

                    UInt32 DataOff1 = reader.ReadUInt32(); // data offset
                    writer.Write(DataOff1);

                    UInt32 DataOff2 = reader.ReadUInt32(); // data offset agaim
                    writer.Write(DataOff2);

                    UInt32 Unknown1 = reader.ReadUInt32(); // unknown
                    writer.Write(Unknown1);

                    UInt32 Unknown2 = reader.ReadUInt32(); // unknown
                    writer.Write(Unknown2);

                    UInt32 UnkOff3 = reader.ReadUInt32(); // Unknown Offset 3
                    writer.Write(UnkOff3);

                    UInt32 UnkOff4 = reader.ReadUInt32(); // Unknown Offset 4
                    writer.Write(UnkOff4);

                    UInt32 ImageOffset = reader.ReadUInt32(); // ImageDataOffset
                    writer.Write(ImageOffset);

                    UInt32 Unknown5 = reader.ReadUInt32(); // Unknown
                    writer.Write(Unknown5);

                    //ASSERT(reader.BaseStream.Position == DataOff1);
                    while (reader.BaseStream.Position < UnkOff3)
                    {
                        if (_ecx_0x14_ == 0x00000000)
                        {
                            eax = _ecx_0x04_;
                            esi = _ecx_0x10_;
                            edx = eax;
                            edx = edx << 0x0B;
                            edx ^= eax;
                            eax = _ecx_0x08_;
                            _ecx_0x04_ = eax;
                            eax = _ecx_0x0C_;
                            _ecx_0x08_ = eax;
                            eax = esi;
                            eax = eax >> 0x0B;
                            eax ^= edx;
                            eax = eax >> 0x08;
                            eax ^= esi;
                            eax ^= edx;
                            _ecx_0x0C_ = esi;
                            _ecx_0x10_ = eax;
                            _ecx_0x14_ = eax;
                            _ecx_0x18_ = 0x00000004;
                        }
                        byte dl = (byte)_ecx_0x14_; // truncate, get lowest byte
                        byte Data = reader.ReadByte();
                        Data ^= dl;
                        writer.Write(Data);
                        _ecx_0x14_ = _ecx_0x14_ >> 0x08;
                        _ecx_0x18_--;
                    }

                    int len = (int)(reader.BaseStream.Length - reader.BaseStream.Position);
                    writer.Write(reader.ReadBytes(len), 0, len);
                }

                return new MemoryStream(retData);
            }
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Return)
                LoadEmotePlayer();
        }

        private void btnShowNodes_Click(object sender, EventArgs e)
        {
            functionGraph.Show();
        }
    }
}
