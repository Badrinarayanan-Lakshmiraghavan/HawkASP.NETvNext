namespace HawkvNext.Extensions
{
    internal static class ByteExtensions
    {
        internal static bool ConstantTimeEquals(this byte[] left, byte[] right)
        {
            bool equal = (left.Length == right.Length);
            if (!equal) right = left;

            for (int i = 0; i < left.Length; i++)
                equal = equal && (left[i] == right[i]);

            return equal;
        }
    }
}