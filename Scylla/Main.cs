using MetroFramework.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Label = System.Windows.Forms.Label;
using ScyllaRes = Scylla.Resources.Resources;

namespace Scylla
{
    public partial class Main : Form
    {
        public static string? REGION;
        public static bool REGIONBOOL = false;
        public static string? BODY;
        public static string? CURRENCY;
        public static bool RANKDISPLAY = true;
        public static bool ACCOUNTEDITOR = true;
        public static bool SKINSONLY = true;
        public static bool CURRENCYSPOOF = true;
        public static bool LEVELSPOOF = true;
        public static bool CHALLENGECOMPLETOR = true;
        public static bool BREAKSETS = true;
        public static bool DISABLEDSTUFF = true;
        public static bool KILLERREVEAL = true;
        public static bool CROSSHAIR = true;
        public static bool RANKREVEAL = true;
        public static Color COLOR;
        public static string ETA = String.Empty;
        public static string MATCHID = String.Empty;
        public static string CHARNAME = String.Empty;
        public static string POS = String.Empty;
        public static string STEAMID = String.Empty;
        public static string BHVRSESSION = String.Empty;
        public static string RANK = String.Empty;
        public static string COUNTRY = String.Empty;
        public static System.Drawing.Image COUNTRYFLAG = null;
        public static string RATING = String.Empty;
        public static string SERVER = String.Empty;
        public static string PLATFORM = String.Empty;
        public static string USERID = String.Empty;
        public static string EMS = String.Empty;
        public static System.Drawing.Image? SELECTEDCROSSHAIR;
        public static int PROFILE = 0;
        public static int VERSION = 0;

        private BackgroundWorker hotkeyWorker;
        [DllImport("user32", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        private const int WM_HOTKEY = 0x0312;
        private int hotkeyId = 1;
        private const int MOD_ALT = 0x1;
        private const int MOD_CONTROL = 0x2;
        private const int MOD_SHIFT = 0x4;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        private static extern bool TranslateMessage([In] ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public Point pt;
        }

        public Main()
        {
            MoveConfigsToNewLocation();
            InitializeComponent();
            hotkeyWorker = new BackgroundWorker();
            hotkeyWorker.DoWork += new DoWorkEventHandler(hotkeyWorker_DoWork);
            hotkeyWorker.RunWorkerAsync();
            KillerRevealer.instance.QueueUpdated += OnQueueUpdated;
            KillerRevealer.instance.MatchUpdated += OnMatchUpdated;
            KillerRevealer.instance.QueueCancelled += OnQueueCancelled;
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            guna2Panel2.Location = new Point(205, 136);
            guna2Panel6.Location = new Point(1000, 1000);
            guna2Panel7.Location = new Point(1000, 1000);
            guna2Panel8.Location = new Point(1000, 1000);
            guna2Panel9.Location = new Point(1000, 1000);
            guna2Panel10.Location = new Point(1000, 1000);
            guna2Panel12.Location = new Point(1000, 1000);
        }

        private void SetConfigButtons(bool enabled)
        {
            guna2Button8.Enabled = enabled;
            guna2Button9.Enabled = enabled;
            guna2Button10.Enabled = enabled;
            guna2Button11.Enabled = enabled;
            guna2Button12.Enabled = enabled;
            guna2TextBox1.Enabled = enabled;
            guna2ComboBox1.Enabled = enabled;
        }

        private bool isArchive(string path)
        {
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Read))
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void MoveConfigsToNewLocation()
        {
            string pathToNewConfigsFolder = Path.Combine(CreateScyllaFolderInRoaming(), "Configs");
            string pathToOldConfigsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Scylla_Configs");
            if (Directory.Exists(pathToOldConfigsFolder))
            {
                foreach (string newPath in Directory.GetFiles(pathToOldConfigsFolder, "*.*"))
                {
                    File.Copy(newPath, newPath.Replace(pathToOldConfigsFolder, pathToNewConfigsFolder), true);
                }
                Directory.Delete(pathToOldConfigsFolder, true);
            }
            else
            {

            }
        }

        private string UpdateJson(string currentJson, string tabPageText, int prestigeLevel, string value)
        {
            JObject data = JObject.Parse(currentJson);
            //var character = data["list"].FirstOrDefault(c => translateKiller((string)c["characterName"]) == tabPageText);
            var character = data["list"].FirstOrDefault(c => CharacterDictionaries.TranslateCharacter((string)c["characterName"]) == tabPageText);
            if (character != null)
            {
                character[value] = prestigeLevel;
            }
            return data.ToString();
        }
        private string UpdateJsonForAll(string currentJson, int prestigeLevel, string value)
        {
            JObject data = JObject.Parse(currentJson);
            foreach (var character in data["list"])
            {
                character[value] = prestigeLevel;
            }
            return data.ToString();
        }
        public static void UpdatedBloodweb()
        {
            string pathToProfile = Path.Combine(CreateScyllaFolderInRoaming(), "Bloodweb.txt");
            int currentQuantity = 0;
            JObject obj = JObject.Parse(BODY);
            string characterNamee = (string)obj["characterName"];
            string json = File.ReadAllText(Path.Combine(CreateScyllaFolderInRoaming(), "Bloodweb.txt"));
            string jsonPROFILE = File.ReadAllText(Path.Combine(CreateScyllaFolderInRoaming(), "Profile.txt"));
            JObject data = JObject.Parse(json);
            JObject dataPROFILE = JObject.Parse(jsonPROFILE);
            foreach (var characterr in dataPROFILE["list"])
            {
                var characterName = (string)characterr["characterName"];
                if (characterName == characterNamee)
                {
                    data["prestigeLevel"] = (int)characterr["prestigeLevel"];
                    foreach (var item in characterr["characterItems"])
                    {
                        string itemId = (string)item["itemId"];
                        if (itemId == "AzarovKey")
                        {
                            currentQuantity = (int)item["quantity"];
                        }
                    }
                }
            }
            bool updateQuantity = false;
            foreach (var item in data["characterItems"])
            {
                string itemId = (string)item["itemId"];
                if (itemId == "Item_Camper_AlexsToolbox")
                {
                    updateQuantity = true;
                }
                if (updateQuantity)
                {
                    item["quantity"] = currentQuantity;
                }
                if (itemId == "Anniversary2023Offering")
                {
                    break;
                }
            }
            File.WriteAllText(pathToProfile, data.ToString());
        }
        private string UpdateForAll(string currentJson, int quantity)
        {
            int amount() => randomizerSwitch.Checked ? Random.Shared.Next(50, quantity) : quantity - 50;
            JObject data = JObject.Parse(currentJson);
            foreach (var character in data["list"])
            {
                var characterName = (string)character["characterName"];
                {
                    bool updateQuantity = false;
                    foreach (var item in character["characterItems"])
                    {
                        string itemId = (string)item["itemId"];
                        if (itemId == "Item_Camper_AlexsToolbox")
                        {
                            updateQuantity = true;
                        }
                        if (updateQuantity)
                        {
                            item["quantity"] = amount();
                        }
                        if (itemId == "Anniversary2023Offering")
                        {
                            item["quantity"] = amount();
                            break;
                        }
                    }
                }
            }
            return data.ToString();
        }
        private string UpdateJsonItems(string currentJson, string selectedPage, int quantity)
        {
            int amount() => randomizerSwitch.Checked ? Random.Shared.Next(50, quantity) : quantity - 50;
            JObject data = JObject.Parse(currentJson);
            foreach (var character in data["list"])
            {
                var characterName = (string)character["characterName"];
                //if (translateKiller(characterName) == selectedPage)
                if (CharacterDictionaries.TranslateKiller(characterName) == selectedPage)
                {
                    bool updateQuantity = false;
                    foreach (var item in character["characterItems"])
                    {
                        string itemId = (string)item["itemId"];
                        if (itemId == "Item_Camper_AlexsToolbox")
                        {
                            updateQuantity = true;
                        }
                        if (updateQuantity)
                        {
                            item["quantity"] = amount();
                        }
                        if (itemId == "Anniversary2023Offering")
                        {
                            item["quantity"] = amount();
                            break;
                        }
                    }
                }
            }
            return data.ToString();
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            guna2Panel2.Location = new Point(1000, 1000);
            guna2Panel6.Location = new Point(1000, 1000);
            guna2Panel7.Location = new Point(205, 136);
            guna2Panel8.Location = new Point(1000, 1000);
            guna2Panel9.Location = new Point(1000, 1000);
            guna2Panel10.Location = new Point(1000, 1000);
            guna2Panel12.Location = new Point(1000, 1000);
        }

        private void Main_Load(object sender, EventArgs e)
        {
            LoadProfileEditor();

            Size = new Size(980, 600);
            guna2HtmlToolTip1.MaximumSize = new Size(500, 500);
            DoubleBuffered = true;
            guna2HtmlLabel1.Text = "Changelog HTML";

            //guna2ToggleSwitch8.Checked = true;
            //guna2ToggleSwitch9.Checked = true;

        }
        private void guna2NumericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (guna2ToggleSwitch2.Checked == true)
            {
                string pathToProfile = Path.Combine(CreateScyllaFolderInRoaming(), "Profile.txt");
                string json = File.ReadAllText(pathToProfile);
                json = UpdateJsonForAll(json, ((int)guna2NumericUpDown1.Value), "bloodWebLevel");
                File.WriteAllText(pathToProfile, json);
            }
        }

        private void CloseApp_Click(object sender, EventArgs e)
        {
            FiddlerCore.Stop();
            DisableProxy();
            System.Environment.Exit(0);
        }
        private void DisableProxy()
        {
            try
            {
                RegistryKey registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
                registry.SetValue("ProxyEnable", 0);
                registry.Close();

                Console.WriteLine("Proxy byla úspěšně vypnuta.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Nastala chyba při vypínání proxy: " + ex.Message);
            }
        }
        public static string CreateScyllaFolderInRoaming()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var scyllaFolderPath = Path.Combine(appDataPath, "Scylla");
            var scyllaConfigsPath = Path.Combine(scyllaFolderPath, "Configs");

            Directory.CreateDirectory(scyllaConfigsPath);

            return scyllaFolderPath;
        }
        private void guna2NumericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            if (guna2ToggleSwitch2.Checked == true)
            {
                string pathToProfile = Path.Combine(CreateScyllaFolderInRoaming(), "Profile.txt");
                string json = File.ReadAllText(pathToProfile);
                json = UpdateJsonForAll(json, ((int)guna2NumericUpDown3.Value), "prestigeLevel");
                File.WriteAllText(pathToProfile, json);
            }
        }
        private void LoadProfileEditor()
        {
            metroTabControl1.TabPages.Clear();
            metroTabControl1.Alignment = TabAlignment.Top;
            string pathToProfile = Path.Combine(CreateScyllaFolderInRoaming(), "Profile.txt");
            string json = File.ReadAllText(pathToProfile);
            string pathToBloodweb = Path.Combine(CreateScyllaFolderInRoaming(), "Profile.txt");
            string jsonBloodweb = File.ReadAllText(pathToBloodweb);
            JObject data = JObject.Parse(json);
            JObject dataBloodweb = JObject.Parse(jsonBloodweb);
            int xPosition = (metroTabControl1.Width - 320) / 2; // výpočet x pozice pro středování

            // Vytvoření záložek pro každou postavu
            //foreach (var character in data["list"].OrderBy(c => translateKiller((string)c["characterName"])))
            foreach (var character in data["list"].OrderBy(c => CharacterDictionaries.TranslateCharacter((string)c["characterName"])))
            {
                //string characterName = translateKiller((string)character["characterName"]);
                string characterName = CharacterDictionaries.TranslateCharacter((string)character["characterName"]);
                var tabPage = new MetroTabPage();
                tabPage.Text = characterName;
                tabPage.UseStyleColors = true;
                tabPage.Theme = MetroFramework.MetroThemeStyle.Dark;
                metroTabControl1.TabPages.Add(tabPage);

                int yPosition = 50; // pozice y pro první tracker

                // TrackBar pro prestigeLevel
                var prestigeLevelNumericUpDown = new Guna.UI2.WinForms.Guna2NumericUpDown();
                prestigeLevelNumericUpDown.Location = new System.Drawing.Point(xPosition, yPosition);
                prestigeLevelNumericUpDown.Width = 200;
                prestigeLevelNumericUpDown.Minimum = 1;
                prestigeLevelNumericUpDown.Maximum = 100;
                prestigeLevelNumericUpDown.BackColor = Color.Transparent;
                prestigeLevelNumericUpDown.Value = (int)character["prestigeLevel"];
                prestigeLevelNumericUpDown.FillColor = System.Drawing.Color.FromArgb(17, 17, 17);
                prestigeLevelNumericUpDown.BorderColor = System.Drawing.Color.FromArgb(0, 197, 255);
                prestigeLevelNumericUpDown.UpDownButtonFillColor = System.Drawing.Color.FromArgb(0, 197, 255);
                prestigeLevelNumericUpDown.ForeColor = System.Drawing.Color.LightGray;
                prestigeLevelNumericUpDown.BorderRadius = 6;
                prestigeLevelNumericUpDown.BorderThickness = 2;
                tabPage.Controls.Add(prestigeLevelNumericUpDown);

                // Popisek pro prestigeLevel
                var prestigeLevelLabel = new Label();
                prestigeLevelLabel.Text = ScyllaRes.prestigeLevel + ": " + prestigeLevelNumericUpDown.Value;
                prestigeLevelLabel.ForeColor = System.Drawing.Color.LightGray;
                prestigeLevelLabel.AutoSize = true;
                prestigeLevelLabel.BackColor = Color.Transparent;
                prestigeLevelLabel.Font = new Font("SugoiUI", 10, FontStyle.Bold);
                prestigeLevelLabel.Location = new System.Drawing.Point(xPosition + 210, yPosition + 5);
                tabPage.Controls.Add(prestigeLevelLabel);

                yPosition += 60; // přidání vzdálenosti mezi trackery

                // TrackBar pro bloodWebLevel
                var bloodWebLevelTrackBar = new Guna.UI2.WinForms.Guna2NumericUpDown();
                bloodWebLevelTrackBar.Location = new System.Drawing.Point(xPosition, yPosition);
                bloodWebLevelTrackBar.Width = 200;
                bloodWebLevelTrackBar.Minimum = 1;
                bloodWebLevelTrackBar.Maximum = 9999;
                bloodWebLevelTrackBar.BackColor = Color.Transparent;
                bloodWebLevelTrackBar.Value = (int)character["bloodWebLevel"];
                bloodWebLevelTrackBar.FillColor = System.Drawing.Color.FromArgb(17, 17, 17);
                bloodWebLevelTrackBar.BorderColor = System.Drawing.Color.FromArgb(0, 197, 255);
                bloodWebLevelTrackBar.UpDownButtonFillColor = System.Drawing.Color.FromArgb(0, 197, 255);
                bloodWebLevelTrackBar.ForeColor = System.Drawing.Color.LightGray;
                bloodWebLevelTrackBar.BorderRadius = 6;
                bloodWebLevelTrackBar.BorderThickness = 2;
                tabPage.Controls.Add(bloodWebLevelTrackBar);

                // Popisek pro bloodWebLevel
                var bloodWebLevelLabel = new Label();
                bloodWebLevelLabel.Text = ScyllaRes.bloodwebLevel + ": " + bloodWebLevelTrackBar.Value;
                bloodWebLevelLabel.ForeColor = System.Drawing.Color.LightGray;
                bloodWebLevelLabel.BackColor = Color.Transparent;
                bloodWebLevelLabel.Font = new Font("SugoiUI", 10, FontStyle.Bold);
                bloodWebLevelLabel.AutoSize = true;
                bloodWebLevelLabel.Location = new System.Drawing.Point(xPosition + 210, yPosition + 5);
                tabPage.Controls.Add(bloodWebLevelLabel);

                yPosition += 60;

                // TrackBar pro AzarovKey
                var azarovKeyTrackBar = new Guna.UI2.WinForms.Guna2NumericUpDown();
                azarovKeyTrackBar.Location = new System.Drawing.Point(xPosition, yPosition);
                azarovKeyTrackBar.Width = 200;
                azarovKeyTrackBar.Minimum = 50;
                azarovKeyTrackBar.Maximum = 5000;
                azarovKeyTrackBar.FillColor = System.Drawing.Color.FromArgb(17, 17, 17);
                azarovKeyTrackBar.BorderColor = System.Drawing.Color.FromArgb(0, 197, 255);
                azarovKeyTrackBar.UpDownButtonFillColor = System.Drawing.Color.FromArgb(0, 197, 255);
                azarovKeyTrackBar.ForeColor = System.Drawing.Color.LightGray;
                azarovKeyTrackBar.BorderRadius = 6;
                azarovKeyTrackBar.BorderThickness = 2;
                foreach (var item in character["characterItems"])
                {
                    if ((string)item["itemId"] == "Item_Camper_AlexsToolbox")
                    {
                        azarovKeyTrackBar.Value = (int)item["quantity"] + 50;
                        break;
                    }
                }
                tabPage.Controls.Add(azarovKeyTrackBar);

                // Popisek pro AzarovKey
                var azarovKeyLabel = new Label();
                azarovKeyLabel.Text = ScyllaRes.itemAmount + ": " + azarovKeyTrackBar.Value;
                azarovKeyLabel.ForeColor = System.Drawing.Color.LightGray;
                azarovKeyLabel.Font = new Font("SugoiUI", 10, FontStyle.Bold);
                azarovKeyLabel.Location = new System.Drawing.Point(xPosition + 210, yPosition + 5);
                azarovKeyLabel.BackColor = Color.Transparent;
                azarovKeyLabel.AutoSize = true;
                tabPage.Controls.Add(azarovKeyLabel);

                // Událost pro změnu hodnoty na azarovKeyTrackBar
                azarovKeyTrackBar.ValueChanged += (object senderTrackBar, EventArgs eTrackBar) =>
                {
                    json = UpdateJsonItems(json, tabPage.Text, (int)azarovKeyTrackBar.Value);
                    File.WriteAllText(pathToProfile, json);
                    azarovKeyLabel.Text = ScyllaRes.itemAmount + ": " + azarovKeyTrackBar.Value;
                };
                bloodWebLevelTrackBar.ValueChanged += (object senderTrackBar, EventArgs eTrackBar) =>
                {
                    json = UpdateJson(json, tabPage.Text, ((int)bloodWebLevelTrackBar.Value), "bloodWebLevel");
                    File.WriteAllText(pathToProfile, json);
                    bloodWebLevelLabel.Text = ScyllaRes.bloodwebLevel + ": " + bloodWebLevelTrackBar.Value;
                };
                prestigeLevelNumericUpDown.ValueChanged += (object senderTrackBar, EventArgs eTrackBar) =>
                {
                    json = UpdateJson(json, tabPage.Text, ((int)prestigeLevelNumericUpDown.Value), "prestigeLevel");
                    File.WriteAllText(pathToProfile, json);
                    prestigeLevelLabel.Text = ScyllaRes.prestigeLevel + ": " + prestigeLevelNumericUpDown.Value;
                };
            }
        }

        private void guna2ToggleSwitch2_CheckedChanged(object sender, EventArgs e)
        {
            if (guna2ToggleSwitch2.Checked == true)
            {
                randomizerSwitch.Enabled = true;
                metroTabControl1.Enabled = false;
                guna2NumericUpDown1.Enabled = true;
                guna2NumericUpDown2.Enabled = true;
                guna2NumericUpDown3.Enabled = true;
            }
            else
            {
                LoadProfileEditor();
                randomizerSwitch.Enabled = false;
                randomizerSwitch.Checked = false;
                metroTabControl1.Enabled = true;
                guna2NumericUpDown1.Enabled = false;
                guna2NumericUpDown2.Enabled = false;
                guna2NumericUpDown3.Enabled = false;
            }
        }

        private void guna2NumericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if (guna2ToggleSwitch2.Checked == true)
            {
                string pathToProfile = Path.Combine(CreateScyllaFolderInRoaming(), "Profile.txt");
                string json = File.ReadAllText(pathToProfile);
                json = UpdateForAll(json, (int)guna2NumericUpDown2.Value);
                File.WriteAllText(pathToProfile, json);
            }
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            guna2Panel2.Location = new Point(1000, 1000);
            guna2Panel6.Location = new Point(1000, 1000);
            guna2Panel7.Location = new Point(1000, 1000);
            guna2Panel8.Location = new Point(1000, 1000);
            guna2Panel9.Location = new Point(1000, 1000);
            guna2Panel10.Location = new Point(205, 136);
            guna2Panel12.Location = new Point(1000, 1000);
        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {
            panelplatform.Visible = false;
            guna2Panel2.Location = new Point(1000, 1000);
            guna2Panel6.Location = new Point(205, 136);
            guna2Panel7.Location = new Point(1000, 1000);
            guna2Panel8.Location = new Point(1000, 1000);
            guna2Panel9.Location = new Point(1000, 1000);
            guna2Panel9.Location = new Point(1000, 1000);
            guna2Panel10.Location = new Point(1000, 1000);
            guna2Panel12.Location = new Point(1000, 1000);
        }

        private void guna2Button7_Click(object sender, EventArgs e)
        {
            panelplatform.Visible = true;
        }

        private void guna2ToggleSwitch1_CheckedChanged(object sender, EventArgs e)
        {
            if (guna2ToggleSwitch1.Checked)
            {
                ACCOUNTEDITOR = true;
                SKINSONLY = false;
            }
            else
            {
                ACCOUNTEDITOR = false;
                SKINSONLY = true;
            }
        }

        private void guna2NumericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            ChangeCurrency();
        }
        public void ChangeCurrency()
        {
            JObject data = JObject.Parse(File.ReadAllText(Path.Combine(CreateScyllaFolderInRoaming(), "Currency.txt")));
            JArray currencies = (JArray)data["list"];

            for (int i = 0; i < currencies.Count; i++)
            {
                var currency = currencies[i];
                if ((string)currency["currency"] == "Shards")
                {
                    currency["balance"] = (int)guna2NumericUpDown4.Value;
                }
                else if ((string)currency["currency"] == "Cells")
                {
                    currency["balance"] = (int)guna2NumericUpDown6.Value;
                }
                else if ((string)currency["currency"] == "Bloodpoints")
                {
                    currency["balance"] = (int)guna2NumericUpDown5.Value;
                }
                else if ((string)currency["currency"] == "BonusBloodpoints")
                {
                    currency["balance"] = 0;
                }
            }
            File.WriteAllText(Path.Combine(CreateScyllaFolderInRoaming(), "Currency.txt"), data.ToString());
        }
        public void ChangeLevel()
        {
            JObject data = JObject.Parse(File.ReadAllText(Path.Combine(CreateScyllaFolderInRoaming(), "Level.txt")));
            data["level"] = (int)guna2NumericUpDown8.Value;
            data["prestigeLevel"] = (int)guna2NumericUpDown9.Value;
            File.WriteAllText(Path.Combine(CreateScyllaFolderInRoaming(), "Level.txt"), data.ToString());
        }
        public void GetLevel()
        {
            string levelFilePath = Path.Combine(CreateScyllaFolderInRoaming(), "Level.txt");
            JObject data = JObject.Parse(File.ReadAllText(levelFilePath));

            guna2NumericUpDown8.Value = (int)data["level"];
            guna2NumericUpDown9.Value = (int)data["prestigeLevel"];

            File.WriteAllText(levelFilePath, data.ToString());
        }
        private void guna2NumericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            ChangeCurrency();
        }

        private void guna2NumericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            ChangeCurrency();
        }

        private void guna2NumericUpDown9_ValueChanged(object sender, EventArgs e)
        {
            ChangeLevel();
        }

        private void guna2NumericUpDown8_ValueChanged(object sender, EventArgs e)
        {
            ChangeLevel();
        }
        public void GetCurrency()
        {
            string currencyFilePath = Path.Combine(CreateScyllaFolderInRoaming(), "Currency.txt");
            JObject data = JObject.Parse(File.ReadAllText(currencyFilePath));
            JArray currencies = (JArray)data["list"];

            for (int i = 0; i < currencies.Count; i++)
            {
                var currency = currencies[i];
                string currencyName = (string)currency["currency"];

                switch (currencyName)
                {
                    case "Shards":
                        guna2NumericUpDown4.Value = (int)currency["balance"];
                        break;
                    case "Cells":
                        guna2NumericUpDown6.Value = (int)currency["balance"];
                        break;
                    case "Bloodpoints":
                        guna2NumericUpDown5.Value = (int)currency["balance"];
                        break;
                    case "BonusBloodpoints":
                        currency["balance"] = 0;
                        break;
                }
            }

            File.WriteAllText(currencyFilePath, data.ToString());
        }
        private void guna2ToggleSwitch3_CheckedChanged(object sender, EventArgs e)
        {
            CURRENCYSPOOF = guna2ToggleSwitch3.Checked;
        }

        private void guna2ToggleSwitch4_CheckedChanged(object sender, EventArgs e)
        {
            LEVELSPOOF = guna2ToggleSwitch4.Checked;
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            guna2Panel2.Location = new Point(1000, 1000);
            guna2Panel6.Location = new Point(1000, 1000);
            guna2Panel7.Location = new Point(1000, 1000);
            guna2Panel8.Location = new Point(205, 136);
            guna2Panel9.Location = new Point(1000, 1000);
            guna2Panel10.Location = new Point(1000, 1000);
            guna2Panel12.Location = new Point(1000, 1000);
        }

        private void guna2ToggleSwitch5_CheckedChanged(object sender, EventArgs e)
        {
            CHALLENGECOMPLETOR = guna2ToggleSwitch5.Checked;
        }

        private void guna2ToggleSwitch7_CheckedChanged(object sender, EventArgs e)
        {
            DISABLEDSTUFF = guna2ToggleSwitch7.Checked;
        }
        public void PopulateComboBoxWithFiles(string folderPath, ComboBox comboBox)
        {
            string[] fileNames = Directory.GetFiles(folderPath);

            comboBox.Items.Clear();

            foreach (string fileName in fileNames)
            {
                comboBox.Items.Add(Path.GetFileName(fileName));
            }
        }
        private void guna2Button8_Click(object sender, EventArgs e)
        {
            SetConfigButtons(false);
            Settings settings = new Settings();
            if (!string.IsNullOrEmpty(guna2TextBox1.Text))
            {
                settings.PackFilesToCfg((Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Scylla"), (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Scylla\\Configs\\" + guna2TextBox1.Text + ".cfg"));
                label24.Text = "Status: " + ScyllaRes.configCreated;
            }
            else
            {
                MessageBox.Show(ScyllaRes.configNoName, "Scylla", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            SetConfigButtons(true);
        }

        private void guna2Button6_Click(object sender, EventArgs e)
        {
            guna2Panel2.Location = new Point(1000, 1000);
            guna2Panel6.Location = new Point(1000, 1000);
            guna2Panel7.Location = new Point(1000, 1000);
            guna2Panel8.Location = new Point(1000, 1000);
            guna2Panel10.Location = new Point(1000, 1000);
            guna2Panel9.Location = new Point(205, 136);
            guna2Panel12.Location = new Point(1000, 1000);
        }

        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void guna2ComboBox1_Click(object sender, EventArgs e)
        {
            PopulateComboBoxWithFiles(((Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Scylla\\Configs")), guna2ComboBox1);
        }
        private void guna2Button9_Click(object sender, EventArgs e)
        {
            SetConfigButtons(false);
            if (guna2ComboBox1.SelectedItem != null)
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string scyllaConfigsPath = Path.Combine(appDataPath, "Scylla\\Configs", guna2ComboBox1.Text);
                string scyllaTempPath = Path.Combine(appDataPath, "Scylla", "Temp");
                string scyllaMainPath = Path.Combine(appDataPath, "Scylla");
                string skinsWithItemsMainPath = Path.Combine(scyllaMainPath, "SkinsWithItems.txt");
                string skinsWithItemsTempPath = Path.Combine(scyllaTempPath, "SkinsWithItems.txt");

                Settings settings = new Settings();
                try
                {
                    settings.UnpackFilesFromCfg(scyllaConfigsPath, scyllaTempPath);
                }
                catch (Exception)
                {
                    MessageBox.Show(ScyllaRes.configInvalid, "Scylla", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    SetConfigButtons(true);
                    return;
                }

                string mergedJson = MergeJsons(File.ReadAllText(skinsWithItemsMainPath), File.ReadAllText(skinsWithItemsTempPath));
                File.WriteAllText(skinsWithItemsTempPath, mergedJson);

                string[] files = Directory.GetFiles(scyllaTempPath);
                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);
                    string destinationFile = Path.Combine(scyllaMainPath, fileName);
                    File.Move(file, destinationFile, true);
                }

                Directory.Delete(scyllaTempPath, true);
                LoadProfileEditor();
                GetCurrency();
                GetLevel();
                label25.Text = ScyllaRes.configLoaded;
            }
            else
            {
                MessageBox.Show(ScyllaRes.configNoSelect, "Scylla", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            SetConfigButtons(true);
        }
        private void guna2Button10_Click(object sender, EventArgs e)
        {
            SetConfigButtons(false);
            if (guna2ComboBox1.SelectedItem != null)
            {
                string selectedFile = (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Scylla\\Configs\\" + guna2ComboBox1.SelectedItem.ToString());
                File.Delete(selectedFile);
                PopulateComboBoxWithFiles(((Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Scylla\\Configs")), guna2ComboBox1);
                label25.Text = ScyllaRes.configDeleted;
            }
            else
            {
                MessageBox.Show(ScyllaRes.configNoSelect, "Scylla", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            SetConfigButtons(true);
        }
        private void guna2Button11_Click(object sender, EventArgs e)
        {
            SetConfigButtons(false);
            OpenFileDialog fileChooser = new OpenFileDialog();
            fileChooser.Filter = "CFG files (*.cfg)|*.cfg";
            if (fileChooser.ShowDialog() == DialogResult.OK)
            {
                string selectedFile = fileChooser.FileName;
                if (isArchive(selectedFile))
                {
                    string destinationPath = (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Scylla\\Configs");
                    string fileName = Path.GetFileName(selectedFile);
                    string destinationFile = Path.Combine(destinationPath, fileName);
                    File.Move(selectedFile, destinationFile);
                    label25.Text = ScyllaRes.configImported;
                }
                else
                {
                    MessageBox.Show(ScyllaRes.configInvalid, "Scylla", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            SetConfigButtons(true);
        }
        private void guna2Button12_Click(object sender, EventArgs e)
        {
            SetConfigButtons(false);
            if (guna2ComboBox1.SelectedItem != null)
            {
                string selectedFile = (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Scylla\\Configs\\" + guna2ComboBox1.SelectedItem.ToString());
                FolderBrowserDialog folderChooser = new FolderBrowserDialog();

                if (folderChooser.ShowDialog() == DialogResult.OK)
                {
                    string destinationFolder = folderChooser.SelectedPath;
                    string destinationFile = Path.Combine(destinationFolder, guna2ComboBox1.SelectedItem.ToString());
                    File.Copy(selectedFile, destinationFile);
                    label25.Text = ScyllaRes.configExported;
                }
            }
            else
            {
                MessageBox.Show(ScyllaRes.configNoName, "Scylla", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            SetConfigButtons(true);
        }
        public static string UpdateLatency(string json, string selectedRegion)
        {
            JObject obj = null;
            try
            {
                obj = JObject.Parse(json);
            }
            catch (JsonReaderException)
            {
                return json;
            }

            JArray latencies = (JArray)obj["latencies"];

            if (latencies == null)
            {
                return json;
            }

            foreach (JObject latency in latencies)
            {
                string regionName = (string)latency["regionName"];
                if (regionName.Equals(selectedRegion))
                {
                    latency["latency"] = 1;
                }
                else
                {
                    latency["latency"] = 999;
                }
            }

            string updatedJson = obj.ToString();
            return updatedJson;
        }
        private void guna2Button13_Click(object sender, EventArgs e)
        {
            REGION = guna2ComboBox2.Text;
            REGIONBOOL = true;
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
        private void UpdateQueueLabel()
        {
            label37.Text = ScyllaRes.waitingTime + ": " + Main.ETA + " " + ScyllaRes.seconds;
            label36.Text = ScyllaRes.position + ": " + Main.POS;
            label35.Text = ScyllaRes.killerRank + ": " + Main.RANK;
            label34.Text = ScyllaRes.killerCountry + ": " + Main.COUNTRY;
            label33.Text = ScyllaRes.killerRating + ": " + Main.RATING;
            revealImageBoxCountry.Image = Main.COUNTRYFLAG;
        }
        private void UpdateMatchLabel()
        {
            label29.Text = ScyllaRes.matchID + ": " + Main.MATCHID;
            label30.Text = ScyllaRes.killerName + ": " + Main.CHARNAME;
            label35.Text = ScyllaRes.killerRank + ": " + Main.RANK;
            label34.Text = ScyllaRes.killerCountry + ": " + Main.COUNTRY;
            label33.Text = ScyllaRes.killerRating + ": " + Main.RATING;
            label28.Text = ScyllaRes.platform + ": " + Main.PLATFORM;
            label27.Text = ScyllaRes.steamID + ": " + Main.STEAMID;
            revealImageBoxCountry.Image = Main.COUNTRYFLAG;
            if (Main.STEAMID != null)
            {
                label10.Visible = true;
            }
        }
        private void UpdateCancelLabel()
        {
            label37.Text = ScyllaRes.waitingTime + ": ";
            label36.Text = ScyllaRes.position + ": ";
            label29.Text = ScyllaRes.matchID + ": ";
            label30.Text = ScyllaRes.killerName + ": ";
            label35.Text = ScyllaRes.killerRank + ": ";
            label34.Text = ScyllaRes.killerCountry + ": ";
            label33.Text = ScyllaRes.killerRating + ": ";
            label28.Text = ScyllaRes.platform + ": ";
            label27.Text = ScyllaRes.steamID + ": ";
            revealImageBoxCountry.Image = null;
        }

        private void guna2Button16_Click(object sender, EventArgs e)
        {
            KillerRevealer.ViewDetails();
        }

        private void guna2Button14_Click(object sender, EventArgs e)
        {
            KillerRevealer.ViewSteamProfile();
        }
        public string MergeJsons(string json1, string json2)
        {
            JObject obj1 = JObject.Parse(json1);
            JObject obj2 = JObject.Parse(json2);

            JArray inventory1 = (JArray)obj1["data"]["inventory"];
            JArray inventory2 = (JArray)obj2["data"]["inventory"];

            foreach (JObject item in inventory1)
            {
                string objectId = (string)item["objectId"];
                JToken existingItem = inventory2.FirstOrDefault(i => (string)i["objectId"] == objectId);

                if (existingItem == null)
                {
                    inventory2.Insert(0, item);
                }
            }
            string mergedJson = obj2.ToString();
            return mergedJson;
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisableProxy();
        }

        private void guna2Panel3_Paint(object sender, PaintEventArgs e)
        {
        }

        private void guna2Button20_Click(object sender, EventArgs e)
        {
            Process[] processes = Process.GetProcessesByName("DeadByDaylight-EGS-Shipping");

            if (processes.Length > 0)
            {
                foreach (Process process in processes)
                {
                    try
                    {
                        process.Kill();
                        Console.WriteLine("Process with name \"{0}\" killed successfully.", "DeadByDaylight-EGS-Shipping");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to kill process with name \"{0}\". Error: {1}", "DeadByDaylight-EGS-Shipping", ex.Message);
                    }
                }
            }
            else
            {
                Console.WriteLine("No process found with name \"{0}\".", "DeadByDaylight-EGS-Shipping");
            }
            FiddlerCore.Stop();
            FiddlerCore.Start();
            LobbyInfo.instance.Init();
            LobbyInfo.instance.Start();
            guna2Button20.Enabled = false;
            guna2Button21.Enabled = false;
            MessageBox.Show(ScyllaRes.startedLaunchGamePlease, "Scylla", MessageBoxButtons.OK, MessageBoxIcon.Information);
            WindowState = FormWindowState.Minimized;
        }
        private void guna2Button21_Click(object sender, EventArgs e)
        {
            Process[] processes = Process.GetProcessesByName("DeadByDaylight-EGS-Shipping");

            if (processes.Length > 0)
            {
                foreach (Process process in processes)
                {
                    try
                    {
                        process.Kill();
                        Console.WriteLine("Process with name \"{0}\" killed successfully.", "DeadByDaylight-EGS-Shipping");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to kill process with name \"{0}\". Error: {1}", "DeadByDaylight-EGS-Shipping", ex.Message);
                    }
                }
            }
            else
            {
                Console.WriteLine("No process found with name \"{0}\".", "DeadByDaylight-EGS-Shipping");
            }
            FiddlerCore.Stop();
            FiddlerCore.Start();
            LobbyInfo.instance.Init();
            LobbyInfo.instance.Start();
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "com.epicgames.launcher://apps/611482b8586142cda48a0786eb8a127c%3A467a7bed47ec44d9b1c9da0c2dae58f7%3ABrill?action=launch&silent=true",
                UseShellExecute = true
            };
            Process.Start(processStartInfo);
            guna2Button20.Enabled = false;
            guna2Button21.Enabled = false;
            MessageBox.Show(ScyllaRes.spoofedSuccessfully, "Scylla", MessageBoxButtons.OK, MessageBoxIcon.Information);
            WindowState = FormWindowState.Minimized;
        }

        private void guna2Button15_Click_1(object sender, EventArgs e)
        {
            var processes = Process.GetProcessesByName(Overlay.PROCESS_NAME);
            if (processes.Length > 0)
            {
                Overlay overlay = new Overlay();
                overlay.Show();
            }
            else
            {
                Console.WriteLine("No process found with name \"{0}\".", Overlay.PROCESS_NAME);
            }
        }

        private void guna2ToggleSwitch8_CheckedChanged(object sender, EventArgs e)
        {
            RANKREVEAL = guna2ToggleSwitch8.Checked;
        }

        private void guna2ToggleSwitch9_CheckedChanged(object sender, EventArgs e)
        {
            KILLERREVEAL = guna2ToggleSwitch9.Checked;
        }
        private void label12_Click(object sender, EventArgs e)
        {
        }

        private void label11_Click(object sender, EventArgs e)
        {
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {
        }

        private void guna2ShadowPanel1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void guna2ToggleSwitch10_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void label50_Click(object sender, EventArgs e)
        {
        }

        private void metroTabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
        private void frm_main_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                ShowInTaskbar = false;
                notifyIcon1.Visible = true;
                notifyIcon1.ShowBalloonTip(1000);
            }
        }
        private void systrayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                restoreWindowFromSystray(sender, e);
            }
            else if (e.Button == MouseButtons.Right)
            {
                systrayContextMenu.Show(Cursor.Position);
            }
        }

        private void restoreWindowFromSystray(object sender, EventArgs e)
        {
            Show();
            ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void minimizeButton_Click(object sender, EventArgs e)
        {
        }

        private void label34_Click(object sender, EventArgs e)
        {
        }

        private void hotkeyWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            RegisterHotKey(IntPtr.Zero, hotkeyId, MOD_CONTROL | MOD_ALT | MOD_SHIFT, (int)Keys.K);

            while (true)
            {
                if (GetMessage(out MSG msg, IntPtr.Zero, 0, 0))
                {
                    if (msg.message == WM_HOTKEY)
                    {
                        // Aktion, die bei Hotkey-Auslösung erfolgen soll
                        this.Invoke(new MethodInvoker(delegate () { guna2Button15.PerformClick(); }));
                    }
                    TranslateMessage(ref msg);
                    DispatchMessage(ref msg);
                }
            }
        }

        private void label53_Click(object sender, EventArgs e)
        {
        }

        private void guna2Panel12_Paint(object sender, PaintEventArgs e)
        {
        }
    }
}
