using Newtonsoft.Json.Linq;
using Qubisoft.Vision6.Constants;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Qubisoft.Vision6
{
    public class Vision6Client
    {
        private readonly string API_ROOT = "https://au1.api.vision6.com/v1/";
        private HttpClient? httpClient;
        private JsonSerializerOptions jsonOptions;

        public Vision6Client()
        {
            jsonOptions = new JsonSerializerOptions();
            jsonOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        }

        public async Task<bool> Authenticate(string bearerToken)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken.Trim());
            HttpResponseMessage response = await httpClient.GetAsync(API_ROOT);

            if (response.IsSuccessStatusCode)
            {
                this.httpClient = httpClient;

                return true;
            }

            dynamic json = await ParseJsonResponse(response);

            throw new Vision6Exception($"{json.type}: {json.developer_message}");
        }

        public async Task<bool> CreateContact(int list, object contact, string removeUnsubscribers = RemoveUnubscriberConstants.NONE)
        {
            if (removeUnsubscribers != RemoveUnubscriberConstants.NONE && removeUnsubscribers != RemoveUnubscriberConstants.LIST && removeUnsubscribers != RemoveUnubscriberConstants.ANY)
            {
                throw new Vision6Exception($"Invalid removeSubscribers value: {removeUnsubscribers}. Accepted values are '{RemoveUnubscriberConstants.NONE}', '{RemoveUnubscriberConstants.LIST}' or '{RemoveUnubscriberConstants.ANY}'.");
            }

            HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{API_ROOT}lists/{list}/contacts?remove_unsubscribers={removeUnsubscribers}", contact, jsonOptions);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            dynamic json = await ParseJsonResponse(response);

            throw new Vision6Exception($"{json.type}: {json.developer_message}");
        }

        public async Task<dynamic> GetContacts(int list, Dictionary<string, string>? filters = null, IEnumerable<string>? fields = null)
        {
            string url = $"{API_ROOT}lists/{list}/contacts";

            if (filters != null && filters.Any())
            {
                url = $"{url}?";

                foreach (var f in filters)
                {
                    url = $"{url}{f.Key}={f.Value}&";
                }
            }

            if (fields != null && fields.Any())
            {
                if (filters != null && filters.Any())
                {
                    url = $"{url}fields={string.Join(",", fields)}";
                } else
                {
                    url = $"{url}?fields={string.Join(",", fields)}";
                }
                
            } else
            {
                url = url.TrimEnd('&');
            }

            HttpResponseMessage response = await httpClient.GetAsync(url);

            dynamic json = await ParseJsonResponse(response);

            if (response.IsSuccessStatusCode)
            {
                return json._embedded.contacts;
            }

            throw new Vision6Exception($"{json.type}: {json.developer_message}");
        }

        private async Task<dynamic> ParseJsonResponse(HttpResponseMessage response)
        {
            string jsonString = await response.Content.ReadAsStringAsync();
            dynamic json = JObject.Parse(jsonString);

            return json;
        }
    }
}