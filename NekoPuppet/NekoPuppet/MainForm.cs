using EmoteEngineNet;
using NekoPuppet.Plugins;
using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.DXGI;
using SharpDX.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Device = SharpDX.Direct3D9.Device;

namespace NekoPuppet
{
    public partial class MainForm : Form
    {
        BindingList<EmoteModuleMetadata> emoteLibs = new BindingList<EmoteModuleMetadata>();

        private EnhancedRenderForm renderForm;
        private Emote emote;
        private Device _device;
        //private List<EmotePlayer> players = new List<EmotePlayer>();

        private EmotePlayer character;
        private EmotePlayer characterOffhand;

        private CharacterContext characterContext;

        private CharacterControlInterface characterIntf;
        private CharacterControlInterface characterOffhandIntf;

        public CharacterControlInterface CharacterInterface
        {
            get { return characterIntf; }
        }

        public CharacterControlInterface CharacterOffhandInterface
        {
            get { return characterOffhandIntf; }
        }

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
            characterContext = CharacterContext.CreateCharacterContext(this);
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
                ColorMode = ColorType.BGRA,
            };
            emoteLibs.Add(meta);
            //emoteLibs.Add(new EmoteModuleMetadata()
            //{
            //    Text = @"NEKOPARA Vol.1",
            //    Path = @"neko1\emotedriver.dll",
            //    Key = "742877301",
            //    Ver = InterfaceVersion.NEKO1,
            //});



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








        //private void RenderBitmap(Stream stream, string key)
        //{
        //    //EmoteModuleMetadata meta = emoteLibs.First();

        //    //Emote tmp_emote = new Emote(this.Handle, 128, 128, true, true);
        //    //tmp_emote.LoadEmoteEngine(Path.Combine(Directory.GetCurrentDirectory(), @"engines", meta.Path), meta.Ver);
        //    //tmp_emote.EmoteInit();



        //    //EmotePlayer player = tmp_emote.CreatePlayer("TMP", TransCryptCharacter(stream, key, meta.Key));
        //    //player.Show();

        //    //player.SetScale(0.4f, 0, 0);
        //    //player.SetCoord(0, 50, 0, 0);
        //    ////player.StartWind(0f, 1f, 0.8f, 0.5f, 0.8f);
        //    //player.SetSmoothing(true);

        //    ////tmp_emote.Draw();

        //    //Device tmp_device = new Device(new IntPtr(tmp_emote.D3Device));

        //    //lock (tmp_emote)
        //    //{
        //    //    //tmp_emote.Update((float)REFRESH * 1000);

        //    //    //_device.Clear(ClearFlags.Target, Color.Transparent, 1.0f, 0);
        //    //    //_device.Clear(ClearFlags.Target, new SharpDX.Mathematics.Interop.RawColorBGRA(0, 0, 0, 0), 1.0f, 0);
        //    //    tmp_device.Clear(ClearFlags.Target, new SharpDX.Mathematics.Interop.RawColorBGRA(0, 255, 0, 127), 1.0f, 0);

        //    //    tmp_device.BeginScene();
        //    //    //device.UpdateSurface(device.GetBackBuffer(0,0),new Surface(new IntPtr(e.D3DSurface)));
        //    //    tmp_emote.Draw();
        //    //    tmp_device.EndScene();
        //    //    try
        //    //    {
        //    //        tmp_device.Present();
        //    //    }
        //    //    catch (SharpDXException exception)
        //    //    {
        //    //        if (exception.ResultCode == SharpDX.Direct3D9.ResultCode.DeviceLost)
        //    //        {
        //    //            Console.WriteLine("Device Lost Detected");
        //    //            tmp_emote.OnDeviceLost();
        //    //            Result r;
        //    //            while ((r = tmp_device.TestCooperativeLevel()) == SharpDX.Direct3D9.ResultCode.DeviceLost)
        //    //            {
        //    //                Thread.Sleep(5);
        //    //            }
        //    //            r = tmp_device.TestCooperativeLevel();
        //    //            if (r == SharpDX.Direct3D9.ResultCode.DeviceNotReset)
        //    //            {
        //    //                tmp_emote.D3DReset();
        //    //                tmp_emote.OnDeviceReset();
        //    //                //e.D3DInitRenderState();
        //    //            }
        //    //            else
        //    //            {
        //    //                Console.WriteLine(r);
        //    //            }
        //    //            //r = _device.TestCooperativeLevel();
        //    //        }
        //    //        else
        //    //        {
        //    //            throw;
        //    //        }
        //    //    }










        //    //    //SharpDX.Direct3D9.Surface tmp_surf = new SharpDX.Direct3D9.Surface(new IntPtr(tmp_emote.D3DSurface));
        //    //    //SharpDX.Direct3D9.Surface tmp_surf = SharpDX.Direct3D9.Surface.CreateOffscreenPlain(tmp_device,  Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, SharpDX.Direct3D9.Format.A8R8G8B8, Pool.Scratch);
        //    //    //SharpDX.Direct3D9.Surface tmp_surf = SharpDX.Direct3D9.Surface.CreateOffscreenPlain(tmp_device,  128, 128, SharpDX.Direct3D9.Format.A8R8G8B8, Pool.Scratch);
        //    //    //SharpDX.Direct3D9.Surface tmp_surf = tmp_device.GetBackBuffer(0,0);
        //    //    //tmp_device.GetFrontBufferData(0, tmp_surf);
        //    //    //tmp_device.GetFrontBufferData(0, s);

        //    //    SharpDX.Direct3D9.Surface tmp_surf = tmp_device.GetRenderTarget(0);

        //    //    DataRectangle dataRectangle = tmp_surf.LockRectangle(LockFlags.None);


        //    //    //int size = dataRectangle.Pitch * 128 * 128;
        //    //    int size = 4 * 128 * 128;
        //    //    byte[] managedArray = new byte[size];
        //    //    Marshal.Copy(dataRectangle.DataPointer, managedArray, 0, size);

        //    //    int sourceX = 0;
        //    //    int sourceY = 0;
        //    //    int sourceHeight = 128;
        //    //    int sourceWidth = 128;

        //    //    /*for (int k = sourceX; k < sourceHeight; k++)
        //    //    {
        //    //        //for (int l = sourceY; l < sourceWidth; l++)
        //    //        //{
        //    //            Marshal.Copy(dataRectangle.DataPointer + (k * Screen.PrimaryScreen.Bounds.Width), managedArray, (k * sourceWidth), sourceWidth * 4);
        //    //        //}
        //    //    }*/
        //    //    tmp_surf.UnlockRectangle();

        //    //    //for (int k = sourceX; k < sourceHeight; k++)
        //    //    //{
        //    //    //    for (int l = sourceY; l < sourceWidth; l++)
        //    //    //    {
        //    //    //        managedArray[(k * dataRectangle.Pitch) + (l * 4)];
        //    //    //    }
        //    //    //}

        //    //    Bitmap output = new Bitmap(128, 128);
        //    //    Rectangle rect = new Rectangle(0, 0, output.Width, output.Height);
        //    //    BitmapData bmpData = output.LockBits(rect, ImageLockMode.ReadWrite, output.PixelFormat);
        //    //    IntPtr ptr = bmpData.Scan0;
        //    //    System.Runtime.InteropServices.Marshal.Copy(managedArray, 0, ptr, managedArray.Length);
        //    //    output.UnlockBits(bmpData);

        //    //    pictureBox1.Image = output;

        //    //    tmp_emote.Dispose();
        //    //}
        //}









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
                    List<ICharacterListViewItem> chars = dr.GetCharacters();
                    if(chars != null)
                        tmpList.AddRange(chars);
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
            //how?
        }

        private void LoadEmotePlayer()
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                foreach (int x in listView1.SelectedIndices)
                {
                    //lock (emote)
                    {
                        ICharacterListViewItem item = (ICharacterListViewItem)listView1.Items[x].Tag;
                        LoadCharacter(item);
                    }
                    break;
                }
            }
        }

        public void LoadCharacter(ICharacterListViewItem item)
        {
            lock (emote)
            {
                //lock (emote)
                {
                    //players.ForEach(dr => emote.DeletePlayer(dr));
                    //players.Clear();

                    if (characterOffhand != null)
                    {
                        emote.DeletePlayer(characterOffhand);
                        characterOffhandIntf = null; // allow falloff via GC, might look into disposable implementation if needed
                    }
                    characterOffhand = character;
                    characterOffhandIntf = characterIntf;
                    if (characterOffhand != null)
                    {
                        //characterOffhand.SetColor(0xffffff00, 20, 0.5f);
                        characterOffhand.Hide(); // temporary
                    }
                }


                EmotePlayer player = emote.CreatePlayer(item.Name, ColorModeCharacter(TransCryptCharacter(item.GetDataStream(), item.Key, emoteLibs.First().Key), item.ColorMode, emoteLibs.First().ColorMode));

                player.SetScale(0.4f, 0, 0);
                player.SetCoord(0, 50, 0, 0);
                player.StartWind(0f, 1f, 0.8f, 0.5f, 0.8f);
                player.SetSmoothing(true);
                //player.SetColor(0xffffff00, 0, 0.0f);

                player.Show();

                //player.SetColor(0xffffffff, 20, 0.5f);

                //players.Add(player);

                character = player;
                characterIntf = new CharacterControlInterface(character);

                //RenderBitmap(item.GetDataStream(), item.Key);


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

        private Stream ColorModeCharacter(Stream stream, ColorType keyIn, ColorType keyOut)
        {
            if (keyIn == keyOut)
                return stream;

            {
                byte[] retData = new byte[stream.Length];

                using (MemoryStream fsOut = new MemoryStream(retData))
                using (BinaryWriter writer = new BinaryWriter(fsOut))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        UInt32 FileMarker = reader.ReadUInt32(); // PSB\0
                        writer.Write(FileMarker);
                        //Console.WriteLine($"FileMarker: {FileMarker}");

                        UInt32 UnkVer = reader.ReadUInt32(); // Perhapse version?
                        writer.Write(UnkVer);
                        //Console.WriteLine($"UnkVer: {UnkVer}");

                        UInt32 DataOff1 = reader.ReadUInt32(); // data offset
                        writer.Write(DataOff1);
                        //Console.WriteLine($"DataOff1: {DataOff1}");

                        UInt32 DataOff2 = reader.ReadUInt32(); // data offset agaim
                        writer.Write(DataOff2);
                        //Console.WriteLine($"DataOff2: {DataOff2}");

                        UInt32 Unknown1 = reader.ReadUInt32(); // unknown
                        writer.Write(Unknown1);
                        //Console.WriteLine($"Unknown1: {Unknown1}");

                        UInt32 Unknown2 = reader.ReadUInt32(); // unknown
                        writer.Write(Unknown2);
                        //Console.WriteLine($"Unknown2: {Unknown2}");

                        UInt32 ResOffTable = reader.ReadUInt32();
                        writer.Write(ResOffTable);
                        //Console.WriteLine($"ResOffTable: {ResOffTable}");

                        UInt32 UnkOff4 = reader.ReadUInt32(); // Unknown Offset 4
                        writer.Write(UnkOff4);
                        //Console.WriteLine($"UnkOff4: {UnkOff4}");

                        UInt32 ImageOffset = reader.ReadUInt32(); // ImageDataOffset
                        writer.Write(ImageOffset);
                        //Console.WriteLine($"ImageOffset: {ImageOffset}");

                        UInt32 Unknown5 = reader.ReadUInt32(); // Unknown
                        writer.Write(Unknown5);
                        //Console.WriteLine($"Unknown5: {Unknown5}");

                        //Console.WriteLine("-----------------");

                        // skip part that's normally encrpyted
                        while (reader.BaseStream.Position < ResOffTable)
                        {
                            writer.Write(reader.ReadByte());
                        }

                        // skip some more stuff
                        while (reader.BaseStream.Position < UnkOff4)
                        {
                            writer.Write(reader.ReadByte());
                        }

                        byte _0D = reader.ReadByte();
                        writer.Write(_0D);

                        byte _unkByte = reader.ReadByte();
                        writer.Write(_unkByte);

                        UInt16 colorMode = reader.ReadUInt16();
                        writer.Write(colorMode);

                        // skip some more stuff
                        while (reader.BaseStream.Position < ImageOffset)
                        {
                            byte tmp = reader.ReadByte();
                            //Console.Write("{0:X2}", tmp);
                            writer.Write(tmp);
                        }
                        //Console.WriteLine();

                        // copy the rest of the file
                        //int len = (int)(reader.BaseStream.Length - reader.BaseStream.Position);
                        //writer.Write(reader.ReadBytes(len), 0, len);
                        while (reader.BaseStream.Position < reader.BaseStream.Length)
                        {
                            byte r = 0;
                            byte g = 0;
                            byte b = 0;
                            byte a = 0;

                            if (colorMode == 0x0F) // 16 bit
                            {
                                byte[] rawCols = reader.ReadBytes(2);

                                switch (keyIn)
                                {
                                    case ColorType.BGRA:
                                        b = (byte)((rawCols[1] & 0xf0) >> 4);
                                        g = (byte)(rawCols[1] & 0x0f);
                                        r = (byte)((rawCols[0] & 0xf0) >> 4);
                                        a = (byte)(rawCols[0] & 0x0f);
                                        break;
                                    case ColorType.RGBA:
                                        r = (byte)((rawCols[1] & 0xf0) >> 4);
                                        g = (byte)(rawCols[1] & 0x0f);
                                        b = (byte)((rawCols[0] & 0xf0) >> 4);
                                        a = (byte)(rawCols[0] & 0x0f);
                                        break;
                                }

                                switch (keyOut)
                                {
                                    case ColorType.BGRA:
                                        writer.Write((byte)(b | (g << 4)));
                                        writer.Write((byte)(r | (a << 4)));
                                        break;
                                    case ColorType.RGBA:
                                        writer.Write((byte)(r | (g << 4)));
                                        writer.Write((byte)(b | (a << 4)));
                                        break;
                                }
                            }
                            else if (colorMode == 0x10) // 32 bit
                            {
                                byte[] rawCols = reader.ReadBytes(4);

                                switch (keyIn)
                                {
                                    case ColorType.BGRA:
                                        b = rawCols[0];
                                        g = rawCols[1];
                                        r = rawCols[2];
                                        a = rawCols[3];
                                        break;
                                    case ColorType.RGBA:
                                        r = rawCols[0];
                                        g = rawCols[1];
                                        b = rawCols[2];
                                        a = rawCols[3];
                                        break;
                                }

                                switch (keyOut)
                                {
                                    case ColorType.BGRA:
                                        writer.Write(b);
                                        writer.Write(g);
                                        writer.Write(r);
                                        writer.Write(a);
                                        break;
                                    case ColorType.RGBA:
                                        writer.Write(r);
                                        writer.Write(g);
                                        writer.Write(b);
                                        writer.Write(a);
                                        break;
                                }
                            }
                            else
                            {
                                throw new NotImplementedException($"Unknown Color Mode 0x{colorMode:X4}");
                            }
                        }
                    }

                    return new MemoryStream(retData);
                }
            }
        }

        // thanks to marcussacana for his cleanup of this function
        // http://pastebin.com/basgJUgG
        private Stream CharacterCrypto(Stream stream, string passKey)
        {
            UInt32 MainKey = 0;
            byte unkIter = 0x0A; // might be string length?  Investigate
            for (byte itr = 0; itr < passKey.Length; itr++)
            {
                MainKey = MainKey * unkIter + ((uint)passKey[itr] - 0x30);
            }

            UInt32 Key1 = 0x075BCD15;
            UInt32 NextKey = 0x159A55E5;
            UInt32 Key2 = 0x1F123BB5;
            UInt32 XorKey = 0x00000000;

            UInt32 RstKey;
            UInt32 TmpXor;

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

                    UInt32 ResOffTable = reader.ReadUInt32();
                    writer.Write(ResOffTable);

                    UInt32 UnkOff4 = reader.ReadUInt32(); // Unknown Offset 4
                    writer.Write(UnkOff4);

                    UInt32 ImageOffset = reader.ReadUInt32(); // ImageDataOffset
                    writer.Write(ImageOffset);

                    UInt32 Unknown5 = reader.ReadUInt32(); // Unknown
                    writer.Write(Unknown5);

                    if (MainKey == 0)
                    {
                        while (reader.BaseStream.Position < ResOffTable)
                        {
                            writer.Write(reader.ReadByte());
                        }
                    }
                    else
                    {
                        //decrypt and copy file data
                        while (reader.BaseStream.Position < ResOffTable)
                        {
                            if (XorKey == 0)
                            {
                                TmpXor = (Key1 << 11) ^ Key1;
                                Key1 = NextKey;
                                NextKey = Key2;
                                RstKey = ((MainKey >> 11) ^ TmpXor) >> 8;
                                RstKey = (RstKey ^ TmpXor) ^ MainKey;
                                Key2 = MainKey;
                                MainKey = RstKey;
                                XorKey = RstKey;
                            }
                            byte Data = reader.ReadByte();
                            Data ^= (byte)XorKey; // truncate, get lowest byte
                            writer.Write(Data);
                            XorKey >>= 8;
                        }
                    }

                    // copy the rest of the file
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

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            functionGraph.NotifyNodesOfClose();
        }
    }
}
