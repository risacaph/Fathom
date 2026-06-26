using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Fathom.Common.Helpers;

#nullable enable

public static class HtmlHelper
{
    private const int BodyTextLimit = 175;

    public static string? GetCharacters(string? body)
    {
        if (string.IsNullOrEmpty(body)) return body;

        var doc = new HtmlDocument();
        doc.LoadHtml(body);

        var textNodes = doc.DocumentNode.SelectNodes("//text()[not(parent::script)]");
        if (textNodes == null) return string.Empty;

        var plainText =  string.Join(" ", textNodes
            .Select(node => node.InnerText)
            .Where(s => !s.Equals("\n")));

        // Clean any leftover Markdown out
        plainText = Regex.Replace(plainText, @"\*\*(.*?)\*\*", "$1"); // Bold with **
        plainText = Regex.Replace(plainText, @"_(.*?)_", "$1"); // Italic with _
        plainText = Regex.Replace(plainText, @"\[(.*?)\]\((.*?)\)", "$1"); // Links [text](url)
        plainText = Regex.Replace(plainText, @"[_*\[\]~]", string.Empty);
        plainText = Regex.Replace(plainText, @"img\d*\((.*?)\)", string.Empty);
        plainText = Regex.Replace(plainText, @"~~~(.*?)~~~", "$1");
        plainText = Regex.Replace(plainText, @"\+{3}(.*?)\+{3}", "$1");
        plainText = Regex.Replace(plainText, @"~~(.*?)~~", "$1");
        plainText = Regex.Replace(plainText, @"__(.*?)__", "$1");
        plainText = Regex.Replace(plainText, @"#\s(.*?)", "$1");


        // Just strip symbols
        plainText = Regex.Replace(plainText, @"[_*\[\]~]", string.Empty);
        plainText = Regex.Replace(plainText, @"img\d*\((.*?)\)", string.Empty);
        plainText = Regex.Replace(plainText, @"~~~", string.Empty);
        plainText = Regex.Replace(plainText, @"\+", string.Empty);
        plainText = Regex.Replace(plainText, @"~~", string.Empty);
        plainText = Regex.Replace(plainText, @"__", string.Empty);

        // Take the first BodyTextLimit characters
        plainText = plainText.Length > BodyTextLimit ? plainText.Substring(0, BodyTextLimit) : plainText;

        return plainText + "…";
    }

    /// <summary>
    /// Returns the full, decoded plain text of an HTML fragment (whitespace collapsed, script/style removed).
    /// Used to feed the full-text search index — unlike <see cref="GetCharacters"/> it does not truncate.
    /// </summary>
    public static string GetPlainText(string? body)
    {
        if (string.IsNullOrEmpty(body)) return string.Empty;

        var doc = new HtmlDocument();
        doc.LoadHtml(body);

        var textNodes = doc.DocumentNode.SelectNodes("//text()[not(ancestor::script) and not(ancestor::style)]");
        if (textNodes == null) return string.Empty;

        var plainText = string.Join(" ", textNodes
            .Select(node => node.InnerText)
            .Where(s => !string.IsNullOrWhiteSpace(s)));

        plainText = System.Net.WebUtility.HtmlDecode(plainText);
        plainText = Regex.Replace(plainText, @"\s+", " ").Trim();

        return plainText;
    }
}
