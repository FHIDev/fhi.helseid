using System;

namespace Fhi.HelseId.Common;

/// <summary>
/// Use this for an outgoing API configuration. Version 3 and 4
/// </summary>
public class HelseIdApiOutgoingKonfigurasjon : HelseIdCommonKonfigurasjon, IApiOutgoingKonfigurasjon
{
    public string Name { get; set; } = "";
    public string Url { get; set; } = "";

    public string Scope { get; set; } = "";
    public Uri Uri => new(Url);
}


/// <summary>
/// This is the schema for the appsetting for outgoing APIs. Version 3 and 4
/// </summary>
public class HelseIdApiOutgoingKonfigurasjoner : IOutgoingApis
{
    public IApiOutgoingKonfigurasjon[] Apis { get; set; } = Array.Empty<IApiOutgoingKonfigurasjon>();
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
    public Uri Uri => new(Url);
}

public interface IOutgoingApis
{
    IApiOutgoingKonfigurasjon[] Apis { get; set; }
}

/// <summary>
/// This is a list of Outgoing Apis.  Version 5
/// </summary>
public class OutgoingApis : IOutgoingApis
{
    public IApiOutgoingKonfigurasjon[] Apis { get; set; } = Array.Empty<IApiOutgoingKonfigurasjon>();
}