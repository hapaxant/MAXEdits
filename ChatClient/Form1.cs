using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using PlayerIOClient;
using Message = PlayerIOClient.Message;

namespace ChatClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // epic hack
                const string DirectoryName = "AntiDLLZone";
                const string FileName = "flash.exe";
                const string DownloadUrl = "https://www.adobe.com/support/flashplayer/debug_downloads.html";
                const string ShouldBeginWith = "https://fpdownload.macromedia.com/pub/flashplayer/updaters/";
                const int EndingLength = 24;// 69/flashplayer_69_sa.exe
                const string ShouldContain = "_sa.exe\"> <img src=\"/images/icons/download.gif\" width=\"16\" height=\"16\" alt=\"Download\" />Download the Flash Player projector</a> </li>";
                const string VersionFileName = "version.txt";

                Directory.CreateDirectory(DirectoryName);//we have to put the flash projector exe in its own directory because it just breaks when there's a .dll file in the same folder as .exe

                if (!File.Exists(DirectoryName + "/" + VersionFileName))
                    if (MessageBox.Show("Flash projector .exe will be now automatically downloaded.", "lol", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information)
                        != DialogResult.Yes)
                    {
                        Environment.Exit(0);
                    }
                    else File.WriteAllText(DirectoryName + "/" + VersionFileName, "firstrun");

                System.Net.WebClient client = new System.Net.WebClient()
                {
                    Proxy = null,
                };// works until version 100

                string str = client.DownloadString(DownloadUrl);
                string[] strv2 = str.Replace("\r", "").Split('\n');
                string line = strv2.Where(x => x.Contains(ShouldContain)).First();
                int index = line.IndexOf(ShouldBeginWith);
                string url = line.Substring(index, ShouldBeginWith.Length + EndingLength);
                string version = line.Substring(index + ShouldBeginWith.Length, 2);
                string lastVersion = File.ReadAllText(DirectoryName + "/" + VersionFileName).Trim();

                if (version != lastVersion)
                {
                    var result = MessageBox.Show($"New version of flash projector is available!" + Environment.NewLine +
                         $"Old version: {lastVersion} | New version: {version}" + Environment.NewLine +
                         $"Download URL: {url}" + Environment.NewLine +
                         $"Do you want to download it?", "Flash update available", lastVersion == "firstrun" ? MessageBoxButtons.OKCancel : MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes || result == DialogResult.OK)
                    {
                        File.WriteAllBytes(DirectoryName + "/" + FileName, client.DownloadData(url));
                        File.WriteAllText(DirectoryName + "/" + VersionFileName, version);
                    }
                    else if (result == DialogResult.Cancel)
                        Environment.Exit(0);
                }

                var p = new Process()
                {
                    EnableRaisingEvents = true,
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = DirectoryName + "\\" + FileName,
                        Arguments = "http://r.playerio.com/r/oldeeremake-xmiwpfxd106ptwe5p7xoq/oldee.swf",
                    }
                };
                p.Exited += delegate
                {
                    Environment.Exit(0);
                };
                p.Start();
                cli = PlayerIO.Authenticate("oldeeremake-xmiwpfxd106ptwe5p7xoq", "public", new Dictionary<string, string>() { { "userId", "despacito" } }, null);
                while (p.MainWindowHandle == IntPtr.Zero) Thread.Sleep(69);
                handle = p.MainWindowHandle;
                timer1.Start();
                con = cli.Multiplayer.CreateJoinRoom("lol", "Chat", true, null, null);
                con.OnDisconnect += OnDisconnect;
                con.OnMessage += OnMessage;
                con.Send("init");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "OOPSIE WOOPSIE!! Uwu We made a fucky wucky!! A wittle fucko boingo! The code monkeys at our headquarters are working VEWY HAWD to fix this!",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }

        private void OnMessage(object sender, Message m)
        {
            switch (m.Type)
            {
                case "timeout":
                    MessageBox.Show("timeout error");
                    Environment.Exit(0);
                    break;
                case "name":
                    XDDDD();//im sorry
                    richTextBox1.SafeInvoke(() => richTextBox1.AppendText("Connected!", Color.ForestGreen));
                    button2.SafeInvoke(() => button2.Text = m.GetString(0));
                    self = m.GetString(0);
                    break;
                case "rename":
                    if (m.GetString(0) == self)
                    {
                        self = m.GetString(1);
                        button2.SafeInvoke(() => button2.Text = m.GetString(1));
                    }
                    else
                    {
                        users.Remove(m.GetString(0));
                        users.Add(m.GetString(1));
                        UpdateOnline();
                    }
                    UpdateChat($"{m.GetString(0)} is now {m.GetString(1)}.", Color.Yellow);
                    break;
                case "oldsay":
                    UpdateChat($"{m.GetString(0)}: {m.GetString(1)}", Color.Gray);
                    break;
                case "say":
                    UpdateChat($"{m.GetString(0)}: {m.GetString(1)}", Color.White);
                    break;
                case "me":
                    UpdateChat($"* {m.GetString(0)} {m.GetString(1)}", Color.YellowGreen);
                    break;
                case "system":
                    UpdateChat(m.GetString(0), Color.OrangeRed);
                    break;
                case "roll":
                    UpdateChat($"* {m.GetString(0)} rolled {m.GetInt(1)}. [{m.GetInt(2)}-{m.GetInt(3)}]", Color.LightSeaGreen);
                    break;
                case "online":
                    for (uint i = 0; i < m.Count; i++)
                    {
                        if (!users.Contains(m.GetString(i))) users.Add(m.GetString(i));
                    }
                    UpdateOnline();
                    break;
                case "add":
                    users.Add(m.GetString(0));
                    UpdateChat($"{m.GetString(0)} has joined the chat.", Color.CadetBlue);
                    UpdateOnline();
                    break;
                case "left":
                    users.Remove(m.GetString(0));
                    UpdateChat($"{m.GetString(0)} has left the chat.", Color.CadetBlue);
                    UpdateOnline();
                    break;
            }
        }

        private void OnDisconnect(object sender, string message)
        {
            MessageBox.Show(message, "Disconnected", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(0);
        }

        IntPtr handle;
        Point lol = new Point(0, 0);
        readonly Control fuck = new Control("", 0, 0, 0, 0); // Works on my machine™
        private void timer1_Tick(object sender, EventArgs e)
        {// Works on my machine™
            //var rect = Despacito.GetWindowRect(handle);
            //var siz = new Size(250, rect.Bottom - rect.Top);
            //this.Size = siz;
            //var loc = new Point(rect.Right, rect.Top);
            //if (loc.X + siz.Width > Screen.FromControl(this).Bounds.Width)
            //{
            //    loc.X = rect.Left - siz.Width;
            //}
            //this.Location = loc;
            //if (lol != loc)
            //{
            //    TopMost = true;
            //    TopMost = false;
            //}
            //lol = loc;
            //MessageBox.Show(this.Size.ToString());

            var rect = Despacito.GetWindowRect(handle);
            var siz = new Size(250, rect.Height /*rect.Bottom - rect.Top*/);
            // Works on my machine™
            //var screen = Screen.FromControl(this);
            //var loc = new Point(rect.Left, rect.Top);
            //var screen2 = Screen.FromRectangle(rect);
            //if(loc.Bounds.)
            //var screen = Screen.FromRectangle(rect);
            //var loc = new Point(rect.Right, rect.Top);
            //var screen = Screen.FromRectangle(rect);
            //if (!screen.Bounds.Contains(loc))
            //{
            //    loc = new Point(loc.X + screen.Bounds.X , loc.Y + screen.Bounds.Y );
            //}
            //if (loc.X + siz.Width > screen.Bounds.Width)
            //{
            //    loc.X = rect.Left - siz.Width;
            //}
            //else
            //{
            //    loc.X = rect.Right;
            //}
            //var loc = new Point(rect.Right, rect.Top);
            //var screen = Screen.FromControl(this);
            //if (!screen.Primary) { loc.X -= screen.Bounds.Width; }
            ////bool isPrimary = screen.Primary;
            ////if (!isPrimary)
            ////{
            ////    loc.X -= screen.Bounds.Width;
            ////}
            //if (loc.X + siz.Width > screen.Bounds.Width)
            //{
            //    loc.X = rect.Left - siz.Width;
            //}

            // Works on my machine™
            var loc = new Point(rect.Right, rect.Top);
            // Works on my machine™
            var screen = Screen.GetBounds(loc);
            //var test = PointToScreen(new Point(-screen.X, -screen.Y));
            //var test = new Control("", loc.X, loc.Y, 0, 0).PointToScreen(new Point(-screen.X, -screen.Y));
            fuck.Location = new Point(loc.X - 8, loc.Y - 8); // Works on my machine™
            var test = fuck.PointToScreen(new Point(-screen.X, -screen.Y)); // Works on my machine™


            if (test.X + siz.Width > screen.Width)
            {// Works on my machine™
                loc.X = rect.Left - siz.Width;
            }
            // Works on my machine™

            this.Size = siz;
            this.Location = loc;// Works on my machine™
            if (lol != loc)
            {
                //UpdateChat(loc.ToString(), Color.White);
                //UpdateChat(rect.ToString(), Color.White);
                //UpdateChat(test.ToString(), Color.White);// Works on my machine™
                TopMost = true;
                TopMost = false;
            }// Works on my machine™
            lol = loc;
        }

        partial class Despacito
        {
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
            [StructLayout(LayoutKind.Sequential)]
            private struct RECT
            {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom;
            }
            public static Rectangle GetWindowRect(IntPtr handle)
            {
                RECT rct = new RECT();
                GetWindowRect(handle, ref rct);
                //return new Rectangle(rct.Left, rct.Top, rct.Right, rct.Bottom);
                return new Rectangle(rct.Left, rct.Top, rct.Right - rct.Left, rct.Bottom - rct.Top);
            }
        }

        Client cli;
        Connection con;
        List<string> users = new List<string>();
        string self = null;
        void UpdateOnline()
        {
            richTextBox2.SafeInvoke(() =>
            {
                StringBuilder sb = new StringBuilder();
                foreach (var user in users)
                {
                    sb.Append(user + " ");
                }
                if (sb.Length > 0) sb.Length -= 1;
                richTextBox2.Clear();
                richTextBox2.Text = sb.ToString();
            });
        }
        void UpdateChat(string str, Color color)
        {
            richTextBox1.SafeInvoke(() =>
            {
                richTextBox1.AppendText(Environment.NewLine + str, color);
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
            });
        }
        void XDDDD()//im sorry
        {
            richTextBox1.SafeInvoke(() =>
            {
                richTextBox1.Clear();
                richTextBox2.Clear();
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Type /nick <nickname> to set a new nickname.", "lol", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                button1.PerformClick();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(textBox1.Text))
            {
                string text = textBox1.Text.Trim();
                con.Send("say", text);
            }
            textBox1.Text = "";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing) e.Cancel = true;
        }
    }
    public static partial class Despacito
    {
        public static void SafeInvoke(this Control c, Action act)
        {
            if (c.InvokeRequired)
            {
                c.Invoke(act);
            }
            else
            {
                act();
            }
        }
        public static Task SafeBeginInvoke(this Control c, Action act)
        {
            return Task.Run(() =>
            {
                if (c.InvokeRequired)
                {
                    c.Invoke(act);
                }
                else
                {
                    act();
                }
            });
        }
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }
}
