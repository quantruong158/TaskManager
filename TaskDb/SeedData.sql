-- Database initialization script for TaskManager

-- Insert default roles if they don't exist
IF NOT EXISTS (SELECT * FROM Roles WHERE RoleName = 'Admin')
BEGIN
    INSERT INTO Roles (RoleName, Description) VALUES ('Admin', 'Administrator');
END

IF NOT EXISTS (SELECT * FROM Roles WHERE RoleName = 'User')
BEGIN
    INSERT INTO Roles (RoleName, Description) VALUES ('User', 'Regular user');
END

-- Create default admin user if one doesn't exist
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'admin@gmail.com')
BEGIN
    DECLARE @AdminRoleId INT;
    SELECT @AdminRoleId = RoleId FROM Roles WHERE RoleName = 'Admin';
    DECLARE @AdminPasswordHash NVARCHAR(MAX) = '$2a$11$aqs6/z/8jCS7q/vJ1h5v5emzgc/be52rG29ES94KE2TEpq353vbE.'; -- Hashed password for 'Admin123!'
    
    INSERT INTO Users (Email, PasswordHash, Name, IsActive, CreatedAt, CreatedBy)
    VALUES ('admin@gmail.com', @AdminPasswordHash, 'Administrator', 1, GETUTCDATE(), NULL);
    
    DECLARE @AdminUserId INT;
    SELECT @AdminUserId = UserId FROM Users WHERE Email = 'admin@gmail.com';
    
    -- Assign admin role
    INSERT INTO UserRoles (UserId, RoleId)
    VALUES (@AdminUserId, @AdminRoleId);
END