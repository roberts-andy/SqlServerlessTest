


# Requirements
This sample uses dotnet 5. You can build and run the sample locally  with VS Code

If you want to deploy to docker, the included docker file will build a linux image with the the build output. Using the docker tools in VS Code you can push that image to an Azure Container Registry 

I have tested running the image locally in WSL2 and also publishing to an Azure Container Registry and running the container in Azure Container Instances. 

If you are running locally then you will need to either hardcode your connection string in Program.cs or set an environment variable called "sqlconnectionstring" with the sql connection string to connect to the deployed database.

you can set the environment variables with this command in windows -- do this before opening VS Code
```bash
[Environment]::SetEnvironmentVariable('sqlconnectionstring', 'yourconnectionstring')
[Environment]::SetEnvironmentVariable('numworkers', '100')
```

numworkers if not supplied will default to 100. i think that if you go over 100 you will exhaust the sql connection pool of the worker process and start to see diminishing returns or actual degradation in performance but i have not tested this.

# Azure Deployments
There are two deployment scripts 
1) in the folder deploy/deployrg is the script to deploy the resource group, the Azure Container Registry, the SQL Server Logical Server and the SQL Server serverless database. 
2) in the folder deploy/deploycontainer deploys a new azure container instance to your resource group.

There are instructions in each deployment foloder for how to execute the script. The process to deploy
1) clone the repo
2) deploy the resource group - arm template in the folder /deploy/deployrg
3) connect to the deployed sql database and created the sql objects listed below 
4) build the .net console app
5) build the image - select "Docker Images:Build image" from the command pallette (crtl-shift-p)
6) push the image to your container registry "Docker Images: Push" from the command pallette

at this point the container should be running and you should be able to look at the TransactionRecord table and see the row count increasing.
Make sure and stop your container instance when you are not using it! If you do not, then SQL will not go idle and you will continue to be billed. The container simply executes in a tight look untill it is killed.  

you can deploy as many container instances as you want... just change the instance name. 



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
