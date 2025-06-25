namespace SICAPI.Data.SQL.Audit;
public interface IAuditableEntity
{
    DateTime CreateDate { get; set; }
    DateTime? UpdateDate { get; set; }
    int CreateUser { get; set; }
    int? UpdateUser { get; set; }

}
