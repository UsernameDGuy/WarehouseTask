/* ============================================================================
   ICAS Technology — Software Engineer assessment
   Setup script: creates the test database, one table, and sample data.

   Run this once in SQL Server Management Studio (or Azure Data Studio, or
   sqlcmd) before you start. It is safe to re-run — it drops and recreates.

   If you do not have SQL Server, any of these are free and fine:
     - SQL Server 2022 Express          (Windows)
     - SQL Server LocalDB               (ships with Visual Studio)
     - mcr.microsoft.com/mssql/server   (Docker, works on Mac/Linux)
   ============================================================================ */

IF DB_ID('ICAS_Test') IS NULL
    CREATE DATABASE ICAS_Test;
GO

USE ICAS_Test;
GO

IF OBJECT_ID('dbo.t_DeviceCfg', 'U') IS NOT NULL
    DROP TABLE dbo.t_DeviceCfg;
GO

CREATE TABLE dbo.t_DeviceCfg
(
    EquipId      INT           NOT NULL PRIMARY KEY,
    TaskID       INT           NOT NULL,
    EquipType    VARCHAR(10)   NOT NULL,
    EquipName    VARCHAR(50)   NOT NULL,
    IpAddress    VARCHAR(20)       NULL,
    Port         INT               NULL,
    SlotIndex    INT               NULL,
    Enable       SMALLINT      NOT NULL,
    Version      FLOAT             NULL,
    [Trigger]    VARCHAR(10)       NULL,
    ScanRate     INT               NULL,
    LastUpdate   DATETIME          NULL
);
GO

INSERT INTO dbo.t_DeviceCfg
    (EquipId, TaskID, EquipType, EquipName, IpAddress, Port, SlotIndex,
     Enable, Version, [Trigger], ScanRate, LastUpdate)
VALUES
    ( 1,  1, 'PLC',  'Infeed Conveyor PLC',      '192.168.10.11', 102,  0, 1, 1.05, 'CYCLIC',  200, '2026-08-14 09:12:33.000'),
    ( 2,  2, 'PLC',  'Sorter PLC',               '192.168.10.12', 102,  1, 1, 1.05, 'CYCLIC',  200, '2026-08-14 09:12:33.000'),
    ( 3,  3, 'SCAN', 'Induction Scanner 1',      '192.168.10.31', 2112, 2, 1, 1.02, 'EVENT',   100, '2026-08-20 17:45:02.000'),
    ( 4,  4, 'SCAN', 'Induction Scanner 2',      NULL,            2112, 3, 1, 1.02, 'EVENT',   100, '2026-08-20 17:45:02.000'),
    ( 5,  5, 'LED',  'Lane 1 Display',           '192.168.10.51', 5000, 4, 0, 1.00, 'ONCHANGE', 500, '2026-07-02 11:03:17.000'),
    ( 6,  6, 'PTL',  'Pick-to-Light Zone A',     '192.168.10.61', 4001, -1, 1, 1.01, 'CYCLIC',  250, '2026-09-01 08:00:00.000'),
    ( 7,  7, 'WGT',  'Checkweigher 1',           '192.168.10.71', 9100, 6, 1, 1.03, 'CYCLIC',    0, '2026-09-03 14:22:41.000'),
    ( 8,  8, 'PLC',  'O''Brien Lift Controller', '192.168.10.13', 102,  7, 1, 1.05, 'CYCLIC',  200, NULL),
    ( 9,  8, 'SCAN', 'Outfeed Scanner',          '192.168.10.32', 2112, 8, 1, 0.99, NULL,      100, '2026-06-11 10:15:00.000'),
    (10, 10, 'LED',  'Marshalling Board',        '192.168.10.52', 5000, 99, 1, 1.00, 'ONCHANGE', 500, '2026-09-10 16:38:55.000');
GO

SELECT COUNT(*) AS RowsLoaded FROM dbo.t_DeviceCfg;
GO
