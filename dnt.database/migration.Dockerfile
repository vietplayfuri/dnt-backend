FROM        alpine:3.7

RUN         apk update
RUN         apk upgrade
RUN         apk add bash
RUN         apk add postgresql-client
RUN         apk add openjdk8-jre
RUN         apk add --no-cache curl

RUN         mkdir -p migration/flyway

COPY        migrate.dev.sh /migration/flyway/migrate.sh
COPY        /flyway-4.2.0 /migration/flyway/flyway-4.2.0
COPY        /migration /migration/flyway/migration
COPY        entrypoint.sh /entrypoint.sh
COPY        restore.dev.sh /restore.sh
COPY        Initial_backup.sql /Initial_backup.sql

CMD         [ "./entrypoint.sh" ]
