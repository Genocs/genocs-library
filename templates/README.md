Genocs Library Template
===

This folder contains the templates to be used to implement an entire solution.


``` cmd
# Pack template with nuget
nuget pack WebApiTemplate.nuspec -NoDefaultExcludes -OutputDirectory .\nuget


# Get the template list 
dotnet new list

# Install with local nuget package
dotnet new install .\nuget\Genocs.ServiceTemplate.5.0.0.nupkg

# Uninstall local nuget package
dotnet new --uninstall .\nuget\Genocs.ServiceTemplate.5.0.0.nupkg

# Some uninstall commands
dotnet new uninstall Genocs.ServiceTemplate
dotnet new uninstall Genocs.CleanArchitecture
dotnet new uninstall Genocs.CleanArchitectureTemplate

dotnet new uninstall  Genocs.MicroserviceTemplate

# Create a new WebApi called 'CommanyName.ServiceName' inside the folder folder
dotnet new gnx-webapi -n CommanyName.ServiceName -o folder
```