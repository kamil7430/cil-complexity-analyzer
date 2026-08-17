FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src/

COPY ["CilComplexityAnalyzer.ContainerWorker.csproj", "./"]
RUN dotnet restore "CilComplexityAnalyzer.ContainerWorker.csproj"

COPY ./ ./
RUN dotnet publish "CilComplexityAnalyzer.ContainerWorker.csproj" \
    -c Release \
    -o /build/ \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0-noble-chiseled AS final
WORKDIR /app/

COPY --from=build /build/ ./

ENV DOTNET_NOLOGO=true
ENV DOTNET_CLI_TELEMETRY_OPTOUT=true

ENTRYPOINT ["dotnet", "CilComplexityAnalyzer.ContainerWorker.dll"]