using Fiddler;

namespace Scylla
{
    public class FiddlerCore
    {
        public static bool nekoukej = true;
        private static readonly string AssemblyDirectory = Path.GetTempPath();
        private const ushort FiddlerCoreListenPort = 0;
        public static bool responded = false;

        public static event FiddlerCore.CookieHandler OnTokenReceived;

        static FiddlerCore()
        {
            FiddlerApplication.BeforeRequest += FiddlerApplication_BeforeRequest;
            FiddlerApplication.AfterSessionComplete += FiddlerApplication_AfterRequest;
            FiddlerCore.EnsureRootCertificate();
        }
        private static void FiddlerApplication_BeforeRequest(Session oSession)
        {
            if (oSession.uriContains("/api/v1/dbd-player-card/set"))
            {
                oSession.utilCreateResponseAndBypassServer();
                oSession.utilSetResponseBody("{\"badge\":\"xxx\",\"banner\":\"xxx\"}");
            }
            if (oSession.uriContains("/api/v1/dbd-character-data/get-all") && Main.ACCOUNTEDITOR)
            {
                oSession.oFlags["x-replywithfile"] = (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Scylla\\Profile.txt");
            }
            if (oSession.uriContains("/api/v1/dbd-character-data/bloodweb") && Main.ACCOUNTEDITOR)
            {
                Main.BODY = oSession.GetRequestBodyAsString();
                Main.UpdatedBloodweb();
                oSession.oFlags["x-replywithfile"] = (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Scylla\\Bloodweb.txt");
            }
            if (oSession.uriContains("v1/inventories") && Main.ACCOUNTEDITOR)
            {
                oSession.oFlags["x-replywithfile"] = (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Scylla\\SkinsWithItems.txt");
            }
            if (oSession.uriContains("v1/inventories") && !Main.ACCOUNTEDITOR && Main.SKINSONLY)
            {
                oSession.oFlags["x-replywithfile"] = (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Scylla\\SkinsONLY.txt");
            }
            if (oSession.uriContains("api/v1/wallet/currencies") && Main.CURRENCYSPOOF)
            {
                oSession.oFlags["x-replywithfile"] = (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Scylla\\Currency.txt");
            }
            if (Main.LEVELSPOOF && oSession.uriContains("api/v1/extensions/playerLevels/getPlayerLevel") || oSession.uriContains("api/v1/extensions/playerLevels/earnPlayerXp"))
            {
                oSession.oFlags["x-replywithfile"] = (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Scylla\\Level.txt");
            }
            if (oSession.uriContains("/catalog") && Main.BREAKSETS)
            {
                oSession.oFlags["x-replywithfile"] = (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Scylla\\Catalog.json");
            }
            if (oSession.uriContains("itemsKillswitch") && Main.DISABLEDSTUFF)
            {
                oSession.oFlags["x-replywithfile"] = (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Scylla\\Disabled.json");
            }
            if (oSession.uriContains("api/v1/queue") && Main.REGIONBOOL == true)
            {
                Console.WriteLine(oSession.GetRequestBodyAsString());
                oSession.utilSetRequestBody(Main.UpdateLatency(oSession.GetRequestBodyAsString(), Main.REGION));
                Console.WriteLine(Main.UpdateLatency(oSession.GetRequestBodyAsString(), Main.REGION));
            }
            if (oSession.uriContains("DISPLAY_RANK_ON_TALLY/raw") && Main.RANKREVEAL)
            {
                oSession.oFlags["x-replywithfile"] = (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Scylla\\RankReveal.json");
            }
        }
        private static void FiddlerApplication_AfterRequest(Session oSession)
        {
            if (oSession.uriContains("/api/v1/archives/stories/update/active-node") && Main.CHALLENGECOMPLETOR)
            {
                try
                {
                    AutoTomesCompleter.CompleteTomes(oSession.GetResponseBodyAsString(), oSession.GetRequestBodyAsString());
                }
                catch
                {

                }
            }
        }
        private static void hui(string pizda)
        {
            FiddlerCore.CookieHandler onTokenReceived = FiddlerCore.OnTokenReceived;
            if (onTokenReceived != null)
                onTokenReceived(pizda);
        }
        private static void EnsureRootCertificate()
        {
            BCCertMaker.BCCertMaker bcCertMaker = new BCCertMaker.BCCertMaker();
            CertMaker.oCertProvider = (ICertificateProvider)bcCertMaker;
            string str = Path.Combine(FiddlerCore.AssemblyDirectory, "defaultCertificate.p12");
            string password = "$0M3$H1T";
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

        public static void Start()
        {
            CONFIG.IgnoreServerCertErrors = true;
            FiddlerApplication.Startup(new FiddlerCoreStartupSettingsBuilder().ListenOnPort((ushort)0).RegisterAsSystemProxy().ChainToUpstreamGateway().DecryptSSL().OptimizeThreadPool().Build());
        }

        public static void Stop()
        {
            FiddlerApplication.Shutdown();
        }

        public delegate void CookieHandler(string cookie);
    }
}
