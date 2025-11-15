// Think of this as Infrastructure as Code (IaC) for deploying resources
// Bicep file to create a storage account and a web app in Azure

// Get the resource group location
@description('Location for all resources.')
param location string = 'italia north'

@description('The name of you Web Site.')
param webSiteName string = 'demo-website'

@description('A unique string to ensure resource names are globally unique.')
param uniqueString string = '{uniqueString(resourceGroup().id)}'

// Generate a unique storage account name
@description('The name of the Storage Account.')
param storageAccountName string = 'helloworldstorage${uniqueString}'

// Create a storage account
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
  }
}

// Create an App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: 'asp-${uniqueString}'
  location: location
  sku: {
    name: 'F1'
    tier: 'Free'
  }
}

// Create a web app
resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: webSiteName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'STORAGE_ACCOUNT_NAME'
          value: storageAccount.name
        }
      ]
    }
  }
}
