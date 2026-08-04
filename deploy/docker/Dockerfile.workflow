FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0

ARG VERSION
ARG COMMIT

LABEL org.opencontainers.image.title="cCoder.Core Workflow"
LABEL org.opencontainers.image.version="${VERSION}"
LABEL org.opencontainers.image.revision="${COMMIT}"
LABEL org.opencontainers.image.source="https://github.com/ccoder-co-uk/cCoder.Core"

COPY Workflow/ /home/site/wwwroot/

ENV ASPNETCORE_URLS=http://+:800;https://+:4433
EXPOSE 800 4433
