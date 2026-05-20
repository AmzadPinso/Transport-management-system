CREATE OR ALTER PROCEDURE dbo.GetUsersPagedWithSearch
    @PageNumber INT,
    @PageSize INT,
    @Search NVARCHAR(100) = NULL,
    @RoleId INT = NULL,
    @SortColumn NVARCHAR(50) = 'UserId',
    @SortDirection NVARCHAR(4) = 'ASC',
    @TotalRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Calculate total records
    SELECT @TotalRecords = COUNT(*) 
    FROM Users
    WHERE (@Search IS NULL OR @Search = '' OR
           UserName LIKE '%' + @Search + '%' OR
           Email LIKE '%' + @Search + '%' OR
           Address LIKE '%' + @Search + '%')
      AND (@RoleId IS NULL OR RoleId = @RoleId);

    -- Fetch paged data
    DECLARE @SQL NVARCHAR(MAX);
    SET @SQL = '
    SELECT 
        u.UserId,
        u.UserName,
        u.Email,
        u.PasswordHash,
        u.Address,
        u.RoleId,
        u.LastLogIN,
        u.IsEmailVerified,
        u.EmailVerificationToken,
        u.EmailVerificationTokenExpiry,
        u.PasswordResetOtp,
        u.PasswordResetOtpExpiry,
        u.PasswordResetOtpAttempts,
        u.CreatedAt,
        u.UpdatedAt
    FROM Users u
    WHERE (@Search IS NULL OR @Search = '''' OR
           u.UserName LIKE ''%'' + @Search + ''%'' OR
           u.Email LIKE ''%'' + @Search + ''%'' OR
           u.Address LIKE ''%'' + @Search + ''%'')
      AND (@RoleId IS NULL OR u.RoleId = @RoleId)
    ORDER BY ' + QUOTENAME(@SortColumn) + ' ' + @SortDirection + '
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;';

    EXEC sp_executesql @SQL,
        N'@Search NVARCHAR(100), @RoleId INT, @PageNumber INT, @PageSize INT',
        @Search, @RoleId, @PageNumber, @PageSize;
END
GO
