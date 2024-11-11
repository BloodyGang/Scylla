using Guna.UI2.WinForms;
using System.Text.RegularExpressions;

namespace Scylla
{
    public partial class Login : Form
    {
        public void ShowCustomMessageDialog(string message)
        {
            // Create a form
            Form customMessageForm = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.CenterParent,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = System.Drawing.Color.FromArgb(255, 54, 57, 63)
            };

            // Create a label to hold the message
            Label lblMessage = new Label
            {
                Text = message,
                AutoSize = true,
                Margin = new Padding(20),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = System.Drawing.Color.White
            };
            customMessageForm.Controls.Add(lblMessage);

            // Create OK button using Guna2Button
            Guna2Button btnOk = new Guna2Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                AutoSize = true,
                Margin = new Padding(20)
            };
            btnOk.Click += (sender, e) => customMessageForm.Close();

            Guna2Button btnOpenUrl = null;

            // Check for URL in message
            var urlMatch = Regex.Match(message, @"https?://[^\s]+");
            if (urlMatch.Success)
            {
                // Initialize Open URL button using Guna2Button
                btnOpenUrl = new Guna2Button
                {
                    Text = "Open URL",
                    AutoSize = true,
                    Margin = new Padding(20)
                };
                btnOpenUrl.Click += (sender, e) =>
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = urlMatch.Value,
                        UseShellExecute = true
                    });
                };
                customMessageForm.Controls.Add(btnOpenUrl);
            }

            customMessageForm.Controls.Add(btnOk);

            // Center buttons
            FlowLayoutPanel panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            panel.Controls.Add(btnOk);
            if (btnOpenUrl != null)
            {
                panel.Controls.Add(btnOpenUrl);
            }

            // Manually set the location to center the panel horizontally
            panel.Location = new Point((lblMessage.Width) / 3 - 10, lblMessage.Bottom);
            customMessageForm.Controls.Add(panel);

            // Show dialog
            customMessageForm.ShowDialog();
        }

        private void SetControlStates(bool enabled)
        {
            guna2Button1.Enabled = enabled;
        }

        public Login()
        {
            CharacterDictionaries.Initialize();
            InitializeComponent();
        }
        private void Form1_Load_1(object sender, EventArgs e)
        {
            DoubleBuffered = true;
            Animation cd = new Animation(this)
            {
                DotCount = 70,
                DotDistance = 100
            };
            cd.Start();

            try
            {
            }
            catch
            {
            }
        }
        private void guna2ControlBox1_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(1);
        }
        private void guna2Button1_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            SetControlStates(false);

            Main main = new Main();
            main.Show();
            Form parentForm = this;
            int parentWidth = parentForm.Width;
            int parentHeight = parentForm.Height;
            int formWidth = main.Width;
            int formHeight = main.Height;
            main.Location = new Point(
                parentForm.Location.X + (parentWidth - formWidth) / 2,
                parentForm.Location.Y + (parentHeight - formHeight) / 2
            );
            Hide();
        }
    }
}