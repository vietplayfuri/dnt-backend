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

flyway-4.2.0/flyway -url=jdbc:postgresql://$hostName:$port/$dbName -user=postgres -password=postgres -locations=filesystem:$PWD/migration migrate
