release?=1.9.0
GIT_COMMIT?=unknown
GIT_BRANCH?=unknown
container=docker.adstreamdev.com/adcosts/costs.net
coreContainerName=${container}:${release}
builderContainer=${container}.builder
builderContainerName=${builderContainer}:${release}
schedulerContainer=${container}.scheduler
schedulerContainerName=${schedulerContainer}:${release}
migrationContainer=${container}.migration
migrationContainerName=${migrationContainer}:${release}

default: buildBuilder build runTests

buildBuilder:
		docker build -t ${builderContainerName} -f costs.net.builder.Dockerfile .

buildCore:
		docker build -t ${coreContainerName} -f costs.net.Dockerfile --build-arg builderContainer=${builderContainerName} --build-arg GIT_COMMIT=${GIT_COMMIT} --build-arg GIT_BRANCH=${GIT_BRANCH} .

buildScheduler:
		docker build -t ${schedulerContainerName} -f costs.scheduler.Dockerfile --build-arg builderContainer=${builderContainerName} --build-arg GIT_COMMIT=${GIT_COMMIT} --build-arg GIT_BRANCH=${GIT_BRANCH} .

buildMigration:
		docker build -t ${migrationContainerName} -f costs.net.migration.Dockerfile .

runTests: schedularTests apiTests pluginTests pluginTests messagingTests coreTests
		docker stop postgres_costs || true && docker rm postgres_costs || true
		docker run -d --name postgres_costs postgres:9.6
		docker run -e "Data__DatabaseConnection__ConnectionString=Host=postgres;Port=5432;Database=costs_test;Pooling=true;User Id=postgres;Password=postgres;" \
		-e "Data__DatabaseConnection__ConnectionStringAdmin=Host=postgres;Port=5432;Database=postgres;Pooling=true;User Id=postgres;Password=postgres;" \
		-e "AppSettings__DbRestoreProcess=bash" \
		-e "AppSettings__DbRestoreFilePath={workDir}..\\..\\..\\..\\costs.net.database\\restore_migrate.sh" \
		-e "AppSettings__DbRestoreArguments=-c \"{filePath} {dbName} {hostName}\"" \
		-i --link postgres_costs:postgres ${builderContainerName} /bin/bash -c "\
		dotnet test --configuration Release --no-build /usr/src/app/costs.net.integration.tests/costs.net.integration.tests.csproj"

pushCore:
		docker push ${coreContainerName}

pushMigration:
		docker push ${migrationContainerName}

messagingTests:
		docker run -i ${builderContainerName} /bin/bash -c "dotnet test --configuration Release --no-build /usr/src/app/costs.net.messaging.test/costs.net.messaging.test.csproj"

pluginTests:
		docker run -i ${builderContainerName} /bin/bash -c "dotnet test --configuration Release --no-build /usr/src/app/costs.net.plugins.tests/costs.net.plugins.tests.csproj"

apiTests:
		docker run -i ${builderContainerName} /bin/bash -c "dotnet test --configuration Release --no-build /usr/src/app/costs.net.api.tests/costs.net.api.tests.csproj"

schedularTests:
		docker run -i ${builderContainerName} /bin/bash -c "dotnet test --configuration Release --no-build /usr/src/app/costs.net.scheduler.tests/costs.net.scheduler.tests.csproj"

coreTests:
		docker run -i ${builderContainerName} /bin/bash -c "dotnet test --configuration Release --no-build /usr/src/app/costs.net.core.tests/costs.net.core.tests.csproj"

pushScheduler:
		docker push ${schedulerContainerName}

build: buildCore buildScheduler buildMigration

push: parallelPush

parallelPush: pushCore pushScheduler pushMigration
