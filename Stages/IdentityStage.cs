using System.Threading.Tasks;
using System;
using System.Linq;
using System.Security.Claims;

namespace HawkvNext.Stages
{
    internal class IdentityStage : PipelineStage
    {
        internal override Task Receive(HawkPipelineContext context)
        {
            string idClaimType = ClaimTypes.NameIdentifier;
            var idClaim = new Claim(idClaimType, context.UserId);

            var identity = new ClaimsIdentity(new[] { idClaim }, HawkConstants.AuthenticationType);
            var additionalClaims = context.AdditionalClaims;

            if (additionalClaims != null && additionalClaims.Count > 0)
            {
                identity.AddClaims(additionalClaims.Where(c => c.Type != idClaimType));
            }

            context.Identity = identity;

            // Inbound pipeline terminates here.
            return Task.FromResult(0);
        }
    }
}