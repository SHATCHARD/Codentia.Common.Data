if exists ( select 1 from sysdatabases where name = 'CECommonData' )
	drop database CECommonData
GO

CREATE DATABASE CECommonData
GO
