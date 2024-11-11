using Fiddler;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Scylla
{
    public sealed class LobbyInfo
    {
        private LobbyInfo()
        {

        }
        private static readonly Lazy<LobbyInfo> lazy = new Lazy<LobbyInfo>(() => new LobbyInfo());
        public static LobbyInfo instance
        {
            get
            {
                return lazy.Value;
            }
        }

        public static async Task<(string CountryName, string CountryFlag)> GetCountryNameInSystemLanguage(string countryCode)
        {
            string language = CultureInfo.CurrentCulture.ThreeLetterISOLanguageName;
            string url = $"https://restcountries.com/v3.1/alpha/{countryCode}";

            using var client = new HttpClient();
            var response = await client.GetStringAsync(url);
            var data = JArray.Parse(response);

            var countryName = data[0]?["translations"]?[language]?["common"]?.ToString()
                              ?? data[0]?["name"]?["common"]?.ToString()
                              ?? countryCode;
            var countryFlag = data[0]?["flags"]?["png"]?.ToString();
            Console.WriteLine($"Country name: {countryName}");
            Console.WriteLine($"Country flag: {countryFlag}");

            return (countryName, countryFlag);
        }

        public static async Task<Image> LoadImageFromUrl(string url)
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync(url);
            using var stream = await response.Content.ReadAsStreamAsync();
            return Image.FromStream(stream);
        }

        public readonly string AssemblyDirectory = Path.GetTempPath();
        private const ushort FiddlerCoreListenPort = 0;
        public bool responded = false;

        public event CookieHandler OnTokenReceived;

        public void Init()
        {
            EnsureRootCertificate();
        }
        public void Start()
        {
            CONFIG.IgnoreServerCertErrors = true;
            FiddlerApplication.BeforeRequest += FiddlerBeforeRequest;
            FiddlerApplication.BeforeResponse += FiddlerBeforeResponse;
            Console.WriteLine("Fiddler started...");
        }
        private void FiddlerBeforeRequest(Session oSession)
        {
            if (oSession.uriContains("api/v1/config"))
            {
                oSession.bBufferResponse = true;
                Main.BHVRSESSION = oSession.oRequest["Cookie"].Replace("bhvrSession=", string.Empty);
                Console.WriteLine("Cookie grabbed successfully.\nCookie: " + Main.BHVRSESSION);
            }
        }

        private void FiddlerBeforeResponse(Session oSession)
        {
            if (Main.KILLERREVEAL)
            {
                if (oSession.uriContains("/api/v1/queue"))
                {
                    if (oSession.uriContains("/cancel"))
                    {
                        Main.SERVER = String.Empty;
                        Main.RANK = String.Empty;
                        Main.RATING = String.Empty;
                        Main.COUNTRY = String.Empty;
                        KillerRevealer.instance.FireQueueCancelled();
                        return;
                    }
                    if (oSession.uriContains("token/issue")) return;
                    Console.WriteLine("In Queue");
                    oSession.bBufferResponse = true;
                    oSession.utilDecodeResponse();
                    string body = oSession.GetResponseBodyAsString();
                    JObject response = JObject.Parse(body);
                    JToken status = response.SelectToken("status");
                    Console.WriteLine(response);

                    if (String.IsNullOrEmpty(body)) return;

                    if (status.ToString().Equals("QUEUED"))
                    {
                        Console.WriteLine("Status: QUEUED");
                        JToken ETA = response.SelectToken("queueData.ETA");
                        JToken Position = response.SelectToken("queueData.position");
                        string s = ETA.ToString(), pos = Position.ToString();
                        // Process queue info using KR
                        KillerRevealer.instance.ProcessQueueInfo(s, pos);
                    }
                    if (status.ToString().Equals("MATCHED"))
                    {
                        oSession.bBufferResponse = true;
                        oSession.utilDecodeResponse();
                        string body2 = oSession.GetResponseBodyAsString();
                        JObject response2 = JObject.Parse(body2);
                        JToken rank = response2.SelectToken("matchData.skill.rank");
                        JToken country = response2.SelectToken("matchData.skill.countries[0]");
                        JToken rating = response2.SelectToken("matchData.skill.rating.rating");
                        JToken server = response2.SelectToken("matchData.props.regions.CrossplayOptOut");
                        JToken charName = response.SelectToken("matchData.props.characterName");
                        Console.WriteLine(rank?.ToString());
                        Console.WriteLine(country?.ToString());
                        Console.WriteLine(rating?.ToString());
                        Console.WriteLine(server?.ToString());
                        string ran = rank?.ToString(), countr = country?.ToString(), rat = rating?.ToString(), ser = server?.ToString(), charac = charName?.ToString();
                        KillerRevealer.instance.ProcessMatchedInfo(ran, countr, rat, ser, charac);
                    }
                    if (!status.ToString().Equals("MATCHED")) return;
                }
                else if (oSession.uriContains("api/v1/match") && !oSession.GetResponseBodyAsString().Contains("forbidden"))
                {
                    oSession.bBufferResponse = true;
                    oSession.utilDecodeResponse();
                    Console.WriteLine(oSession.GetResponseBodyAsString());
                    string body = oSession.GetResponseBodyAsString();
                    JObject response = JObject.Parse(body);
                    JToken matchID = response.SelectToken("matchId");
                    JToken cloudid = response.SelectToken("sideA[0]");
                    JToken platform = response.SelectToken("props.platform");
                    string mid = matchID?.ToString(), plat = platform?.ToString(), cid = cloudid?.ToString();
                    KillerRevealer.instance.ProcessMatchInfo(mid, cid, plat);
                }
                //, ran = rank.ToString(), countr = country.ToString(), rat = rating.ToString()
                //ran, countr, rat, ser,
                //, ran = rank.ToString(), countr = country.ToString(), rat = rating.ToString()
                //ran, countr, rat, ser,

                else if (oSession.uriContains("api/v1/party"))
                {
                    if (oSession.uriContains("api/v1/party/details")) return;
                    oSession.bBufferResponse = true;
                    oSession.utilDecodeResponse();
                    string body = oSession.GetResponseBodyAsString();
                    if (String.IsNullOrEmpty(body)) return;
                    JObject response = JObject.Parse(body);
                    JToken mmState = response.SelectToken("gameSpecificState._partyMatchmakingSettings._matchmakingState");
                    if (mmState != null)
                    {
                        if (mmState.ToString().Equals("None"))
                        {
                            KillerRevealer.instance.FireQueueCancelled();
                        }
                    }
                    else
                    {
                        Console.WriteLine("Matchmaking State Token is null. At Queue Cancelling");
                    }

                }
            }
        }

        private void EnsureRootCertificate()
        {
            BCCertMaker.BCCertMaker bcCertMaker = new BCCertMaker.BCCertMaker();
            CertMaker.oCertProvider = (ICertificateProvider)bcCertMaker;

            string str = Path.Combine(LobbyInfo.instance.AssemblyDirectory, "certificate.p12");
            string password = "kokot";
            if (!File.Exists(str))
            {
                CertMaker.removeFiddlerGeneratedCerts(true);
                bcCertMaker.CreateRootCertificate();
                bcCertMaker.WriteRootCertificateAndPrivateKeyToPkcs12File(str, password, (string)null);
            }
            else
                bcCertMaker.ReadRootCertificateAndPrivateKeyFromPkcs12File(str, password, (string)null);
            if (CertMaker.rootCertIsTrusted())
                return;
            CertMaker.trustRootCert();
        }
        public void Stop()
        {
            FiddlerApplication.Shutdown();
        }

        public delegate void CookieHandler(string cookie);
    }
}
