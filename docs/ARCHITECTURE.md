# Architecture

## Graph

```mermaid
graph LR
    subgraph Facade [Facade Layer]
        IDE[IDE\nIntegration] <--> TF[Test\nFramework]
    end

    User[User] <--> TF
    IDE <--> User

    TF <--> TC[Test\nCoordinator]

    subgraph Containers [Tests Layer]
        TC <--> S1[Student #1 Code\nin Container]
        TC <--> S2[Student #2 Code\nin Container]
        TC ~~~ Dots[. . .]
        TC <--> SN[Student #n Code\nin Container]
    end

    %% Ukrycie ramki dla kropek
    style Dots fill:none,stroke:none
```

# Tech stack

* .NET 11 (preview, releasing on November 2026)

## Test Framework / IDE Integration

* [Microsoft.Testing.Platform](https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-intro)

## Test Coordinator

* [Testcontainers](https://testcontainers.com/)
