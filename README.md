# Azure SignalR + Functions + WPF Example

An example of using Azure Signal R service to send data from one endpoint to several other endpoints.

## Intent

The intent of this repository is to serve as an example of how to set this up in a serverless fashion. If you run into issues setting it up please post an issue on the repo. And if you want to add a PR - all contributions are welcomed.

## Steps to Reproduce

1. Clone the repository
1. Go into the [repo-root-folder]/deploy/aad 
1. Run the following in a bash shell ``` ./create-application.sh -p [prefix] -s [postfix] ``` where [prefix] is the resource prefix and [postfix] is the resource postfix
1. After it finishes, record the output as we will need some of the values later
1. Next we will create an Azure AD Application registration using the steps below
    1. Go to the [Azure Portal](https://portal.azure.com/) and click on Azure Active Directory
    1. Inside Azure Active Directory, go into App Registrations
    1. Find the application registration we just created, will be named something like [prefix]visualassist[postfix]
    1. Inside the application registration screen select Authentication on the left
    1. Inside the Authentication tab, click on the button "Add a platform"
    1. When the new blade shows up, select Mobile and desktop applications
    1. Inside there, add a custom redirect URI of http://localhost/
    1. Save that data and it should now show up
    1. On the left select "Expose and API"
    1. From that screen, select the "set" button and it should name your application api://[guid] where [guid] is some unique identifier
    1. From that point, click on the "Add a scope" button
    1. Inside that window, enter "publish" as the scope name
    1. Go ahead and fill the required fields with anything you want and then ensure the state is Enabled
    1. After that click on Add scope
1. Navigate into the [repo-root-folder]/deploy/bicep folder
1. Run the following shell script in bash ``` ./deploy-azure.sh -p [prefix] -s [postfix] -g [resource-group-name] -l [location] -a [valid-audience] where the values are in the [table below](#bicep-variables)
1. Navigate into the [repo-root-folder]/deploy/application folder
1. Run the following shell script in bash ``` ./deploy-application.sh -p [prefix] -s [postfix] ``` where [prefix] is the resource prefix and [postfix] is the resource postfix
1. Double check that all the resources are deployed to Azure, it should be a SignalR Service, Function Application and Application Insights instance
1. Open up the [repo-root-folder]/src/ui/VisualAssist.UserInterface.sln solution file
1. Once inside there, go into the App.config file
1. Change the values to be what you need in your environment description in the [table below](#ui-variables)

## Variables

### Bicep Variables

| Variable            | Description                                                        | Example                                    |
| ------------------- | ------------------------------------------------------------------ | ------------------------------------------ |
| prefix              | The resource prefix                                                | jw                                         |
| postfix             | The resource postfix                                               | dev                                        |
| resource-group-name | The resource group name for your Azure resources                   | TestGroup                                  |
| location            | The location, I used westus2                                       | westus2                                    |
| valid-audience      | The Azure AD application audience, example api://[azure-ad-app-id] | api://89c7d6ac-586a-4dc8-bbc3-1c8952c2f757 |

### UI Variables

| Variable                 | Description                                  | Example                                            |
| ------------------------ | -------------------------------------------- | -------------------------------------------------- |
| ClientId                 | The Azure AD Client ID (or App ID)           | 89c7d6ac-586a-4dc8-bbc3-1c8952c2f757               |
| Tenant                   | Your Azure AD Tenant (az account show)       | 72f988bf-86f1-41af-91ab-2d7cd011db47               |
| Instance                 | The instance your Azure AD application is in | https://login.microsoftonline.com/                 |
| Scope                    | The scope for your application               | api://89c7d6ac-586a-4dc8-bbc3-1c8952c2f757/publish |
| SignalRNegotiateEndpoint | The URI to your function application         | https://jweafadev.azurewebsites.net/api            |
| FunctionAppUrl           | The root URI for your function app           | https://jweafadev.azurewebsites.net/               |
