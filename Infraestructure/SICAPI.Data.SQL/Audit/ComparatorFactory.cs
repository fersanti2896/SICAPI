
namespace SICAPI.Data.SQL.Audit;

internal static class ComparatorFactory
{
    internal static Comparator GetComparator(Type type)
    {

        if (type == typeof(DateTime?))
        {
            return new NullableDateComparator();
        }

        if (type == typeof(DateTime))
        {
            return new DateComparator();
        }

        if (type == typeof(string))
        {
            return new StringComparator();
        }

        if (type == null)
        {
            return new NullableComparator();
        }

        if (type.IsValueType)
        {
            return new ValueTypeComparator();
        }

        return new Comparator();
    }
}
