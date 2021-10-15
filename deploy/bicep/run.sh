#!/bin/bash

set -e
set -x

while getopts ":p:s:g:l:a:" arg; do
    case $arg in
        p) ResourcePrefix=$OPTARG;;
        s) ResourcePostfix=$OPTARG;;
        g) ResourceGroupName=$OPTARG;;
        l) ResourceGroupLocation=$OPTARG;;
        a) ValidAudience=$OPTARG;;
    esac
done

usage() {
    script_name=`basename $0`
    echo "Please use ./$script_name -p resourcePrefix -s resourcePostfix -g resourceGroupName -l resourceGroupLocation -a validAudience"
}

if [ -z "$ResourcePrefix" ]; then
    usage
    exit 1
fi

if [ -z "$ResourcePostfix" ]; then
    usage
    exit 1
fi

if [ -z "$ResourceGroupName" ]; then
    usage
    exit 1
fi

if [ -z "$ResourceGroupLocation" ]; then
    usage
    exit 1
fi

if [ -z "$ValidAudience" ]; then
    usage
    exit 1
fi

resourcePrefix=$ResourcePrefix
resourcePostfix=$ResourcePostfix
resourceGroupName=$ResourceGroupName
resourceGroupLocation=$ResourceGroupLocation
validAudience=$ValidAudience
clientId=$(az ad app list --display-name "${resourcePrefix}visualassist${resourcePostfix}" --query "[0].appId" --output tsv)
tenantId=$(az account show --query tenantId --output tsv)
issuer="https://sts.windows.net/${tenantId}"

az group create --name $resourceGroupName --location $resourceGroupLocation
az deployment group create --template-file ./main.bicep --resource-group $resourceGroupName --parameters "resourcePrefix=${resourcePrefix}" --parameters "resourcePostfix=${resourcePostfix}" --parameters "resourceGroupLocation=${resourceGroupLocation}" --parameters "clientId=${clientId}" --parameters "allowedAudiences=[ '${validAudience}' ]" --parameters "issuer=${issuer}"
