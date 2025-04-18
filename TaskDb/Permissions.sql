CREATE TABLE Permissions (
    PermissionId INT PRIMARY KEY IDENTITY(1,1),
    PermissionName VARCHAR(100) UNIQUE NOT NULL,
    Description TEXT NULL
);