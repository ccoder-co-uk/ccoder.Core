FROM node:24-bookworm-slim AS frontend
WORKDIR /source/src/cCoder.Core.FrontEnd

COPY src/cCoder.Core.FrontEnd/package.json src/cCoder.Core.FrontEnd/package-lock.json ./
RUN npm ci

COPY src/cCoder.Core.FrontEnd/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY . .
COPY --from=frontend /source/src/cCoder.Core.FrontEnd/stage/ src/cCoder.Core.FrontEnd/stage/

RUN dotnet restore src/Apps/Web/Web.csproj --source https://api.nuget.org/v3/index.json --no-cache
RUN dotnet publish src/Apps/Web/Web.csproj --configuration Release --no-restore --output /app/publish /p:ShouldRunBuildScript=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish/ ./

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=10s --timeout=5s --retries=20 --start-period=20s \
    CMD curl --fail --silent http://localhost:8080/Health || exit 1

ENTRYPOINT ["dotnet", "Web.dll"]
