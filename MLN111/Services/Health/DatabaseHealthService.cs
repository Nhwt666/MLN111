using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using MLN111.Data;
using MLN111.Dto.Health;

namespace MLN111.Services.Health;

public sealed class DatabaseHealthService : IDatabaseHealthService
{
    private readonly AppDbContext _db;

    public DatabaseHealthService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DatabaseHealthResponse> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var ok = await _db.Database.CanConnectAsync(cancellationToken);
            sw.Stop();
            return new DatabaseHealthResponse(ok, sw.ElapsedMilliseconds, ok ? null : "Không kết nối tới PostgreSQL.");
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new DatabaseHealthResponse(false, sw.ElapsedMilliseconds, ex.Message);
        }
    }
}
