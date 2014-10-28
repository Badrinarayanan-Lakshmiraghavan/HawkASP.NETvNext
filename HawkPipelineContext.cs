using Microsoft.AspNet.Http;
using System.Collections.Generic;
using System.Security.Claims;
using System;
using System.IO;

namespace HawkvNext
{
    internal class HawkPipelineContext
    {
        private Stream originalResponseStream { get; set; }

        internal HawkPipelineContext(HttpContext context, HawkAuthenticationOptions options)
        {
            this.HttpContext = context;
            this.Options = options;
            this.Challenge = HawkConstants.Scheme;
        }

        public HttpContext HttpContext { get; private set; }

        public HawkAuthenticationOptions Options { get; private set; }

        public HttpRequest Request
        {
            get
            {
                return HttpContext.Request;
            }
        }

        public HttpResponse Response
        {
            get
            {
                return HttpContext.Response;
            }
        }

        /// <summary>
        /// id field
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// exp in case of bewit and ts otherwise
        /// </summary>
        public ulong Timestamp { get; set; }

        /// <summary>
        /// nonce field
        /// </summary>
        public string Nonce { get; set; }

        /// <summary>
        /// mac
        /// </summary>
        public byte[] Mac { get; set; }


        /// <summary>
        /// hash
        /// </summary>
        public byte[] PayloadHash { get; set; }

        /// <summary>
        /// ext
        /// </summary>
        public string ApplicationSpecificData { get; set; }

        /// <summary>
        /// Additional claims that need to made part of the identity, 
        /// on successful authentication.
        /// </summary>
        public IList<Claim> AdditionalClaims { get; set; }

        /// <summary>
        /// Shared symmetric key that is exchanged out-of-band 
        /// between the service and the client.
        /// </summary>
        public byte[] CryptographicKey { get; set; }

        /// <summary>
        /// The hashing algorithm that the client and the service have agreed to use
        /// to create the payload hash as well as HMAC of the request and timestamp.
        /// </summary>
        public string AlgorithmName { get; set; }

        /// <summary>
        /// The identity established on successful authentication.
        /// </summary>
        public ClaimsIdentity Identity { get; set; }

        /// <summary>
        /// True, if the request contains a bewit in query string instead of
        /// Authorization request header.
        /// </summary>
        public bool IsBewit { get; set; }

        /// <summary>
        /// Path and the query string used while normalizing a message.
        /// </summary>
        public string PathAndQueryString { get; set; }

        /// <summary>
        /// Host name used while normalizing a message.
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// Port used while normalizing a message.
        /// </summary>
        public int HostPort { get; set; }

        /// <summary>
        /// The instance request is received in UNIX time millis.
        /// </summary>
        public ulong Now { get; set; }

        /// <summary>
        /// The challenge to be sent with WWW-Authenticate, in case of 401.
        /// </summary>
        public string Challenge { get; set; }

        /// <summary>
        /// True, if identity established is authentic and the authentication
        /// type is "Hawk".
        /// </summary>
        public bool IsSuccessfulHawkAuthentication
        {
            get
            {
                return this.Identity != null && this.Identity.IsAuthenticated &&
                            HawkConstants.AuthenticationType.Equals(
                                        this.Identity.AuthenticationType);
            }
        }

        /// <summary>
        /// True, if Server-Authorization header is required to be created.
        /// This header is created only when all the following are true:
        /// (1) Authentication is successful.
        /// (2) Authentication is not based on bewit.
        /// (3) The server is configured to send the header.
        /// (4) Status code is not 401.
        /// </summary>
        public bool IsServerAuthorizationRequired
        {
            get
            {
                return this.IsSuccessfulHawkAuthentication &&
                            (!this.IsBewit) &&
                                this.Options.EnableServerAuthorization &&
                                    this.Response.StatusCode != 401;
            }
        }

        /// <summary>
        /// Registers the passed in stream to be restored later as part of teardown.
        /// </summary>
        /// <param name="originalStream">The original response body stream</param>
        public void RegisterForResponseStreamRestoration(Stream originalStream)
        {
            this.originalResponseStream = originalStream;
        }

        /// <summary>
        ///  Restores the original stream registered.
        /// </summary>
        public void RestoreOriginalResponseStream()
        {
            if (this.originalResponseStream != null)
            {
                this.Response.Body = this.originalResponseStream;
                this.originalResponseStream = null;
            }
        }
    }
}