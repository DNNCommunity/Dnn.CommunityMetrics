using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace Dnn.CommunityMetrics
{
    /// <summary>
    /// Entry point API for Twitter requests
    /// </summary>
    public class TwitterAPI
    {
        public OAuth.Tokens Tokens { get; set; }

        public readonly string BaseUrl = "https://api.twitter.com/1.1/";

        public TwitterAPI() { }

        /// <summary>
        /// Creates a Twitter API object using Twitter authentication keys.
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="accessTokenSecret"></param>
        /// <param name="consumerKey"></param>
        /// <param name="consumerSecret"></param>
        public TwitterAPI(string accessToken, string accessTokenSecret, string consumerKey, string consumerSecret)
        {
            Tokens = new OAuth.Tokens();
            Tokens.AccessToken = accessToken;
            Tokens.AccessTokenSecret = accessTokenSecret;
            Tokens.ConsumerKey = consumerKey;
            Tokens.ConsumerSecret = consumerSecret;
        }

        /// <summary>
        /// Performs a GET request on Twitter API using querystring parameters provided.
        /// </summary>
        /// <param name="method">The API method to use (ie 'statuses/user_timeline.json')</param>
        /// <param name="querystring">A list of querystring parameters</param>
        /// <returns></returns>
        public List<JSONObject> Get(string method, Parameters querystring)
        {
            return Request(RequestType.GET, method, querystring);
        }

        /// <summary>
        /// Performs a GET request on Twitter API using a raw URL or API method name
        /// </summary>
        /// <param name="urlOrMethodAndParameters">A Raw URL (ie 'https://api.twitter.com..') or API method name (ie 'statuses/user_timeline.json')</param>
        /// <returns></returns>
        public List<JSONObject> Get(string urlOrMethodAndParameters)
        {
            return Request(RequestType.GET, urlOrMethodAndParameters, null);
        }

        /// <summary>
        /// Performs a POST request on Twitter API using an API method and parameter information
        /// </summary>
        /// <param name="method">The API method to use (ie 'statuses/user_timeline.json')</param>
        /// <param name="form">A list of form parameter</param>
        /// <returns></returns>
        public List<JSONObject> Post(string method, Parameters form)
        {
            return Request(RequestType.POST, method, form);
        }

        /// <summary>
        /// Performs a DELETE request on Twitter API using an API method and parameter information
        /// </summary>
        /// <param name="method"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        public List<JSONObject> Delete(string method, Parameters form)
        {
            return Request(RequestType.DELETE, method, form);
        }

        private enum RequestType { GET, POST, DELETE }

        private List<JSONObject> Request(RequestType type, string urlFragment, Parameters parameters)
        {
            Uri uri = GetUri(type, urlFragment, parameters);
            WebRequest request = HttpWebRequest.Create(uri);
            request.Method = type.ToString();
            if (type == RequestType.POST || type == RequestType.DELETE)
            {
                var json = parameters.ToJson();
                parameters = new Parameters();
                request.ContentType = "application/json";
                using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write(json);
                }

            }
            OAuth.Sign(request, Tokens, parameters);
            try
            {
                using (WebResponse response = request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string responseText = reader.ReadToEnd();
                        object result = JsonConvert.DeserializeObject(responseText);
                        if (result is Newtonsoft.Json.Linq.JArray)
                            return JsonConvert.DeserializeObject<List<JSONObject>>(responseText);
                        if (result is Newtonsoft.Json.Linq.JToken)
                            return new List<JSONObject> { JsonConvert.DeserializeObject<JSONObject>(responseText) };
                        return new List<JSONObject>();
                    }
                }
            }
            catch (Exception ex)
            {
                request.Abort();
                throw ex;
            }
        }

        private Uri GetUri(RequestType type, string urlFragment, Parameters parameters)
        {
            string baseUrl = BaseUrl + urlFragment;
            if (urlFragment.ToLower().StartsWith("http"))
            {
                baseUrl = urlFragment;
            }
            try
            {
                switch (type)
                {
                    case RequestType.POST:
                    case RequestType.DELETE:
                        return new Uri(baseUrl);

                    default:
                        if (parameters == null || parameters.Count == 0) return new Uri(baseUrl);
                        if (!baseUrl.Contains("?")) baseUrl += "?";
                        else if (!baseUrl.EndsWith("&")) baseUrl += "&";
                        return new Uri(baseUrl + parameters.ToString());
                }
            }
            catch
            {
                throw new Exception(String.Format("The url you are using is not well formed: {0} ", baseUrl));
            }
        }
    }

    /// <summary>
    /// Encapsulates GET/POST parameter functionality
    /// </summary>
    public class Parameters : Dictionary<string, object>
    {
        public Parameters() : base() { }

        public Parameters(string field, object value)
            : base()
        {
            this.Add(field, value);
        }

        /// <summary>
        /// Returns a URL/Formbody representation of the parameters
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            List<string> output = new List<string>();
            foreach (string key in this.Keys)
            {
                output.Add(String.Format("{0}={1}", UrlEncode(key), UrlEncode(this[key])));
            }
            return String.Join("&", output.ToArray());
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }

        /// <summary>
        /// Parses objects into URL escaped strings
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string UrlEncode(object obj)
        {
            string result = Uri.EscapeDataString(Parse(obj));

            // These characters are not escaped by EscapeDataString() 
            result = result
                .Replace("(", "%28")
                .Replace(")", "%29")
                .Replace("$", "%24")
                .Replace("!", "%21")
                .Replace("*", "%2A")
                .Replace("'", "%27");

            // Tilde gets escaped but is a reserved character and is thus allowed.
            // @see https://dev.twitter.com/oauth/overview/percent-encoding-parameters
            result = result.Replace("%7E", "~");
            return result;
        }

        /// <summary>
        /// Parses object into strings
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Parse(object obj)
        {
            string result = String.Empty;
            if (obj != null)
            {
                switch (obj.GetType().FullName)
                {
                    case "System.Boolean":
                        result = String.Format("{0}", obj).ToLower();
                        break;
                    case "System.DateTime":
                        result = String.Format("{0}", ((DateTime)obj).Ticks);
                        break;
                    default:
                        result = String.Format("{0}", obj);
                        break;
                }
            }
            return result;
        }
    }

    /// <summary>
    /// Encapsulates API JSON results
    /// </summary>
    public class JSONObject : Dictionary<string, object>
    {
        /// <summary>
        /// Gets a JSON property by key
        /// </summary>
        /// <param name="key">Name of the property to obtain</param>
        /// <returns></returns>
        public object Get(string key)
        {
            if (key != null)
            {
                if (this.ContainsKey(key)) return this[key];
                else if (key.Contains('.'))
                {
                    JSONObject parent = this;
                    string[] segments = key.Split('.');
                    for (var i = 0; i < segments.Length - 1; i++)
                    {
                        key = segments[i];
                        if (!parent.ContainsKey(key)) break;
                        parent = parent.Get<JSONObject>(key);
                    }
                    key = segments[segments.Length - 1];
                    if (parent.ContainsKey(key))
                        return parent.Get(key);
                }
            }
            return null;
        }

        /// <summary>
        /// Gets an strongly typed JSON property by key
        /// </summary>
        /// <typeparam name="T">The type to convert value to (int, string etc..)</typeparam>
        /// <param name="key">Name of the property to obtain</param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            object output = Get(key);
            if (output != null)
            {
                try
                {
                    return (T)Convert.ChangeType(output, typeof(T));
                }
                catch
                {
                    return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(output));
                }
            }
            return default(T);
        }

        /// <summary>
        /// Gets a strongly typed list of items from a JSON property
        /// </summary>
        /// <typeparam name="T">The type for list items (int, string etc..)</typeparam>
        /// <param name="key">Name of the property to obtain</param>
        /// <returns></returns>
        public List<T> GetList<T>(string key)
        {
            return Get<List<T>>(key);
        }
    }

    /// <summary>
    /// Used to sign HTTP requests using Twitter OAuth authentication
    /// </summary>
    public static class OAuth
    {
        /// <summary>
        /// Signs a web request by adding signed OAuth Authorisation header
        /// </summary>
        /// <param name="request">The WebRequest object to add OAuth header to</param>
        /// <param name="tokens">Twitter OAuth API Parameters</param>
        public static void Sign(WebRequest request, Tokens tokens, Parameters parameters)
        {
            parameters = parameters ?? new Parameters();

            if (request.RequestUri.Query.Contains("?"))
            {
                string query = request.RequestUri.Query.Substring(1);
                foreach (string queryParameter in query.Split('&'))
                {
                    string[] kvp = queryParameter.Split('=');
                    if (!parameters.ContainsKey(kvp[0]))
                    {
                        try { parameters.Add(kvp[0], kvp[1]); }
                        catch { }
                    }
                }
            }

            parameters.Add("oauth_version", "1.0");
            parameters.Add("oauth_nonce", GetNonce());
            parameters.Add("oauth_timestamp", GetTimeStamp());
            parameters.Add("oauth_signature_method", "HMAC-SHA1");
            parameters.Add("oauth_consumer_key", tokens.ConsumerKey);
            parameters.Add("oauth_consumer_secret", tokens.ConsumerSecret);

            if (!String.IsNullOrEmpty(tokens.AccessToken))
            {
                parameters.Add("oauth_token", tokens.AccessToken);
            }

            if (!String.IsNullOrEmpty(tokens.AccessTokenSecret))
            {
                parameters.Add("oauth_token_secret", tokens.AccessTokenSecret);
            }

            // Add signaure
            parameters.Add("oauth_signature", GetSignature(request, parameters, tokens));

            // Append OAuth header
            request.Headers.Add("Authorization", GetOAuth(parameters));

        }

        /// <summary>
        /// Twitter OAuth tokens
        /// </summary>
        public class Tokens
        {
            public string ConsumerKey { get; set; }
            public string ConsumerSecret { get; set; }
            public string AccessToken { get; set; }
            public string AccessTokenSecret { get; set; }
        }

        #region Helper Methods

        /// <summary>
        /// Get the timestamp for the signature        
        /// </summary>
        private static string GetTimeStamp()
        {
            // UNIX time of the current UTC time
            TimeSpan span = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(span.TotalSeconds, CultureInfo.CurrentCulture).ToString(CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Get a nonce
        /// </summary>
        private static string GetNonce()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }

        /// <summary>
        /// Generates the signature.
        /// @see https://dev.twitter.com/oauth/overview/creating-signatures
        /// </summary>
        private static string GetSignature(WebRequest request, Parameters parameters, Tokens tokens)
        {
            string signatureBaseString = GetSignatureBaseString(request, parameters);

            // Create our hash key 
            string key = String.Format("{0}&{1}",
                Parameters.UrlEncode(tokens.ConsumerSecret),
                Parameters.UrlEncode(tokens.AccessTokenSecret));

            // Generate the hash
            HMACSHA1 hmacsha1 = new HMACSHA1(Encoding.UTF8.GetBytes(key));
            byte[] signatureBytes = hmacsha1.ComputeHash(Encoding.UTF8.GetBytes(signatureBaseString));
            return Convert.ToBase64String(signatureBytes);
        }

        /// <summary>
        /// Generates the sigature base string
        /// </summary>
        private static string GetSignatureBaseString(WebRequest request, Parameters parameters)
        {

            StringBuilder parameterString = new StringBuilder();

            string[] secretParameters = new string[]  {
                                                "oauth_consumer_secret",
                                                "oauth_token_secret",
                                                "oauth_signature"
                                               };

            // Loop through an ordered set of parameters
            // excluding any secret oauth parameters
            foreach (var item in (from p in parameters
                                  where (!secretParameters.Contains(p.Key))
                                  orderby p.Key, p.Value
                                  select p))
            {
                if (parameterString.Length > 0) parameterString.Append("&");

                string value = Parameters.Parse(item.Value);

                parameterString.Append(
                    String.Format("{0}={1}",
                        Parameters.UrlEncode(item.Key),
                        Parameters.UrlEncode(value)));
            }

            // Join output 
            return String.Format("{0}&{1}&{2}",
                request.Method.ToUpper(),
                Parameters.UrlEncode(NormalizeUrl(request.RequestUri)),
                Parameters.UrlEncode(parameterString.ToString()));
        }

        /// <summary>
        /// Returns a string representation of OAuth header
        /// </summary>
        private static string GetOAuth(Parameters parameters)
        {
            List<string> output = new List<string>();
            foreach (string key in new string[] {
                                                      "oauth_consumer_key",
                                                      "oauth_nonce",
                                                      "oauth_signature",
                                                      "oauth_signature_method",
                                                      "oauth_timestamp",
                                                      "oauth_token",
                                                      "oauth_version"
                                                  })
            {
                if (parameters.ContainsKey(key))
                    output.Add(String.Format("{0}=\"{1}\"", Parameters.UrlEncode(key), Parameters.UrlEncode(parameters[key])));
            }
            return String.Format("OAuth {0}", String.Join(", ", output.ToArray()));
        }

        /// <summary>
        /// Normalizes the URL.
        /// </summary>
        private static string NormalizeUrl(Uri url)
        {
            return String.Format("{0}://{1}{2}", url.Scheme, url.Host, url.AbsolutePath);
        }

        #endregion
    }
}