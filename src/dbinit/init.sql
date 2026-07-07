-- Idempotent database bootstrap.
-- Ensures the application database exists before the backend connects.
-- The full schema is created/updated by EF Core migrations at backend startup.
IF DB_ID('ElectorDb') IS NULL
BEGIN
    PRINT 'Creating database ElectorDb...';
    CREATE DATABASE ElectorDb;
END
ELSE
BEGIN
    PRINT 'Database ElectorDb already exists.';
END
GO
