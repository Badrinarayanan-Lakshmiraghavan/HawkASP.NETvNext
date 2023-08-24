Hawk ASP.NET vNext Middleware
Badrinarayanan Lakshmiraghavan
=========
This is an initial attempt to redesign the Hawk OWIN middleware that is part of the [Thinktecture IdentityModel](https://github.com/thinktecture/Thinktecture.IdentityModel/tree/master/source/Hawk) into an ASP.NET vNext middleware. This middleware is based on ASP.NET vNext nightly bits and hence do not expect a stable codebase at this point. Things will change, as ASP.NET vNext is being actively developed.

The current design is based on two pipelines respectively for handling the request `Receive` and producing the response `Send`. Hawk authentication pipeline is composed of the following stages.

<ol>
<li>Header Stage</li>
<li>Bewit Stage</li>
<li>User Stage</li>
<li>Mac Stage</li>
<li>Freshness Stage</li>
<li>Body Hash Stage</li>
<li>Ext Stage</li>
<li>Identity Stage</li>
</ol>

A stage is defined by an abstract class `PipelineStage`.

```csharp
using System.Threading.Tasks;

namespace HawkvNext.Stages
{
    internal abstract class PipelineStage
    {
        internal PipelineStage Next { get; set; }

        internal virtual async Task Receive(HawkPipelineContext context)
        {
            await this.Next.Receive(context);
        }

        internal virtual async Task Send(HawkPipelineContext context)
        {
            await this.Next.Send(context);
        }
    }
}
```

The concrete classes corresponding to each stage is assembled into a pipeline by the sub-classed authentication handler class and request is run through the pipeline by calling the `Receive` method. Then, the pipeline is reversed and the response flows through the pipeline through the calling of the `Send` method.

<h2>Receive Pipeline</h2>

<img src="https://lbadri.files.wordpress.com/2014/10/receive.jpg"/>

<h2>Send Pipeline</h2>

<img src="https://lbadri.files.wordpress.com/2014/10/send.jpg"/>

<h2>Client Application</h2>
You need to send the authorization header/bewit with correct crypto stuff like MAC, payload hash, etc. To make that task easier, you can use the existing NuGet package [Thinktecture.IdentityModel.Hawk](https://www.nuget.org/packages/Thinktecture.IdentityModel.Hawk/).

Create a console app and add two NuGet packages - Microsoft.AspNet.WebApi.Client and Thinktecture.IdentityModel.Hawk. Then, copy paste the code below and you are all set to talk to the Hawk authentication middleware.

```csharp
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Thinktecture.IdentityModel.Hawk.Client;
using Thinktecture.IdentityModel.Hawk.Core;
using Thinktecture.IdentityModel.Hawk.Core.Helpers;
using Thinktecture.IdentityModel.Hawk.WebApi;

namespace ClientApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string uri = "http://localhost:5000";

            var credential = new Credential()
            {
                Id = "dh37fgj492je",
                Algorithm = SupportedAlgorithms.SHA256,
                User = "Steve",
                Key = Convert.FromBase64String("wBgvhp1lZTr4Tb6K6+5OQa1bL9fxK7j8wBsepjqVNiQ=")
            };

            // GET and POST using the Authorization header
            var options = new ClientOptions()
            {
                CredentialsCallback = () => credential,
                RequestPayloadHashabilityCallback = (r) => true,
                NormalizationCallback = (req) =>
                {
                    string name = "X-Request-Header-To-Protect";
                    return req.Headers.ContainsKey(name) ?
                                name + ":" + req.Headers[name].First() : null;
                },
                EnableResponseValidation = true,
                VerificationCallback = (resp, ext) => "hello".Equals(ext)
            };

            var handler = new HawkValidationHandler(options);

            HttpClient client = HttpClientFactory.Create(handler);
            client.DefaultRequestHeaders.Add("X-Request-Header-To-Protect", "secret");

            var response = client.GetAsync(uri).Result;
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);

            response = client.PostAsJsonAsync(uri, credential.User).Result;
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);

            // GET using Bewit
            var hawkClient = new HawkClient(options);
            var request = new HttpRequestMessage() { RequestUri = new Uri(uri) };

            string bewit = hawkClient.CreateBewit(new WebApiRequestMessage(request),
                                                        lifeSeconds: 60);

            // Bewit is handed off to a client needing temporary access to the resource.
            var clientNeedingTempAccess = new WebClient();
            var resource = clientNeedingTempAccess.DownloadString(uri + "?bewit=" + bewit);
            Console.WriteLine(resource);

            Console.Read();
        }
    }
}
```
