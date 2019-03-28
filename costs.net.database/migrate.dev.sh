#!/bin/sh

set -e

flyway-4.2.0/flyway -url=jdbc:postgresql://$POSTGRES_HOST:$POSTGRES_PORT/$POSTGRES_DATABASE_NAME -user=$POSTGRESUSER -password=$POSTGRESPASSWORD -locations=filesystem:$PWD/migration migrate
