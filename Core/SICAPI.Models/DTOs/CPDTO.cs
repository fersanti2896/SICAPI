
namespace SICAPI.Models.DTOs;

public class CPDTO
{
    public int id_postalCodes { get; set; }
    public string d_codigo { get; set; }
    public string c_estado { get; set; }
    public string d_estado { get; set; }
    public string c_mnpio { get; set; }
    public string D_mnpio { get; set; }
    public List<CPInfoDTO> neighborhoods { get; set; }
}
