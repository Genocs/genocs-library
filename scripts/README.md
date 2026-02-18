# Description

This folder contains useful script you can use to automate tasks. The scripts are bash or powershell scripts.

## How to obtain the list of repository on Azure DevOps

[Repository List](https://docs.microsoft.com/en-us/rest/api/azure/devops/git/repositories/list?view=azure-devops-rest-5.0)

```api
https://docs.microsoft.com/en-us/rest/api/azure/devops/git/repositories/list?view=azure-devops-rest-5.0
https://dev.azure.com/genocs/_apis/git/repositories?api-version=5.0
```
### List of repositories


### Window Services

Following some commands that you can use to handle windows services

``` cmd
echo To query the available windows services 
sc query

echo To remove the registration  
SC DELETE [service_name]
```


## How to clone the repositories at once

1. Open a bash shell
2. Run the following command

```bash
./git-clone-all.sh
```

3. Checkout develop branch

```bash
./git-pull-all.sh
```
