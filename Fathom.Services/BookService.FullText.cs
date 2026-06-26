using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Docnet.Core;
using Docnet.Core.Models;
using Fathom.Common.Helpers;
using Microsoft.Extensions.Logging;
using VersOne.Epub;

namespace Fathom.Services;

public partial class BookService
{
    /// <summary>
    /// Extracts the full plain-text content of a book file (EPUB or PDF) for the full-text search index.
    /// Best-effort: returns an empty string for unsupported formats or on failure.
    /// </summary>
    public async Task<string> ExtractPlainTextAsync(string bookFilePath, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(bookFilePath) || !directoryService.FileSystem.File.Exists(bookFilePath))
            return string.Empty;

        try
        {
            if (bookFilePath.EndsWith(".epub", StringComparison.OrdinalIgnoreCase))
                return await ExtractEpubTextAsync(bookFilePath, ct);
            if (bookFilePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                return ExtractPdfText(bookFilePath, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[FullText] Could not extract text from {File}", bookFilePath);
        }

        return string.Empty;
    }

    private async Task<string> ExtractEpubTextAsync(string filePath, CancellationToken ct)
    {
        using var book = await EpubReader.OpenBookAsync(filePath, LenientBookReaderOptions);
        if (book == null) return string.Empty;

        var sb = new StringBuilder();
        var readingOrder = await book.GetReadingOrderAsync();
        foreach (var contentFileRef in readingOrder)
        {
            ct.ThrowIfCancellationRequested();
            var html = await contentFileRef.ReadContentAsync();
            var text = HtmlHelper.GetPlainText(html);
            if (!string.IsNullOrWhiteSpace(text)) sb.Append(text).Append('\n');
        }

        return sb.ToString();
    }

    private string ExtractPdfText(string filePath, CancellationToken ct)
    {
        using var docReader = DocLib.Instance.GetDocReader(filePath, new PageDimensions(1080, 1920));
        var pageCount = docReader.GetPageCount();

        var sb = new StringBuilder();
        for (var i = 0; i < pageCount; i++)
        {
            ct.ThrowIfCancellationRequested();
            using var pageReader = docReader.GetPageReader(i);
            var text = pageReader.GetText();
            if (!string.IsNullOrWhiteSpace(text)) sb.Append(text).Append('\n');
        }

        return sb.ToString();
    }
}
