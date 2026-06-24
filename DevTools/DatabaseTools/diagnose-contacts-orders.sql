-- DIAGNOSTIC SCRIPT: Orders/Contacts FK Validation Issue
-- Run this against your SQL Server database to diagnose the ContactID FK failures

PRINT '=== CONTACTS/ORDERS FK DIAGNOSTIC ===';
PRINT 'Timestamp: ' + CAST(GETDATE() AS NVARCHAR(50));
PRINT '';

-- 1. Check if ContactsTbl exists and has data
PRINT '1. ContactsTbl Status:';
IF OBJECT_ID('dbo.ContactsTbl', 'U') IS NULL
BEGIN
    PRINT '   ERROR: ContactsTbl does not exist!';
END
ELSE
BEGIN
    DECLARE @contactCount INT;
    SELECT @contactCount = COUNT(*) FROM dbo.ContactsTbl;
    PRINT '   ContactsTbl exists with ' + CAST(@contactCount AS NVARCHAR(10)) + ' rows';
    
    -- Show sample ContactIDs
    PRINT '   Sample ContactIDs (first 10):';
    SELECT TOP 10 ContactID, CompanyName FROM dbo.ContactsTbl ORDER BY ContactID;
    
    PRINT '   ContactID range: MIN=' + CAST((SELECT MIN(ContactID) FROM dbo.ContactsTbl) AS NVARCHAR(10)) + 
          ', MAX=' + CAST((SELECT MAX(ContactID) FROM dbo.ContactsTbl) AS NVARCHAR(10));
END
PRINT '';

-- 2. Check AccessSrc.OrdersTbl (staging data)
PRINT '2. AccessSrc.OrdersTbl Status:';
IF OBJECT_ID('AccessSrc.OrdersTbl', 'U') IS NULL
BEGIN
    PRINT '   ERROR: AccessSrc.OrdersTbl does not exist!';
    PRINT '   This suggests the staging step (option MS) was not run or failed.';
END
ELSE
BEGIN
    DECLARE @orderCount INT;
    SELECT @orderCount = COUNT(*) FROM AccessSrc.OrdersTbl;
    PRINT '   AccessSrc.OrdersTbl exists with ' + CAST(@orderCount AS NVARCHAR(10)) + ' rows';
    
    -- Check what columns exist (CustomerID vs ContactID)
    PRINT '   Columns in AccessSrc.OrdersTbl:';
    SELECT 
        COLUMN_NAME as ColumnName,
        DATA_TYPE as DataType,
        IS_NULLABLE as IsNullable
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'AccessSrc' 
      AND TABLE_NAME = 'OrdersTbl'
    ORDER BY ORDINAL_POSITION;
    
    -- Show sample CustomerID/ContactID values
    PRINT '   Sample CustomerID values (first 10):';
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'AccessSrc' AND TABLE_NAME = 'OrdersTbl' AND COLUMN_NAME = 'CustomerID')
    BEGIN
        SELECT TOP 10 OrderID, CustomerID FROM AccessSrc.OrdersTbl ORDER BY OrderID;
        PRINT '   CustomerID range: MIN=' + CAST((SELECT MIN(CustomerID) FROM AccessSrc.OrdersTbl) AS NVARCHAR(10)) + 
              ', MAX=' + CAST((SELECT MAX(CustomerID) FROM AccessSrc.OrdersTbl) AS NVARCHAR(10));
    END
    ELSE IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'AccessSrc' AND TABLE_NAME = 'OrdersTbl' AND COLUMN_NAME = 'ContactID')
    BEGIN
        SELECT TOP 10 OrderID, ContactID FROM AccessSrc.OrdersTbl ORDER BY OrderID;
        PRINT '   ContactID range: MIN=' + CAST((SELECT MIN(ContactID) FROM AccessSrc.OrdersTbl) AS NVARCHAR(10)) + 
              ', MAX=' + CAST((SELECT MAX(ContactID) FROM AccessSrc.OrdersTbl) AS NVARCHAR(10));
    END
    ELSE
    BEGIN
        PRINT '   WARNING: Neither CustomerID nor ContactID column found!';
    END
END
PRINT '';

-- 3. Check Column Mapping (CustomerID -> ContactID transformation)
PRINT '3. Column Mapping Analysis:';
IF OBJECT_ID('AccessSrc.OrdersTbl', 'U') IS NOT NULL AND OBJECT_ID('dbo.ContactsTbl', 'U') IS NOT NULL
BEGIN
    -- Check if CustomerIDs in orders exist as ContactIDs in contacts
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'AccessSrc' AND TABLE_NAME = 'OrdersTbl' AND COLUMN_NAME = 'CustomerID')
    BEGIN
        DECLARE @missingFKs INT;
        SELECT @missingFKs = COUNT(DISTINCT o.CustomerID)
        FROM AccessSrc.OrdersTbl o
        LEFT JOIN dbo.ContactsTbl c ON c.ContactID = o.CustomerID
        WHERE o.CustomerID IS NOT NULL 
          AND c.ContactID IS NULL;
        
        PRINT '   Orders with CustomerID that do not exist in ContactsTbl.ContactID: ' + CAST(@missingFKs AS NVARCHAR(10));
        
        IF @missingFKs > 0
        BEGIN
            PRINT '   Sample missing CustomerIDs (first 10):';
            SELECT TOP 10 o.CustomerID, COUNT(*) as OrderCount
            FROM AccessSrc.OrdersTbl o
            LEFT JOIN dbo.ContactsTbl c ON c.ContactID = o.CustomerID  
            WHERE o.CustomerID IS NOT NULL 
              AND c.ContactID IS NULL
            GROUP BY o.CustomerID
            ORDER BY o.CustomerID;
        END
        
        -- Show overlap analysis
        DECLARE @totalOrderCustomers INT, @matchingCustomers INT;
        SELECT @totalOrderCustomers = COUNT(DISTINCT CustomerID) FROM AccessSrc.OrdersTbl WHERE CustomerID IS NOT NULL;
        SELECT @matchingCustomers = COUNT(DISTINCT o.CustomerID)
        FROM AccessSrc.OrdersTbl o
        INNER JOIN dbo.ContactsTbl c ON c.ContactID = o.CustomerID
        WHERE o.CustomerID IS NOT NULL;
        
        PRINT '   Unique CustomerIDs in orders: ' + CAST(@totalOrderCustomers AS NVARCHAR(10));
        PRINT '   CustomerIDs that match ContactsTbl.ContactID: ' + CAST(@matchingCustomers AS NVARCHAR(10));
        PRINT '   Match percentage: ' + CAST(CASE WHEN @totalOrderCustomers > 0 THEN (@matchingCustomers * 100.0 / @totalOrderCustomers) ELSE 0 END AS NVARCHAR(10)) + '%';
    END
    ELSE
    BEGIN
        PRINT '   Cannot analyze - CustomerID column not found in AccessSrc.OrdersTbl';
    END
END
PRINT '';

-- 4. Check Migration_OrphanedOrders table for detailed failures
PRINT '4. Orphaned Orders Analysis:';
IF OBJECT_ID('dbo.Migration_OrphanedOrders', 'U') IS NULL
BEGIN
    PRINT '   Migration_OrphanedOrders table does not exist (normal if no migration attempted yet)';
END
ELSE
BEGIN
    DECLARE @orphanCount INT;
    SELECT @orphanCount = COUNT(*) FROM dbo.Migration_OrphanedOrders;
    PRINT '   Orphaned orders recorded: ' + CAST(@orphanCount AS NVARCHAR(10));
    
    IF @orphanCount > 0
    BEGIN
        PRINT '   Breakdown by missing FK type:';
        SELECT 
            MissingFkName,
            COUNT(*) as Count,
            MIN(MissingFkValue) as MinValue,
            MAX(MissingFkValue) as MaxValue
        FROM dbo.Migration_OrphanedOrders 
        GROUP BY MissingFkName
        ORDER BY COUNT(*) DESC;
        
        PRINT '   Sample orphaned records (first 5):';
        SELECT TOP 5 
            SourceOrderId,
            MissingFkName,
            MissingFkValue,
            DetectedAt
        FROM dbo.Migration_OrphanedOrders
        ORDER BY DetectedAt DESC;
    END
END
PRINT '';

-- 5. Recommendations
PRINT '5. DIAGNOSTIC SUMMARY & RECOMMENDATIONS:';
PRINT '   Based on the results above:';
PRINT '   ';
PRINT '   IF ContactsTbl is empty or missing:';
PRINT '     -> Run steps M + N to migrate Contacts/Customers before Orders';
PRINT '     -> Check CustomersTbl -> ContactsTbl migration first';
PRINT '   ';
PRINT '   IF ContactsTbl has data but CustomerID ranges don''t match:';
PRINT '     -> Check if Access CustomerIDs were remapped during migration';
PRINT '     -> Verify CustomersTbl.CustomerID -> ContactsTbl.ContactID mapping';
PRINT '     -> May need to update CustomNormalizeRunner CustomerID lookup logic';
PRINT '   ';  
PRINT '   IF AccessSrc.OrdersTbl missing:';
PRINT '     -> Run option MS (Stage Access) first';
PRINT '   ';
PRINT '   IF Column mapping issue (CustomerID vs ContactID):';
PRINT '     -> Check CSV column mappings in PlanHumanReviewImporter';
PRINT '     -> Verify OrdersTbl.schema.json ColumnActions';
PRINT '';

PRINT '=== END DIAGNOSTIC ===';