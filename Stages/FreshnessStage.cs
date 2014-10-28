using System.Threading.Tasks;
using System;
using System.Text;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using HawkvNext.Extensions;

namespace HawkvNext.Stages
{
    internal class FreshnessStage : PipelineStage
    {
        private const string PREAMBLE = "hawk.1.ts";
        private bool isStale;

        internal override async Task Receive(HawkPipelineContext context)
        {
            if (!context.IsBewit) // This freshness check not applicable to Bewit.
            {
                ulong offset = Convert.ToUInt64(context.Options.LocalTimeOffsetMillis);
                ulong now = context.Now + offset;

                ulong shelfLife = (Convert.ToUInt64(context.Options.ClockSkewSeconds) * 1000);
                var age = Math.Abs((context.Timestamp * 1000.0) - now);

                this.isStale = (age > shelfLife);

                if (this.isStale)
                {
                    return; // Stop inbound processing
                }
            }

            await this.Next.Receive(context);
        }

        internal override async Task Send(HawkPipelineContext context)
        {
            // If status code is 401 and timestamp is stale, add ts and tsm to the hawk challenge.
            if (context.Response.StatusCode == 401 && this.isStale)
            {
                ulong unixTimeNowMillis = DateTime.UtcNow.UnixTimeMillis();
                ulong offset = Convert.ToUInt64(context.Options.LocalTimeOffsetMillis);
                double fresh = Math.Floor((unixTimeNowMillis + offset) / 1000.0);

                var builder = new StringBuilder();
                builder.Append(PREAMBLE).Append("\n").Append(fresh.ToString()).Append("\n");

                byte[] data = Encoding.UTF8.GetBytes(builder.ToString());

                using (var algorithm = KeyedHashAlgorithm.Create("hmac" + context.AlgorithmName))
                {
                    algorithm.Key = context.CryptographicKey;
                    byte[] mac = algorithm.ComputeHash(data);
                    string tsm = Convert.ToBase64String(mac);

                    context.Challenge = String.Format("{0} ts=\"{1}\", tsm=\"{2}\"",
                                                            HawkConstants.Scheme,
                                                            fresh.ToString(),
                                                            tsm);
                }
            }

            await this.Next.Send(context);
        }
    }
}