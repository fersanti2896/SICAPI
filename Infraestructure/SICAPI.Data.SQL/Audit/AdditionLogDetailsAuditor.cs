
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using SICAPI.Data.SQL.Audit;

public class AdditionLogDetailsAuditor : ChangeLogDetailsAuditor
{
    public AdditionLogDetailsAuditor(EntityEntry dbEntry, AuditLog log) : base(dbEntry, log)
    {
    }

    /// <summary>
    /// Treat unchanged entries as added entries when creating audit records.
    /// </summary>
    /// <returns></returns>
    protected internal override EntityState StateOfEntity()
    {
        if (DbEntry.State == EntityState.Unchanged)
        {
            return EntityState.Added;
        }

        return base.StateOfEntity();
    }

    protected override bool IsValueChanged(string propertyName)
    {
        var propertyType = DbEntry.Entity.GetType().GetProperty(propertyName);
        object defaultValue = propertyType.GetValue(propertyName);
        object currentValue = CurrentValue(propertyName);

        Comparator comparator = ComparatorFactory.GetComparator(propertyType.PropertyType);

        return !comparator.AreEqual(defaultValue, currentValue);
    }

    protected override object OriginalValue(string propertyName)
    {
        return null;
    }
}
