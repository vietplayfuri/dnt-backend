dbName=$1
hostName=$2
port=$3
if [ "${1}" == "" ]
then
    dbName=costs
fi
if [ "${2}" == "" ]
then
    hostName=localhost
fi
if [ "${3}" == "" ]
then
    port=5432
fi

PGPASSWORD=postgres psql -h $hostName -p $port -U postgres $dbName < Initial_backup.sql
