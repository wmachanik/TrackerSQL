-- Test script with intentional errors
PRINT 'Starting test migration with intentional errors...';

-- This should work fine
SELECT 'Test batch 1 - This should succeed' as TestMessage;
GO

-- This should fail - invalid column name
SELECT NonExistentColumn FROM ContactsTbl;
GO

-- This should also fail - invalid table name  
INSERT INTO NonExistentTable (Col1, Col2) VALUES (1, 'test');
GO

-- This should work again
SELECT 'Test batch 4 - This should succeed after errors' as TestMessage;
GO

PRINT 'Test migration completed with expected errors';
