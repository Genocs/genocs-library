validate-webapi-openapi-build:
	dotnet build src/Genocs.WebApi.OpenApi/Genocs.WebApi.OpenApi.csproj -f net10.0 -c Debug --nologo -warnaserror -p:BuildProjectReferences=false

validate-webapi-openapi-tests:
	dotnet test src/tests/Genocs.WebApi.OpenApi.UnitTests/Genocs.WebApi.OpenApi.UnitTests.csproj -c Debug --nologo

validate-webapi-openapi: validate-webapi-openapi-build validate-webapi-openapi-tests
