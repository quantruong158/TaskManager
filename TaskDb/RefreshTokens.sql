CREATE TABLE RefreshTokens (
        TokenId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        Token NVARCHAR(200) NOT NULL,
        ExpiryDate DATETIME NOT NULL,
        Created DATETIME NOT NULL,
        CreatedByIp NVARCHAR(50) NULL,
        Revoked DATETIME NULL,
        RevokedByIp NVARCHAR(50) NULL,
        ReplacedByToken NVARCHAR(200) NULL,
        ReasonRevoked NVARCHAR(200) NULL,
        IsExpired BIT,
        IsActive BIT,
        CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
    );

GO

CREATE INDEX [IX_RefreshTokens_Token] ON [dbo].[RefreshTokens] ([Token])

GO

CREATE INDEX [IX_RefreshTokens_UserId] ON [dbo].[RefreshTokens] ([UserId])
