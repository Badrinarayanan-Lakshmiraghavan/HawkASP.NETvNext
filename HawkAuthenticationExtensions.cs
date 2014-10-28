using System;
using Microsoft.AspNet.Builder;
using Microsoft.Framework.OptionsModel;

namespace HawkvNext
{
    public static class HawkAuthenticationExtensions
    {
        //public static IServiceCollection ConfigureCookieAuthentication([NotNull] this IServiceCollection services, [NotNull] Action<CookieAuthenticationOptions> configure)
        //{
        //    return services.Configure(configure);
        //}

        public static IApplicationBuilder UseHawkAuthentication(this IApplicationBuilder app, Action<HawkAuthenticationOptions> configureOptions = null, string optionsName = "")
        {
            return app.UseMiddleware<HawkAuthenticationMiddleware>(
                new ConfigureOptions<HawkAuthenticationOptions>(configureOptions ?? (o => { }))
                {
                    Name = optionsName
                });
        }
    }
}