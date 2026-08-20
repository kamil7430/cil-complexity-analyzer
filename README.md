# CIL Complexity Analyzer

A tool for deterministic algorithm complexity analysis in C# achieved through CIL bytecode instrumentation and
instruction counting. By modifying the compiled assembly before execution, it provides fair, hardware-independent
performance evaluation unaffected by CPU speed, system load, or clock precision. It is designed primarily for automated
grading and benchmarking of student code submissions.

## Requirements
* Docker
* .NET 10

## Quick Start

TODO *(probably when framework arrives)*

## Related

- [kamil7430/cil-complexity-analyzer-container-worker DockerHub repository](https://hub.docker.com/repository/docker/kamil7430/cil-complexity-analyzer-container-worker/general)

## Further Reading

- [Architecture documentation](./docs/ARCHITECTURE.md)
- Our Engineering Thesis *(TBA;>)*