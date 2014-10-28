using System;
using Microsoft.Framework.OptionsModel;
using Microsoft.AspNet.Security;
using HawkvNext.Notifications;

namespace HawkvNext
{
    public class HawkAuthenticationOptions : AuthenticationOptions
    {
        public HawkAuthenticationOptions()
        {
            this.ClockSkewSeconds = 60;
            this.EnableServerAuthorization = true;
            this.Notifications = new HawkAuthenticationNotifications();
            this.IsBewitSupported = true;
        }

        /// <summary>
        /// Local time offset in milliseconds.
        /// </summary>
        public int LocalTimeOffsetMillis { get; set; }

        /// <summary>
        /// Skew allowed between the client and the server clocks in seconds. Default is 60 seconds.
        /// </summary>
        public int ClockSkewSeconds { get; set; }

        /// <summary>
        /// If true, the Server-Authorization header is sent in the response. Default is true.
        /// </summary>
        public bool EnableServerAuthorization { get; set; }

        public IHawkAuthenticationNotifications Notifications { get; set; }

        public bool IsBewitSupported { get; set; }
    }
}