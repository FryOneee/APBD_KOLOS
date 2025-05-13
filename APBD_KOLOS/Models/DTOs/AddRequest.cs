namespace APBD_KOLOS.Models.DTOs;

public class AddRequest
{
    public int id { get; set; }
    public DateTime RentalDate { get; set; }
    public List<AddMovieDTO> movies { get; set; }
}