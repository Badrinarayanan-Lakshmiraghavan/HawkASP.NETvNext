using System.Threading.Tasks;
using System;
using System.Text;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using HawkvNext.Extensions;
using HawkvNext.Notifications;

namespace HawkvNext.Stages
{
    internal class MacStage : PipelineStage
    {
        private const string HEADER_PREAMBLE = "hawk.1.header";
        private const string BEWIT_PREAMBLE = "hawk.1.bewit";
        private const string RESPONSE_PREAMBLE = "hawk.1.response";

        internal override async Task Receive(HawkPipelineContext context)
        {
            bool isAuthentic = false;

            if (context.CryptographicKey != null)
            {
                var hostContext = new HawkDetermineHostDetailsContext(context.HttpContext,
                                                                        context.Options);

                context.Options.Notifications.DetermineHostDetails(hostContext);
                context.HostName = hostContext.HostName;
                context.HostPort = hostContext.HostPort;

                context.PathAndQueryString = context.Request.Path + 
                                                context.Request.GetQueryStringSansBewit(
                                                                            context.IsBewit);

                string preamble = context.IsBewit ? BEWIT_PREAMBLE : HEADER_PREAMBLE;
                byte[] mac = this.NormalizeMessageAndComputeMac(context, preamble);

                isAuthentic = mac.ConstantTimeEquals(context.Mac);

            }

            if (!isAuthentic)
            {
                return; // Authentication fails - stop further processing.
            }

            await this.Next.Receive(context);
        }

        internal override async Task Send(HawkPipelineContext context)
        {
            if (context.IsServerAuthorizationRequired)
            {
                byte[] mac = this.NormalizeMessageAndComputeMac(context, RESPONSE_PREAMBLE);
                context.Mac = mac;
            }

            await this.Next.Send(context);
        }

        private byte[] NormalizeMessageAndComputeMac(HawkPipelineContext context, string preamble)
        {
            // NORMALIZED MESSAGE FORMAT:
            // preamble\n
            // timestamp\n
            // nonce\n
            // HTTP method\n
            // uri path and query string\n
            // host name\n
            // port\n
            // payload hash\n
            // application specific data\n

            // Preamble is 
            // hawk.1.header for requests containing Authorization header,
            // hawk.1.bewit for requests containing bewit, and 
            // hawk.1.response for response with Server-Authorization header.

            var builder = new StringBuilder();
            builder.Append(preamble).Append("\n")
                .Append(context.Timestamp).Append("\n")
                .Append(context.Nonce ?? String.Empty).Append("\n")
                .Append(context.Request.Method).Append("\n")
                .Append(context.PathAndQueryString).Append("\n")
                .Append(context.HostName).Append("\n")
                .Append(context.HostPort).Append("\n")
                .Append(context.PayloadHash == null ? String.Empty : Convert.ToBase64String(context.PayloadHash)).Append("\n")
                .Append(context.ApplicationSpecificData ?? String.Empty).Append("\n");

            byte[] normalizedMessage = Encoding.UTF8.GetBytes(builder.ToString());

            using (var algorithm = KeyedHashAlgorithm.Create("hmac" + context.AlgorithmName))
            {
                algorithm.Key = context.CryptographicKey;
                return algorithm.ComputeHash(normalizedMessage);
            }
        }

        
    }
}