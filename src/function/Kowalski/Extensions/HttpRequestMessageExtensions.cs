using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Kowalski.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        public static HttpResponseMessage CreateErrorResponse(this HttpRequestMessage req, HttpStatusCode statusCode, string message, TraceWriter log)
        {
            log.Error(message);
            return req.CreateErrorResponse(statusCode, message);
        }

        public static string GetQueryStringValue(this HttpRequestMessage req, string parameter, string defaultValue = null)
        {
            var queryString = req.GetQueryNameValuePairs();
            var value = queryString.FirstOrDefault(q => q.Key.EqualsIgnoreCase(parameter)).Value;

            return !string.IsNullOrWhiteSpace(value) ? value : defaultValue;
        }
    }
}
