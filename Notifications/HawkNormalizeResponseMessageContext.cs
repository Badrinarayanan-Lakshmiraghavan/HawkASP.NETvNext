using Microsoft.AspNet.Security.Notifications;
using Microsoft.AspNet.Http;
using System;

namespace HawkvNext.Notifications
{
    public class HawkNormalizeResponseMessageContext : BaseContext<HawkAuthenticationOptions>
    {
        public HawkNormalizeResponseMessageContext(HttpContext context, HawkAuthenticationOptions options)
            : base(context, options)
        {
            NormalizedResponseMessage = String.Empty;
        }

        public string NormalizedResponseMessage { get; set; }
    }
}