// Program.cs
using APBD_KOLOS.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IBookingService, BookingService>();
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

    const string dropDbScript = @"
IF DB_ID('APBD8_2') IS NOT NULL
BEGIN
    ALTER DATABASE APBD8_2 SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE APBD8_2;
END;
";

    const string createDbScript = @"
CREATE DATABASE APBD8_2;
";

    const string schemaAndSeedScript = @"
DROP TABLE IF EXISTS dbo.Booking_Attraction;
DROP TABLE IF EXISTS dbo.Booking;
DROP TABLE IF EXISTS dbo.Attraction;
DROP TABLE IF EXISTS dbo.Employee;
DROP TABLE IF EXISTS dbo.Guest;

-- Create tables
CREATE TABLE dbo.Attraction (
    attraction_id INT NOT NULL PRIMARY KEY,
    name NVARCHAR(100) NOT NULL,
    price DECIMAL(10,2) NOT NULL
);

CREATE TABLE dbo.Booking (
    booking_id INT NOT NULL PRIMARY KEY,
    guest_id INT NOT NULL,
    employee_id INT NOT NULL,
    date DATETIME NOT NULL
);

CREATE TABLE dbo.Booking_Attraction (
    booking_id INT NOT NULL,
    attraction_id INT NOT NULL,
    amount INT NOT NULL,
    PRIMARY KEY (booking_id, attraction_id)
);

CREATE TABLE dbo.Employee (
    employee_id INT NOT NULL PRIMARY KEY,
    first_name NVARCHAR(100) NOT NULL,
    last_name NVARCHAR(100) NOT NULL,
    employee_number NVARCHAR(22) NOT NULL
);

CREATE TABLE dbo.Guest (
    guest_id INT NOT NULL PRIMARY KEY,
    first_name NVARCHAR(100) NOT NULL,
    last_name NVARCHAR(100) NOT NULL,
    date_of_birth DATETIME NOT NULL
);

-- Add foreign keys
ALTER TABLE dbo.Booking_Attraction
    ADD CONSTRAINT FK_Booking_Attraction_Attraction
    FOREIGN KEY (attraction_id) REFERENCES dbo.Attraction(attraction_id);

ALTER TABLE dbo.Booking_Attraction
    ADD CONSTRAINT FK_Booking_Attraction_Booking
    FOREIGN KEY (booking_id) REFERENCES dbo.Booking(booking_id);

ALTER TABLE dbo.Booking
    ADD CONSTRAINT FK_Booking_Employee
    FOREIGN KEY (employee_id) REFERENCES dbo.Employee(employee_id);

ALTER TABLE dbo.Booking
    ADD CONSTRAINT FK_Booking_Guest
    FOREIGN KEY (guest_id) REFERENCES dbo.Guest(guest_id);

-- Seed data: Attractions
INSERT INTO dbo.Attraction (attraction_id, name, price) VALUES
    (1, 'Roller Coaster', 15.00),
    (2, 'Ferris Wheel', 10.00),
    (3, 'Haunted House', 12.50),
    (4, 'Water Slide', 8.00);

-- Seed data: Employees
INSERT INTO dbo.Employee (employee_id, first_name, last_name, employee_number) VALUES
    (1, 'Alice', 'Johnson', 'EMP001'),
    (2, 'Bob', 'Smith', 'EMP002'),
    (3, 'Carol', 'Taylor', 'EMP003');

-- Seed data: Guests
INSERT INTO dbo.Guest (guest_id, first_name, last_name, date_of_birth) VALUES
    (1, 'John', 'Doe', '1990-05-15'),
    (2, 'Emma', 'Wilson', '1985-08-20'),
    (3, 'Liam', 'Brown', '2000-12-01');

-- Seed data: Bookings
INSERT INTO dbo.Booking (booking_id, guest_id, employee_id, date) VALUES
    (1, 1, 1, '2024-07-01 10:00:00'),
    (2, 2, 2, '2024-07-01 11:00:00'),
    (3, 3, 1, '2024-07-02 09:30:00');

-- Seed data: Booking_Attraction
INSERT INTO dbo.Booking_Attraction (booking_id, attraction_id, amount) VALUES
    (1, 1, 2),
    (1, 2, 1),
    (2, 3, 3),
    (3, 4, 2);
";

    using var conn = new SqlConnection(_csMaster);
    conn.Open();

    using (var cmd = new SqlCommand(dropDbScript, conn))
        cmd.ExecuteNonQuery();
    using (var cmd = new SqlCommand(createDbScript, conn))
        cmd.ExecuteNonQuery();

    conn.ChangeDatabase("APBD8_2");

    using (var cmd = new SqlCommand(schemaAndSeedScript, conn))
        cmd.ExecuteNonQuery();
}