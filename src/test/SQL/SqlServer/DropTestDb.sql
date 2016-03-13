if exists ( select 1 from sysdatabases where name = 'CECommonData' )
	begin
		ALTER DATABASE CECommonData SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

		drop database CECommonData
	end	
GO

CREATE DATABASE CECommonData
GO
