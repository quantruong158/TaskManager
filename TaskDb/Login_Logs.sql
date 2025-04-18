CREATE TABLE [dbo].[Login_Logs]
(
	LogId INT PRIMARY KEY IDENTITY(1,1), 
    [Email] VARCHAR(255) NOT NULL, 
    [IsSuccess] BIT NOT NULL, 
    [Timestamp] DATETIME NOT NULL, 
    [UserAgent] NVARCHAR(255) NOT NULL, 
    [AttemptIp] VARCHAR(50) NOT NULL,

)

GO

CREATE INDEX [IX_Login_Logs_Email] ON [dbo].[Login_Logs] ([Email])
