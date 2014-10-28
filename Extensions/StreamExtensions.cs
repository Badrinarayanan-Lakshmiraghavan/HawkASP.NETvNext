using System;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using System.Security.Cryptography;

namespace HawkvNext.Extensions
{
    internal static class StreamExtensions
    {
        private const string PREAMBLE = "hawk.1.payload";

        internal static Task<byte[]> ComputeRequestBodyHashAsync(this Stream stream, HawkPipelineContext context)
        {
            return stream.ComputeHashAsync(context, context.Request.ContentType);
        }

        internal static Task<byte[]> ComputeResponseBodyHashAsync(this Stream stream, HawkPipelineContext context)
        {
            return stream.ComputeHashAsync(context, context.Response.ContentType);
        }

        private static async Task<byte[]> ComputeHashAsync(this Stream stream, HawkPipelineContext context, string contentType)
        {
            stream.Seek(0, SeekOrigin.Begin);

            string content = String.Empty;
            using (var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
            {
                content = await reader.ReadToEndAsync();
            }

            stream.Seek(0, SeekOrigin.Begin);

            var builder = new StringBuilder();

            if (!String.IsNullOrWhiteSpace(contentType))
            {
                int index = contentType.IndexOf(';');
                if (index > 0)
                    contentType = contentType.Substring(0, index);
            }

            builder
                .Append(PREAMBLE).Append("\n")
                .Append(contentType == null ? String.Empty : contentType.ToLower()).Append("\n")
                .Append(content).Append("\n");

            byte[] normalizedContent = Encoding.UTF8.GetBytes(builder.ToString());

            using (var hashAlgorithm = HashAlgorithm.Create(context.AlgorithmName))
            {
                return hashAlgorithm.ComputeHash(normalizedContent);
            }
        }
    }
}