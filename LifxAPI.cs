using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace HomeAlive
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

    public class LifxAPI
    {
        private static readonly HttpClient client = new HttpClient();

        private static readonly string BaseAPI = "http://localhost:6969";
        private static readonly string LightStatusAPI = "/lifx/status/";
        private static readonly string CommandAPI = "/lifx/";

        public string Label { get; set; }

        public LifxAPI(string label)
        {
            this.Label = label;
        }

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
            HttpResponseMessage response = await client.PostAsync(BaseAPI + CommandAPI, data).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            string res = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<LightListResponse>(res);
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

        public LightListResponse Toggle()
        {
            var payload = new Dictionary<string, string>
            {
                {"light_label", Label},
                {"command", "toggle"}
            };
            return APIPost(payload).Result;
        }

        public LightListResponse TurnOn()
        {
            var payload = new Dictionary<string, string>
            {
                {"light_label", Label},
                {"command", "on"}
            };
            return APIPost(payload).Result;
        }

        public LightListResponse TurnOff()
        {
            var payload = new Dictionary<string, string>
            {
                {"light_label", Label},
                {"command", "off"}
            };
            return APIPost(payload).Result;
        }

        public LightListResponse BrightnessUp()
        {
            var payload = new Dictionary<string, string>
            {
                {"light_label", Label},
                {"command", "b+"}
            };
            return APIPost(payload).Result;
        }

        public LightListResponse BrightnessDown()
        {
            var payload = new Dictionary<string, string>
            {
                {"light_label", Label},
                {"command", "b-"}
            };
            return APIPost(payload).Result;
        }
    }

}