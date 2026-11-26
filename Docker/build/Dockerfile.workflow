FROM mcr.microsoft.com/dotnet/aspnet:10.0

ARG VERSION
ARG COMMIT

LABEL org.opencontainers.image.title="cCoder.Core Workflow"
LABEL org.opencontainers.image.version="${VERSION}"
LABEL org.opencontainers.image.revision="${COMMIT}"
LABEL org.opencontainers.image.source="https://github.com/ccoder-co-uk/cCoder.Core"

WORKDIR /app
COPY Workflow/ ./

ENV ASPNETCORE_URLS=http://+:800;https://+:4433
EXPOSE 800 4433
ENTRYPOINT ["dotnet", "Workflow.dll"]
