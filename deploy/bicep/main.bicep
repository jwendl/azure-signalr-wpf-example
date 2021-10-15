param resourcePrefix string
param resourcePostfix string

param clientId string
param issuer string
param allowedAudiences array

param resourceGroupLocation string = resourceGroup().location

resource userManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: '${toLower(resourcePrefix)}umi${toLower(resourcePostfix)}'
  location: resourceGroupLocation
}

resource publishService 'Microsoft.SignalRService/signalR@2021-06-01-preview' = {
  name: '${toLower(resourcePrefix)}asr${toLower(resourcePostfix)}'
  location: resourceGroupLocation
  sku: {
    name: 'Standard_S1'
    capacity: 5
    tier: 'Standard'
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userManagedIdentity.id}': {}
    }
  }
}

resource appStorage 'Microsoft.Storage/storageAccounts@2021-04-01' = {
  name: '${toLower(resourcePrefix)}aas${toLower(resourcePostfix)}'
  location: resourceGroupLocation
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
  }
}

resource functionAppInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${toLower(resourcePrefix)}aai${toLower(resourcePostfix)}'
  location: resourceGroupLocation
  kind: 'web'
  properties: {
    Application_Type: 'web'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

resource functionServerFarm 'Microsoft.Web/serverfarms@2021-01-15' = {
  name: '${toLower(resourcePrefix)}aff${toLower(resourcePostfix)}'
  location: resourceGroupLocation
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
}

resource functionApplication 'Microsoft.Web/sites@2021-01-15' = {
  name: '${toLower(resourcePrefix)}afa${toLower(resourcePostfix)}'
  location: resourceGroupLocation
  kind: 'functionapp'
  properties: {
    httpsOnly: true
    serverFarmId: functionServerFarm.id
    clientAffinityEnabled: true
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${appStorage.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(appStorage.id, appStorage.apiVersion).keys[0].value}'
        }
        {
          name: 'AzureSignalRConnectionString'
          value: 'Endpoint=https://${publishService.properties.hostName}.service.signalr.net;AuthType=aad;ClientId=${userManagedIdentity.properties.clientId};Version=1.0;'
        }
        {
          'name': 'FUNCTIONS_EXTENSION_VERSION'
          'value': '~3'
        }
        {
          'name': 'FUNCTIONS_WORKER_RUNTIME'
          'value': 'dotnet'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${appStorage.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(appStorage.id, appStorage.apiVersion).keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: 'visualassistapp'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: functionAppInsights.properties.InstrumentationKey
        }
      ]
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userManagedIdentity.id}': {}
    }
  }
  resource functionAuthSettings 'config' = {
    name: 'authsettings'
    properties: {
      allowedAudiences: allowedAudiences
      clientId: clientId
      issuer: issuer
      unauthenticatedClientAction: 'RedirectToLoginPage'
      validateIssuer: true
    }
  }  
}
