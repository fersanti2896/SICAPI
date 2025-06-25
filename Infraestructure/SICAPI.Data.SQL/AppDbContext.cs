
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SICAPI.Data.SQL.Audit;

namespace SICAPI.Data.SQL;

public partial class AppDbContext : DbContext
{
    private readonly IHttpContextAccessor? HttpContextAccessor;

    public readonly Int64? IdUser;
    public AppDbContext(DbContextOptions<AppDbContext> options)
    : base(options)
    {
    }

    public void AuditChanges(object userName, string IP)
    {
        var objectChanges = ChangeTracker.Entries().Where(p => p.State == EntityState.Deleted || p.State == EntityState.Modified);
        foreach (EntityEntry ent in objectChanges)
        {
            var eventType = (ent.State == EntityState.Added) ?
                EventType.Added : (ent.State == EntityState.Deleted) ? EventType.Deleted : EventType.Modified;

            AuditLog record = CreateLogRecord(userName, eventType, ent, IP);

            if (record != null)
            {
                ChangeTracker.Context.Add(record);
            }
        }
    }
    internal AuditLog CreateLogRecord(object userName, EventType eventType, EntityEntry entry, string IP)
    {
        Type entityType = entry.Entity.GetType();

        //Buscamos el atributo de TrackChanges
        TrackChangesAttribute trackChangesAttribute = entityType.GetCustomAttributes(true).OfType<TrackChangesAttribute>().SingleOrDefault();
        bool value = trackChangesAttribute != null;

        if (!value)
            return null;

        DateTime changeTime = DateTime.UtcNow;

        //Buscamos el nombre del campo de la llave primaria
        var pkName = (entry).Metadata.FindPrimaryKey().Properties.Select(s => new { s.Name }).FirstOrDefault();
        //Recuperamos el valor de la llave primaria
        var values = (entry).Property(pkName.Name).OriginalValue;
        if (userName == null) { userName = "default"; }
        var newlog = new AuditLog
        {
            UserID = userName.ToString(),
            EventDateUTC = changeTime,
            EventType = eventType,
            TypeFullName = entityType.FullName,
            RecordId = values.ToString(),
            IP = IP
        };

        var detailsAuditor = GetDetailsAuditor(eventType, newlog, entry);

        newlog.LogDetails = detailsAuditor.CreateLogDetails().ToList();

        if (newlog.LogDetails.Any())
            return newlog;
        else
            return null;
    }

    private ChangeLogDetailsAuditor GetDetailsAuditor(EventType eventType, AuditLog newlog, EntityEntry entry)
    {
        switch (eventType)
        {
            case EventType.Added:
                return new AdditionLogDetailsAuditor(entry, newlog);

            case EventType.Deleted:
                return new DeletetionLogDetailsAuditor(entry, newlog);

            case EventType.Modified:
                return new ChangeLogDetailsAuditor(entry, newlog);

            default:
                return null;
        }
    }
}
