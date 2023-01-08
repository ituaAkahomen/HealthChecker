using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnnualHealthCheckJs.Tools
{
    public static class Context
    {
        private static IHttpContextAccessor HttpContextAccessor;
        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            HttpContextAccessor = httpContextAccessor;
        }

        private static Uri GetAbsoluteUri()
        {
            try
            {
                var request = HttpContextAccessor.HttpContext.Request;
                UriBuilder uriBuilder = new UriBuilder();
                uriBuilder.Scheme = request.Scheme;
                uriBuilder.Host = request.Host.Host;
                uriBuilder.Path = request.Path.ToString();
                uriBuilder.Query = request.QueryString.ToString();
                return uriBuilder.Uri;
            }
            catch
            {
                return null;
            }
        }

        // Similar methods for Url/AbsolutePath which internally call GetAbsoluteUri
        public static string GetAbsoluteUrl()
        {
            return GetAbsoluteUri().AbsoluteUri;
        }

        public static string GetAbsolutePath()
        {
            return GetAbsoluteUri().AbsolutePath;
        }

        public static string GetSchemeHost()
        {
            var uri = GetAbsoluteUri();
            if (uri != null)
                return string.Format("{0}://{1}", uri.Scheme, uri.Host);
            else
                return "https://annualhealthcheck.com";
        }
    }
}
