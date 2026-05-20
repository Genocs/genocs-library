validate-messaging-build:
	dotnet build src/Genocs.Messaging/Genocs.Messaging.csproj -f net10.0 -c Debug --nologo -warnaserror
	dotnet build src/Genocs.Messaging.Outbox/Genocs.Messaging.Outbox.csproj -f net10.0 -c Debug --nologo -warnaserror
	dotnet build src/Genocs.Messaging.AzureServiceBus/Genocs.Messaging.AzureServiceBus.csproj -f net10.0 -c Debug --nologo -warnaserror
	dotnet build src/Genocs.Messaging.RabbitMQ/Genocs.Messaging.RabbitMQ.csproj -f net10.0 -c Debug --nologo -warnaserror

validate-messaging-tests:
	dotnet test src/tests/Genocs.Messaging.UnitTests/Genocs.Messaging.UnitTests.csproj -c Debug --nologo
	dotnet test src/tests/Genocs.Messaging.RabbitMQ.UnitTests/Genocs.Messaging.RabbitMQ.UnitTests.csproj -c Debug --nologo

validate-messaging: validate-messaging-build validate-messaging-tests