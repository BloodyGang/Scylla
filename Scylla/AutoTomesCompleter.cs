using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net;

namespace Scylla
{
    public static class AutoTomesCompleter
    {
        private static string POSTRequest(string json, string path)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://brill.live.bhvrdbd.com/api/v1" + path);
            httpWebRequest.ServicePoint.Expect100Continue = false;
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/ResponseBody";
            httpWebRequest.Accept = "*/*";
            httpWebRequest.Headers["Accept-Encoding"] = "deflate, gzip";
            httpWebRequest.UserAgent = "DeadByDaylight/++DeadByDaylight+Live-CL-281719 Windows/10.0.18363.1.256.64bit";
            httpWebRequest.ContentType = "application/ResponseBody";
            Cookie cookie = new Cookie();
            cookie.Name = "bhvrSession";
            cookie.Value = Main.BHVRSESSION;
            cookie.Domain = "brill.live.bhvrdbd.com";
            httpWebRequest.CookieContainer = new CookieContainer();
            httpWebRequest.CookieContainer.Add(cookie);
            using (Stream requestStream = httpWebRequest.GetRequestStream())
            {
                using (StreamWriter streamWriter = new StreamWriter(requestStream))
                    streamWriter.Write(json);
            }
            using (WebResponse response = httpWebRequest.GetResponse())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    Debug.WriteLine(response.GetResponseStream());
                    using (StreamReader streamReader = new StreamReader(responseStream))
                        return streamReader.ReadToEnd();
                }
            }
        }
        public static void CompleteTomes(string ResponseBody, string ReguestBody)
        {
            try
            {
                JObject jsonObject = JObject.Parse(ResponseBody);
                JObject jsonObject2 = JObject.Parse(ReguestBody);

                JArray questEvents = jsonObject.SelectToken("activeNodesFull[0].objectives[0].questEvent") as JArray;
                if (questEvents == null)
                {
                    return;
                }
                int neededProgression = jsonObject.SelectToken("activeNodesFull[0].objectives[0].neededProgression")?.Value<int>() ?? 0;
                if (neededProgression == 0)
                {
                    return;
                }
                string matchId = "8F665744-3751-405C-B859-3DEE35EC1E4E";
                string role = (string)jsonObject2["role"];
                if (role == "both")
                {
                    role = "killer";
                }

                List<string> questEventStrings = new List<string>();
                foreach (int repetitionValue in new int[] { 4, 666, 666666 })
                {
                    List<string> individualQuestEventStrings = new List<string>();
                    foreach (JObject questEvent in questEvents)
                    {
                        string parametersString = "{\"parameters\":\"" + questEvent["parameters"].Value<string>() + "\",\"questEventId\":\"" + questEvent["questEventId"].Value<string>() + "\",\"repetition\":" + repetitionValue.ToString() + ",\"operation\":\"" + questEvent["operation"].Value<string>() + "\"}";
                        individualQuestEventStrings.Add(parametersString);
                    }
                    string questEventString = "{\"matchId\":\"" + matchId + "\",\"questEvents\":[" + string.Join(",", individualQuestEventStrings) + "],\"role\":\"" + role + "\"}";
                    questEventStrings.Add(questEventString);
                }

                foreach (string questEventString in questEventStrings)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        POSTRequest(questEventString, "/archives/stories/update/quest-progress-v2");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
