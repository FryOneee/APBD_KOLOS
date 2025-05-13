// Services/WarehouseService.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using APBD_KOLOS.Models;
using APBD_KOLOS.Models.DTOs;

namespace APBD_KOLOS.Services
{
    public class RentService : IRentService
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

        public async Task<ConsumerRentDTO> getRental(int customerId)
        {

            try
            {
                ConsumerRentDTO rental = null;
                const string sql = @"
select c.first_name,
 c.last_name, 
 r.rental_id,
 r.rental_date,
 r.return_date,
 s.name,
 ri.price_at_rental,
 m.title
from Customer c
 inner join Rental r on c.customer_id=r.customer_id
 inner join Status s on r.status_id=s.status_id
 inner join Rental_Item ri on r.rental_id=ri.rental_id
 inner join Movie m on m.movie_id=ri.movie_id
 where c.customer_id=@customerId";

                
                await using var conn = new SqlConnection(_cs);
                await using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                cmd.Parameters.AddWithValue("@name", "Rented");
                await conn.OpenAsync();
                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var id = reader.GetInt32(reader.GetOrdinal("rental_id"));
                    var title = reader.GetString(reader.GetOrdinal("title"));
                    var price = reader.GetDecimal(reader.GetOrdinal("price_at_rental"));

                    if (rental == null)
                    {
                        rental = new ConsumerRentDTO
                        {
                            firstName = reader.GetString(reader.GetOrdinal("first_name")),
                            lastName = reader.GetString(reader.GetOrdinal("last_name")),
                            rentals = new List<RentalsDTO>()
                        };
                    }

                    var rentalDto = rental.rentals.Find(r => r.id == id);

                    if (rentalDto == null)
                    {
                        rentalDto = new RentalsDTO
                        {
                            id = reader.GetInt32(reader.GetOrdinal("rental_id")),
                            rentalDate = reader.GetDateTime(reader.GetOrdinal("rental_date")),
                            returnDate = reader.IsDBNull(reader.GetOrdinal("return_date"))
                                ? null
                                : reader.GetDateTime(reader.GetOrdinal("return_date")),
                            status = reader.GetString(reader.GetOrdinal("name")),
                            movies = new List<MovieDTO>()
                        };
                        rental.rentals.Add(rentalDto);


                    }


                    rentalDto.movies.Add(new MovieDTO
                    {
                        title = reader.GetString(reader.GetOrdinal("title")),
                        priceAtRental = reader.GetDecimal(reader.GetOrdinal("price_at_rental")),
                    });


                }

                return rental;
            }
            catch
            {
                throw;
            }
        }


        public async Task<int> addRental(int userId, AddRequest request)
        {
            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            int newId;

            // czy klien istnieje
            try
            {


                using (var cmd = new SqlCommand(
                           "SELECT COUNT(1) FROM Customer WHERE customer_id=@customer_id",
                           conn, tx))
                {
                    cmd.Parameters.AddWithValue("@customer_id", userId);
                    bool exist = (int)await cmd.ExecuteScalarAsync() > 0;
                    if (!exist)
                        throw new KeyNotFoundException("user doesnt exist");
                }




                foreach (var movie in request.movies)
                {
                    using (var cmd = new SqlCommand(
                               "SELECT COUNT(1) FROM Movie WHERE title=@title",
                               conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@title", movie.title);
                        bool exist = (int)await cmd.ExecuteScalarAsync() > 0;
                        if (!exist)
                            throw new KeyNotFoundException("movie doesnt exist");
                    }


                    const string sql = @"
insert into rental (rental_id,rental_date,return_date,customer_id,status_id) values 
                          (@customer_id, @rental_date, @retuen_date, @customer_id,@status_id)";

                    using (var cmd = new SqlCommand(
                               @"insert into rental (rental_id,rental_date,return_date,customer_id,status_id) 
values  (@rental_id, @rental_date, @retuen_date, @customer_id,@status_id);",
                               conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@rental_id", request.id);
                        cmd.Parameters.AddWithValue("@rental_date", request.RentalDate);
                        cmd.Parameters.AddWithValue("@retuen_date", null);
                        cmd.Parameters.AddWithValue("@customer_id", userId);
                        cmd.Parameters.AddWithValue("@status_id", "Rented");
                        newId = (int)await cmd.ExecuteScalarAsync();
                    }

                }
                tx.Commit();
                return request.id;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}


