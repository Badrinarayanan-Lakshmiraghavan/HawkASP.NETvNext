using Microsoft.AspNet.Security.Infrastructure;
using Microsoft.AspNet.Builder;
using System;
using Microsoft.Framework.OptionsModel;

namespace HawkvNext
{
    public class HawkAuthenticationMiddleware : AuthenticationMiddleware<HawkAuthenticationOptions>
    {
        public HawkAuthenticationMiddleware(RequestDelegate next,
            IServiceProvider services,
            IOptions<HawkAuthenticationOptions> options,
            ConfigureOptions<HawkAuthenticationOptions> configureOptions)
            : base(next, services, options, configureOptions)
        { }

        protected override AuthenticationHandler<HawkAuthenticationOptions> CreateHandler()
        {
            return new HawkAuthenticationHandler();
        }
    }
}