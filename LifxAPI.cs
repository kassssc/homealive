using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace ConsoleManaged
{
    public class LightListResponse
    {
        public List<Light> Results { get; set; }
    }

    public class Light
    {
        public string Label { get; set; }
        public string Power { get; set; }
        public int Brightness { get; set; }
    }

    public static class LifxAPI
    {
        private static HttpClient client = new HttpClient();

        private const string BaseAPI = "http://localhost:6969";
        private const string LightStatusAPI = "/lifx/status/";
        private const string CommandAPI = "/lifx/";

        public async static Task<LightListResponse> APIGet()
        {
            HttpResponseMessage response = await client.GetAsync(BaseAPI + LightStatusAPI);
            response.EnsureSuccessStatusCode();
            string res = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<LightListResponse>(res);
        }

        public async static Task<LightListResponse> APIPost(Dictionary<string, string> payload)
        {
            var json = JsonConvert.SerializeObject(payload);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(BaseAPI + CommandAPI, data).ConfigureAwait(false);
            //response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Console.WriteLine(responseString);
            //System.Timers.Timer timer = new System.Timers.Timer(250);
            //timer.AutoReset = true;
            //timer.Enabled = true;
            return JsonConvert.DeserializeObject<LightListResponse>(responseString);
        }

        public static LightListResponse GetLightStatus()
        {
            return APIGet().Result;
        }

        public static LightListResponse TurnOnAll()
        {
            var payload = new Dictionary<string, string>
            {
              {"command", "all on"}
            };
            return APIPost(payload).Result;
        }

        public static LightListResponse TurnOffAll()
        {
            var payload = new Dictionary<string, string>
            {
              {"command", "all off"}
            };
            return APIPost(payload).Result;
        }

        public static LightListResponse Toggle(string label)
        {
            var payload = new Dictionary<string, string>
            {
                {"light_label", label},
                {"command", "toggle"}
            };
            return APIPost(payload).Result;
        }

        public static LightListResponse TurnOn(string label)
        {
            var payload = new Dictionary<string, string>
            {
                {"light_label", label},
                {"command", "on"}
            };
            return APIPost(payload).Result;
        }

        public static LightListResponse TurnOff(string label)
        {
            var payload = new Dictionary<string, string>
            {
                {"light_label", label},
                {"command", "off"}
            };
            return APIPost(payload).Result;
        }

        public static LightListResponse BrightnessUp(string label)
        {
            var payload = new Dictionary<string, string>
            {
                {"light_label", label},
                {"command", "b+"}
            };
            return APIPost(payload).Result;
        }

        public static LightListResponse BrightnessDown(string label)
        {
            var payload = new Dictionary<string, string>
            {
                {"light_label", label},
                {"command", "b-"}
            };
            return APIPost(payload).Result;
        }
    }
    
}