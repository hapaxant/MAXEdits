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
                const string DirectoryName = "AntiDLLZone";
                const string FileName = "despacito.exe";
                //if (Directory.Exists(DirectoryName))
                //{
                //    //File.Delete(DirectoryName + "/" + FileName);
                //    //Directory.Delete(DirectoryName);
                //}
                Directory.CreateDirectory(DirectoryName);
                Assembly ass = Assembly.GetExecutingAssembly();
                using (Stream lol = ass.GetManifestResourceStream(ass.GetManifestResourceNames().Where(x => x.EndsWith(".exe")).ToList()[0]))// lmao
                using (FileStream kek = new FileStream(DirectoryName + "/" + FileName, FileMode.Create))
                {
                    lol.CopyTo(kek);
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
                    File.Delete(DirectoryName + "/" + FileName);
                    Directory.Delete(DirectoryName);
                    Application.Exit();
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
                MessageBox.Show(ex.ToString(), "despacito", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void OnMessage(object sender, Message m)
        {
            switch (m.Type)
            {
                case "timeout":
                    MessageBox.Show("this shouldnt happen wtf");
                    Application.Exit();
                    break;
                case "name":
                    XDDDD();
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
                    UpdateChat($"* {m.GetString(0)} rolled {m.GetInt(1)}.", Color.LightSeaGreen);
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
            Application.Exit();
        }

        IntPtr handle;
        Point lol = new Point(0, 0);
        private void timer1_Tick(object sender, EventArgs e)
        {
            var rect = Despacito.GetWindowRect(handle);
            var siz = new Size(250, rect.Bottom - rect.Top);
            this.Size = siz;
            var loc = new Point(rect.Right, rect.Top);
            if (loc.X + siz.Width > Screen.GetBounds(this).Width)
            {
                loc.X = rect.Left - siz.Width;
            }
            this.Location = loc;
            if (lol != loc)
            {
                TopMost = true;
                TopMost = false;
            }
            lol = loc;
            //MessageBox.Show(this.Size.ToString());
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
        void XDDDD()
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
