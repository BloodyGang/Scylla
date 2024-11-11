using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Scylla
{
    public partial class Overlay : Form
    {
        public const string PROCESS_NAME = "DeadByDaylight-EGS-Shipping";

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(Keys vKey);

        public static IntPtr handle = IntPtr.Zero;

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        public static RECT rect;

        public struct RECT
        {
            public int left, top, right, bottom;
        }

        public Overlay()
        {
            InitializeComponent();
            KillerRevealer.instance.QueueUpdated += OnQueueUpdated;
            KillerRevealer.instance.MatchUpdated += OnMatchUpdated;
            KillerRevealer.instance.QueueCancelled += OnQueueCancelled;
        }

        private void Overlay_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            this.BackColor = Color.Wheat;
            this.TransparencyKey = Color.Wheat;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;

            int initialStyle = GetWindowLong(this.Handle, -20);
            SetWindowLong(this.Handle, -20, initialStyle | 0x8000 | 0x20);

            var processes = Process.GetProcessesByName(PROCESS_NAME);
            if (processes.Length > 0)
            {
                handle = processes[0].MainWindowHandle;
                GetWindowRect(handle, out rect);
                this.Size = new Size(rect.right - rect.left, rect.bottom - rect.top);
                this.Left = rect.left;
                this.Top = rect.top;
                backgroundWorker1.RunWorkerAsync();
            }
            else
            {
                this.Close();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                IntPtr foregroundWindowHandle = GetForegroundWindow();
                if (handle != IntPtr.Zero && foregroundWindowHandle == handle)
                {
                    GetWindowRect(handle, out rect);
                    this.Size = new Size(rect.right - rect.left, rect.bottom - rect.top);
                    this.Left = rect.left;
                    this.Top = rect.top;
                    this.Show();
                }
                else
                {
                    this.Hide();
                }
                Thread.Sleep(10);
            }

        }
        private void UpdateQueueLabel()
        {
            label1.Text = "Waiting time: " + Main.ETA + " seconds";
            label2.Text = "Position: " + Main.POS;
            label3.Text = "Killer Rank: " + Main.RANK;
            label4.Text = "Killer Country: " + Main.COUNTRY;
            label5.Text = "Killer Rating: " + Main.RATING;
            revealImageBoxCountry.Image = Main.COUNTRYFLAG;
            label1.Visible = true;
            label2.Visible = true;
            label8.Visible = true;
            label7.Visible = true;
            label3.Visible = true;
            label4.Visible = true;
            label5.Visible = true;
            label9.Visible = true;
            label10.Visible = true;
        }

        private void UpdateMatchLabel()
        {
            label1.Visible = true;
            label2.Visible = true;
            label8.Visible = true;
            label7.Visible = true;
            label3.Visible = true;
            label4.Visible = true;
            label5.Visible = true;
            label9.Visible = true;
            label10.Visible = true;
            label8.Text = "Match ID: " + Main.MATCHID;
            label7.Text = "Killer Name: " + Main.CHARNAME;
            label3.Text = "Killer Rank: " + Main.RANK;
            label4.Text = "Killer Country: " + Main.COUNTRY;
            label5.Text = "Killer Rating: " + Main.RATING;
            label9.Text = "Platform: " + Main.PLATFORM;
            label10.Text = "Steam ID: " + Main.STEAMID;
            revealImageBoxCountry.Image = Main.COUNTRYFLAG;
            if (Main.STEAMID != null)
            {
                label10.Visible = true;
            }
        }
        private void UpdateCancelLabel()
        {
            label1.Text = "Waiting time: ";
            label2.Text = "Position: ";
            label8.Text = "Match ID: ";
            label7.Text = "Killer Name: ";
            label3.Text = "Killer Rank: ";
            label4.Text = "Killer Country: ";
            label5.Text = "Killer Rating: ";
            label9.Text = "Platform: ";
            label10.Text = "Steam ID: ";
            revealImageBoxCountry.Image = null;
            label1.Visible = false;
            label2.Visible = false;
            label8.Visible = false;
            label7.Visible = false;
            label3.Visible = false;
            label4.Visible = false;
            label5.Visible = false;
            label9.Visible = false;
            label10.Visible = false;
        }
        private void OnQueueUpdated(object sender, EventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(delegate
            {
                this.UpdateQueueLabel();
            }));
        }

        private void OnMatchUpdated(object sender, EventArgs e)
        {
            this.BeginInvoke(new MethodInvoker(delegate
            {
                this.UpdateMatchLabel();
            }));
        }

        private void OnQueueCancelled(object sender, EventArgs e)
        {
            Console.WriteLine("Cancel Event listened");
            this.BeginInvoke(new MethodInvoker(delegate
            {
                this.UpdateCancelLabel();
            }));
        }
    }
}
