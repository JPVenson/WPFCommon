DECLARE @Database NVARCHAR(100) = 'BatchRemoting'

DECLARE @spidstr varchar(8000)
SELECT @spidstr=COALESCE(@spidstr,';' )+'kill '+CONVERT(VARCHAR, spid)+ '; '
    FROM master..sysprocesses WHERE dbid=db_id(@Database)

SELECT @spidstr
use master
EXEC(@spidstr) 


IF  EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = @Database)
EXEC ('DROP DATABASE ' + @Database);
EXEC ('CREATE DATABASE ' + @Database);

USE BatchRemoting

EXEC ('USE ' + @Database);

USE [BatchRemoting]
GO

CREATE USER [BatchRemotingJobServer] FOR LOGIN [BatchRemotingJobServer]
GO
USE [BatchRemoting]
GO
EXEC sp_addrolemember N'db_datareader', N'BatchRemotingJobServer'
GO
USE [BatchRemoting]
GO
EXEC sp_addrolemember N'db_datawriter', N'BatchRemotingJobServer'
GO

CREATE USER [DPC\Henning.Heusser] FOR LOGIN [DPC\Henning.Heusser]
GO
USE [BatchRemoting]
GO
EXEC sp_addrolemember N'db_owner', N'DPC\Henning.Heusser'
GO

--------------------------------------------------------------------------------------------------------------------------------------------------------------

CREATE TABLE BatchServer
(
	BatchServer_ID INT IDENTITY(1,1) PRIMARY KEY CLUSTERED NOT NULL,
	ServerName NVARCHAR(100) NOT NULL,		
	ServerIP NVARCHAR(100) NOT NULL,	
	Working BIT NOT NULL,
	IsOnline BIT DEFAULT(0) NOT NULL
)

--CREATE TABLE ExecutionPriority
--(
--	ExecutionPriority_ID INT IDENTITY(1,1) PRIMARY KEY CLUSTERED NOT NULL,	
--	Descriptor NVARCHAR(500) NOT NULL,
--	ExecutionOrder INT NOT NULL
--)

CREATE TABLE ServerBatch 
(
	ServerBatch_ID INT IDENTITY(1,1) PRIMARY KEY CLUSTERED NOT NULL,
	ExecuteUser NVARCHAR(100) NOT NULL,
	Descriptor NVARCHAR(500) NULL,
	ExecutionStartTime DATETIME2 DEFAULT GETDATE() NULL,	
	ExecutionEndTime DATETIME2 DEFAULT GETDATE() NULL,	
	IsEnabled BIT DEFAULT(1) NOT NULL,
	ID_BatchGroup INT NULL,
	ID_BatchState INT DEFAULT 1 NOT NULL,
	ID_BatchType INT NOT NULL,
	ID_BatchServer INT NULL,	
	--ID_ExecutionPriority INT NOT NULL,
	RowState ROWVERSION
)

CREATE TABLE BatchGroup
(
	BatchGroup_ID INT IDENTITY(1,1) PRIMARY KEY CLUSTERED NOT NULL,
	ExecuteUser NVARCHAR(100) NOT NULL,
	Descriptor NVARCHAR(500) NULL,
	IsTemplate BIT DEFAULT(0) NOT NULL,
	IsInUse BIT DEFAULT(0) NOT NULL
)

CREATE TABLE BatchJob 
(
	BatchJob_ID INT IDENTITY(1,1) PRIMARY KEY CLUSTERED NOT NULL,
	ExecutionStartTime DATETIME2 DEFAULT GETDATE() NULL,	
	ExecutionEndTime DATETIME2 DEFAULT GETDATE() NULL,	
	JobName NVARCHAR(100) NOT NULL,
	TargetFileName NVARCHAR(200) NULL,
	DatabaseID INT NULL,
	Paramenter NVARCHAR(100) NOT NULL,
	Error NVARCHAR(MAX) NULL,
	IsEnabled BIT DEFAULT(1) NOT NULL,
	ID_JobType INT NOT NULL,
	ID_ServerBatch INT NULL,
	ID_BatchState INT DEFAULT 2 NOT NULL,
	RowState ROWVERSION
)

CREATE TABLE BatchState 
(
	BatchState_ID INT IDENTITY(1,1) PRIMARY KEY CLUSTERED NOT NULL,
	Value NVARCHAR(100) NOT NULL
)

CREATE TABLE BatchType 
(
	BatchType_ID INT IDENTITY(1,1) PRIMARY KEY CLUSTERED NOT NULL,
	BatchTypeKey NVARCHAR(100) NOT NULL,		
	BatchTypeProgram NVARCHAR(100) NOT NULL,		
	BatchTypeCommand NVARCHAR(500) NOT NULL,
	StudyName NVARCHAR(100) NOT NULL,
	DisplayName NVARCHAR(100) NOT NULL
)

CREATE TABLE JobType 
(
	JobType_ID INT IDENTITY(1,1) PRIMARY KEY CLUSTERED NOT NULL,
	JobTypeKey NVARCHAR(100) NOT NULL,			
	DisplayName NVARCHAR(100) NOT NULL
)


CREATE TABLE MAP_BatchServer_BatchType
(
	MAP_BatchServer_BatchType_ID INT IDENTITY(1,1) PRIMARY KEY CLUSTERED NOT NULL,
	ID_BatchType INT NOT NULL,
	ID_BatchServer INT NOT NULL
)

CREATE TABLE [Errors]
(
	[Errors_ID] [int] PRIMARY KEY CLUSTERED IDENTITY(1,1) NOT NULL,
	[ProgramRunID] [int] NULL,
	[ErrorMessage] [nvarchar](max) NULL,
)


--------------------------------------------------------------------------------------------------------------------------------------------------------------

ALTER TABLE ServerBatch WITH CHECK ADD CONSTRAINT [FK_ServerBatch_BatchGroup] FOREIGN KEY (ID_BatchGroup)
REFERENCES BatchGroup(BatchGroup_ID)

--ALTER TABLE ServerBatch WITH CHECK ADD CONSTRAINT [FK_ServerBatch_ExecutionPriority] FOREIGN KEY (ID_ExecutionPriority)
--REFERENCES ExecutionPriority(ExecutionPriority_ID)

ALTER TABLE ServerBatch WITH CHECK ADD CONSTRAINT [FK_ServerBatch_BatchType] FOREIGN KEY (ID_BatchType)
REFERENCES BatchType(BatchType_ID)

ALTER TABLE ServerBatch WITH CHECK ADD CONSTRAINT [FK_ServerBatch_BatchState] FOREIGN KEY (ID_BatchState)
REFERENCES BatchState(BatchState_ID)

ALTER TABLE ServerBatch WITH CHECK ADD CONSTRAINT [FK_ServerBatch_BatchServer] FOREIGN KEY (ID_BatchServer)
REFERENCES BatchServer(BatchServer_ID)

ALTER TABLE MAP_BatchServer_BatchType WITH CHECK ADD CONSTRAINT [FK_ServerBatch_MAP_BatchServer_BatchType_BatchType] FOREIGN KEY (ID_BatchServer)
REFERENCES BatchServer(BatchServer_ID)

ALTER TABLE MAP_BatchServer_BatchType WITH CHECK ADD CONSTRAINT [FK_BatchType_MAP_BatchServer_BatchType_ServerBatch] FOREIGN KEY (ID_BatchType)
REFERENCES BatchType(BatchType_ID)

ALTER TABLE BatchJob WITH CHECK ADD CONSTRAINT [FK_BatchJob_JobType] FOREIGN KEY (ID_JobType)
REFERENCES JobType(JobType_ID)

ALTER TABLE BatchJob WITH CHECK ADD CONSTRAINT [FK_BatchJob_ServerBatch] FOREIGN KEY (ID_ServerBatch)
REFERENCES ServerBatch(ServerBatch_ID)

ALTER TABLE BatchJob WITH CHECK ADD CONSTRAINT [FK_BatchJob_BatchState] FOREIGN KEY (ID_BatchState)
REFERENCES BatchState(BatchState_ID)

--------------------------------------------------------------------------------------------------------------------------------------------------------------

INSERT INTO BatchType ("BatchTypeKey","StudyName","BatchTypeProgram","DisplayName","BatchTypeCommand") 
VALUES ('TIMSS 2015 FT G4','TIMSS2015FTG4','C:\Program Files\IEA\DPE Processing Module Testing_TIMSS2015G4FT\ProcessingStepCommandLine.exe', 'TIMSS 2015 FT G4 ', '{0} {1} {2} {3}')
INSERT INTO BatchType ("BatchTypeKey","StudyName","BatchTypeProgram","DisplayName","BatchTypeCommand") 
VALUES ('TIMSS 2015 FT G8','TIMSS2015FTG8','C:\Program Files\IEA\DPE Processing Module Testing_TIMSS2015G4FT\ProcessingStepCommandLine.exe', 'TIMSS 2015 FT G8', '{0} {1} {2} {3}')
--INSERT INTO BatchType ("BatchTypeKey","StudyName","BatchTypeProgram","DisplayName","BatchTypeCommand") 
--VALUES ('Testing TIMSS Advanced 2015 FT','TIMSSAdvanced2015FT','C:\Program Files\IEA\DPE Processing Module Testing_TIMSS2015G4FT\ProcessingStepCommandLine.exe', 'Testing TIMSS Advanced 2015 FT', '{0} {1} {2} {3}')


---ORDER IMPORTANT!
INSERT INTO BatchState ("Value") VALUES ('OK')
INSERT INTO BatchState ("Value") VALUES ('Working')
INSERT INTO BatchState ("Value") VALUES ('Failed')
INSERT INTO BatchState ("Value") VALUES ('Pending')
INSERT INTO BatchState ("Value") VALUES ('Scheduled')
INSERT INTO BatchState ("Value") VALUES ('Aborted')
INSERT INTO BatchState ("Value") VALUES ('Skipped')


--INSERT INTO ExecutionPriority ("Descriptor","ExecutionOrder") VALUES ('Realtime',0)
--INSERT INTO ExecutionPriority ("Descriptor","ExecutionOrder") VALUES ('Below Realtime',1)
--INSERT INTO ExecutionPriority ("Descriptor","ExecutionOrder") VALUES ('High',2)
--INSERT INTO ExecutionPriority ("Descriptor","ExecutionOrder") VALUES ('Below High',3)
--INSERT INTO ExecutionPriority ("Descriptor","ExecutionOrder") VALUES ('Normal',4)
--INSERT INTO ExecutionPriority ("Descriptor","ExecutionOrder") VALUES ('Below Normal',5)
--INSERT INTO ExecutionPriority ("Descriptor","ExecutionOrder") VALUES ('Low',6)
--INSERT INTO ExecutionPriority ("Descriptor","ExecutionOrder") VALUES ('Below Low',7)
-----END

INSERT INTO JobType ("JobTypeKey", "DisplayName") VALUES ('IMPORT','IMPORT')
INSERT INTO JobType ("JobTypeKey", "DisplayName") VALUES ('PRECleaning','Pre-Cleaning')
INSERT INTO JobType ("JobTypeKey", "DisplayName") VALUES ('StructureCheck','Structure Check')
INSERT INTO JobType ("JobTypeKey", "DisplayName") VALUES ('Cleaning','Cleaning')
INSERT INTO JobType ("JobTypeKey", "DisplayName") VALUES ('PostCleaning','Post-Cleaning')
INSERT INTO JobType ("JobTypeKey", "DisplayName") VALUES ('FinalMerge','Final Merge')
INSERT INTO JobType ("JobTypeKey", "DisplayName") VALUES ('FinalCheck','Final-Check')
INSERT INTO JobType ("JobTypeKey", "DisplayName") VALUES ('FINALEXPORT','FINAL-EXPORT')
INSERT INTO JobType ("JobTypeKey", "DisplayName") VALUES ('Statistics','Statistics')

GO
CREATE PROC RemoveJobServersTypes @ServerID INT
AS
DELETE MAP_BatchServer_BatchType WHERE ID_BatchServer = @ServerID
GO

CREATE PROC SetServerBatchsServerProp @ServerID INT, @ServerBatchID INT, @Success BIT OUT
AS
	DECLARE @batchserver INT;
	SET @batchserver = (SELECT s.ID_BatchServer FROM ServerBatch s WHERE s.ServerBatch_ID = @ServerBatchID)
	IF(@batchserver IS NULL)
		BEGIN
			UPDATE ServerBatch SET ID_BatchServer = @ServerID WHERE ServerBatch_ID = @ServerBatchID
			SET @Success = 1
		END
	ELSE
		SET @Success = 0
GO

GRANT EXECUTE ON  SetServerBatchsServerProp TO [BatchRemotingJobServer]
GRANT EXECUTE ON  RemoveJobServersTypes TO [BatchRemotingJobServer]