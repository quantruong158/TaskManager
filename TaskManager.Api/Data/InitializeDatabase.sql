-- Database initialization script for TaskManager

-- Create Users table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        UserId INT IDENTITY(1,1) PRIMARY KEY,
        Email NVARCHAR(255) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(MAX) NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME NOT NULL,
        CreatedBy NVARCHAR(100) NOT NULL,
        UpdatedAt DATETIME NULL,
        UpdatedBy NVARCHAR(100) NULL
    );
END

-- Create Roles table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
BEGIN
    CREATE TABLE Roles (
        RoleId INT IDENTITY(1,1) PRIMARY KEY,
        RoleName NVARCHAR(50) NOT NULL UNIQUE,
        Description NVARCHAR(200) NULL
    );
END

-- Create Permissions table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Permissions')
BEGIN
    CREATE TABLE Permissions (
        PermissionId INT IDENTITY(1,1) PRIMARY KEY,
        PermissionName NVARCHAR(100) NOT NULL UNIQUE,
        Description NVARCHAR(200) NULL
    );
END

-- Create UserRoles table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserRoles')
BEGIN
    CREATE TABLE UserRoles (
        UserRoleId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        RoleId INT NOT NULL,
        CreatedAt DATETIME NOT NULL,
        CreatedBy NVARCHAR(100) NOT NULL,
        CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
        CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId) ON DELETE CASCADE,
        CONSTRAINT UQ_UserRoles_UserId_RoleId UNIQUE (UserId, RoleId)
    );
END

-- Create RolePermissions table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RolePermissions')
BEGIN
    CREATE TABLE RolePermissions (
        RolePermissionId INT IDENTITY(1,1) PRIMARY KEY,
        RoleId INT NOT NULL,
        PermissionId INT NOT NULL,
        CONSTRAINT FK_RolePermissions_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId) ON DELETE CASCADE,
        CONSTRAINT FK_RolePermissions_Permissions FOREIGN KEY (PermissionId) REFERENCES Permissions(PermissionId) ON DELETE CASCADE,
        CONSTRAINT UQ_RolePermissions_RoleId_PermissionId UNIQUE (RoleId, PermissionId)
    );
END

-- Create Tasks table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tasks')
BEGIN
    CREATE TABLE Tasks (
        TaskId INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        Priority INT NOT NULL DEFAULT 0,
        Status NVARCHAR(50) NOT NULL DEFAULT 'New',
        DueDate DATETIME NULL,
        AssignedToUserId INT NULL,
        CreatedAt DATETIME NOT NULL,
        CreatedBy NVARCHAR(100) NOT NULL,
        UpdatedAt DATETIME NULL,
        UpdatedBy NVARCHAR(100) NULL,
        CONSTRAINT FK_Tasks_Users FOREIGN KEY (AssignedToUserId) REFERENCES Users(UserId)
    );
END

-- Create TaskStatusHistory table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TaskStatusHistory')
BEGIN
    CREATE TABLE TaskStatusHistory (
        StatusHistoryId INT IDENTITY(1,1) PRIMARY KEY,
        TaskId INT NOT NULL,
        OldStatus NVARCHAR(50) NOT NULL,
        NewStatus NVARCHAR(50) NOT NULL,
        ChangedAt DATETIME NOT NULL,
        ChangedBy NVARCHAR(100) NOT NULL,
        CONSTRAINT FK_StatusHistory_Tasks FOREIGN KEY (TaskId) REFERENCES Tasks(TaskId) ON DELETE CASCADE
    );
END

-- Create Comments table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Comments')
BEGIN
    CREATE TABLE Comments (
        CommentId INT IDENTITY(1,1) PRIMARY KEY,
        TaskId INT NOT NULL,
        UserId INT NOT NULL,
        CommentText NVARCHAR(MAX) NOT NULL,
        CreatedAt DATETIME NOT NULL,
        UpdatedAt DATETIME NULL,
        CONSTRAINT FK_Comments_Tasks FOREIGN KEY (TaskId) REFERENCES Tasks(TaskId) ON DELETE CASCADE,
        CONSTRAINT FK_Comments_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
    );
END

-- Create RefreshTokens table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RefreshTokens')
BEGIN
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
        IsExpired BIT AS (CASE WHEN ExpiryDate < GETUTCDATE() THEN 1 ELSE 0 END),
        IsActive BIT AS (CASE WHEN Revoked IS NULL AND ExpiryDate >= GETUTCDATE() THEN 1 ELSE 0 END),
        CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
    );

    CREATE INDEX IX_RefreshTokens_Token ON RefreshTokens(Token);
    CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
END

-- Insert default roles if they don't exist
IF NOT EXISTS (SELECT * FROM Roles WHERE RoleName = 'Admin')
BEGIN
    INSERT INTO Roles (RoleName, Description) VALUES ('Admin', 'Administrator with full access');
END

IF NOT EXISTS (SELECT * FROM Roles WHERE RoleName = 'Manager')
BEGIN
    INSERT INTO Roles (RoleName, Description) VALUES ('Manager', 'Manager with task management capabilities');
END

IF NOT EXISTS (SELECT * FROM Roles WHERE RoleName = 'User')
BEGIN
    INSERT INTO Roles (RoleName, Description) VALUES ('User', 'Regular user with limited access');
END

-- Insert default permissions
-- Admin permissions
IF NOT EXISTS (SELECT * FROM Permissions WHERE PermissionName = 'users.manage')
BEGIN
    INSERT INTO Permissions (PermissionName, Description) VALUES ('users.manage', 'Can manage users');
END

IF NOT EXISTS (SELECT * FROM Permissions WHERE PermissionName = 'roles.assign')
BEGIN
    INSERT INTO Permissions (PermissionName, Description) VALUES ('roles.assign', 'Can assign roles');
END

-- Task permissions
IF NOT EXISTS (SELECT * FROM Permissions WHERE PermissionName = 'tasks.create')
BEGIN
    INSERT INTO Permissions (PermissionName, Description) VALUES ('tasks.create', 'Can create tasks');
END

IF NOT EXISTS (SELECT * FROM Permissions WHERE PermissionName = 'tasks.edit')
BEGIN
    INSERT INTO Permissions (PermissionName, Description) VALUES ('tasks.edit', 'Can edit tasks');
END

IF NOT EXISTS (SELECT * FROM Permissions WHERE PermissionName = 'tasks.delete')
BEGIN
    INSERT INTO Permissions (PermissionName, Description) VALUES ('tasks.delete', 'Can delete tasks');
END

IF NOT EXISTS (SELECT * FROM Permissions WHERE PermissionName = 'tasks.view_all')
BEGIN
    INSERT INTO Permissions (PermissionName, Description) VALUES ('tasks.view_all', 'Can view all tasks');
END

IF NOT EXISTS (SELECT * FROM Permissions WHERE PermissionName = 'tasks.assign')
BEGIN
    INSERT INTO Permissions (PermissionName, Description) VALUES ('tasks.assign', 'Can assign tasks');
END

-- Assign permissions to roles
-- Admin role permissions
DECLARE @AdminRoleId INT, @ManagerRoleId INT, @UserRoleId INT;
DECLARE @PermissionId INT;

SELECT @AdminRoleId = RoleId FROM Roles WHERE RoleName = 'Admin';
SELECT @ManagerRoleId = RoleId FROM Roles WHERE RoleName = 'Manager';
SELECT @UserRoleId = RoleId FROM Roles WHERE RoleName = 'User';

-- Admin gets all permissions
DECLARE permission_cursor CURSOR FOR
SELECT PermissionId FROM Permissions;

OPEN permission_cursor;
FETCH NEXT FROM permission_cursor INTO @PermissionId;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (SELECT * FROM RolePermissions WHERE RoleId = @AdminRoleId AND PermissionId = @PermissionId)
    BEGIN
        INSERT INTO RolePermissions (RoleId, PermissionId) VALUES (@AdminRoleId, @PermissionId);
    END
    FETCH NEXT FROM permission_cursor INTO @PermissionId;
END

CLOSE permission_cursor;
DEALLOCATE permission_cursor;

-- Manager permissions
IF EXISTS (SELECT * FROM Permissions WHERE PermissionName = 'tasks.create')
BEGIN
    SELECT @PermissionId = PermissionId FROM Permissions WHERE PermissionName = 'tasks.create';
    IF NOT EXISTS (SELECT * FROM RolePermissions WHERE RoleId = @ManagerRoleId AND PermissionId = @PermissionId)
    BEGIN
        INSERT INTO RolePermissions (RoleId, PermissionId) VALUES (@ManagerRoleId, @PermissionId);
    END
END

IF EXISTS (SELECT * FROM Permissions WHERE PermissionName = 'tasks.edit')
BEGIN
    SELECT @PermissionId = PermissionId FROM Permissions WHERE PermissionName = 'tasks.edit';
    IF NOT EXISTS (SELECT * FROM RolePermissions WHERE RoleId = @ManagerRoleId AND PermissionId = @PermissionId)
    BEGIN
        INSERT INTO RolePermissions (RoleId, PermissionId) VALUES (@ManagerRoleId, @PermissionId);
    END
END

IF EXISTS (SELECT * FROM Permissions WHERE PermissionName = 'tasks.view_all')
BEGIN
    SELECT @PermissionId = PermissionId FROM Permissions WHERE PermissionName = 'tasks.view_all';
    IF NOT EXISTS (SELECT * FROM RolePermissions WHERE RoleId = @ManagerRoleId AND PermissionId = @PermissionId)
    BEGIN
        INSERT INTO RolePermissions (RoleId, PermissionId) VALUES (@ManagerRoleId, @PermissionId);
    END
END

IF EXISTS (SELECT * FROM Permissions WHERE PermissionName = 'tasks.assign')
BEGIN
    SELECT @PermissionId = PermissionId FROM Permissions WHERE PermissionName = 'tasks.assign';
    IF NOT EXISTS (SELECT * FROM RolePermissions WHERE RoleId = @ManagerRoleId AND PermissionId = @PermissionId)
    BEGIN
        INSERT INTO RolePermissions (RoleId, PermissionId) VALUES (@ManagerRoleId, @PermissionId);
    END
END

-- User permissions
IF EXISTS (SELECT * FROM Permissions WHERE PermissionName = 'tasks.create')
BEGIN
    SELECT @PermissionId = PermissionId FROM Permissions WHERE PermissionName = 'tasks.create';
    IF NOT EXISTS (SELECT * FROM RolePermissions WHERE RoleId = @UserRoleId AND PermissionId = @PermissionId)
    BEGIN
        INSERT INTO RolePermissions (RoleId, PermissionId) VALUES (@UserRoleId, @PermissionId);
    END
END

-- Create default admin user if one doesn't exist
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'admin@taskmanager.com')
BEGIN
    DECLARE @AdminPasswordHash NVARCHAR(MAX) = '$2a$11$AUX8XXJvTnrZN5HBSCYee.6qCGQKOPlXGYE8vG9cKSj3vH3DeyUDK'; -- Hashed password for 'Admin123!'
    
    INSERT INTO Users (Email, PasswordHash, Name, IsActive, CreatedAt, CreatedBy)
    VALUES ('admin@taskmanager.com', @AdminPasswordHash, 'Administrator', 1, GETUTCDATE(), 'System');
    
    DECLARE @AdminUserId INT;
    SELECT @AdminUserId = UserId FROM Users WHERE Email = 'admin@taskmanager.com';
    
    -- Assign admin role
    INSERT INTO UserRoles (UserId, RoleId, CreatedAt, CreatedBy)
    VALUES (@AdminUserId, @AdminRoleId, GETUTCDATE(), 'System');
END