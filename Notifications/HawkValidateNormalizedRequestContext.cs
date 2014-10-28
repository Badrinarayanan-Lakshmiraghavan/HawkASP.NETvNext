using Microsoft.AspNet.Security.Notifications;
using Microsoft.AspNet.Http;

namespace HawkvNext.Notifications
{
    public class HawkValidateNormalizedRequestContext : BaseContext<HawkAuthenticationOptions>
    {
        public HawkValidateNormalizedRequestContext(HttpContext context,
                                                        HawkAuthenticationOptions options,
                                                        string extFieldValue)
            : base(context, options)
        {
            this.ExtFieldValue = extFieldValue;
        }

        /// <summary>
        /// The normalized form of request, as sent by the client in the 'ext' field
        /// of the HTTP Authorization header.
        /// </summary>
        public string ExtFieldValue { get; protected set; }

        public bool IsValid { get; set; }
    }
}