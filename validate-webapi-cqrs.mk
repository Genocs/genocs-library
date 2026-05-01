validate-webapi-cqrs-build:
	dotnet build src/Genocs.WebApi.CQRS/Genocs.WebApi.CQRS.csproj -f net10.0 -c Debug --nologo -warnaserror -p:BuildProjectReferences=false

validate-webapi-cqrs-tests:
	dotnet test src/tests/Genocs.WebApi.CQRS.UnitTests/Genocs.WebApi.CQRS.UnitTests.csproj -c Debug --nologo

validate-webapi-cqrs: validate-webapi-cqrs-build validate-webapi-cqrs-tests
