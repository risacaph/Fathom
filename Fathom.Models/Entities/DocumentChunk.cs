using Microsoft.EntityFrameworkCore;

namespace Fathom.Models.Entities;

/// <summary>
/// A chunk of a chapter's text together with its embedding vector, used for semantic search / RAG (F6).
/// Populated by the semantic indexer when an AI provider with embeddings is configured. The vector is stored
/// as a packed little-endian float32 BLOB; similarity is computed in-process (cosine).
/// </summary>
[Index(nameof(LibraryId))]
[Index(nameof(ChapterId))]
[Index(nameof(SeriesId))]
public class DocumentChunk
{
    public int Id { get; set; }

    /// <summary>Chapter this chunk was extracted from.</summary>
    public int ChapterId { get; set; }

    /// <summary>Denormalized for fast scoping of search to accessible libraries/series.</summary>
    public int SeriesId { get; set; }
    public int LibraryId { get; set; }

    /// <summary>Zero-based order of this chunk within the chapter.</summary>
    public int ChunkIndex { get; set; }

    /// <summary>The plain text of the chunk (returned as a snippet and used as RAG context).</summary>
    public required string Content { get; set; }

    /// <summary>Embedding vector, packed as little-endian float32 (4 bytes per dimension).</summary>
    public required byte[] Embedding { get; set; }

    /// <summary>Embedding model id used to produce <see cref="Embedding"/> (vectors are only comparable within a model).</summary>
    public string EmbeddingModel { get; set; } = string.Empty;
}
