# 1 Initial Build
FROM docker.adstreamdev.com/adcosts/costs.net.builder.base:1.0.0.5.1

RUN mkdir -p /usr/src/app
COPY . /usr/src/app/
WORKDIR /usr/src/app

RUN dotnet restore costs.net.sln
RUN dotnet build --configuration Release costs.net.sln

RUN chmod -R 777 /usr/src/app


COPY entrypoint.sh /opt/costs.net/entrypoint.sh
