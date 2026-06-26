using System;

namespace Fathom.Common;

/// <summary>
/// Exception that is caught by the exception middleware, and returns NotFound
/// </summary>
public class FathomNotFoundException: Exception
{

    public FathomNotFoundException()
    {
    }

    public FathomNotFoundException(string message) : base(message)
    {
    }

    public FathomNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
