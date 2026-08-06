# CIL Complexity Analyzer

## About
**CIL Complexity Analyzer** is a tool for deterministic algorithm complexity analysis in C# achieved through CIL bytecode instrumentation and instruction counting. By modifying the compiled assembly before execution, it provides fair, hardware-independent performance evaluation unaffected by CPU speed, system load, or clock precision. It is designed primarily for automated grading and benchmarking of student code submissions.

## Requirements
* **Docker Compose** (recommended for isolated execution)
* **.NET 10 SDK** or newer (for local development and building)

## How It Works

### Inputs
1. **Source File (`.cs`)**: The student's C# code file containing the algorithm implementation.
2. **Estimation Strategy**: Configuration defining how complexity is calculated (e.g., basic block counting, specific IL instruction weights, or loop/branch tracking).

### Processing & Instrumentation
1. The source `.cs` file is compiled into a `.dll` assembly.
2. **Mono.Cecil** instruments the byte code by injecting execution counter calls into strategic CIL locations.
3. The instrumented assembly is executed inside a sandbox environment against standard test inputs.

### Output
The execution yields a structured report containing:
* **Metric Value**: Total executed CIL instruction count (or weighted operation score).
* **Evaluation Verdict**: Pass/Fail assessment based on a comparison against baseline threshold values (e.g., within 2x of the reference solution score).
