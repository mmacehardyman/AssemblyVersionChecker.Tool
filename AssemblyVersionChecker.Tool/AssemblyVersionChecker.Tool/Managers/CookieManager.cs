using Alienlab.NetExtensions;
using System;
using System.IO;
using System.Net;

namespace AssemblyVersionChecker.Tool.Managers
{
    public class CookieManager
    {
        public string GetMarketplaceCookie(string username, string password)
        {
            const string baseUri = "https://dev.sitecore.net";
            var request = FormHelper.CreatePostRequest(new Uri(baseUri + @"/api/authorization"));
            request.ContentType = @"application/json;charset=UTF-8";
            var cookies = new CookieContainer();
            request.CookieContainer = cookies;
            var content = "{" + $"\"username\":\"{username}\",\"password\":\"{password}\"" + "}";
            request.ContentLength = content.Length;
            using (var inputStream = request.GetRequestStream())
            {
                using (var writer = new StreamWriter(inputStream))
                {
                    writer.Write(content);
                }
            }

            using (var response = (HttpWebResponse) request.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    using (var streamReader = new StreamReader(responseStream))
                    {
                        if (streamReader.ReadToEnd() == "true")
                        {
                            var sitecoreNetCookies = cookies.GetCookies(new Uri("http://sitecore.net"));
                            var marketplaceCookie = sitecoreNetCookies["marketplace_login"];
                            if (marketplaceCookie == null)
                            {
                                throw new InvalidOperationException("The username or password or both are incorrect, or an unexpected error happened");
                            }

                            return marketplaceCookie + "; " + GetSessionCookie(baseUri);
                        }
                    }
                }
            }


            throw new InvalidOperationException("The username or password or both are incorrect, or an unexpected error happened");
        }

        private string GetSessionCookie(string url)
        {
            var request = FormHelper.CreateRequest(new Uri(url));
            var cookies = new CookieContainer();
            request.CookieContainer = cookies;

            using (request.GetResponse())
            {
                return cookies.GetCookies(new Uri("http://dev.sitecore.net"))["ASP.NET_SessionId"].ToString();
            }
        }
    }
}