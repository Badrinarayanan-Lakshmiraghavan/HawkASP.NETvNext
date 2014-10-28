using Microsoft.AspNet.Security.Notifications;
using Microsoft.AspNet.Http;
using System.Collections.Generic;
using System.Security.Claims;

namespace HawkvNext.Notifications
{
    public class HawkFindUserContext : BaseContext<HawkAuthenticationOptions>
    {
        public HawkFindUserContext(HttpContext context, HawkAuthenticationOptions options, string userId)
            : base(context, options)
        {
            this.UserId = userId;
        }

        /// <summary>
        /// The identifier that uniquely identies the user making the request. This will be
        /// mapped to the claim of type ClaimTypes.NameIdentifier.
        /// </summary>
        public string UserId { get; protected set; }

        /// <summary>
        /// Any other claims that will need to be associated with the identity established, if
        /// authentication is successful.
        /// </summary>
        public IList<Claim> AdditionalClaims { get; set; }

        /// <summary>
        /// The shared symmetric key that is exchanged out-of-band between the service and the client.
        /// </summary>
        public byte[] CryptographicKey { get; set; }

        /// <summary>
        /// The hashing algorithm that the client and the service have agreed to use to create the 
        /// payload hash as well as HMAC of the request and timestamp. The string provided here
        /// must be one of the valid hashName values of the Create method of
        /// System.Security.Cryptography.HashAlgorithm.
        /// </summary>
        public string AlgorithmName { get; set; }
    }
}