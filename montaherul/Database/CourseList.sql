USE [Teacher]
GO
/****** Object:  StoredProcedure [dbo].[Get_All_Driver]    Script Date: 11/6/2025 10:34:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- exec Get_All_Course '',10,1
ALTER PROCEDURE [dbo].[Get_All_Course]  
(
	
    @Search NVARCHAR(255) = NULL,
    @DisplayLength INT,
    @DisplayStart INT
)
AS
BEGIN
    DECLARE @FirstRec INT, @LastRec INT;

    SET @FirstRec = (@DisplayStart - 1) * @DisplayLength + 1;
    SET @LastRec = @DisplayStart * @DisplayLength;

    DECLARE @SQLQuery NVARCHAR(MAX);
    
    SET @SQLQuery = '
    WITH CTE_Course_LIST AS
    (
        SELECT 
            ROW_NUMBER() OVER (ORDER BY c.id desc) AS RowNum, 
            c.*, t.name AS TeacherName,
            COUNT(*) OVER() AS TOTALCOUNT
        from Courses c
        LEFT OUTER JOIN TeacherModel t ON t.id=c.Teacherid
        WHERE 1=1
    ';

    IF @Search IS NOT NULL AND @Search <> ''
    BEGIN
        SET @SQLQuery = @SQLQuery + ' AND (' + @Search + ')'
    END

    SET @SQLQuery = @SQLQuery + '
    )
    SELECT * FROM CTE_Course_LIST
    WHERE RowNum >= @FirstRec AND RowNum <= @LastRec ORDER BY RowNum asc;';

    PRINT @SQLQuery;

    EXEC sp_executesql 
        @SQLQuery,
        N'@FirstRec INT, @LastRec INT ',
        @FirstRec = @FirstRec, 
        @LastRec = @LastRec;
		
END
