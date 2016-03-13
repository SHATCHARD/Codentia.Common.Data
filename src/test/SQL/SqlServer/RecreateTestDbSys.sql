use master
GO

if exists ( select 1 from sysdatabases where name = 'CECommonDataSys' )
begin
		ALTER DATABASE CECommonDataSys SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
	drop database CECommonDataSys
end
GO

if exists ( select 1 from sysdatabases where name = 'CECommonDataSys_CreateDB' )
begin
		ALTER DATABASE CECommonDataSys_CreateDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
	drop database CECommonDataSys_CreateDB
end
GO

CREATE DATABASE CECommonDataSys
GO

USE CECommonDataSys
GO

CREATE TABLE TestTable1
(
	TestTable1Id	INT IDENTITY(1,1),
	TestInt			INT NOT NULL DEFAULT 1,
	TestString		NVARCHAR(10) NOT NULL DEFAULT 'ABCDEFGH',
	TestDateTime	DATETIME NOT NULL DEFAULT GETDATE(),
	TestDecimal		DECIMAL(20,10) NOT NULL,
	TestBit			BIT NOT NULL,
	TestGuid		UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID()
)
GO

ALTER TABLE dbo.TestTable1 ADD CONSTRAINT PK_TestTable1_TestTable1Id PRIMARY KEY (TestTable1Id)
GO

INSERT INTO dbo.TestTable1
(
	TestInt,
	TestString,
	TestDateTime,
	TestDecimal,
	TestBit
)
VALUES
(
	3,
	'ABCDEFGHIJ',
	'2007-06-01',
	17.5,
	1
)
GO

CREATE TABLE TestTable2
(
	TestTable2Id	INT IDENTITY(1,1),
	TestTable1Id	INT NOT NULL,
	TestInt			INT NOT NULL DEFAULT 1,
	TestString		NVARCHAR(10) NOT NULL DEFAULT 'ABCDEFGH',
	TestDateTime	DATETIME NOT NULL DEFAULT GETDATE()
)
GO

ALTER TABLE dbo.TestTable2 ADD CONSTRAINT FK_TestTable2_TestTable1Id FOREIGN KEY (TestTable1Id) REFERENCES dbo.TestTable1 (TestTable1Id)
GO

INSERT INTO TestTable2 (TestTable1Id, TestInt, TestString, TestDateTime)
VALUES
(
	1,
	2,
	'ABCDEFGHIJ',
	'2007-06-01'
)
GO

CREATE TABLE TestSchema
(
	TestInt				INT,
	TestTinyInt			TINYINT,
	TestChar			CHAR(5),
	TestVarcharMax		VARCHAR(MAX),
	TestVarchar			VARCHAR(10),
	TestNVarcharMax		NVARCHAR(MAX),
	TestNVarchar		NVARCHAR(10),
	TestDecimal			DECIMAL(9,2),
	TestBit				BIT,
	TestDateTime		DATETIME,
	TestSmallDateTime	SMALLDATETIME,
	TestUniqueIdent		UNIQUEIDENTIFIER	
)
GO

CREATE PROCEDURE dbo.TestProc1
AS
	BEGIN
		SET NOCOUNT ON
	END
GO


CREATE FUNCTION dbo.fn_FunctInline
	(
	)
	RETURNS TABLE
AS
	RETURN
	(
			SELECT 1 FuncInt
			UNION ALL
			SELECT 2
	
	)

GO


CREATE FUNCTION dbo.fn_FunctTable 
	(
	)
	RETURNS @tab TABLE (MyInt INT,
						MyVarChar VARCHAR(10)
						)
BEGIN
			INSERT INTO @tab
			VALUES (1, 'ABCDEFG')			
			RETURN
END
	
GO


CREATE FUNCTION dbo.fn_FunctScalar()
RETURNS INT
BEGIN
	RETURN 2
END

GO

CREATE VIEW dbo.TestView
AS
	(
			SELECT 1 ViewInt
			UNION ALL
			SELECT 2
	
	)	

GO


