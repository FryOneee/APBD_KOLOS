using APBD_KOLOS_2.Models.DTOs;

namespace APBD_KOLOS_2.Services
{
    public interface IRentService
    {
        Task<ConsumerRentDTO> getRental(int userId);
        Task<int> addRental(int userId,AddRequest rental);
    }
}