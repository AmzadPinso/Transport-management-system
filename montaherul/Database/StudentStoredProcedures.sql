ALTER PROCEDURE dbo.GetStudentsPagedWithSearch
    @PageNumber INT,
    @PageSize INT,
    @Search NVARCHAR(100) = NULL,
    @SortColumn NVARCHAR(50) = 'Id',
    @SortDirection NVARCHAR(4) = 'ASC'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SQL NVARCHAR(MAX);

    SET @SQL = '
    SELECT 
        s.Id,
        s.Name,
        s.Age,
        s.Email,
        s.Address,
        s.CourseId,
        c.CourseName
    FROM Students s
    LEFT JOIN Courses c ON s.CourseId = c.Id
    WHERE 
        (@Search IS NULL OR @Search = '''' OR
         s.Name LIKE ''%'' + @Search + ''%'' OR
         s.Email LIKE ''%'' + @Search + ''%'' OR
         s.Address LIKE ''%'' + @Search + ''%'')
    ORDER BY ' + QUOTENAME(@SortColumn) + ' ' + @SortDirection + '
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) FROM Students
    WHERE 
        (@Search IS NULL OR @Search = '''' OR
         Name LIKE ''%'' + @Search + ''%'' OR
         Email LIKE ''%'' + @Search + ''%'' OR
         Address LIKE ''%'' + @Search + ''%'')
    ';

    EXEC sp_executesql @SQL,
        N'@Search NVARCHAR(100), @PageNumber INT, @PageSize INT',
        @Search, @PageNumber, @PageSize;
END