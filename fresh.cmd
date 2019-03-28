set dbName=%1
set hostName=%2
set port=%3
if "%1" == "" (
    set dbName=costs
)
if "%2" == "" (
    set hostName=localhost
)
if "%3" == "" (
    set port=5432
)
set PGPASSWORD=postgres
psql -h %hostName% -p %port% -U postgres -c "SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = '%dbName%'"
psql -h %hostName% -p %port% -U postgres -c "drop database if exists %dbName%" postgres
psql -h %hostName% -p %port% -U postgres -c "create database %dbName%" postgres
cd costs.net.database
cmd.exe /C "restore_migrate.cmd %dbName% %hostName% %port%"

curl -XPOST localhost:5000/v1/admin/bootstrapacl?$id$=4ef31ce1766ec96769b399c0 -d "{ "createDb": "true", "deleteDb": "true" }"  -H "Content-Type: application/json"
curl -XPOST localhost:5000/v1/admin/repopulateElastic?$id$=4ef31ce1766ec96769b399c0 -d "{}"  -H "Content-Type: application/json"

cd ..