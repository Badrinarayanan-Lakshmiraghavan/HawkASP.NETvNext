using System;
using Microsoft.AspNet.Http;

namespace HawkvNext.Extensions
{
    internal static class HttpRequestExtensions
    {
        internal static string GetQueryStringSansBewit(this HttpRequest request, bool isBewit)
        {
            string queryString = request.QueryString.ToString();

            if (isBewit)
            {
                string bewit = request.Query[HawkConstants.Bewit];

                queryString = queryString.Replace(HawkConstants.Bewit + "=" + bewit, String.Empty)
                                        .Replace("&&", "&")
                                            .Replace("?&", "?")
                                                .Trim('&').Trim('?');

                if (queryString != String.Empty)
                    queryString = "?" + queryString;
            }

            return queryString;
        }
    }
}