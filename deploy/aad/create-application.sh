#!/bin/bash

set -e
set -x

while getopts ":p:s:g:l:" arg; do
    case $arg in
        p) ResourcePrefix=$OPTARG;;
        s) ResourcePostfix=$OPTARG;;
    esac
done

usage() {
    script_name=`basename $0`
    echo "Please use ./$script_name -p resourcePrefix -s resourcePostfix"
}

if [ -z "$ResourcePrefix" ]; then
    usage
    exit 1
fi

if [ -z "$ResourcePostfix" ]; then
    usage
    exit 1
fi

resourcePrefix=$ResourcePrefix
resourcePostfix=$ResourcePostfix

az ad app create --display-name "${resourcePrefix}visualassist${resourcePostfix}" --available-to-other-tenants true --native-app true

appId=$(az ad app list --display-name "${resourcePrefix}visualassist${resourcePostfix}" --query "[0].appId" --output tsv)
az ad app credential reset --id $appId --append

az ad app list --display-name "${resourcePrefix}visualassist${resourcePostfix}" --query [].appId
az account show --query tenantId
