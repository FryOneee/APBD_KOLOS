namespace APBD_KOLOS.Models.DTOs;

public class AddResevationReques
{
    public int bookingId { get; set; }
    public int guestId { get; set; }
    public string employeeNumber { get; set; }
    public List<AtractionFromReservation> attractions { get; set; }
}