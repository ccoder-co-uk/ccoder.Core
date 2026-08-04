FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY . .

RUN dotnet restore src/Apps/Workflow/Workflow.csproj --source https://api.nuget.org/v3/index.json --no-cache
RUN dotnet publish src/Apps/Workflow/Workflow.csproj --configuration Release --no-restore --output /app/publish

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0

COPY --from=build /app/publish/ /home/site/wwwroot/
