// Program.cs
using APBD_KOLOS.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IRentService, RentService>();
builder.Services.AddOpenApi();

var app = builder.Build();

InitializeDatabase();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();
app.Run();

static void InitializeDatabase()
{
    const string _csMaster =
        "Data Source=localhost;" +
        "User ID=SA;" +
        "Password=yourStrong(9)Password;" +
        "Initial Catalog=master;" +
        "Integrated Security=False;" +
        "Connect Timeout=30;" +
        "Encrypt=False;" +
        "TrustServerCertificate=True;";

    const string createDbScript = @"
IF DB_ID('APBD8_2') IS NULL
BEGIN
    CREATE DATABASE APBD8_2;
END;";

    const string schemaScript = @"
DROP TABLE IF EXISTS dbo.Rental_Item;
DROP TABLE IF EXISTS dbo.Rental;
DROP TABLE IF EXISTS dbo.Status;
DROP TABLE IF EXISTS dbo.Movie;
DROP TABLE IF EXISTS dbo.Customer;

-- Create tables
CREATE TABLE dbo.Customer (
    customer_id INT NOT NULL PRIMARY KEY,
    first_name NVARCHAR(100) NOT NULL,
    last_name  NVARCHAR(200) NOT NULL
);

CREATE TABLE dbo.Movie (
    movie_id     INT NOT NULL PRIMARY KEY,
    title        NVARCHAR(200) NOT NULL,
    release_date DATETIME NOT NULL,
    price_per_day DECIMAL(10,2) NOT NULL
);

CREATE TABLE dbo.Status (
    status_id INT NOT NULL PRIMARY KEY,
    name      NVARCHAR(200) NOT NULL
);

CREATE TABLE dbo.Rental (
    rental_id   INT NOT NULL PRIMARY KEY,
    rental_date DATETIME NOT NULL,
    return_date DATETIME NULL,
    customer_id INT NOT NULL,
    status_id   INT NOT NULL
);

CREATE TABLE dbo.Rental_Item (
    rental_id        INT NOT NULL,
    movie_id         INT NOT NULL,
    price_at_rental  DECIMAL(10,2) NOT NULL,
    CONSTRAINT PK_Rental_Item PRIMARY KEY (rental_id, movie_id)
);

-- Add foreign keys
ALTER TABLE dbo.Rental
    ADD CONSTRAINT FK_Rental_Customer FOREIGN KEY (customer_id) REFERENCES dbo.Customer(customer_id),
        CONSTRAINT FK_Rental_Status   FOREIGN KEY (status_id)   REFERENCES dbo.Status(status_id);

ALTER TABLE dbo.Rental_Item
    ADD CONSTRAINT FK_RentalItem_Rental FOREIGN KEY (rental_id) REFERENCES dbo.Rental(rental_id),
        CONSTRAINT FK_RentalItem_Movie  FOREIGN KEY (movie_id)  REFERENCES dbo.Movie(movie_id);

-- Seed data
INSERT INTO dbo.Status (status_id, name) VALUES
    (1, N'Rented'), (2, N'Returned'), (3, N'Late');

INSERT INTO dbo.Customer (customer_id, first_name, last_name) VALUES
    (1, N'Alice', N'Johnson'),
    (2, N'Bob',   N'Smith'),
    (3, N'Charlie', N'Davis');

INSERT INTO dbo.Movie (movie_id, title, release_date, price_per_day) VALUES
    (1, N'Inception',       '2010-07-16', 3.99),
    (2, N'The Matrix',      '1999-03-31', 2.99),
    (3, N'Interstellar',    '2014-11-07', 4.49),
    (4, N'The Godfather',   '1972-03-24', 2.49),
    (5, N'Avengers: Endgame','2019-04-26',4.99);

INSERT INTO dbo.Rental (rental_id, rental_date, return_date, customer_id, status_id) VALUES
    (1001, '2025-04-25T10:00:00', '2025-04-28T15:30:00', 1, 2),
    (1002, '2025-05-01T14:00:00', NULL,                 2, 1),
    (1003, '2025-04-30T18:45:00', '2025-05-03T10:00:00', 3, 2),
    (1004, '2025-05-03T12:15:00', NULL,                 1, 1);

INSERT INTO dbo.Rental_Item (rental_id, movie_id, price_at_rental) VALUES
    (1001, 1, 3.99),
    (1001, 2, 2.99),
    (1002, 3, 4.49),
    (1003, 4, 2.49),
    (1004, 5, 4.99);

";

    using var conn = new SqlConnection(_csMaster);
    conn.Open();
    using (var cmd = new SqlCommand(createDbScript, conn))
        cmd.ExecuteNonQuery();

    conn.ChangeDatabase("APBD8_2");

    using (var cmd = new SqlCommand(schemaScript, conn))
        cmd.ExecuteNonQuery();
}