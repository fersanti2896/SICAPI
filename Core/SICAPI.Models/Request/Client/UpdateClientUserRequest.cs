namespace SICAPI.Models.Request.Client;

public class UpdateClientUserRequest
{
    public int ClientId { get; set; }
    public int NewUserId { get; set; } // ID del nuevo vendedor/repartidor asignado
}
