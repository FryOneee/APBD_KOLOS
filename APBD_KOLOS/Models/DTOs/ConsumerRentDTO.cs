namespace APBD_KOLOS.Models.DTOs;

public class ConsumerRentDTO
{
    public string firstName { get; set; }
    public string lastName { get; set; }
    public List<RentalsDTO> rentals { get; set; }
}