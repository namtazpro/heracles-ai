#!/bin/bash -e
output_blob=$OUTPUT_LOG_NAME
echo "<h2>Heracles Infrastructure </h2>" >> $output_blob
echo "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~" 
echo         Deploying Heracles Infrastructure 
echo "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
echo ---Global Variables
echo "HERACLES_ALIAS: $HERACLES_ALIAS"
echo "CHANIA_ALIAS: $CHANIA_ALIAS"
echo "HERACLES_LOCATION: $HERACLES_LOCATION"
echo

# set local variables
# Derive as many variables as possible
applicationName="${HERACLES_ALIAS}"
resourceGroupName="${applicationName}-rg"
storageAccountName=${applicationName}$RANDOM
functionAppName="${applicationName}-gen-func"
acrName="${applicationName}acr"
planName="${applicationName}-plan"

echo ---Derived Variables
echo "Application Name: $applicationName"
echo "Resource Group Name: $resourceGroupName"
echo "Storage Account Name: $storageAccountName"
echo "Function App Name: $functionAppName"
echo "ACR Name: $acrName"

echo "Creating resource group $resourceGroupName in $HERACLES_LOCATION"
az group create -l "$HERACLES_LOCATION" --n "$resourceGroupName" --tags  HeraclesInstance=$HERACLES_INSTANCE Application=heracles MicrososerviceName=heracles MicroserviceID=$applicationName PendingDelete=true >> $output_blob
echo "<p>Resource Group: $resourceGroupName</p>" >> $output_blob

echo "Creating storage account $storageAccountName in $resourceGroupName"
az storage account create \
 --name $storageAccountName \
 --location $HERACLES_LOCATION \
 --resource-group $resourceGroupName \
 --sku Standard_LRS -o none
echo "<p>Storage Account: $storageAccountName</p>" >> $output_blob
connectionString=$(az storage account show-connection-string -n $storageAccountName -g $resourceGroupName --query connectionString -o tsv)
export AZURE_STORAGE_CONNECTION_STRING="$connectionString"
echo "<p>Storage Connection String: $connectionString</p>" >> $output_blob

echo "Creating azure container registry $acrName in $resourceGroupName"
az acr create -l $HERACLES_LOCATION --sku basic -n $acrName --admin-enabled -g $resourceGroupName -o none
acrUser=$(az acr credential show -n $acrName --query username -o tsv)
acrPassword=$(az acr credential show -n $acrName --query passwords[0].value -o tsv)
echo "::set-output name=acr_password::$acrpassword"
echo "<p>ACR: $acrName</p>" >> $output_blob
echo "<p>ACR User Name: $acrUser</p>" >> $output_blob 
echo "<p>ACR Password: $acrPassword</p>" >> $output_blob 

echo "Creating serverless function app $functionAppName in $resourceGroupName"
az functionapp plan create --resource-group $resourceGroupName --name $planName --location $HERACLES_LOCATION --number-of-workers 1 --sku EP1 --is-linux
echo "<p>App Service Plan: $planName</p>" >> $output_blob
az functionapp create \
 --name $functionAppName \
  --storage-account $storageAccountName \
  --plan $planName \
  --resource-group $resourceGroupName \
  --functions-version 3 \
  --docker-registry-server-user $acrUser \
  --docker-registry-server-password $acrPassword \
  --runtime dotnet -o none
echo "<p>Function App: $functionAppName</p>" >> $output_blob

# We'll use this storage account to hold external configuration for users and sessions in user simulation processing
az storage container create -n "zodiac-generator-config" --public-access off -o none
sampleGeneratorParameters='{"Users": [{"Id": "user1@tenant.onmicrosoft.com", "Password": "password"},{"Id": "user2@tenant.onmicrosoft.com","Password": "password"}],"Sessions": [{"Steps": ["capricorn-go-red", "cap021", "cap023", "cap024" ] }, { "Steps": [ "capricorn-go-rainbow", "cap013", "cap019", "cap006" ] },{ "Steps": [ "capricorn-go-blue", "cap003" ] }]}'
echo "$sampleGeneratorParameters" > GeneratorParameters.json
az storage blob upload -c "zodiac-generator-config" -f GeneratorParameters.json -n GeneratorParameters.json >> $output_blob
echo "<p>GeneratorParameters.json was written to $storageAccountName, container=zodiac-generator-config, blob=GeneratorParameters.json <b>!! You will need to edit GeneratorParameters.json</b>" >> $output_blob

chaniaBaseUrl="https://$CHANIA_ALIAS-web.azurewebsites.net"
settings="HeraclesContext__MinimumThinkTimeInMilliseconds=1000 HeraclesContext__UserSimulationEnabled=false HeraclesContext__UserTestingParametersStorageConnectionString=$connectionString HeraclesContext__BaseUrl=$chaniaBaseUrl" 

echo "Updating App Settings for $functionAppName"
echo "<p>Function App Settings:" >> $output_blob
az webapp config appsettings set -g $resourceGroupName -n $functionAppName --settings $settings >> $output_blob
echo "</p>" >> $output_blob
if [ "$HERACLES_OUTPUT_LOGGING" = TRUE ]; then
 cat $output_blob
fi
