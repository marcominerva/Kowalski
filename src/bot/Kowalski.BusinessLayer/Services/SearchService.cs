using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kowalski.BusinessLayer.Services
{
    public static class SearchService
    {
        private static readonly string searchUri;
        private static readonly string searchSubscriptionKey;
        private static readonly string culture;

        static SearchService()
        {
            culture = ConfigurationManager.AppSettings["Culture"];
            searchUri = ConfigurationManager.AppSettings["SearchUri"];
            searchSubscriptionKey = ConfigurationManager.AppSettings["SearchSubscriptionKey"];
        }

        public static async Task<string> SearchAsync(string query)
        {
            // Construct the URI of the search request
            var uriQuery = $"{searchUri}?q={Uri.EscapeDataString(query)}&mkt={culture}";

            // Perform the Web request and get the response
            var request = WebRequest.Create(uriQuery);
            request.Headers["Ocp-Apim-Subscription-Key"] = searchSubscriptionKey;

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            var content = await reader.ReadToEndAsync();
                            var json = JObject.Parse(content);

                            var results = json["webPages"]["value"];
                            var bestResult = results.FirstOrDefault(v => v["name"].ToString().Contains("Wikipedia")) ?? results.FirstOrDefault();

                            // Try to normalize the text using a super simply algorithm.
                            var text = bestResult?["snippet"]?.ToString();
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                text = text.Replace("Biografia.", string.Empty).Replace("Descrizione.", string.Empty).Trim();
                                text = GetNormalizedMessage(text);
                                text = FirstSentence(text);

                                var openParenthesis = text.IndexOf('(');
                                if (openParenthesis > -1)
                                {
                                    text = text.Substring(0, openParenthesis);
                                }

                                text = text.Replace(", ,", ",").Replace(" ,", ",").Replace("...", string.Empty).Trim().TrimEnd('e').Trim().TrimEnd(',').Trim();
                            }

                            return text;
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private static string GetNormalizedMessage(string message)
        {
            message = Regex.Replace(message, @"\(.*?\)", "");
            message = Regex.Replace(message, @"\s{2,}", " ");

            return message;
        }

        private static string FirstSentence(string paragraph)
        {
            for (var i = 0; i < paragraph.Length; i++)
            {
                switch (paragraph[i])
                {
                    case '.':
                        if (i < (paragraph.Length - 2) && char.IsWhiteSpace(paragraph[i + 1]) && char.IsLower(paragraph[i + 2]))
                        {
                            break;
                        }

                        if (i < (paragraph.Length - 1) && char.IsWhiteSpace(paragraph[i + 1]))
                        {
                            goto case '!';
                        }

                        break;

                    case '?':
                    case '!':
                        return paragraph.Substring(0, i + 1);
                }
            }

            return paragraph;
        }
    }
}
