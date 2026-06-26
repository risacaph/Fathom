using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fathom.Models.Entities;

namespace Fathom.API.Repositories;

public interface IMangaFileRepository
{
    void Update(MangaFile file);
    Task<IList<MangaFile>> GetAllWithMissingExtension(CancellationToken ct = default);
    Task<MangaFile?> GetByKoreaderHash(string hash, CancellationToken ct = default);
}
