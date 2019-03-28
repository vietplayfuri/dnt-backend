# 1 Initial Build
ARG builderContainer
FROM $builderContainer as builder

RUN dotnet publish -c Release --output /opt/costs.net costs.net.scheduler.host

# 2 Final Build
FROM        microsoft/dotnet:2.1.2-aspnetcore-runtime-alpine

RUN mkdir -p /adcosts/scheduler

COPY --from=builder /opt/costs.net /adcosts/scheduler

ARG         GIT_COMMIT
ARG         GIT_BRANCH
ENV         AppSettings__GitCommit ${GIT_COMMIT}
ENV         AppSettings__GitBranch ${GIT_BRANCH}

WORKDIR /adcosts/scheduler

ENTRYPOINT [ "dotnet", "costs.net.scheduler.host.dll"]
