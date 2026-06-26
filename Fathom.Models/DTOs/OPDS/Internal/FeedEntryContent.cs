using System.Xml.Serialization;

namespace Fathom.Models.DTOs.OPDS;

public sealed record FeedEntryContent
{
    [XmlAttribute("type")]
    public string Type = "text";
    [XmlText]
    public string Text;
}
