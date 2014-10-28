using System.Threading.Tasks;
using System;
using System.Text;

namespace HawkvNext.Stages
{
    internal class BewitStage : PipelineStage
    {
        internal override async Task Receive(HawkPipelineContext context)
        {
            if (context.Options.IsBewitSupported && 
                    "GET".Equals(context.Request.Method, StringComparison.OrdinalIgnoreCase))
            {
                string bewit = context.Request.Query[HawkConstants.Bewit];
                if (!String.IsNullOrWhiteSpace(bewit))
                {
                    bewit = bewit.Replace('-', '+').Replace('_', '/');
                    int pad = 4 - (bewit.Length % 4);
                    pad = pad > 2 ? 0 : pad;
                    bewit = bewit.PadRight(bewit.Length + pad, '=');

                    bewit = Encoding.UTF8.GetString(Convert.FromBase64String(bewit));

                    // bewit: id\exp\mac\ext
                    var parts = bewit.Split('\\');

                    if (parts.Length == 4)
                    {
                        ulong timestamp = 0;
                        if (UInt64.TryParse(parts[1], out timestamp))
                        {
                            if (timestamp * 1000 < context.Now)
                                return; // Expired bewit - stop further processing.

                            context.Timestamp = timestamp;
                            context.UserId = parts[0];
                            context.Mac = Convert.FromBase64String(parts[2]);
                            context.ApplicationSpecificData = parts[3];
                            context.IsBewit = true;
                        }
                    }
                }
            }

            await this.Next.Receive(context);
        }
    }
}