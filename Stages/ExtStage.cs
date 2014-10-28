using System.Threading.Tasks;
using System;
using System.Text;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using HawkvNext.Notifications;

namespace HawkvNext.Stages
{
    internal class ExtStage : PipelineStage
    {
        internal override async Task Receive(HawkPipelineContext context)
        {
            if (!String.IsNullOrWhiteSpace(context.ApplicationSpecificData))
            {
                var validationContext = new HawkValidateNormalizedRequestContext(
                                                    context.HttpContext,
                                                        context.Options,
                                                            context.ApplicationSpecificData);

                context.Options.Notifications.ValidateNormalizedRequest(validationContext);

                if (!validationContext.IsValid)
                    return;
            }

            await this.Next.Receive(context);

            context.ApplicationSpecificData = null;
        }

        internal override async Task Send(HawkPipelineContext context)
        {
            if (context.IsServerAuthorizationRequired)
            {
                var normalizeContext = new HawkNormalizeResponseMessageContext(context.HttpContext, context.Options);
                context.Options.Notifications.NormalizeResponse(normalizeContext);
                
                context.ApplicationSpecificData = normalizeContext.NormalizedResponseMessage;
            }

            await this.Next.Send(context);
        }
    }
}