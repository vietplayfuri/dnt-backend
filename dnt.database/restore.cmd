set dbName=%1
if "%1" == "" (
    set dbName=costs
)
set hostName=%2
if "%2" == "" (
    set hostName=localhost
)
set port=%3
if "%3" == "" (
    set port=5432
)
set PGPASSWORD=postgres
psql -h %hostName% -p %port% -U postgres %dbName% < Initial_backup.sql