using System;

namespace Fhi.HelseId.Web.ExtensionMethods;

public class ConfigurationException : Exception
{
    public ConfigurationException(string message) : base(message)
    {
       
    }
}