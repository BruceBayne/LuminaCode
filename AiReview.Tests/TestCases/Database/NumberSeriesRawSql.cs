/// <summary>
///	MaxRating: 10
/// ReviewPrompt: database
/// </summary>


public static long IncreaseNumberSeriesById(int numberSeriesId)
{
    const string sql = @"DECLARE @UpdatedID TABLE (CurrentNumber decimal(22,0));
            UPDATE [NumberSeries] SET CurrentNumber = 
            CASE  
                WHEN CurrentNumber + 1 > NumberSeriesEnd THEN NumberSeriesEnd
                ELSE CurrentNumber + 1 
            END
               ,IsActive = 
            CASE 
                WHEN NumberSeriesEnd IS NULL THEN 1 
                ELSE CASE 
                    WHEN CurrentNumber + 1 > NumberSeriesEnd THEN 0 
                    ELSE 1 
                END 
            END
            OUTPUT CASE 
                WHEN inserted.NumberSeriesEnd IS NULL THEN inserted.CurrentNumber 
                ELSE CASE 
                    WHEN inserted.IsActive=0 AND inserted.CurrentNumber = inserted.NumberSeriesEnd THEN inserted.CurrentNumber + 1
                    ELSE inserted.CurrentNumber 
                END 
            END INTO @UpdatedID
            WHERE IsActive=1 AND Id = @NumberSeriesId AND (NumberSeriesEnd IS NULL OR (NumberSeriesEnd IS NOT NULL AND CurrentNumber <= NumberSeriesEnd));
            SELECT TOP 1 * FROM @UpdatedID";

    //we set IsActive = false && CurrentNumber = NumberSeriesEnd when last number received and was equal to NumberSeriesEnd but CurrentNumber was incremented
    using (var connection = GetSqlConnection())
    {
        var command = new SqlCommand(sql, connection);

        command.Parameters.Add(new SqlParameter("NumberSeriesId", numberSeriesId));

        connection.Open();
        var result = command.ExecuteScalar();
        if (result == null)
        {
            throw new UserNotifyException(
                $"Valid Number series with Id {numberSeriesId} was not found for department");
        }

        return (long)(decimal)result;
    }
}