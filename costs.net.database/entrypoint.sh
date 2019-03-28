#!/bin/sh
set -e

./restore.sh

cd /migration/flyway

./migrate.sh $POSTGRES_DATABASE_NAME $POSTGRES_HOST $POSTGRES_PORT $POSTGRESUSER $POSTGRESPASSWORD

