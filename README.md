Hawk ASP.NET vNext Middleware
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

