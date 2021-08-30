
# TODO: 
Add a file named App.config. Add the following XML to the file and replace your the connection string to the connection string of your SQL Server

```xml
<?xml version="1.0" encoding="utf-8" ?>  
<configuration>  
  <appSettings>  
    <add key="sqlconnectionstring" value="<your-connection-string>"/>  
  </appSettings>  
</configuration>  
```


# Requirements
This sample uses dotnet 5. You can build and run the sample locally  with VS Code

If you want to deploy to docker, the included docker file will build a linux image with the the build output. Using the docker tools in VS Code you can push that image to an Azure Container Registry 

I have tested running the image locally in WSL2 and also publishing to an Azure Container Registry and running the container in Azure Container Instances. 

# Azure Deployments
There are two deployment scripts 
1) to deploy the resource group, the Azure Container Registry, the SQL Server Logical Server and the SQL Server serverless database. 
2) deploys a new azure container instance to your resource group. 

# What does this sample do? 
The is a REALLY simple test ... I start 100 workers that are all calling a stored proc to insert into a table in SQL. The test was set up to drive and drive load against SQL Server serverless to see if it was going to have the desired throughput for an application that was considering SQL Serverless. Use ACI you can spin up multiple container instances to run at the same time and generate additional load. 

The database only has two objects - A table and a stored procedure:

```sql
CREATE TABLE [dbo].[TransactionRecord](
	[transactionId] [uniqueidentifier] NOT NULL,
	[transactionTime] [datetime2](7) NOT NULL,
	[transactionAmount] [decimal](18, 0) NOT NULL,
	[paymentToken] [nvarchar](256) NOT NULL,
	[settlementStatus] [int] NOT NULL,
	[settlementTime] [datetime2](7) NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[TransactionRecord] ADD PRIMARY KEY NONCLUSTERED 
(
	[transactionId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO


create procedure [dbo].[uspNewTransaction] 
    @transactionId uniqueidentifier 
    , @transactionTime DateTime2 
    , @transactionAmount decimal 
    , @paymentToken nvarchar(256) 
AS

insert into TransactionRecord (transactionId, transactionTime, transactionAmount, paymentToken, settlementStatus, settlementTime)
values (@transactionId, @transactionTime, @transactionAmount, @paymentToken, 0, null)
GO
```
