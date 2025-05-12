// Controllers/WarehouseController.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using APBD_KOLOS_2.Models;
using APBD_KOLOS_2.Models.DTOs;
using APBD_KOLOS_2.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;


namespace APBD_KOLOS_2.Controllers
{
    [Route("api/consumers")]
    [ApiController]
    public class RentController : ControllerBase
    {
        private readonly IRentService _rentService;
        public RentController(IRentService rentService)
            => _rentService = rentService;

        [HttpGet("{id}/rentals")]
        public async Task<IActionResult> getRent(int id)
        {
            try
            {
                var rents = await _rentService.getRental(id);
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
        
        [HttpPost("{id}/rentals")]
        public async Task<IActionResult> addRental(int id,AddRequest request)
        {
            try
            {
                var rents = await _rentService.addRental(id,request);
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