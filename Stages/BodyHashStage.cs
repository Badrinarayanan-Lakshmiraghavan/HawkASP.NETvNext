using System.Threading.Tasks;
using Microsoft.AspNet.Security;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Security.Claims;
using HawkvNext.Extensions;
using HawkvNext.Notifications;

namespace HawkvNext.Stages
{
    internal class BodyHashStage : PipelineStage
    {
        private Stream stream = null;
        private MemoryStream responseBuffer = null;
        private bool hashResponseBody = false;

        internal override async Task Receive(HawkPipelineContext context)
        {
            if (context.PayloadHash != null && context.PayloadHash.Length > 0)
            {
                if (!await this.IsRequestHashValidAsync(context))
                {
                    return; // Body hash mismatch - Stop further processing.
                }
            }

            await this.Next.Receive(context);

            context.PayloadHash = null; // Clear this to ensure request payload is not used for server authorization.

            if (context.IsSuccessfulHawkAuthentication)
            {
                var hashabilityContext = new HawkCheckResponseBodyHashabilityContext(
                                                    context.HttpContext,
                                                        context.Options);

                context.Options.Notifications.CheckResponseBodyHashability(hashabilityContext);
                this.hashResponseBody = (context.Options.AuthenticationMode == AuthenticationMode.Active) && 
                                            hashabilityContext.HashResponseBody;

                if (this.hashResponseBody)
                {
                    this.stream = context.Response.Body;
                    this.responseBuffer = new MemoryStream();
                    context.Response.Body = this.responseBuffer;

                    // Register for the original stream to be restored later. 
                    // This is done to ensure stream switch happens even if an
                    // exception is thrown by a downstream middleware.
                    context.RegisterForResponseStreamRestoration(this.stream);
                }
            }
        }

        internal override async Task Send(HawkPipelineContext context)
        {
            var bodyStream = context.Response.Body;

            // Server authorization required and response body hash must be included.
            if (context.IsServerAuthorizationRequired && this.hashResponseBody)
            {
                byte[] hash = await bodyStream.ComputeResponseBodyHashAsync(context);

                context.PayloadHash = hash;
            }

            await this.Next.Send(context);

            // Doing the stream restoration here in the call stack unwind because we want
            // the Server-Authorization header to be ready before we could write the buffer
            // back into the original stream and restore it.
            if (this.responseBuffer != null)
            {
                this.responseBuffer.Seek(0, SeekOrigin.Begin);
                await this.responseBuffer.CopyToAsync(this.stream);

                context.RestoreOriginalResponseStream();
            }
        }      

        private async Task<bool> IsRequestHashValidAsync(HawkPipelineContext context)
        {
            var bodyStream = context.Request.Body;

            if (!bodyStream.CanSeek)
            {
                var buffer = new MemoryStream();
                await bodyStream.CopyToAsync(buffer);

                bodyStream = buffer;
                context.Request.Body = bodyStream;
            }

            byte[] computedHash = await bodyStream.ComputeRequestBodyHashAsync(context);

            return computedHash.ConstantTimeEquals(context.PayloadHash);
        }
    }
}