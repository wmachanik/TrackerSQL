SELECT
    s.name AS SchemaName,
    t.name AS TableName,
    SUM(p.rows) AS TotalRowCount
FROM sys.tables t
JOIN sys.schemas s ON t.schema_id = s.schema_id
JOIN sys.partitions p ON t.object_id = p.object_id
WHERE s.name = 'dbo'   -- change this
  AND p.index_id IN (0,1)         -- heap or clustered index only
GROUP BY s.name, t.name
ORDER BY TotalRowCount DESC;