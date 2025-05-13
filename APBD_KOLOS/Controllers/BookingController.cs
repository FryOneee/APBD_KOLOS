// Controllers/WarehouseController.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using APBD_KOLOS.Models;
using APBD_KOLOS.Models.DTOs;
using APBD_KOLOS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;


namespace APBD_KOLOS.Controllers
{
    [Route("api/bookings")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        public BookingController(IBookingService bookingService)
            => _bookingService = bookingService;

        [HttpGet("{id}")]
        public async Task<IActionResult> getBooking(int id)
        {
            try
            {
                var rents = await _bookingService.GetBookingFromId(id);
                return Ok(rents);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { error = "Database error: " + ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Unexpected error: " + ex.Message });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> addReservation(AddResevationReques request)
        {
            try
            {
                var rents = await _bookingService.AddRezervation(request);
                return Ok(rents);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { error = "Database error: " + ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Unexpected error: " + ex.Message });
            }
        }

        
        
    }
}