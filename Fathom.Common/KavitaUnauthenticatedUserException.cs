using System;

namespace Fathom.Common;

/// <summary>
/// The user does not exist (aka unauthorized). This will be caught by middleware and Unauthorized() returned to UI
/// </summary>
/// <remarks>This will always log to Security Log</remarks>
public class FathomUnauthenticatedUserException : Exception
{
    public FathomUnauthenticatedUserException()
    { }

    public FathomUnauthenticatedUserException(string message) : base(message)
    { }

    public FathomUnauthenticatedUserException(string message, Exception inner)
        : base(message, inner) { }
}
