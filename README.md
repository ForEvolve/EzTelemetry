# EzTelemetry

EzTelemetry makes logging, events, traces, and metrics easy.

An EzTelemetry implementation accumulate the telemetry for a session, for one or more operation, then send that telemetry to its destination at the end.
This allows adding session, and operation level properties that are propagated to all telemetry item under that tree.

> This is a **work in progress**. APIs are not stable and will most likely change.

## Status

[![Build, Test, and Deploy](https://github.com/ForEvolve/EzTelemetry/actions/workflows/main.yml/badge.svg)](https://github.com/ForEvolve/EzTelemetry/actions/workflows/main.yml)

# How to install?

| Install                                              | NuGet.org                                                                                                                                          | Feedz.io                                                                                                                                                                                                                                                               |
| ---------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `dotnet add package EzTelemetry`                     | [![NuGet.org](https://img.shields.io/nuget/vpre/EzTelemetry)](https://www.nuget.org/packages/EzTelemetry/)                                         | [![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https%3A%2F%2Ff.feedz.io%2Fforevolve%2Feztelemetry%2Fshield%2FEzTelemetry%2Flatest)](https://f.feedz.io/forevolve/eztelemetry/packages/EzTelemetry/latest/download)                                         |
| `dotnet add package EzTelemetry.ApplicationInsights` | [![NuGet.org](https://img.shields.io/nuget/vpre/EzTelemetry.ApplicationInsights)](https://www.nuget.org/packages/EzTelemetry.ApplicationInsights/) | [![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https%3A%2F%2Ff.feedz.io%2Fforevolve%2Feztelemetry%2Fshield%2FEzTelemetry.ApplicationInsights%2Flatest)](https://f.feedz.io/forevolve/eztelemetry/packages/EzTelemetry.ApplicationInsights/latest/download) |
| `dotnet add package EzTelemetry.MediatR`             | [![NuGet.org](https://img.shields.io/nuget/vpre/EzTelemetry.MediatR)](https://www.nuget.org/packages/EzTelemetry.MediatR/)                         | [![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https%3A%2F%2Ff.feedz.io%2Fforevolve%2Feztelemetry%2Fshield%2FEzTelemetry.MediatR%2Flatest)](https://f.feedz.io/forevolve/eztelemetry/packages/EzTelemetry.MediatR/latest/download)                         |

## Versioning

The packages follows _semantic versioning_ and uses [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) under the hood to automate versioning based on Git commits.

## Pre-release

> Pre-release packages source: https://f.feedz.io/forevolve/eztelemetry/nuget/index.json

You can add a NuGet source using the [CLI](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-nuget-add-source), a `nuget.config` file (below), or Visual Studio.
I recommend using a `nuget.config` because you can check it in Git, so the people that clone your repository don't have to do any additional setup.

**nuget.config**

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <packageSources>
        <add key="Feedz.io (EzTelemetry)" value="https://f.feedz.io/forevolve/eztelemetry/nuget/index.json" />
        <add key="NuGet official package source" value="https://nuget.org/api/v2/" />
    </packageSources>
    <activePackageSource>
        <add key="All" value="(Aggregate source)" />
    </activePackageSource>
</configuration>
```

# Usage

More docs to come once the APIs are more stable.
Until then, here are a few glimpses.

## Using Application Insights

**Startup.cs**

```csharp
services
    .AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_CONNECTIONSTRING"])
    .AddEzTelemetry()
;
```

## Using Application Insights and MediatR

**Startup.cs**

```csharp
services
    .AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_CONNECTIONSTRING"])
    .AddEzTelemetry()
    .AddEzTelemetrySessionBehavior()
;
```

## Usage

Inject `ITelemetrySession`, call the `StartOperation()` method, add telemetry to you session.
You can then add events, metrics, exceptions, and traces to the `ITelemetryOperation` object.

> `ITelemetrySession` are _scoped_, so the DI container creates one per HTTP request.

```csharp
public class MyClass
{
    private readonly ITelemetrySession _telemetrySession;
    public MyClass(ITelemetrySession telemetrySession)
    {
        _telemetrySession = telemetrySession ?? throw new ArgumentNullException(nameof(telemetrySession));
    }

    public void MyMethod(object someParameter)
    {
        // Session-wise properties, cascade to all other telemetry sent over the session.
        _telemetrySession.AddProperty("Source", "MyClass:MyMethod");
        _telemetrySession.AddProperties(someParameter, prefix: "someParameter");

        // Create the telemetry operation (ex.: the method's scope)
        var operation = _telemetrySession.StartOperation();

        // Add operation-level properties that is attached to all operation telemetry
        operation.AddProperty("SomeKey", "Some info that is attached to all session telemetry.");

        // Add actual telemetry
        operation
            .AddMetric($"ElapsedTime", 123)
            .AddEvent("UserDidSomething")
            .AddTrace("You are here!")
        ;
        try
        {
            // some code
        }
        catch (Exception ex)
        {
            operation.AddException(ex);
            throw;
        }
    }
}
```

There's also a few extension methods to help out with the operation stuff.
Check for `ITelemetrySession` extension methods.

# Build and Test

To build or test, simply run `dotnet` commands like:

```bash
dotnet build
dotnet test
dotnet run
# ...
```

See [.github/workflows/main.yml](.github/workflows/main.yml) for more information about the master CI build.

# How to contribute?

If you would like to contribute to the Framework, first, thank you for your interest and please read [Contributing to ForEvolve open source projects](https://github.com/ForEvolve/ForEvolve-Framework/tree/master/CONTRIBUTING.md) for more information.

## Contributor Covenant Code of Conduct

Also, please read the [Contributor Covenant Code of Conduct](https://github.com/ForEvolve/ForEvolve-Framework/tree/master/CODE_OF_CONDUCT.md) that applies to all ForEvolve repositories.
