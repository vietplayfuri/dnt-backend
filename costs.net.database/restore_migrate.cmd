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
cmd.exe /C "%cd%\restore.cmd %dbName% %hostName% %port%"
cmd.exe /C "%cd%\migrate.cmd %dbName% %hostName% %port%"