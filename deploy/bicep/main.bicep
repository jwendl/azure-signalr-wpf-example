param resourcePrefix string
param resourcePostfix string

param clientId string
param allowedAudiences array

param resourceGroupLocation string = resourceGroup().location

resource userManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: '${toLower(resourcePrefix)}umi${toLower(resourcePostfix)}'
  location: resourceGroupLocation
}

resource keyVault 'Microsoft.KeyVault/vaults@2021-06-01-preview' = {
  name: '${toLower(resourcePrefix)}akv${toLower(resourcePostfix)}'
  location: resourceGroupLocation
  properties: {
    accessPolicies: [
      {
        objectId: userManagedIdentity.properties.principalId
        tenantId: subscription().tenantId
        permissions: {
          secrets: [ 
            'get'
           ]
        }
      }
    ]
    sku: {
      name: 'standard'
      family: 'A'
    }
    tenantId: subscription().tenantId
  }
  resource keyVaultSecret 'secrets' = {
    name: 'signalrkey'
    properties: {
      value: listKeys('${functionApplication.id}/host/default/', functionApplication.apiVersion).systemkeys.signalr_extension
    }
  }
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
          value: 'Endpoint=https://${publishService.properties.hostName};AuthType=aad;Version=1.0;'
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
    type: 'SystemAssigned, UserAssigned'
    userAssignedIdentities: {
      '${userManagedIdentity.id}': {}
    }
  }
  resource functionAuthSettings 'config' = {
    name: 'authsettingsV2'
    properties: {
      globalValidation: {
        requireAuthentication: true
        unauthenticatedClientAction: 'Return401'
      }
      httpSettings: {
        requireHttps: true
      }
      identityProviders: {
        azureActiveDirectory: {
          enabled: true
          registration: {
            clientId: clientId
            openIdIssuer: 'https://sts.windows.net/${subscription().tenantId}/'
          }
          validation: {
            allowedAudiences: allowedAudiences
          }
        }
      }
      platform: {
        enabled: true
      }
    }
  }  
}

resource aadRoleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroup().name, 'owner')
  properties: {
    principalId: functionApplication.identity.principalId
    principalType: 'ServicePrincipal'
    // Found using az role definition list --output table --query '[].{name:name, roleName:roleName, description:description}' | grep SignalR
    roleDefinitionId: '${subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '420fcaa2-552c-430f-98ca-3264be4806c7')}'
  }
}

resource serviceOwnerRoleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroup().name, 'owner')
  properties: {
    principalId: functionApplication.identity.principalId
    principalType: 'ServicePrincipal'
    // Found using az role definition list --output table --query '[].{name:name, roleName:roleName, description:description}' | grep SignalR
    roleDefinitionId: '${subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7e4f1700-ea5a-4f59-8f37-079cfe29dce3')}'
  }
}
