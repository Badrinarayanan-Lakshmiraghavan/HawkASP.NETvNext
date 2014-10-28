using Microsoft.AspNet.Security.Notifications;
using Microsoft.AspNet.Http;

namespace HawkvNext.Notifications
{
    public class HawkCheckResponseBodyHashabilityContext : BaseContext<HawkAuthenticationOptions>
    {
        public HawkCheckResponseBodyHashabilityContext(HttpContext context, HawkAuthenticationOptions options)
            : base(context, options) { }

        public bool HashResponseBody { get; set; }
    }
}