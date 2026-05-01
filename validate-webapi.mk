validate-webapi-build:
	dotnet build src/Genocs.WebApi/Genocs.WebApi.csproj -f net10.0 -c Debug --nologo -warnaserror

validate-webapi-tests:
	dotnet test src/tests/Genocs.WebApi.UnitTests/Genocs.WebApi.UnitTests.csproj -c Debug --nologo

validate-webapi: validate-webapi-build validate-webapi-tests
