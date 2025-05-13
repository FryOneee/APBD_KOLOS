namespace APBD_KOLOS.Models.DTOs;

public class infoReservationDTO
{
    public DateTime date { get; set; }
    public guestDTO guest { get; set; }
    public employeeDTO employee { get; set; }
    public List<attractionDTO> attractions { get; set; }
    
}