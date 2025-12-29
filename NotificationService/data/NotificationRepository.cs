using Dapper;
using Microsoft.Data.SqlClient;
using NotificationService.models;

public class NotificationRepository
{
    private readonly string _conn;
    public NotificationRepository(string conn) => _conn = conn;

    public async Task<List<Notification>> GetPendingAsync(int take = 50)
    {
        const string sql = @"
            SELECT TOP (@Take)
                Id,
                AppointmentId,
                PatientId,
                DoctorId,
                Status,
                Message,
                CreatedAt,
                Published,
                DeletionDate
            FROM dbo.Notifications
            WHERE Published = 0
            ORDER BY CreatedAt ASC;";

        using var c = new SqlConnection(_conn);
        var rows = await c.QueryAsync<Notification>(sql, new { Take = take });
        return rows.AsList();
    }

    public async Task MarkPublishedAsync(long id)
    {
        const string sql = @"
            UPDATE dbo.Notifications
            SET Published = 1
            WHERE Id = @Id;";

        using var c = new SqlConnection(_conn);
        await c.ExecuteAsync(sql, new { Id = id });
    }
}
