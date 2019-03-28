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
flyway-4.2.0\flyway -url=jdbc:postgresql://%hostName%:%port%/%dbName% -user=postgres -password=postgres -locations=filesystem:%cd%\migration migrate