using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using System.Security.Claims;
using Microsoft.Framework.OptionsModel;
using Microsoft.AspNet.Security;
using Microsoft.AspNet.Http.Security;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using HawkvNext.Notifications;

namespace HawkvNext
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseHawkAuthentication(o =>
            {
                o.AuthenticationType = HawkConstants.AuthenticationType;
                //o.AuthenticationMode = AuthenticationMode.Passive;

                o.Notifications = new HawkAuthenticationNotifications()
                {
                    OnFindUser = (context) =>
                    {
                        // Use context.UserId to retrieve details
                        context.AlgorithmName = "SHA256";
                        context.CryptographicKey = Convert.FromBase64String("wBgvhp1lZTr4Tb6K6+5OQa1bL9fxK7j8wBsepjqVNiQ=");
                        context.AdditionalClaims = new List<Claim>() { new Claim(ClaimTypes.Name, "Badri") };

                        return Task.FromResult(0);
                    },
                    OnValidateNormalizedRequest = (context) =>
                    {
                        // Use context.Request to get to the message parts protected
                        // and verify if context.ExtFieldValue matches. If matches,
                        // set context.IsValid = true.
                        string headerName = "X-Request-Header-To-Protect";
                        string value = headerName + ":" + context.Request.Headers[headerName];
                        context.IsValid = context.ExtFieldValue.Equals(value);
                    },
                    OnCheckResponseBodyHashability = (context) => context.HashResponseBody = true,
                    OnNormalizeResponse = (context) => context.NormalizedResponseMessage = "hello"
                };
            });


            app.Run(async (HttpContext context) =>
            {
                //var result = await context.AuthenticateAsync(HawkConstants.AuthenticationType);
                //if (result != null && result.Identity != null)
                //{
                //    context.User = new ClaimsPrincipal(result.Identity);
                //}

                if (!context.User.Identity.IsAuthenticated)
                {
                    context.Response.StatusCode = 401;
                    //context.Response.Challenge(HawkConstants.AuthenticationType);
                }
                else
                {
                    await context.Response.WriteAsync(context.User.Identity.Name);
                }
            });
        }
    }
}


