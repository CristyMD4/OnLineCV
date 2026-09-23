USE [OnlineCV];
GO

-- View the current CV document and its last update time.
SELECT
    [Id],
    [UpdatedAt],
    JSON_QUERY([ContentJson]) AS [ContentJson]
FROM [dbo].[CvDocuments];
GO

-- View contact messages, newest first.
SELECT
    [Id],
    [Name],
    [Email],
    [Message],
    [Status],
    [CreatedAt],
    [UpdatedAt]
FROM [dbo].[ContactSubmissions]
ORDER BY [CreatedAt] DESC;
GO

-- Read selected CV fields directly from SQL Server JSON.
SELECT
    JSON_VALUE([ContentJson], '$.identity.name') AS [Name],
    JSON_VALUE([ContentJson], '$.identity.role') AS [Role],
    JSON_VALUE([ContentJson], '$.identity.email') AS [Email]
FROM [dbo].[CvDocuments]
WHERE [Id] = 1;
GO
