# Architecture

> This is just a sketch -- it can change in time.

## Graph

```mermaid
graph LR
    subgraph Facade [Facade Layer]
        IDE[IDE\nIntegration] <--> TF[Test\nFramework]
    end

    User[User] <--> TF
    IDE <--> User

    TF <--> TE[Test\nExecutor]

    subgraph Containers [Tests Layer]
        TE <--> S1[Student #1 Code\nin Container]
        TE <--> S2[Student #2 Code\nin Container]
        TE ~~~ Dots[. . .]
        TE <--> SN[Student #n Code\nin Container]
    end

    %% Ukrycie ramki dla kropek
    style Dots fill:none,stroke:none
```

# Tech stack

* .NET 11 (preview, releasing on November 2026)

## Test Framework / IDE Integration

* [Microsoft.Testing.Platform](https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-intro)

## Test Executor

* [Testcontainers](https://testcontainers.com/)
* [Mono Cecil](https://www.mono-project.com/docs/tools+libraries/libraries/Mono.Cecil/)
