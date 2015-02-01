using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Structura.SharedComponents.WebUtilities
{
    public static class StringExtensionsWeb
    {
        public static string ToSafeHtmlString(this string s, bool htmlEncode = false)
        {
            if (null == s)
                return "";

            string result = s;

            // we can skip the html encode if we secured the Html with SanitizeUserInputHtml
            if (htmlEncode)
            {
                HttpUtility.HtmlEncode(s);
            }
            string result1 = result.Trim('\r', '\n', ' ', '\t', ' ');
            result1 = result1
                .Replace("  ", " ");
            result1 = result1
                .Replace("\r\n", "<br/>");
            result1 = result1
                .Replace("\n", "<br/>");
            return result1;
        }

        public static IEnumerable<string> ExtractImages(this string html)
        {
            // Disabled: |(\bhttps?://pic.twitter.com/[a-zA-Z0-9]+\b)
            foreach (
                Match match in
                    Regex.Matches(html, @"(\bhttps?://[-A-Za-z0-9+&@#/%?=~_()|!:,.;]*[-A-Za-z0-9+&@#/%=~_()|][\.](jpg|jpeg|png|gif)\b)",
                                  RegexOptions.IgnoreCase | RegexOptions.Multiline))
            {
                yield return match.Groups[0].Value;
            }
        }

        public static string RemoveImageUrls(this string html)
        {
            return Regex.Replace(html, @"(\bhttps?://[-A-Za-z0-9+&@#/%?=~_()|!:,.;]*[-A-Za-z0-9+&@#/%=~_()|][\.](jpg|jpeg|png|gif)\b)",
                                  string.Empty, RegexOptions.IgnoreCase | RegexOptions.Multiline);
        }

        public static string ToSeoUrlString(this string input, int maximumLength)
        {

            var builder = new StringBuilder();
            var words = HttpUtility.HtmlDecode(input).Split(' ', '\'', '_', '{', '}', '|', '\\', '^', '~', '[', ']', '%', ':', '(', ')', ',', '.', '/', '&', '?');
            foreach (string word in words)
            {
                if (builder.Length > 0 && word.Length > 0)
                    builder.Append('-');
                builder.Append(word);
                if (builder.Length > maximumLength)
                    break;
            }
            if (builder.Length == 0)
                builder.Append("unknown");
            return HttpUtility.UrlEncode(builder.ToString());
        }
        public static string ToTextAreaString(this string source)
        {
            return HttpUtility.HtmlDecode(source.Replace("<br/>", "\r\n"));
        }

        public static string RichTextToDisplayString(this string source)
        {
            return HttpUtility.HtmlDecode(source).Replace("\r\n", "<br/>");
        }

        
    }
}