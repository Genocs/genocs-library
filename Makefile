build:
	dotnet build
start-demo:
	dotnet run --project ./src/demo/WebApi/Host.csproj -c Debug
nuget:
	nuget pack -NoDefaultExcludes -OutputDirectory nupkgs
publish:
	dotnet publish --os linux --arch x64 -c Release --self-contained
publish-to-hub:
	dotnet publish --os linux --arch x64 -c Release -p:ContainerRegistry=docker.io -p:ContainerImageName=genocs/telegram_integration-webapi --self-contained
tp: # terraform plan
	cd infrastructure/terraform && terraform plan
ta: # terraform apply
	cd infrastructure/terraform && terraform apply
td: # terraform destroy
	cd infrastructure/terraform && terraform destroy
dcu: # docker-compose up : webapi + postgresql
	docker compose -f infrastructure/docker/demo/docker-compose.yml up -d
dcd: # docker-compose down : webapi + postgresql
	docker compose -f infrastructure/docker/demo/docker-compose.yml down
fds: # force rededeploy aws ecs service
	aws ecs update-service --force-new-deployment --service dotnet-webapi --cluster genocs
gw: # git docker workflow to push docker image to the repository based on the main branch
	@echo triggering github workflow to push docker image to container
	@echo ensure that you have the gh-cli installed and authenticated.
	gh workflow run dockerhub-publish -f push_to_docker=true

d-build: # docker build images
	@echo building docker images
	@echo you should have docker installed and running
	bash ./src/apps/scripts/build-images.sh
d-push: # docker tag and push images
	@echo tag images as latest and publish
	bash ./src/apps/scripts/push-images.sh