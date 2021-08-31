this command will deploy a new azure container instance that will generate load ... 


```bash
az deployment group create -g sqlserverlesstest-rg -f .\template.json -p containerName='<name of the new azure container instance>' imageName='name of the image from ACR' imageRegistryLoginServer='from ACR' imageUsername='admin username form ACR' imagePassword='password from ACR=' sqlconnectionstring='yourconnectionstring'
```

# Parameters
containerName: name of the new container instance that will be created

imageName: Name of the image from your Azure Container Repository for example - YOUR-ACR.azurecr.io/sqlonly:latest

imageRegistryLoginServer: YOUR-ACR.azurecr.io

imageUsername: From the access keys page in ACR

imagePassword: From the access keys page in ACR

sqlconnectionstring: connection string to the sql database