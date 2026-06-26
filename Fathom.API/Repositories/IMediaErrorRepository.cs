using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fathom.Models.DTOs.MediaErrors;
using Fathom.Models.Entities;

namespace Fathom.API.Repositories;

public interface IMediaErrorRepository
{
    void Attach(MediaError error);
    void Remove(IList<MediaError> errors);
    Task<IEnumerable<MediaErrorDto>> GetAllErrorDtosAsync(CancellationToken ct = default);
    Task<bool> ExistsAsync(MediaError error, CancellationToken ct = default);
    Task DeleteAll(CancellationToken ct = default);
    Task<List<MediaError>> GetAllErrorsAsync(IList<string> comments, CancellationToken ct = default);
}
