AZ CLI Command to deploy the reosurce group
```bash
 az deployment group create --resource-group testdeploy --template-file .\template.json --parameters sqlServerUser='sqlusername' sqlServerPassword='sqlpassword' sqlServerName='nameofthesqlserver' containerRegistryName='nameoftheazurecontainerregistry'
```