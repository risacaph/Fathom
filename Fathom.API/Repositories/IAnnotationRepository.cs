using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fathom.Common.Helpers;
using Fathom.Models.DTOs;
using Fathom.Models.DTOs.Annotations;
using Fathom.Models.DTOs.Filtering.v2.Requests;
using Fathom.Models.DTOs.Reader;
using Fathom.Models.Entities.User;

namespace Fathom.API.Repositories;

public interface IAnnotationRepository
{
    void Attach(AppUserAnnotation annotation);
    void Update(AppUserAnnotation annotation);
    void Remove(AppUserAnnotation annotation);
    void Remove(IEnumerable<AppUserAnnotation> annotations);
    Task<AnnotationDto?> GetAnnotationDto(int id, CancellationToken ct = default);
    Task<AppUserAnnotation?> GetAnnotation(int id, CancellationToken ct = default);
    Task<IList<AppUserAnnotation>> GetAllAnnotations(CancellationToken ct = default);
    Task<IList<AppUserAnnotation>> GetAnnotations(int userId, IList<int> ids, CancellationToken ct = default);
    Task<IList<FullAnnotationDto>> GetFullAnnotationsByUserIdAsync(int userId, CancellationToken ct = default);
    Task<IList<FullAnnotationDto>> GetFullAnnotations(int userId, IList<int> annotationIds, CancellationToken ct = default);
    Task<PagedList<AnnotationDto>> GetAnnotationDtos(int userId, AnnotationFilterDto filter, UserParams userParams, CancellationToken ct = default);
    Task<List<SeriesDto>> GetSeriesWithAnnotations(int userId, CancellationToken ct = default);
}
