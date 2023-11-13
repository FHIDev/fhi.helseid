# Fhi.AuthControllers

This package contains an account controller with endpoints for account login and logout.
It depends on the [Fhi.HelseId](https://www.nuget.org/packages/Fhi.HelseId) package.

The intention of the package is to save the developer from writing code that is always the same.
There is no need for any other initialization that adding the package to your project.

## Installation

Just add the package to your project from [Nuget](https://www.nuget.org/packages/Fhi.AuthControllers).

## Usage

The package adds a controller named `AccountController` with the following GET endpoints:

* Login
* Logout
* Ping

The last `Ping` endpoint is used to check if controller is installed.`



