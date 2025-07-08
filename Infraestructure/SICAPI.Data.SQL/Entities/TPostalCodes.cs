
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SICAPI.Data.SQL.Entities;

[Table("tPostalCodes")]
public class TPostalCodes
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int id_postalCodes { get; set; }
    public string c_estado { get; set; }
    public string d_estado { get; set; }
    public string c_mnpio { get; set; }
    public string D_mnpio { get; set; }
    public string d_codigo { get; set; }
    public string d_asenta { get; set; }
    public string id_asenta_cpcons { get; set; }
    public string d_CP { get; set; }
}