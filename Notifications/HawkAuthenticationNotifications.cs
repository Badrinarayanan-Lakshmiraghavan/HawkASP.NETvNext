using System.Threading.Tasks;
using System;
using System.Text.RegularExpressions;

namespace HawkvNext.Notifications
{
    /// <summary>
    /// Default implementation of IHawkAuthenticationNotifications
    /// </summary>
    public class HawkAuthenticationNotifications : IHawkAuthenticationNotifications
    {
        public HawkAuthenticationNotifications()
        {
            this.OnFindUser = context => Task.FromResult(0);
            this.OnDetermineHostDetails = context => DefaultBehavior.DetermineHostDetails(context);
            this.OnNormalizeResponse = context => { };
            this.OnValidateNormalizedRequest = context => { };
            this.OnCheckResponseBodyHashability = context => { };
        }

        public Func<HawkFindUserContext, Task> OnFindUser { get; set; }

        public Action<HawkDetermineHostDetailsContext> OnDetermineHostDetails { get; set; }

        public Action<HawkNormalizeResponseMessageContext> OnNormalizeResponse { get; set; }

        public Action<HawkValidateNormalizedRequestContext> OnValidateNormalizedRequest { get; set; }

        public Action<HawkCheckResponseBodyHashabilityContext> OnCheckResponseBodyHashability { get; set; }


        public Task FindUser(HawkFindUserContext context)
        {
            return OnFindUser.Invoke(context);
        }

        public void DetermineHostDetails(HawkDetermineHostDetailsContext context)
        {
            OnDetermineHostDetails.Invoke(context);
        }

        public void NormalizeResponse(HawkNormalizeResponseMessageContext context)
        {
            OnNormalizeResponse.Invoke(context);
        }

        public void ValidateNormalizedRequest(HawkValidateNormalizedRequestContext context)
        {
            OnValidateNormalizedRequest.Invoke(context);
        }

        public void CheckResponseBodyHashability(HawkCheckResponseBodyHashabilityContext context)
        {
            OnCheckResponseBodyHashability.Invoke(context);
        }

        internal static class DefaultBehavior
        {
            internal static readonly Action<HawkDetermineHostDetailsContext> DetermineHostDetails = context =>
            {
                string host = context.Request.Headers["X-Forwarded-Host"];
                if (String.IsNullOrWhiteSpace(host))
                    host = context.Request.Headers["Host"];

                string hostName = String.Empty;
                string port = String.Empty;

                string pattern = @"^(?:(?:\r\n)?\s)*((?:[^:]+)|(?:\[[^\]]+\]))(?::(\d+))?(?:(?:\r\n)?\s)*$";
                var match = Regex.Match(host, pattern);

                if (match.Success && match.Groups.Count == 3)
                {
                    hostName = match.Groups[1].Value;

                    if (!String.IsNullOrWhiteSpace(hostName))
                    {
                        port = match.Groups[2].Value;
                    }
                }

                if (String.IsNullOrWhiteSpace(port))
                {
                    port = context.Request.Headers["X-Forwarded-Port"];
                }

                if (String.IsNullOrWhiteSpace(port))
                {
                    string scheme = context.Request.Headers["X-Forwarded-Proto"];
                    if (String.IsNullOrWhiteSpace(scheme))
                    {
                        scheme = context.Request.Scheme;
                    }

                    port = "https".Equals(scheme, StringComparison.OrdinalIgnoreCase) ? "443" : "80";
                }

                context.HostName = hostName;

                int portNumber;
                if (Int32.TryParse(port, out portNumber))
                    context.HostPort = portNumber;
            };
        }
    }
}