using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;

namespace ApiDiaryLibrary
{
    class ApiRest
    {
        private string keyAccess;

        private const string apiUrl = "https://api.dnevnik.ru/v2.0/";

        public ApiRest(string keyAccess)
        {
            this.keyAccess = keyAccess;

            var content = Get<IRestResponse>(new RestRequest(), "users/me");

            CheckConnection(content);
        }

        public ApiRest(string login, string pass)
        {
            var client = new RestClient();
            client.BaseUrl = new Uri("https://api.dnevnik.ru/v2/authorizations/bycredentials");

            var getToken = new RestRequest();

            getToken.AddHeader("Content-Type", "application/json");
            getToken.AddJsonBody(JsonConvert.SerializeObject(new Credentials(pass, login)));

            var response = client.Post(getToken);

            CheckConnection(response);

            var json = ((JObject)JsonConvert.DeserializeObject(response.Content));

            keyAccess = json["accessToken"].Value<string>();
        }

        public ApiRest(string login, string pass, string client_id, string client_secret, string scope)
        {
            var client = new RestClient();
            client.BaseUrl = new Uri("https://api.dnevnik.ru/v2/authorizations/bycredentials");

            var getToken = new RestRequest();

            getToken.AddHeader("Content-Type", "application/json");
            getToken.AddJsonBody(JsonConvert.SerializeObject(new Credentials(pass, login, client_id, client_secret, scope)));

            var response = client.Post(getToken);

            CheckConnection(response);

            var json = ((JObject)JsonConvert.DeserializeObject(response.Content));

            keyAccess = json["accessToken"].Value<string>();
        }

        internal IRestResponse Get<type>(RestRequest request, string url)
        {
            var client = new RestClient();

            client.BaseUrl = new Uri(apiUrl + url);

            request.AddHeader("Access-Token", keyAccess);
            request.AddHeader("Accept", "application/json");

            return client.Get(request);
        }

        internal string Get(RestRequest request, string url)
        {
            var client = new RestClient();

            client.BaseUrl = new Uri(apiUrl + url);

            request.AddHeader("Access-Token", keyAccess);
            request.AddHeader("Accept", "application/json");

            return client.Get(request).Content;
        }

        internal string Post(RestRequest request, string url)
        {
            var client = new RestClient();

            client.BaseUrl = new Uri(apiUrl + url);

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Access-Token", keyAccess);

            return client.Post(request).Content;
        }

        internal string Delete(RestRequest request, string url)
        {
            var client = new RestClient();

            client.BaseUrl = new Uri(apiUrl + url);

            request.AddHeader("Access-Token", keyAccess);

            return client.Delete(request).Content;
        }

        internal string Put(RestRequest request, string url)
        {
            var client = new RestClient();

            client.BaseUrl = new Uri(apiUrl + url);

            request.AddHeader("Access-Token", keyAccess);

            return client.Put(request).Content;
        }

        private void CheckConnection(IRestResponse content)
        {
            JObject json;

            if (content.StatusCode != System.Net.HttpStatusCode.ServiceUnavailable)
            {
                json = ((JObject)JsonConvert.DeserializeObject(content.Content));

                if (json.ContainsKey("type"))
                    switch (json["type"].Value<string>())
                    {
                        case string text when (text == "authorizationFailed" || text == "invalidToken" || text == "parameterInvalid"):
                            throw new Exception(json["description"].Value<string>());
                        case string text when (text == "apiServerError" || text == "apiUnknownError"):
                            throw new Exception("Неизвестная ошибка в API, проверьте правильность параметров");
                    }
            }
            else throw new Exception("Сайт лежит или ведутся технические работы, использование api временно невозможно");
        }

        /*
         * 
         * Функции
         * 
         */
    }
}
