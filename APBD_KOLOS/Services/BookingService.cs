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
    b.date       AS booking_date,
    g.first_name                   AS guest_first,
    g.last_name                    AS guest_last,
    g.date_of_birth                AS guest_dob,
    e.first_name                   AS emp_first,
    e.last_name                    AS emp_last,
    e.employee_number              AS emp_number,
    a.name                         AS attraction_name,
    a.price                        AS attraction_price,
    ba.amount                      AS attraction_amount
FROM Guest g
JOIN Booking b      ON g.guest_id = b.guest_id
JOIN Employee e     ON b.employee_id = e.employee_id
JOIN Booking_Attraction ba ON b.booking_id = ba.booking_id
JOIN Attraction a   ON ba.attraction_id = a.attraction_id
WHERE b.booking_id = @booking_id;
";

            await using var conn = new SqlConnection(_cs);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@booking_id", bookingId);

            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                if (bookingInfo == null)
                {
                    bookingInfo = new infoReservationDTO
                    {
                        date = reader.GetDateTime(reader.GetOrdinal("booking_date")),
                        guest = new guestDTO
                        {
                            firstName    = reader.GetString(reader.GetOrdinal("guest_first")),
                            lastName     = reader.GetString(reader.GetOrdinal("guest_last")),
                            dateOfBirth  = reader.GetDateTime(reader.GetOrdinal("guest_dob"))
                        },
                        employee = new employeeDTO
                        {
                            firstName       = reader.GetString(reader.GetOrdinal("emp_first")),
                            lastName        = reader.GetString(reader.GetOrdinal("emp_last")),
                            employeeNumber  = reader.GetString(reader.GetOrdinal("emp_number"))
                        },
                        attractions = new List<attractionDTO>()
                    };
                }

                bookingInfo.attractions.Add(new attractionDTO
                {
                    name   = reader.GetString(reader.GetOrdinal("attraction_name")),
                    price  = reader.GetDecimal(reader.GetOrdinal("attraction_price")),
                    amount = reader.GetInt32(reader.GetOrdinal("attraction_amount"))
                });
            }

            return bookingInfo;
        }

        public async Task<int> AddRezervation(AddResevationReques request)
        {
            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            try
            {
                int employeeId;
                await using (var cmdEmp = new SqlCommand(
                    "SELECT employee_id FROM Employee WHERE employee_number = @emp_num", conn, tx))
                {
                    cmdEmp.Parameters.AddWithValue("@emp_num", request.employeeNumber);
                    var empObj = await cmdEmp.ExecuteScalarAsync();
                    if (empObj == null)
                        throw new KeyNotFoundException("Employee nie istnieje.");
                    employeeId = (int)empObj;
                }

                await using (var cmdGuest = new SqlCommand(
                    "SELECT 1 FROM Guest WHERE guest_id = @guest_id", conn, tx))
                {
                    cmdGuest.Parameters.AddWithValue("@guest_id", request.guestId);
                    var exists = (int?)await cmdGuest.ExecuteScalarAsync() == 1;
                    if (!exists)
                        throw new KeyNotFoundException("Guest nie istnieje.");
                }

                await using (var cmdIns = new SqlCommand(
                    @"INSERT INTO Booking (guest_id, employee_id, date)
                      VALUES (@guest_id, @emp_id, @date);
                      SELECT SCOPE_IDENTITY();", conn, tx))
                {
                    cmdIns.Parameters.AddWithValue("@guest_id", request.guestId);
                    cmdIns.Parameters.AddWithValue("@emp_id", employeeId);
                    cmdIns.Parameters.AddWithValue("@date", DateTime.UtcNow);
                    var newIdObj = await cmdIns.ExecuteScalarAsync();
                    if (newIdObj == null)
                        throw new Exception("Nie udało się dodać rezerwacji.");
                    request.bookingId = Convert.ToInt32(newIdObj);
                }

                foreach (var attr in request.attractions)
                {
                    int attractionId;
                    await using (var cmdAtr = new SqlCommand(
                        "SELECT attraction_id FROM Attraction WHERE name = @name", conn, tx))
                    {
                        cmdAtr.Parameters.AddWithValue("@name", attr.name);
                        var atrObj = await cmdAtr.ExecuteScalarAsync();
                        if (atrObj == null)
                            throw new KeyNotFoundException($"Atrakcja '{attr.name}' nie istnieje.");
                        attractionId = (int)atrObj;
                    }

                    await using (var cmdInsBa = new SqlCommand(
                        @"INSERT INTO Booking_Attraction (booking_id, attraction_id, amount)
                          VALUES (@booking_id, @attraction_id, @amount);", conn, tx))
                    {
                        cmdInsBa.Parameters.AddWithValue("@booking_id", request.bookingId);
                        cmdInsBa.Parameters.AddWithValue("@attraction_id", attractionId);
                        cmdInsBa.Parameters.AddWithValue("@amount", attr.amount);
                        await cmdInsBa.ExecuteNonQueryAsync();
                    }
                }

                tx.Commit();
                return request.bookingId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}