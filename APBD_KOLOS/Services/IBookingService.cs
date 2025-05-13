using APBD_KOLOS.Models.DTOs;

namespace APBD_KOLOS.Services
{
    public interface IBookingService
    {
        Task<infoReservationDTO> GetBookingFromId(int id);
        Task<int> AddRezervation(AddResevationReques reservation);
    }
}