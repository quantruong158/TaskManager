CREATE TABLE Tasks (
    TaskId INT PRIMARY KEY IDENTITY(1,1),
    Title VARCHAR(255) NOT NULL,
    Description TEXT NULL,
    Priority NVARCHAR(10) CHECK (Priority IN ('low', 'medium', 'high')) NOT NULL,
    StatusId INT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    AssignedTo INT NULL,
    CreatedBy INT NULL,
    UpdatedBy INT NULL,
    FOREIGN KEY (StatusId) REFERENCES Statuses(StatusId) ON DELETE SET NULL,
    FOREIGN KEY (AssignedTo) REFERENCES Users(UserId) ON DELETE SET NULL,
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserId) ON DELETE NO ACTION,
    FOREIGN KEY (UpdatedBy) REFERENCES Users(UserId) ON DELETE NO ACTION
);