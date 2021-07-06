using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using YarinGeorge.LibTools.SpoonacularApi.Client;
using YarinGeorge.LibTools.SpoonacularApi.Models;

namespace YarinGeorge.LibTools.SpoonacularApi.Client
{

    
    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public class API
    {
        public string APIkey { get; set; }
    }

    public class APIRequest
    {
        private static readonly HttpClient HTTPClient = new HttpClient();
        
        public static async Task<Rootobject> GetRecipeByIDAsync(API APIkey, decimal ID)
        {
            var Response = await HTTPClient.GetStringAsync(
                $"https://api.spoonacular.com/recipes/{ID}/information?apiKey={APIkey.APIkey}&number=1&includeNutrition=false").ConfigureAwait(false);

            Regex regex = new Regex("\\<[^\\>]*\\>");
            var Format = regex.Replace(Response, string.Empty);
            var Format2 = Format.Replace(".n", ".\n");
            string Format3 = Format2.Replace(@"\\n", "\n").Replace(@"\", string.Empty);
            
            var account = JsonConvert.DeserializeObject<Rootobject>(Format3);
            
            return account;
        }
    }
}
