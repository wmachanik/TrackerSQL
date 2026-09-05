/* Go-live: upsert SystemPreferencesHdrTbl (Woo flags only). */
SET NOCOUNT ON;
IF EXISTS (SELECT 1 FROM dbo.SystemPreferencesHdrTbl WHERE PrefsID = 1)
  UPDATE dbo.SystemPreferencesHdrTbl
  SET WooCommerceEnabled = 1,
      WooWizardCompleted = 1,
      UpdatedAt = SYSUTCDATETIME(),
      UpdatedBy = N'golive-import'
  WHERE PrefsID = 1;
ELSE
  INSERT INTO dbo.SystemPreferencesHdrTbl (PrefsID, WooCommerceEnabled, WooWizardCompleted, UpdatedAt, UpdatedBy)
  VALUES (1, 1, 1, SYSUTCDATETIME(), N'golive-import');
GO
