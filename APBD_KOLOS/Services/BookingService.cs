// Services/BookingService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using APBD_KOLOS.Models;
using APBD_KOLOS.Models.DTOs;

namespace APBD_KOLOS.Services
{
    public class BookingService : IBookingService
    {
        private const string _cs =
            "Data Source=localhost;" +
            "User ID=SA;" +
            "Password=yourStrong(9)Password;" +
            "Initial Catalog=APBD8_2;" +
            "Integrated Security=False;" +
            "Connect Timeout=30;" +
            "Encrypt=False;" +
            "TrustServerCertificate=True;";

        public async Task<infoReservationDTO> GetBookingFromId(int bookingId)
        {
            infoReservationDTO bookingInfo = null;

            const string sql = @"
SELECT 
    b.date,
    g.first_name AS guest_first_name,
    g.last_name  AS guest_last_name,
    g.date_of_birth,
    e.first_name AS emp_first_name,
    e.last_name  AS emp_last_name,
    e.employee_number,
    a.name,
    a.price,
    ba.amount
FROM Guest g
INNER JOIN Booking b             ON g.guest_id = b.guest_id
INNER JOIN Employee e            ON b.employee_id = e.employee_id
INNER JOIN Booking_Attraction ba ON b.booking_id = ba.booking_id
INNER JOIN Attraction a          ON a.attraction_id = ba.attraction_id
WHERE b.booking_id = @booking_id;
";

            await using var conn = new SqlConnection(_cs);
            await using var cmd  = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@booking_id", bookingId);
            await conn.OpenAsync();

            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if (bookingInfo == null)
                {
                    bookingInfo = new infoReservationDTO
                    {
                        date        = reader.GetDateTime(reader.GetOrdinal("date")),
                        guest       = new guestDTO
                        {
                            firstName   = reader.GetString(reader.GetOrdinal("guest_first_name")),
                            lastName    = reader.GetString(reader.GetOrdinal("guest_last_name")),
                            dateOfBirth = reader.GetDateTime(reader.GetOrdinal("date_of_birth"))
                        },
                        employee    = new employeeDTO
                        {
                            firstName      = reader.GetString(reader.GetOrdinal("emp_first_name")),
                            lastName       = reader.GetString(reader.GetOrdinal("emp_last_name")),
                            employeeNumber = reader.GetString(reader.GetOrdinal("employee_number"))
                        },
                        attractions = new List<attractionDTO>()
                    };
                }

                bookingInfo.attractions.Add(new attractionDTO
                {
                    name   = reader.GetString(reader.GetOrdinal("name")),
                    price  = reader.GetDecimal(reader.GetOrdinal("price")),
                    amount = reader.GetInt32(reader.GetOrdinal("amount"))
                });
            }

            return bookingInfo;
        }

        public async Task<int> AddRezervation(AddResevationReques request)
        {
            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();
            await using var tx = (SqlTransaction)await conn.BeginTransactionAsync();

            try
            {
                int employee_number;
                using (var cmd = new SqlCommand(
                           "SELECT COUNT(1) FROM Employee WHERE employee_number = @employee_number",
                           conn, tx))
                {
                    cmd.Parameters.AddWithValue("@employee_number", request.employeeNumber);
                    var exists = (int)await cmd.ExecuteScalarAsync() > 0;
                    if (!exists)
                        throw new KeyNotFoundException("Employee doesn't exist.");
                }

                using (var cmd = new SqlCommand(
                           "SELECT COUNT(1) FROM Guest WHERE guest_id = @guest_id",
                           conn, tx))
                {
                    cmd.Parameters.AddWithValue("@guest_id", request.guestId);
                    var exists = (int)await cmd.ExecuteScalarAsync() > 0;
                    if (!exists)
                        throw new KeyNotFoundException("Guest doesn't exist.");
                }
                
                using (var cmd = new SqlCommand(
                           "SELECT employee_id FROM Employee WHERE employee_number = @employee_number",
                           conn, tx))
                {
                    cmd.Parameters.AddWithValue("@employee_number", request.employeeNumber);
                    var exists = (string)await cmd.ExecuteScalarAsync();
                    employee_number=exists?.Length??0;
                    if (exists.Length==0)
                        throw new KeyNotFoundException("Guest doesn't exist.");
                }


                int newBookingId;
                using (var cmd = new SqlCommand(
                           @"
INSERT INTO Booking (guest_id, employee_id, date)
VALUES (@guest_id, @employee_id, @date);

",
                           conn, tx))
                {
                    cmd.Parameters.AddWithValue("@guest_id",   request.guestId);
                    cmd.Parameters.AddWithValue("@employee_id", employee_number);
                    cmd.Parameters.AddWithValue("@date",        DateTime.UtcNow);

                    newBookingId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                foreach (var attraction in request.attractions)
                {
                    int attrId;
                    using (var cmd = new SqlCommand(
                               "SELECT attraction_id FROM Attraction WHERE name = @name",
                               conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@name", attraction.name);
                        var result = await cmd.ExecuteScalarAsync();
                        if (result == null)
                            throw new KeyNotFoundException($"Attraction  doesn't exist.");
                        attrId = (int)result;
                    }

                    using (var cmd = new SqlCommand(
                               @"
INSERT INTO Booking_Attraction (booking_id, attraction_id, amount)
VALUES (@booking_id, @attraction_id, @amount);
",
                               conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@booking_id",    newBookingId);
                        cmd.Parameters.AddWithValue("@attraction_id", attrId);
                        cmd.Parameters.AddWithValue("@amount",        attraction.amount);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                await tx.CommitAsync();
                return newBookingId;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }
}