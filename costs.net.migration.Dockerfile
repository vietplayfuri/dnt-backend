FROM        alpine:3.7

RUN         apk update
RUN         apk upgrade
RUN         apk add bash
RUN         apk add postgresql-client
RUN         apk add openjdk8-jre
RUN         apk add --no-cache curl

RUN         mkdir -p /opt/costs.net/costs.net.database
COPY        costs.net.database /opt/costs.net/costs.net.database
WORKDIR     /opt/costs.net/costs.net.database
RUN         rm entrypoint.sh
COPY        costs.net.database/entrypoint.migrate.sh /opt/costs.net/costs.net.database/entrypoint.sh
RUN         chmod 777 -R /opt/costs.net/costs.net.database && chown root -R /opt/costs.net/costs.net.database
ENTRYPOINT  [ "./entrypoint.sh" ]
