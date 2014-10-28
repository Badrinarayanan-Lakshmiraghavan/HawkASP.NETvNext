using System.Threading.Tasks;
using System;
using System.Linq;
using System.Security.Claims;
using HawkvNext.Notifications;

namespace HawkvNext.Stages
{
    internal class UserStage : PipelineStage
    {
        internal override async Task Receive(HawkPipelineContext pipelineContext)
        {
            if (String.IsNullOrWhiteSpace(pipelineContext.UserId))
            {
                return; // User ID is not known. Stop further processing.
            }

            var userContext = new HawkFindUserContext(pipelineContext.HttpContext,
                                                        pipelineContext.Options,
                                                        pipelineContext.UserId);

            await pipelineContext.Options.Notifications.FindUser(userContext);

            pipelineContext.AlgorithmName = userContext.AlgorithmName;
            pipelineContext.CryptographicKey = userContext.CryptographicKey;
            pipelineContext.AdditionalClaims = userContext.AdditionalClaims;

            await this.Next.Receive(pipelineContext);
        }
    }
}