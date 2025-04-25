CREATE TABLE Activity_Logs (
    LogId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    Action VARCHAR(255) NOT NULL,
    TargetTable NVARCHAR(20) CHECK (TargetTable IN ('tasks', 'users', 'comments', 'statuses')) NOT NULL,
    TargetId INT NOT NULL,
    Timestamp DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);