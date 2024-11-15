#!/bin/bash

group="beckshome-container-apps-rg"
app="dotnet-lucene-search"

az containerapp delete -g $group -n $app
# echo
# az group delete --name $group