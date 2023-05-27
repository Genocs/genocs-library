This folder contains the template to be used to implement a new service.

```sh

# Get the template list
dotnet new list

# pack the template with nuget (not dotnet)
nuget pack -NoDefaultExcludes -OutputDirectory ./nuget

dotnet new list


dotnet new install ./nuget/Genocs.ServiceTemplate.5.0.0.nupkg

dotnet new --uninstall ./nuget/Genocs.ServiceTemplate.5.0.0.nupkg

dotnet new uninstall Genocs.ServiceTemplate

# create a new service called 'Acme.Orders' inside orders folder
dotnet new gnx-webapi -n Acme.Orders -o orders
```
