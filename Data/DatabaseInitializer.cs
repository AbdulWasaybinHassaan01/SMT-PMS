using Microsoft.Data.SqlClient;
using TechWorldPMS.Security;

namespace TechWorldPMS.Data
{
    public class DatabaseInitializer
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(IDbConnectionFactory connectionFactory, ILogger<DatabaseInitializer> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            try
            {
                await EnsureDatabaseExistsAsync();
                await CreateTablesAsync();
                await SeedInitialDataAsync();
                _logger.LogInformation("Database initialized and verified successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the database.");
                throw;
            }
        }

        private async Task EnsureDatabaseExistsAsync()
        {
            var builder = new SqlConnectionStringBuilder(_connectionFactory.ConnectionString);
            string targetDatabase = builder.InitialCatalog;

            builder.InitialCatalog = "master";
            using var masterConn = new SqlConnection(builder.ConnectionString);
            await masterConn.OpenAsync();

            string checkDbQuery = "SELECT COUNT(*) FROM sys.databases WHERE name = @DbName";
            using var checkCmd = new SqlCommand(checkDbQuery, masterConn);
            checkCmd.Parameters.AddWithValue("@DbName", targetDatabase);

            int dbExists = (int)(await checkCmd.ExecuteScalarAsync() ?? 0);

            if (dbExists == 0)
            {
                _logger.LogInformation("Database {Database} does not exist. Creating now...", targetDatabase);
                string createDbQuery = $"CREATE DATABASE [{targetDatabase}]";
                using var createCmd = new SqlCommand(createDbQuery, masterConn);
                await createCmd.ExecuteNonQueryAsync();
                _logger.LogInformation("Database {Database} created successfully.", targetDatabase);
            }
        }

        private async Task CreateTablesAsync()
        {
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            string createTablesSql = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories')
                BEGIN
                    CREATE TABLE Categories (
                        CategoryId INT IDENTITY(1,1) PRIMARY KEY,
                        CategoryName NVARCHAR(100) NOT NULL UNIQUE,
                        Description NVARCHAR(250) NULL,
                        ColorBadge NVARCHAR(50) NOT NULL DEFAULT 'primary'
                    );
                END

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
                BEGIN
                    CREATE TABLE Products (
                        ProductId INT IDENTITY(1,1) PRIMARY KEY,
                        Name NVARCHAR(150) NOT NULL,
                        CategoryId INT NOT NULL FOREIGN KEY REFERENCES Categories(CategoryId) ON DELETE CASCADE,
                        Price DECIMAL(18,2) NOT NULL,
                        StockQuantity INT NOT NULL DEFAULT 0,
                        Description NVARCHAR(MAX) NULL,
                        ImageUrl NVARCHAR(500) NULL,
                        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                        UpdatedAt DATETIME2 NULL
                    );
                END

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
                BEGIN
                    CREATE TABLE Users (
                        UserId INT IDENTITY(1,1) PRIMARY KEY,
                        FullName NVARCHAR(150) NOT NULL,
                        Email NVARCHAR(150) NOT NULL UNIQUE,
                        PasswordHash NVARCHAR(255) NOT NULL,
                        Role NVARCHAR(50) NOT NULL DEFAULT 'Customer',
                        Address NVARCHAR(250) NULL,
                        City NVARCHAR(100) NULL,
                        PostalCode NVARCHAR(20) NULL,
                        Phone NVARCHAR(50) NULL,
                        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
                    );
                END

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CartItems')
                BEGIN
                    CREATE TABLE CartItems (
                        CartItemId INT IDENTITY(1,1) PRIMARY KEY,
                        UserId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId) ON DELETE CASCADE,
                        ProductId INT NOT NULL FOREIGN KEY REFERENCES Products(ProductId) ON DELETE CASCADE,
                        Quantity INT NOT NULL DEFAULT 1,
                        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
                    );
                END

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Orders')
                BEGIN
                    CREATE TABLE Orders (
                        OrderId INT IDENTITY(1,1) PRIMARY KEY,
                        UserId INT NOT NULL FOREIGN KEY REFERENCES Users(UserId),
                        CustomerName NVARCHAR(150) NOT NULL,
                        CustomerEmail NVARCHAR(150) NOT NULL,
                        OrderDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                        TotalAmount DECIMAL(18,2) NOT NULL,
                        Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
                        PaymentMethod NVARCHAR(100) NOT NULL,
                        ShippingAddress NVARCHAR(250) NOT NULL,
                        City NVARCHAR(100) NOT NULL,
                        PostalCode NVARCHAR(20) NULL,
                        Phone NVARCHAR(50) NOT NULL,
                        Notes NVARCHAR(500) NULL
                    );
                END

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OrderItems')
                BEGIN
                    CREATE TABLE OrderItems (
                        OrderItemId INT IDENTITY(1,1) PRIMARY KEY,
                        OrderId INT NOT NULL FOREIGN KEY REFERENCES Orders(OrderId) ON DELETE CASCADE,
                        ProductId INT NOT NULL,
                        ProductName NVARCHAR(150) NOT NULL,
                        UnitPrice DECIMAL(18,2) NOT NULL,
                        Quantity INT NOT NULL,
                        SubTotal DECIMAL(18,2) NOT NULL,
                        ImageUrl NVARCHAR(500) NULL
                    );
                END
            ";

            using var cmd = new SqlCommand(createTablesSql, conn);
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task SeedInitialDataAsync()
        {
            using var conn = await _connectionFactory.CreateOpenConnectionAsync();

            // 1. Seed Categories if empty
            string checkCategoriesSql = "SELECT COUNT(*) FROM Categories";
            using var catCmd = new SqlCommand(checkCategoriesSql, conn);
            int catCount = (int)(await catCmd.ExecuteScalarAsync() ?? 0);

            if (catCount == 0)
            {
                string insertCategoriesSql = @"
                    INSERT INTO Categories (CategoryName, Description, ColorBadge) VALUES 
                    ('Laptops', 'High performance laptops, ultrabooks, and workstations', 'primary'),
                    ('Accessories', 'Keyboards, mice, headsets, cables, and docks', 'purple'),
                    ('Monitors', 'High refresh rate, 4K, and ultra-wide desktop monitors', 'success'),
                    ('Gaming', 'Dedicated gaming rigs, handhelds, and gaming gear', 'danger'),
                    ('Audio', 'Noise-canceling headphones, studio mics, and speakers', 'warning'),
                    ('Components', 'CPUs, GPUs, Motherboards, RAM, and Storage', 'info');
                ";
                using var insertCatCmd = new SqlCommand(insertCategoriesSql, conn);
                await insertCatCmd.ExecuteNonQueryAsync();
                _logger.LogInformation("Seeded default Categories.");
            }

            // 2. Seed Products if empty
            string checkProductsSql = "SELECT COUNT(*) FROM Products";
            using var prodCmd = new SqlCommand(checkProductsSql, conn);
            int prodCount = (int)(await prodCmd.ExecuteScalarAsync() ?? 0);

            if (prodCount == 0)
            {
                string insertProductsSql = @"
                    DECLARE @LaptopsId INT = (SELECT CategoryId FROM Categories WHERE CategoryName = 'Laptops');
                    DECLARE @AccessoriesId INT = (SELECT CategoryId FROM Categories WHERE CategoryName = 'Accessories');
                    DECLARE @MonitorsId INT = (SELECT CategoryId FROM Categories WHERE CategoryName = 'Monitors');
                    DECLARE @GamingId INT = (SELECT CategoryId FROM Categories WHERE CategoryName = 'Gaming');
                    DECLARE @AudioId INT = (SELECT CategoryId FROM Categories WHERE CategoryName = 'Audio');
                    DECLARE @ComponentsId INT = (SELECT CategoryId FROM Categories WHERE CategoryName = 'Components');

                    INSERT INTO Products (Name, CategoryId, Price, StockQuantity, Description, ImageUrl, CreatedAt) VALUES
                    ('Dell Inspiron 15', @LaptopsId, 89999.00, 15, 'Powerful laptop for work, study and gaming. Features Intel Core i5, 16GB RAM, 512GB NVMe SSD and crisp 15.6 FHD anti-glare screen.', '/images/products/dell-inspiron-15.png', GETUTCDATE()),
                    ('Logitech G733', @AccessoriesId, 34999.00, 20, 'Wireless LightSpeed RGB gaming headset with crystal clear audio, Blue VO!CE broadcast filters and ultra-comfortable suspension headband.', '/images/products/logitech-g733.png', GETUTCDATE()),
                    ('Samsung Monitor 24""', @MonitorsId, 27999.00, 12, '24-inch Full HD borderless IPS display with 75Hz refresh rate, AMD FreeSync and eye-saver flicker-free technology.', '/images/products/samsung-monitor-24.png', GETUTCDATE()),
                    ('ASUS ROG Strix G16', @GamingId, 325000.00, 4, 'Pro esports gaming laptop with Intel Core i9-13980HX, NVIDIA GeForce RTX 4070, 240Hz Nebula Display and liquid metal cooling.', '/images/products/asus-rog-strix.png', GETUTCDATE()),
                    ('HP Pavilion 15', @LaptopsId, 145000.00, 8, 'Ultra-slim business laptop with Intel 13th Gen, 16GB RAM, 1TB SSD, backlit keyboard and all-day battery life with fast charge.', '/images/products/hp-pavilion-15.png', GETUTCDATE()),
                    ('Razer DeathAdder V3', @AccessoriesId, 18500.00, 25, 'Ergonomic 59g lightweight competitive wired gaming mouse with Focus Pro 30K Optical Sensor and Gen-3 Optical Switches.', '/images/products/razer-deathadder.png', GETUTCDATE()),
                    ('Corsair K70 RGB Pro', @AccessoriesId, 29000.00, 3, 'Mechanical gaming keyboard with Cherry MX Speed Silver switches, per-key RGB backlighting and durable aluminum frame.', '/images/products/corsair-k70.png', GETUTCDATE()),
                    ('Apple MacBook Air M2', @LaptopsId, 280000.00, 6, 'Strikingly thin design with Apple M2 chip, 13.6-inch Liquid Retina display, 18 hours battery life, MagSafe charging port.', '/images/products/macbook-air-m2.png', GETUTCDATE()),
                    ('Sony WH-1000XM5', @AudioId, 78000.00, 5, 'Industry-leading noise-canceling wireless headphones with 2 processors, 8 microphones, 30hr battery and ultra-clear hands-free calling.', '/images/products/sony-xm5.png', GETUTCDATE()),
                    ('LG UltraGear 27"" QHD', @MonitorsId, 68000.00, 7, '27-inch QHD (2560x1440) Nano IPS gaming monitor with 1ms response time, 165Hz refresh rate and HDR400 color coverage.', '/images/products/lg-ultragear.png', GETUTCDATE()),
                    ('NVIDIA RTX 4080 Super', @ComponentsId, 295000.00, 2, 'Unstoppable 4K ray-tracing GPU with 16GB GDDR6X, DLSS 3.5 neural rendering and high-efficiency Ada Lovelace architecture.', '/images/products/rtx-4080-super.png', GETUTCDATE()),
                    ('Kingston 32GB DDR5 RAM', @ComponentsId, 24500.00, 18, 'Fury Beast 32GB (2x16GB) 6000MHz DDR5 CL36 desktop memory kit with Intel XMP 3.0 and customizable RGB light bar.', '/images/products/kingston-ddr5.png', GETUTCDATE()),
                    ('Test RTX 5090 Beast', @ComponentsId, 450000.00, 3, 'Next-generation flagship graphics card Beast Edition with 32GB GDDR7, extreme 3.5-slot cooler and RGB illuminated armor.', '/images/products/rtx-5090-beast.png', GETUTCDATE());
                ";
                using var insertProdCmd = new SqlCommand(insertProductsSql, conn);
                await insertProdCmd.ExecuteNonQueryAsync();
                _logger.LogInformation("Seeded default Products matching TechWORLD showcase.");
            }

            // 3. Seed Users if empty
            string checkUsersSql = "SELECT COUNT(*) FROM Users";
            using var userCmd = new SqlCommand(checkUsersSql, conn);
            int userCount = (int)(await userCmd.ExecuteScalarAsync() ?? 0);

            if (userCount == 0)
            {
                string adminHash = PasswordHasher.HashPassword("admin123");
                string customerHash = PasswordHasher.HashPassword("customer123");

                string insertUsersSql = @"
                    INSERT INTO Users (FullName, Email, PasswordHash, Role, Address, City, PostalCode, Phone, CreatedAt) VALUES
                    ('Admin Manager', 'admin@techworld.com', @AdminHash, 'Admin', 'TechWORLD HQ, Blue Area', 'Islamabad', '44000', '+92 300 1234567', GETUTCDATE()),
                    ('Hamza Customer', 'customer@gmail.com', @CustomerHash, 'Customer', 'House # 42, Street 9, Sector F-8/2', 'Islamabad', '44000', '+92 321 9876543', GETUTCDATE());
                ";
                using var insertUserCmd = new SqlCommand(insertUsersSql, conn);
                insertUserCmd.Parameters.AddWithValue("@AdminHash", adminHash);
                insertUserCmd.Parameters.AddWithValue("@CustomerHash", customerHash);
                await insertUserCmd.ExecuteNonQueryAsync();
                _logger.LogInformation("Seeded default Admin and Customer accounts.");
            }
        }
    }
}
