FROM microsoft/dotnet:2.1.302-sdk

RUN apt-get update
RUN apt-get install -y postgresql-client default-jre

RUN mkdir -p /usr/src/app
COPY . /usr/src/app/
WORKDIR /usr/src/app

RUN dotnet restore costs.net.sln

RUN rm -r /usr/src/app
