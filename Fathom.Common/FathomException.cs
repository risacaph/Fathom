using System;

namespace Fathom.Common;

/// <summary>
/// These are used for errors to send to the UI that should not be reported to Sentry
/// </summary>
public class FathomException : Exception
{
    public FathomException()
    { }

    public FathomException(string message) : base(message)
    { }

    public FathomException(string message, Exception inner)
        : base(message, inner) { }
}
