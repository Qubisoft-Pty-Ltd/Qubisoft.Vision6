using Newtonsoft.Json.Linq;
using Qubisoft.Vision6.Constants;
using Qubisoft.Vision6.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Transactions;

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

            if (httpClient != null)
            {
                HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{API_ROOT}lists/{list}/contacts?remove_unsubscribers={removeUnsubscribers}", contact, jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                dynamic json = await ParseJsonResponse(response);

                throw new Vision6Exception($"{json.type}: {json.developer_message}");

            }
            throw new Exception("API not properly Authenticated");
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
                }
                else
                {
                    url = $"{url}?fields={string.Join(",", fields)}";
                }

            }
            else
            {
                url = url.TrimEnd('&');
            }

            if (httpClient != null)
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);

                dynamic json = await ParseJsonResponse(response);

                if (response.IsSuccessStatusCode)
                {
                    return json._embedded.contacts;
                }
                throw new Vision6Exception($"{json.type}: {json.developer_message}");
            }
            throw new Exception("API not properly Authenticated");
        }

        public async Task<dynamic> GetContact(int list, int contactId, IEnumerable<string>? fields = null)
        {
            string url = $"{API_ROOT}lists/{list}/contacts/{contactId}";


            if (fields != null && fields.Any())
            {
                url = $"{url}?fields={string.Join(",", fields)}";
            }
            if (httpClient != null)
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);

                dynamic json = await ParseJsonResponse(response);

                if (response.IsSuccessStatusCode)
                {
                    return json;
                }

                throw new Vision6Exception($"{json.type}: {json.developer_message}");
            }
            throw new Exception("API not properly Authenticated");
        }

        public async Task<dynamic> UpdateContact(int list, int id, Contact contactData)
        {
            string url = $"{API_ROOT}lists/{list}/contacts/{id}";

            StringContent body = new(JsonSerializer.Serialize(contactData), Encoding.UTF8, "application/json");

            if (httpClient != null)
            {
                HttpResponseMessage response = await httpClient.PatchAsync(url, body);

                dynamic json = await ParseJsonResponse(response);

                if (response.IsSuccessStatusCode)
                {
                    return json;
                }
                throw new Vision6Exception($"{json.type}: {json.developer_message}");
            }
            throw new Exception("API not properly Authenticated");
        }

        public async Task<dynamic> UpdateContacts(int list, List<Contact> contactData)
        {

            string url = $"{API_ROOT}lists/{list}/contacts";

            Dictionary<string, dynamic> bodyData = new Dictionary<string, dynamic>();
            bodyData.Add("contacts", contactData);

            StringContent body = new(JsonSerializer.Serialize(bodyData), Encoding.UTF8, "application/json");

            if (httpClient != null)
            {
                HttpResponseMessage response = await httpClient.PatchAsync(url, body);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return false;
                }
                throw new Vision6Exception($"{response.StatusCode}: {response.Content}");
            }
            throw new Exception("API not properly Authenticated");
        }

        public async Task<dynamic> DeleteContact(int list, int id)
        {
            string url = $"{API_ROOT}lists/{list}/contacts/{id}";
            if (httpClient != null)
            {
                HttpResponseMessage response = await httpClient.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return false;
                }
                throw new Vision6Exception($"{response.StatusCode}: {response.Content}");
            }
            throw new Exception("API not properly Authenticated");
        }


        public async Task<dynamic> DeleteContacts(int list, List<int> ids)
        {
            string url = $"{API_ROOT}lists/{list}/contacts?id";
            if (ids.Any())
            {
                url = $"{url}:in={string.Join(",", ids)}";
            }
            if (httpClient != null)
            {
                HttpResponseMessage response = await httpClient.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return false;
                }
                throw new Vision6Exception($"{response.StatusCode}: {response.Content}");
            }
            throw new Exception("API not properly Authenticated");
        }

        public async Task<dynamic> GetLists(Dictionary<string, string>? filters = null, IEnumerable<string>? fields = null)
        {
            string url = $"{API_ROOT}lists";

            if (filters != null && filters.Any() && fields != null && fields.Any())
            {
                url = $"{url}?";
                foreach (var f in filters)
                {
                    url = $"{url}{f.Key}={f.Value}&";
                }
                url = $"{url}fields={string.Join(",", fields)}";
            }
            else if (filters != null && filters.Any())
            {
                url = $"{url}?";
                foreach (var f in filters)
                {
                    url = $"{url}{f.Key}={f.Value}&";
                }
                url = url.TrimEnd('&');
            }
            else if (fields != null && fields.Any())
            {
                url = $"{url}?fields={string.Join(",", fields)}";
            }


            if (httpClient != null)
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);

                dynamic json = await ParseJsonResponse(response);

                if (response.IsSuccessStatusCode)
                {
                    return json._embedded.lists;
                }
                throw new Vision6Exception($"{json.type}: {json.developer_message}");
            }
            throw new Exception("API not properly Authenticated");
        }


        public async Task<dynamic> GetList(int id, IEnumerable<string>? expands = null, IEnumerable<string>? fields = null)
        {
            string url = $"{API_ROOT}lists/{id}";
            if (expands != null && expands.Any() && fields != null && fields.Any()) {
                url = $"{url}?expand={string.Join(",", expands)}&fields={string.Join(",", fields)}";
            }
            else if (expands != null && expands.Any())
            {
                url = $"{url}?expand={string.Join(",", expands)}";
            }
            else if (fields != null && fields.Any())
            {
                url = $"{url}?fields={string.Join(",", fields)}";
            }

            if (httpClient != null)
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);

                dynamic json = await ParseJsonResponse(response);

                if (response.IsSuccessStatusCode)
                {
                    return json;
                }
                throw new Vision6Exception($"{json.type}: {json.developer_message}");
            }
            throw new Exception("API not properly Authenticated");
        }

        public async Task<dynamic> GetMessages(Dictionary<string, string>? filters = null, IEnumerable<string>? fields = null)
        {
            string url = $"{API_ROOT}messages";
            if (filters != null && filters.Any() && fields != null && fields.Any())
            {
                url = $"{url}?";
                foreach (var f in filters)
                {
                    url = $"{url}{f.Key}={f.Value}&";
                }
                url = $"{url}fields={string.Join(",", fields)}";
            }
            else if (filters != null && filters.Any())
            {
                url = $"{url}?";
                foreach (var f in filters)
                {
                    url = $"{url}{f.Key}={f.Value}&";
                }
                url = url.TrimEnd('&');
            }
            else if (fields != null && fields.Any())
            {
                url = $"{url}?fields={string.Join(",", fields)}";
            }

            if (httpClient != null)
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);

                dynamic json = await ParseJsonResponse(response);

                if (response.IsSuccessStatusCode)
                {
                    return json._embedded.messages;
                }
                throw new Vision6Exception($"{json.type}: {json.developer_message}");
            }
            throw new Exception("API not properly Authenticated");
        }

        public async Task<dynamic> GetMessage(int id, IEnumerable<string>? expands = null, IEnumerable<string>? fields = null)
        {
            string url = $"{API_ROOT}messages/{id}";

            if (expands != null && expands.Any() && fields != null && fields.Any())
            {
                url = $"{url}?expand={string.Join(",", expands)}&fields={string.Join(",", fields)}";
            }
            else if (expands != null && expands.Any())
            {
                url = $"{url}?expand={string.Join(",", expands)}";
            }
            else if (fields != null && fields.Any())
            {
                url = $"{url}?fields={string.Join(",", fields)}";
            }

            if (httpClient != null)
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);

                dynamic json = await ParseJsonResponse(response);

                if (response.IsSuccessStatusCode)
                {
                    return json;
                }
                throw new Vision6Exception($"{json.type}: {json.developer_message}");
            }
            throw new Exception("API not properly Authenticated");
        }

        public async Task<dynamic> GetMessageContent(int id, IEnumerable<string>? expands = null, IEnumerable<string>? fields = null)
        {
            string url = $"{API_ROOT}messages/{id}/content";

            if (expands != null && expands.Any() && fields != null && fields.Any())
            {
                url = $"{url}?expand={string.Join(",", expands)}&fields={string.Join(",", fields)}";
            }
            else if (expands != null && expands.Any())
            {
                url = $"{url}?expand={string.Join(",", expands)}";
            }
            else if (fields != null && fields.Any())
            {
                url = $"{url}?fields={string.Join(",", fields)}";
            }

            if (httpClient != null)
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);

                dynamic json = await ParseJsonResponse(response);

                if (response.IsSuccessStatusCode)
                {
                    return json;
                }
                throw new Vision6Exception($"{json.type}: {json.developer_message}");
            }
            throw new Exception("API not properly Authenticated");
        }

        public async Task<dynamic> GetTransactionGroups(Dictionary<string, string>? filters = null, IEnumerable<string>? fields = null)
        {
            string url = $"{API_ROOT}transactional-groups";

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
                }
                else
                {
                    url = $"{url}?fields={string.Join(",", fields)}";
                }

            }
            else
            {
                url = url.TrimEnd('&');
            }

            if (httpClient != null)
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);

                dynamic json = await ParseJsonResponse(response);

                if (response.IsSuccessStatusCode)
                {
                    return json;
                }
                throw new Vision6Exception($"{json.type}: {json.developer_message}");
            }
            throw new Exception("API not properly Authenticated");
        }

        public async Task<dynamic> GetTransactionGroup(int id, IEnumerable<string>? fields = null)
        {
            string url = $"{API_ROOT}transactional-groups/{id}";

            if (fields != null && fields.Any())
            {
                url = $"{url}?fields={string.Join(",", fields)}";
            }

            if (httpClient != null)
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);

                dynamic json = await ParseJsonResponse(response);

                if (response.IsSuccessStatusCode)
                {
                    //according to the documentation it should be _embedded.transactional-groups
                    return json._embedded;
                }
                throw new Vision6Exception($"{json.type}: {json.developer_message}");
            }
            throw new Exception("API not properly Authenticated");
        }

        public async Task<dynamic> TransactionalSend(Models.Transaction bodyContent)
        {
            string url = $"{API_ROOT}transactional-sends";
            StringContent body = new(JsonSerializer.Serialize(bodyContent), Encoding.UTF8, "application/json");


            if (httpClient != null)
            {
                HttpResponseMessage response = await httpClient.PostAsync(url, body);

                if (response.IsSuccessStatusCode)
                {
                    return response;
                }
                throw new Vision6Exception($"failed {response}");
            }
            throw new Exception("API not properly Authenticated");
        }

        private async Task<dynamic> ParseJsonResponse(HttpResponseMessage response)
        {
            string jsonString = await response.Content.ReadAsStringAsync();
            dynamic json = JObject.Parse(jsonString);

            return json;
        }
    }
}