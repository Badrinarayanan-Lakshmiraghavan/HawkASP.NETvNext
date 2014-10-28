using System.Threading.Tasks;
using System;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace HawkvNext.Stages
{
    internal class HeaderStage : PipelineStage
    {
        private const string VALUE_MATCH_PATTERN = @"^[ \w\!#\$%&'\(\)\*\+,\-\.\/\:;<\=>\?@\[\]\^`\{\|\}~]+$";
        private const string PARAMETER_MATCH_PATTERN = @"(\w+)=""([^""\\]*)""\s*(?:,\s*|$)";
        private const string SPECIFIC_PARAMETER_MATCH_PATTERN = @"({0})=""([^""\\]*)""\s*(?:,\s*|$)";

        private const string ID = "id";
        private const string TS = "ts";
        private const string NONCE = "nonce";
        private const string EXT = "ext";
        private const string MAC = "mac";
        private const string HASH = "hash";

        private const string REQUEST_HEADER = "Authorization";
        private const string RESPONSE_HEADER = "Server-Authorization";

        internal override async Task Receive(HawkPipelineContext context)
        {
            var header = context.Request.Headers[REQUEST_HEADER];
            if (IsHeaderValid(header))
            {
                if (ParseHeader(header.Substring(HawkConstants.Scheme.Length + 1), context))
                {
                    bool isInvalid = (String.IsNullOrWhiteSpace(context.UserId) ||
                                     String.IsNullOrWhiteSpace(context.Nonce) ||
                                        (context.Mac == null || context.Mac.Length == 0) ||
                                            context.Timestamp <= 0);
                    if (isInvalid)
                    {
                        // Authorization header fields present but invalid,
                        // stop further processing.
                        return;
                    }
                }
            }

            await this.Next.Receive(context);
        }

        internal override async Task Send(HawkPipelineContext context)
        {
            if (context.IsServerAuthorizationRequired)
            {
                var result = new StringBuilder();
                result.AppendFormat("{0}=\"{1}\", ", MAC, Convert.ToBase64String(context.Mac));

                if (!String.IsNullOrWhiteSpace(context.ApplicationSpecificData))
                    result.AppendFormat("{0}=\"{1}\", ", EXT, context.ApplicationSpecificData);

                if (context.PayloadHash != null)
                    result.AppendFormat("{0}=\"{1}\", ", HASH, Convert.ToBase64String(context.PayloadHash));

                string authorization = result.ToString().Trim().Trim(',');

                if (!String.IsNullOrWhiteSpace(authorization))
                {
                    context.Response.Headers.AppendValues(RESPONSE_HEADER, String.Format("{0} {1}", HawkConstants.Scheme, authorization));
                }
            }

            return; // Outbound processing terminates here
        }

        private bool IsHeaderValid(string header)
        {
            return (!String.IsNullOrWhiteSpace(header) &&
                        (header.Length > HawkConstants.Scheme.Length + 2) &&
                            HawkConstants.Scheme.Equals(header.Substring(0, HawkConstants.Scheme.Length),
                                    StringComparison.OrdinalIgnoreCase));
        }

        private bool ParseHeader(string header, HawkPipelineContext context)
        {
            var keysToBeProcessed = new HashSet<string>() { ID, TS, NONCE, EXT, MAC, HASH };

            var replacedString = Regex.Replace(header, PARAMETER_MATCH_PATTERN, (Match match) =>
            {
                string key = match.Groups[1].Value.Trim();
                string value = match.Groups[2].Value.Trim();

                bool isValidValue = Regex.Match(value, VALUE_MATCH_PATTERN).Success;
                bool isValidKey = keysToBeProcessed.Any(k => k == key); // Key is neither duplicate nor bad

                if (isValidValue && isValidKey)
                {
                    switch (key)
                    {
                        case ID: context.UserId = value; break;

                        case TS:
                            {
                                ulong timestamp;
                                if (UInt64.TryParse(value, out timestamp))
                                {
                                    context.Timestamp = timestamp;
                                    break;
                                }
                                else
                                    return value;
                            }

                        case NONCE: context.Nonce = value; break;
                        case EXT: context.ApplicationSpecificData = value; break;
                        case MAC: context.Mac = Convert.FromBase64String(value); break;
                        case HASH: context.PayloadHash = Convert.FromBase64String(value); break;
                    }

                    keysToBeProcessed.Remove(key); // Processed

                    return String.Empty;
                }
                else
                    return value;
            });

            return replacedString == String.Empty; // No more, no less -> valid parameter data
        }
    }
}