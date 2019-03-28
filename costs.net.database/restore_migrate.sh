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

sh -c "$PWD/restore.sh $dbName $hostName $port"
sh -c "$PWD/migrate.sh $dbName $hostName $port"
