CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Email VARCHAR(255) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    Name VARCHAR(100) NOT NULL,
    CreatedAt DATETIME,
    UpdatedAt DATETIME,
    CreatedBy INT NULL,
    UpdatedBy INT NULL,
    IsActive BIT DEFAULT 1,
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserId) ON DELETE NO ACTION,
    FOREIGN KEY (UpdatedBy) REFERENCES Users(UserId) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_Users_Email] ON [dbo].[Users] ([Email])
