# Fhi.HelseId

## Introduction

The Fhi.HelseId component is package handling authentication and authorization access to NHN HelseId.  It can be used on Clients and APIs.  

The component encapsulates the access logic, so all you need to do is to enable this in the startup class of your web app or web api.  

It also contains calls to the HPR register, so that you can use a health persons categories for role control.  

All features of the components can be controlled using featureflags from your configuration, either hardcoded or loaded from your appsettings.json.

The component uses the "Backend for Frontend" pattern, based off a  Authorization Code flow, for details see [here](https://www.nhn.no/helseid/grunnleggende-kunnskap/autentiseringsflyt-og-grant-types/)

The component requires minimum .NET Core app 3.1.

## [Documentation](https://github.com/folkehelseinstituttet/fhi.helseid/wiki)

## [Demo app](https://github.com/folkehelseinstituttet/fhi.helseid.demo)

## Status

Latest CI Build ![Lastest CI build](https://img.shields.io/github/workflow/status/folkehelseinstituttet/fhi.helseid/Fhi.HelseId.CI?style=plastic)

Latest nuget package


## Access to HelseId

You need to order access to HelseId.  For test contact [here](), and prod production contact [here]().