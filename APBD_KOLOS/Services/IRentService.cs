using APBD_KOLOS.Models.DTOs;

namespace APBD_KOLOS.Services
{
    public interface IRentService
    {
        Task<ConsumerRentDTO> getRental(int userId);
        Task<int> addRental(int userId,AddRequest rental);
    }
}