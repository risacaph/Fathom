using System;
using System.Threading;
using System.Threading.Tasks;
using Fathom.Models.Entities.Enums;

namespace Fathom.API.Services;

public interface IMediaErrorService
{
    void ReportMediaIssue(string filename, MediaErrorProducer producer, string errorMessage, string details);
    void ReportMediaIssue(string filename, MediaErrorProducer producer, string errorMessage, Exception ex);
    Task ReportMediaIssueAsync(string filename, MediaErrorProducer producer, string errorMessage, string details, CancellationToken ct = default);
    Task ReportMediaIssueAsync(string filename, MediaErrorProducer producer, string errorMessage, Exception ex, CancellationToken ct = default);
}
