#!/bin/sh
set -e

AppSettings__FlywayJdbcUrl=${AppSettings__FlywayJdbcUrl:-jdbc:postgresql://${DatabaseHost}:${DatabasePort}/costs}

cd /adcosts/app

# Run flyway migrations at startup. This may cause problems depending on upgrade strategy.
sleep 3
./costs.net.database/flyway-4.2.0/flyway -url=${AppSettings__FlywayJdbcUrl} -locations=filesystem:./costs.net.database/migration  migrate

dotnet costs.net.host.dll
