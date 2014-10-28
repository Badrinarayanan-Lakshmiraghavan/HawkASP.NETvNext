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