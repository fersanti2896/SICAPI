using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SICAPI.Data.SQL.Audit;
public class DeletetionLogDetailsAuditor : ChangeLogDetailsAuditor
{
    public DeletetionLogDetailsAuditor(EntityEntry dbEntry, AuditLog log) : base(dbEntry, log)
    {
    }

    protected override bool IsValueChanged(string propertyName)
    {
        var propertyType = DbEntry.Entity.GetType().GetProperty(propertyName);
        object defaultValue = null;
        try
        {
            defaultValue = propertyType.GetValue(propertyName);
        }
        catch { }
        if (defaultValue == null)
        {
            return false;
        }
        object orginalvalue = OriginalValue(propertyName);

        Comparator comparator = ComparatorFactory.GetComparator(propertyType.PropertyType);

        return !comparator.AreEqual(defaultValue, orginalvalue);
    }

    protected override object CurrentValue(string propertyName)
    {
        return null;
    }
}
