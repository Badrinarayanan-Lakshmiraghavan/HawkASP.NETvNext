using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Security;

namespace HawkvNext.Notifications
{
    /// <summary>
    /// Specifies callback methods which the HawkAuthenticationMiddleware invokes to 
    /// enable a developer have control over the authentication process.
    /// </summary>
    public interface IHawkAuthenticationNotifications
    {
        /// <summary>
        /// Called when the middleware needs to find the attributes of the user making the 
        /// request, such as the cryptographic key issued to the user, etc.
        /// </summary>
        Task FindUser(HawkFindUserContext context);

        /// <summary>
        /// Called when the middleware needs the host name and port.
        /// </summary>
        void DetermineHostDetails(HawkDetermineHostDetailsContext context);

        /// <summary>
        /// Called when the middleware needs the normalized form of the response
        /// message to be put into the 'ext' field in the Server-Authorization response header.
        /// </summary>
        void NormalizeResponse(HawkNormalizeResponseMessageContext context);

        /// <summary>
        /// Called when the middleware needs to know if the normalized form of the request
        /// message matches the value in the 'ext' field coming in the Authorization header.
        /// </summary>
        void ValidateNormalizedRequest(HawkValidateNormalizedRequestContext context);


        /// <summary>
        /// Called when the middleware needs to know if the response body must be hashed and
        /// included in the MAC ('mac' field) sent in the Server-Authorization response header.
        /// </summary>
        void CheckResponseBodyHashability(HawkCheckResponseBodyHashabilityContext context);
    }
}