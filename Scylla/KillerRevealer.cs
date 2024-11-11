using Newtonsoft.Json;
using System.Diagnostics;

namespace Scylla
{
    public sealed class KillerRevealer
    {
        private KillerRevealer()
        {

        }
        private static readonly Lazy<KillerRevealer> lazy = new Lazy<KillerRevealer>(() => new KillerRevealer());
        public static KillerRevealer instance
        {
            get
            {
                return lazy.Value;
            }
        }

        public EventHandler QueueUpdated;
        public EventHandler MatchUpdated;
        public EventHandler QueueCancelled;
        public void ProcessQueueInfo(string eta, string pos)
        {
            Console.WriteLine("Starting to convert ETA");
            Main.ETA = CalculateETA(eta).ToString();
            Main.POS = pos;
            Console.WriteLine("Queue Globals updated");
            CallQueueUpdated();
            Console.WriteLine("Queue Event fired");
        }
        public void ProcessMatchedInfo(string rank, string country, string rating, string server, string charac)
        {
            Main.RATING = CalculateMMR(rating).ToString();
            Main.RANK = rank;
            Main.SERVER = server;
            try
            {
                Main.CHARNAME = CharacterDictionaries.TranslateKiller(charac);
            }
            catch (Exception)
            {
                Main.CHARNAME = "Unknown";
            }
            try
            {
                var (countryName, countryFlag) = Task.Run(async () => await LobbyInfo.GetCountryNameInSystemLanguage(country)).Result;
                Main.COUNTRY = countryName;
                Main.COUNTRYFLAG = Task.Run(async () => await LobbyInfo.LoadImageFromUrl(countryFlag)).Result;
            }
            catch (Exception)
            {
            }
        }
        public void ProcessMatchInfo(string matchid, string cloudid, string plat)
        {
            Main.MATCHID = matchid;
            Main.PLATFORM = plat;
            //try
            //{
            //    //Main.CHARNAME = translateKiller(charName);
            //    Main.CHARNAME = charName;
            //}
            //catch (Exception)
            //{
            //    Main.CHARNAME = "Unknown";
            //}
            try
            {
                string[] providerInfo = FindKillerSteam("https://steam.live.bhvrdbd.com/", cloudid, Main.BHVRSESSION).Result;
                string steamid = !string.IsNullOrEmpty(providerInfo[1]) ? providerInfo[1] : "Console Player";
                Main.STEAMID = steamid;
                Console.WriteLine("Steam ID: " + steamid);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Find Killer Steam Error: " + ex.Message);
            }
            Console.WriteLine("Match Globals updated");
            CallMatchUpdated();
            Console.WriteLine("Match Event fired");
        }
        public void FireQueueCancelled()
        {
            Console.WriteLine("Cancel Event called");
            QueueCancelled?.Invoke(this, EventArgs.Empty);
            Main.SERVER = String.Empty;
            Main.RANK = String.Empty;
            Main.RATING = String.Empty;
            Main.COUNTRY = String.Empty;
            Console.WriteLine("Cancel Event fired");
        }

        #region Helper Methods
        private int CalculateMMR(string input)
        {
            Console.WriteLine(input);
            string s = input.Split(',')[0];
            double d = double.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
            return Convert.ToInt32(d);
        }
        private int CalculateETA(string input)
        {
            Console.WriteLine(input);
            string s = input.Split(',')[0];
            double d = double.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
            double e = Math.Abs(Math.Round(d / 1000.0));
            Console.WriteLine("Done converting");
            return Convert.ToInt32(e);
        }
        public class SteamRootobject
        {
            public string providerId { get; set; }
            public string provider { get; set; }
        }
        public async Task<string[]?> FindKillerSteam(string url, string PCID, string bhvrSession)
        {
            try
            {
                Uri baseAddress = new Uri(url);
                HttpClientHandler handler = new HttpClientHandler
                {
                    UseCookies = false
                };
                HttpClient client = new HttpClient(handler)
                {
                    BaseAddress = baseAddress
                };
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "api/v1/players/" + PCID + "/provider/provider-id");
                httpRequestMessage.Headers.Add("Accept", "*/*");
                httpRequestMessage.Headers.Add("Accept-Encoding", "deflate, gzip");
                httpRequestMessage.Headers.Add("Cookie", "bhvrSession=" + bhvrSession);
                httpRequestMessage.Headers.Add("x-kraken-client-platform", "steam");
                httpRequestMessage.Headers.Add("x-kraken-client-provider", "steam");
                httpRequestMessage.Headers.Add("x-kraken-client-version", "4.5.0");
                httpRequestMessage.Headers.Add("User-Agent", "DeadByDaylight/++DeadByDaylight+Live-CL-404154 Windows/10.0.19042.1.768.64bit");
                SteamRootobject rootobject = JsonConvert.DeserializeObject<SteamRootobject>(await (await client.SendAsync(httpRequestMessage)).Content.ReadAsStringAsync());

                string[] s = new string[2];
                if (!string.IsNullOrEmpty(rootobject.providerId))
                {
                    Debug.WriteLine("Provider: " + rootobject.provider.ToString());
                    Debug.WriteLine("Provider ID: " + rootobject.providerId.ToString());
                    s[0] = rootobject.provider.ToString();
                    s[1] = rootobject.providerId.ToString();
                }
                else
                {
                    Debug.WriteLine("Killer isnt from Steam Platform.");
                    s[0] = "";
                    s[1] = "";
                }
                return s;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return null;
            }
        }
        public static void ViewSteamProfile()
        {
            Console.WriteLine("Globals ID: " + Main.STEAMID);
            bool valid = !Main.STEAMID.Equals("ERR") && !string.IsNullOrEmpty(Main.STEAMID);
            Console.WriteLine("Valid: " + valid);
            if (valid || Main.STEAMID == "Console Player")
            {
                try
                {
                    string uri = "https://steamcommunity.com/profiles/" + Main.STEAMID;
                    Process.Start(uri);
                }
                catch
                {
                    MessageBox.Show("Killer isnt from Steam Platform.");
                }
            }
        }

        #endregion
        public static void ViewDetails()
        {
            bool valid = !Main.STEAMID.Equals("ERR") && !string.IsNullOrEmpty(Main.STEAMID);
            if (valid || Main.STEAMID == "Console Player")
            {
                try
                {
                    string uri = "https://dbd-info.com/player-profile/" + Main.STEAMID + "/overview";
                    Process.Start(uri);
                }
                catch
                {
                    MessageBox.Show("Killer isn't from Steam Platform.");
                }
            }
        }
        private void CallQueueUpdated()
        {
            QueueUpdated?.Invoke(this, EventArgs.Empty);
        }
        private void CallMatchUpdated()
        {
            MatchUpdated?.Invoke(this, EventArgs.Empty);
        }

    }

}
