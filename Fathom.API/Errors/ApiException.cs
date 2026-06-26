namespace Fathom.API.Errors;

public record ApiException(int Status, string? Message = null, string? Details = null);
