using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Fhi.HelseId.Common;

/// <summary>
/// Use this for an outgoing API configuration. Version 3 and 4
/// </summary>
public class HelseIdApiOutgoingKonfigurasjon : HelseIdCommonKonfigurasjon, IApiOutgoingKonfigurasjon
{
    public string Name { get; set; } = "";
    public string Url { get; set; } = "";

    public string Scope { get; set; } = "";
    
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public Uri Uri => new(Url);
}


/// <summary>
/// This is the schema for the appsetting for outgoing APIs. Version 3 and 4
/// </summary>
public class HelseIdApiOutgoingKonfigurasjoner : IOutgoingApis
{
    public IApiOutgoingKonfigurasjon[] Apis { get; set; } = Array.Empty<IApiOutgoingKonfigurasjon>();
    public Uri UriToApiByName(string name)
    {
        var url = Apis.FirstOrDefault(o => o.Name == name)?.Url ?? throw new InvalidApiNameException(name); ;
        return new Uri(url);
    }
}

public interface IApiOutgoingKonfigurasjon
{
    string Name { get; set; }
    string Url { get; set; }
    string Scope { get; set; }
    Uri Uri { get; }
}

/// <summary>
/// This is the schema for the appsetting for outgoing APIs. Version 5
/// </summary>
public class ApiOutgoingKonfigurasjon : IApiOutgoingKonfigurasjon
{
    public string Name { get; set; } = "";
    public string Url { get; set; } = "";

    public string Scope { get; set; } = "";

    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public Uri Uri => new(Url);
}

public interface IOutgoingApis
{
    IApiOutgoingKonfigurasjon[] Apis { get; set; }
    Uri UriToApiByName(string name);
}

/// <summary>
/// This is a list of Outgoing Apis.  Version 5
/// </summary>
//public class OutgoingApis : IOutgoingApis
//{
//    public IApiOutgoingKonfigurasjon[] Apis { get; set; } = Array.Empty<IApiOutgoingKonfigurasjon>();

    
//}


[Serializable]
public class InvalidApiNameException : Exception
{
    //
    // For guidelines regarding the creation of new exception types, see
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
    // and
    //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
    //

    public InvalidApiNameException() { }
    public InvalidApiNameException(string message) : base(message) { }
    public InvalidApiNameException(string message, Exception inner) : base(message, inner) { }

    protected InvalidApiNameException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}