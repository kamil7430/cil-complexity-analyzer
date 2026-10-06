FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src/

WORKDIR /src/CilComplexityAnalyzer.ContainerWorkerUtils/
COPY ./CilComplexityAnalyzer.ContainerWorkerUtils/ ./

WORKDIR /src/CilComplexityAnalyzer.Contract/
COPY ./CilComplexityAnalyzer.Contract/ ./

WORKDIR /src/CilComplexityAnalyzer.ContainerWorker/
COPY ./CilComplexityAnalyzer.ContainerWorker/CilComplexityAnalyzer.ContainerWorker.csproj ./

WORKDIR /src/
RUN dotnet restore ./CilComplexityAnalyzer.ContainerWorker/CilComplexityAnalyzer.ContainerWorker.csproj

COPY ./CilComplexityAnalyzer.ContainerWorker ./CilComplexityAnalyzer.ContainerWorker
RUN dotnet publish ./CilComplexityAnalyzer.ContainerWorker/CilComplexityAnalyzer.ContainerWorker.csproj \
    -c Release \
    -o /build/ \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled AS final

WORKDIR /app/results/
WORKDIR /app/

COPY --from=build /build/ ./

ENV DOTNET_NOLOGO=true
ENV DOTNET_CLI_TELEMETRY_OPTOUT=true

ENTRYPOINT ["dotnet", "CilComplexityAnalyzer.ContainerWorker.dll"]