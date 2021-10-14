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

az ad app create --display-name "${resourcePrefix}client${resourcePostfix}" --available-to-other-tenants true --native-app
az ad app create --display-name "${resourcePrefix}server${resourcePostfix}" --available-to-other-tenants true

az ad app list --display-name "${resourcePrefix}client${resourcePostfix}" --query [].appId
az ad app list --display-name "${resourcePrefix}server${resourcePostfix}" --query [].appId
