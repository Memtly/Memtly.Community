FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG TARGETARCH
ARG BUILD_CONFIGURATION=Release

RUN apt-get update && \
    apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_22.x | bash - && \
    apt-get install -y nodejs && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Memtly.Community/Memtly.Community.csproj", "Memtly.Community/Memtly.Community/"]
COPY ["Memtly.Core/Memtly.Core/Memtly.Core.csproj", "Memtly.Core/Memtly.Core/"]

RUN --mount=type=secret,id=nuget_token \
    export NUGET_TOKEN=$(cat /run/secrets/nuget_token | tr -d '[:space:]') && \
    echo "Token length: ${#NUGET_TOKEN}" && \
    echo "Token starts with: ${NUGET_TOKEN:0:4}" && \
    dotnet restore -a $TARGETARCH "./Memtly.Community/Memtly.Community/Memtly.Community.csproj"

COPY . .
WORKDIR "/src/Memtly.Community"
RUN dotnet build "./Memtly.Community.csproj" -a $TARGETARCH -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Memtly.Community.csproj" -a $TARGETARCH -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN mkdir -p /app/config
ENTRYPOINT ["dotnet", "Memtly.Community.dll"]ENTRYPOINT ["dotnet", "Memtly.Community.dll"]
