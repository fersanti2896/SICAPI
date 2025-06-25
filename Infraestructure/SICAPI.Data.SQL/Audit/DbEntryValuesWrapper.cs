using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SICAPI.Data.SQL.Audit;

public class DbEntryValuesWrapper
{
    protected readonly EntityEntry _dbEntry;
    private PropertyValues _entryValues = null;

    private PropertyValues EntryPropertyValues => _entryValues ?? (_entryValues = _dbEntry.GetDatabaseValues());

    public DbEntryValuesWrapper(EntityEntry dbEntry)
    {
        _dbEntry = dbEntry;
    }

    public object OriginalValue(string propertyName)
    {
        object originalValue = null;

        originalValue = _dbEntry.Property(propertyName).OriginalValue;

        return originalValue;
    }
}
