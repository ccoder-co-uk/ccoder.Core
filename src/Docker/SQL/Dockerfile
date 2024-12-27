FROM mcr.microsoft.com/mssql/server:2022-latest AS sql

ENV SA_PASSWORD=test123!!
ENV ACCEPT_EULA=y

USER root

# This is provided by the Pipeline run, it is the build result of a hosted services run
COPY Docker/SQL/Init-DB.sh /tmp/Init-DB.sh

COPY Docker/HostedServices /tmp/hostedServices

# Override the app settings
COPY Docker/SQL/HostedServices-appsettings.json /tmp/hostedServices/appsettings.json

RUN ["/bin/bash", "/tmp/Init-DB.sh"]