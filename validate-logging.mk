test-logging:
	dotnet test src/tests/Genocs.Logging.UnitTests/Genocs.Logging.UnitTests.csproj -c Debug --nologo --no-build

validate-logging:
	dotnet build src/Genocs.Logging/Genocs.Logging.csproj -c Debug --nologo
	dotnet test src/tests/Genocs.Logging.UnitTests/Genocs.Logging.UnitTests.csproj -c Debug --nologo --no-build
