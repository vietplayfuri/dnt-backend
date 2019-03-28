ARG builderContainer
FROM $builderContainer as builder

RUN dotnet publish -c Release --output /opt/costs.net costs.net.host

# 2 Final Build
FROM        microsoft/dotnet:2.1.2-aspnetcore-runtime-alpine

RUN mkdir -p /adcosts/app

COPY --from=builder /opt/costs.net /adcosts/app

ARG         GIT_COMMIT
ARG         GIT_BRANCH
ENV         AppSettings__GitCommit ${GIT_COMMIT}
ENV         AppSettings__GitBranch ${GIT_BRANCH}

RUN chmod -R 777 /adcosts/app

EXPOSE 5000

WORKDIR /adcosts/app

ENTRYPOINT [ "dotnet", "/adcosts/app/costs.net.host.dll"]
