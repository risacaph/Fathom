using Markdig;

namespace Fathom.Services.Extensions;

public static class MarkdownExtensions
{
    public static MarkdownPipelineBuilder UseGithub(this MarkdownPipelineBuilder pipeline)
    {
        return pipeline.UsePipeTables()
            .UseFootnotes()
            .UseMathematics()
            .UseGenericAttributes(); // Always last!
    }
}
