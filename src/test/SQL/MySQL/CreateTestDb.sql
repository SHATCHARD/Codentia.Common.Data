CREATE PROCEDURE TestProc001()
BEGIN
END;
~~
CREATE PROCEDURE TestProc002(IN param1 INT)
BEGIN
END;
~~
CREATE PROCEDURE TestProc003()
BEGIN
	SELECT 1, 2, 3;
END;
~~
CREATE PROCEDURE TestProc004(IN param2 INT)
BEGIN	
	SELECT @Param2, @Param2 * 2, @Param2 * 3;
END;
~~
CREATE PROCEDURE TestProc005()	
BEGIN
	SELECT 1, 2, 3;

	SELECT 1, 2, 3;
END;
~~
CREATE PROCEDURE TestProc006(IN Param3 INT)
BEGIN
	SELECT @Param3, @Param3 * 2, @Param3 * 3;

	SELECT @Param3, @Param3 * 2, @Param3 * 3;
END;
~~
CREATE TABLE Table001
(
	Column1		INT NOT NULL DEFAULT 0
);

INSERT INTO Table001 (Column1) VALUES (1);
~~
CREATE PROCEDURE TestProc007()
BEGIN
	UPDATE Table001 SET Column1 = 10;
END;
~~
CREATE PROCEDURE TestProc008 (
	IN int16 INT,
	IN int32 INT,
	IN int64 INT,
	IN guid BINARY(32),
	IN strfixed VARCHAR(100),
	IN str TEXT,
	IN bln BIT,
	IN dt1 DATETIME,
	IN dt2 DATETIME,
	IN dec1 DECIMAL,
	IN xml TEXT,
	IN money DECIMAL,
	IN byt TINYINT(1))
BEGIN
END;
~~
CREATE PROCEDURE TestProc010 (OUT param1 BIT)
BEGIN
	SET @param1 = 1;
END;
~~
CREATE PROCEDURE TestProc_046 (OUT param1 VARCHAR(10))
BEGIN
	SET @param1 = '0123456789';
END;
~~
CREATE PROCEDURE TestProc_050()
BEGIN
	SELECT 'TestProc_050';
END;
~~
CREATE PROCEDURE TestProc_051()
BEGIN
	SELECT 1;
END;
~~
CREATE PROCEDURE TestProc_052()
BEGIN
	SELECT 42;
END;
~~
