namespace APBD_KOLOS_2.Models.DTOs;

public class RentalsDTO
{
    public int id { get; set; }
    public DateTime rentalDate { get; set; }
    public DateTime? returnDate { get; set; }
    public string status { get; set; }
    public List<MovieDTO> movies { get; set; }
}