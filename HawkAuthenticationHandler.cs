using Microsoft.AspNet.Security;
using Microsoft.AspNet.Http.Security;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Security.Infrastructure;
using System.Security.Claims;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using HawkvNext.Stages;
using HawkvNext.Extensions;

namespace HawkvNext
{
    public class HawkAuthenticationHandler : AuthenticationHandler<HawkAuthenticationOptions>
    {
        private Pipeline hawkPipeline = new Pipeline();
        private bool outboundInvokeInProgress = false;

        protected override Task InitializeCoreAsync()
        {
            hawkPipeline.Initialize(Context, Options);
            return Task.FromResult(0);
        }

        protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            await hawkPipeline.BuildForward().Receive(hawkPipeline.Context);

            return new AuthenticationTicket(hawkPipeline.Context.Identity, null);
        }

        protected override AuthenticationTicket AuthenticateCore()
        {
            return AuthenticateCoreAsync().GetAwaiter().GetResult();
        }

        protected override void ApplyResponseGrant() { }

        protected override void ApplyResponseChallenge() { }

        protected override async Task TeardownCoreAsync()
        {
            hawkPipeline.Context.RestoreOriginalResponseStream();
        }

        // Called as part of TeardownAsync - ApplyResponseCoreAsync is guranteed to
        // run only once per request.
        protected override async Task ApplyResponseCoreAsync()
        {
            this.outboundInvokeInProgress = true;

            await hawkPipeline.BuildReverse().Send(hawkPipeline.Context);

            AddChallenge(hawkPipeline.Context.Challenge);
        }

        protected override void ApplyResponseCore() // Called when response headers are sent
        {
            if (!this.outboundInvokeInProgress)
                ApplyResponseCoreAsync().GetAwaiter().GetResult();
        }

        private void AddChallenge(string challenge)
        {
            if (Response.StatusCode != 401)
            {
                return;
            }

            // If passive and no challenge, do not send header.
            if (ChallengeContext == null && Options.AuthenticationMode == AuthenticationMode.Passive)
            {
                return;
            }

            Context.Response.Headers.AppendValues("WWW-Authenticate", challenge);
        } 

        class Pipeline
        {
            private HawkPipelineContext pipelineContext = null;
            private List<PipelineStage> stages = new List<PipelineStage>();

            internal HawkPipelineContext Context
            {
                get
                {
                    return this.pipelineContext;
                }
            }

            internal void Initialize(HttpContext context, HawkAuthenticationOptions options)
            {
                this.pipelineContext = new HawkPipelineContext(context, options)
                {
                    Now = DateTime.UtcNow.UnixTimeMillis()
                };

                stages.Add(new HeaderStage()); // Must be the first
                stages.Add(new BewitStage());
                stages.Add(new UserStage());
                stages.Add(new MacStage());
                stages.Add(new FreshnessStage());
                stages.Add(new BodyHashStage());
                stages.Add(new ExtStage());
                stages.Add(new IdentityStage()); // Must be the last
            }

            internal PipelineStage BuildForward()
            {
                return this.Build(reverse: true);
            }

            internal PipelineStage BuildReverse()
            {
                return this.Build(reverse: false);
            }

            private PipelineStage Build(bool reverse)
            {
                var pipeline = new List<PipelineStage>(this.stages);

                if (reverse)
                    pipeline.Reverse();

                PipelineStage first = pipeline.First();
                pipeline.RemoveAt(0);

                foreach (PipelineStage stage in pipeline)
                {
                    stage.Next = first;
                    first = stage;
                }

                return first;
            }
        }
    }
}