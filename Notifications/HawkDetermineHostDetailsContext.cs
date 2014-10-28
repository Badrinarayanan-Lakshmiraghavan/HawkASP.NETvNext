using Microsoft.AspNet.Security.Notifications;
using Microsoft.AspNet.Http;

namespace HawkvNext.Notifications
{
    public class HawkDetermineHostDetailsContext : BaseContext<HawkAuthenticationOptions>
    {
        public HawkDetermineHostDetailsContext(HttpContext context, HawkAuthenticationOptions options)
            : base(context, options) { }

        public string HostName { get; set; }

        public int HostPort { get; set; }
    }
}