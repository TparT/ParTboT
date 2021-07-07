using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YarinGeorge.ApiClients.TrackerGG.StatsModels;

namespace YarinGeorge.ApiClients.TrackerGG
{
    public class TrackerggClient
    {
        public string _ApiToken { private get; set; }
        private RestClient _client = new RestClient(BaseRoutes.BaseURL);
        public TrackerggClient(string ApiToken)
        {
            _ApiToken = ApiToken;
            _client.AddDefaultHeader("TRN-Api-Key", ApiToken);
        }

        public async Task<ApexLegendsStats> GetPlayerApexLegendsStats(string Identifier, string platform)
            => await DoGetRequestAsync<ApexLegendsStats>($"{GameRoutes.ApexLegends}{BaseRoutes.Standard}{BaseRoutes.Profile}{platform}/{Identifier}");

        public async Task<CsGoStats> GetPlayerCSGOStats(string Identifier)
            => await DoGetRequestAsync<CsGoStats>($"{GameRoutes.CSGO}{BaseRoutes.Standard}{BaseRoutes.Profile}{PlatformRoutes.Steam}/{Identifier}");


        private async Task<T> DoGetRequestAsync<T>(string URL)
        {
            IRestResponse response = await _client.ExecuteGetAsync(new RestRequest(URL)).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(response.Content);
        }
    }
}
