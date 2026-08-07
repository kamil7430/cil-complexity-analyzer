# Architecture

> This is just a sketch -- it can change over time.

## Graphs

### System Overview

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

### Test Executor Flow

```mermaid
graph LR
  CF[C# Source File] .-> Co[Test\nCoordinator]
  TD[Test Inputs and\nExpected Output] .-> Co
  TS[Test Settings] .-> Co

  subgraph TE [Test Executor]
    Co --> SA
    SA[Static\nAnalyzer] --> Cr[Compiler]
    Cr --> CII[CIL Instruction\nInjector]
    CII --> Ex[Executor]
    Ex --> Co
  end

  Co .-> TR[Test Result]
  
  TR .-> EM[Error Message]
  TR .-> Correctness
  TR .-> Complexity
```

# Tech stack

* .NET 10
* Docker

## Test Framework / IDE Integration

* [Microsoft.Testing.Platform](https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-intro)

## Test Executor

* [Testcontainers](https://testcontainers.com/)
* [Mono Cecil](https://www.mono-project.com/docs/tools+libraries/libraries/Mono.Cecil/)
