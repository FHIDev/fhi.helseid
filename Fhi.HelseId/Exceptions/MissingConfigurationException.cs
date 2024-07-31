using System;

namespace Fhi.HelseId.Exceptions;

public class MissingConfigurationException : Exception
{
    public MissingConfigurationException(string outgoingApisName) : base(outgoingApisName) { }
    public MissingConfigurationException() { }
    public MissingConfigurationException(string message, Exception inner) : base(message, inner) { }
}
