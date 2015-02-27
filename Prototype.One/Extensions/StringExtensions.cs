
namespace Prototype.One.Extensions
{
    public static class StringExtensions
    {
        public static string Format(this string formatString, params object[] args)
        {
            return string.Format(formatString, args);
        }
    }
}
