using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MyMvcApp.Migrations
{
    /// <inheritdoc />
    public partial class CSDL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Motorcycles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false),
                    Engine = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Power = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gear = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsHiddenByStock = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AvailabilityStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "ForSale")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Motorcycles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Promotions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiscountAmount = table.Column<long>(type: "bigint", nullable: false),
                    UsageLimit = table.Column<int>(type: "int", nullable: false),
                    UsedCount = table.Column<int>(type: "int", nullable: false),
                    StartsAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndsAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promotions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StaffMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoreLocation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StoreSettings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreSettings", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<long>(type: "bigint", nullable: false),
                    DiscountAmount = table.Column<long>(type: "bigint", nullable: false),
                    PromotionCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    ShippingAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductContactRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MotorcycleId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsContacted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductContactRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductContactRequests_Motorcycles_MotorcycleId",
                        column: x => x.MotorcycleId,
                        principalTable: "Motorcycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MotorcycleId = table.Column<int>(type: "int", nullable: false),
                    ReviewerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReviewerEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdminReply = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_Motorcycles_MotorcycleId",
                        column: x => x.MotorcycleId,
                        principalTable: "Motorcycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    MotorcycleId = table.Column<int>(type: "int", nullable: false),
                    MotorcycleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnitPrice = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Motorcycles_MotorcycleId",
                        column: x => x.MotorcycleId,
                        principalTable: "Motorcycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Motorcycles",
                columns: new[] { "Id", "AvailabilityStatus", "Brand", "Code", "Engine", "Gear", "Image", "IsActive", "Name", "Power", "Price", "StockQuantity", "Type" },
                values: new object[,]
                {
                    { 1, "ForSale", "Kawasaki", "KAW-NIN-400", "399 cc", "6 cấp", "photo-1568772585407-9361f9bf3a87", true, "Kawasaki Ninja 400", "45 mã lực", 160000000L, 1, "Sportbike" },
                    { 2, "ForSale", "Yamaha", "YAM-MT-03", "321 cc", "6 cấp", "photo-1558981403-c5f9899a28bc", true, "Yamaha MT-03", "42 mã lực", 139000000L, 1, "Nakedbike" },
                    { 3, "ForSale", "Honda", "HON-CB-500", "471 cc", "6 cấp", "photo-1558981806-ec527fa84c39", true, "Honda CB500F", "47 mã lực", 184000000L, 1, "Nakedbike" },
                    { 4, "ForSale", "Ducati", "DUC-MON-937", "937 cc", "6 cấp", "photo-1558981403-c5f9899a28bc", true, "Ducati Monster", "111 mã lực", 390000000L, 1, "Nakedbike" },
                    { 5, "ForSale", "Honda", "HON-CBR-650", "649 cc", "6 cấp", "photo-1558981806-ec527fa84c39", true, "Honda CBR650R", "94 mã lực", 254000000L, 1, "Sportbike" },
                    { 6, "ForSale", "Kawasaki", "KAW-Z-650", "649 cc", "6 cấp", "photo-1568772585407-9361f9bf3a87", true, "Kawasaki Z650", "68 mã lực", 189000000L, 1, "Nakedbike" },
                    { 7, "ForSale", "Honda", "HON-WAVE-110", "109.2 cc", "4 cấp", "photo-1558981806-ec527fa84c39", true, "Honda Wave Alpha 110", "8.2 mã lực", 18000000L, 1, "Xe số" },
                    { 8, "ForSale", "Honda", "HON-VIS-110", "109.5 cc", "Vô cấp", "photo-1558981403-c5f9899a28bc", true, "Honda Vision", "8.8 mã lực", 32000000L, 1, "Xe tay ga" },
                    { 9, "ForSale", "Honda", "HON-SH-160", "156.9 cc", "Vô cấp", "photo-1558981806-ec527fa84c39", true, "Honda SH160i", "16.2 mã lực", 96000000L, 1, "Xe tay ga" },
                    { 10, "ForSale", "Yamaha", "YAM-EXC-155", "155 cc", "6 cấp", "photo-1568772585407-9361f9bf3a87", true, "Yamaha Exciter 155 VVA", "17.7 mã lực", 55000000L, 1, "Côn tay" },
                    { 11, "ForSale", "Yamaha", "YAM-NVX-155", "155.1 cc", "Vô cấp", "photo-1558981403-c5f9899a28bc", true, "Yamaha NVX 155", "15.4 mã lực", 55000000L, 1, "Xe tay ga" },
                    { 12, "ForSale", "Yamaha", "YAM-XSR-155", "155 cc", "6 cấp", "photo-1558981806-ec527fa84c39", true, "Yamaha XSR155", "19.3 mã lực", 77000000L, 1, "Nakedbike" },
                    { 13, "ForSale", "Kawasaki", "KAW-VER-650", "649 cc", "6 cấp", "photo-1568772585407-9361f9bf3a87", true, "Kawasaki Versys 650", "67 mã lực", 340000000L, 1, "Adventure" },
                    { 14, "ForSale", "Kawasaki", "KAW-VUL-650", "649 cc", "6 cấp", "photo-1558981403-c5f9899a28bc", true, "Kawasaki Vulcan S", "61 mã lực", 249000000L, 1, "Cruiser" },
                    { 15, "ForSale", "Honda", "HON-WIN-150", "149.1 cc", "6 cấp", "photo-1558981806-ec527fa84c39", true, "Honda Winner X", "15.4 mã lực", 47000000L, 1, "Côn tay" },
                    { 16, "ForSale", "Honda", "HON-ADV-160", "156.9 cc", "Vô cấp", "photo-1568772585407-9361f9bf3a87", true, "Honda ADV 160", "16 mã lực", 94000000L, 1, "Adventure" },
                    { 17, "ForSale", "Honda", "HON-REB-500", "471 cc", "6 cấp", "photo-1558981403-c5f9899a28bc", true, "Honda Rebel 500", "46 mã lực", 181000000L, 1, "Cruiser" },
                    { 18, "ForSale", "Suzuki", "SUZ-RAI-150", "147.3 cc", "6 cấp", "photo-1558981806-ec527fa84c39", true, "Suzuki Raider R150", "18.2 mã lực", 52000000L, 1, "Côn tay" },
                    { 19, "ForSale", "Suzuki", "SUZ-GSX-150", "147.3 cc", "6 cấp", "photo-1568772585407-9361f9bf3a87", true, "Suzuki GSX-S150", "18.9 mã lực", 69000000L, 1, "Nakedbike" },
                    { 20, "ForSale", "Ducati", "DUC-PAN-V2", "955 cc", "6 cấp", "photo-1558981403-c5f9899a28bc", true, "Ducati Panigale V2", "155 mã lực", 600000000L, 1, "Sportbike" },
                    { 21, "ForSale", "Vespa", "VES-PRI-150", "154.8 cc", "Vô cấp", "photo-1558981806-ec527fa84c39", true, "Vespa Primavera 150", "12.9 mã lực", 175000000L, 1, "Xe tay ga" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Motorcycles_Code",
                table: "Motorcycles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_MotorcycleId",
                table: "OrderItems",
                column: "MotorcycleId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumber",
                table: "Orders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductContactRequests_IsContacted_CreatedAtUtc",
                table: "ProductContactRequests",
                columns: new[] { "IsContacted", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductContactRequests_MotorcycleId",
                table: "ProductContactRequests",
                column: "MotorcycleId");

            migrationBuilder.CreateIndex(
                name: "IX_Promotions_Code",
                table: "Promotions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_MotorcycleId",
                table: "Reviews",
                column: "MotorcycleId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffMembers_Email",
                table: "StaffMembers",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "ProductContactRequests");

            migrationBuilder.DropTable(
                name: "Promotions");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "StaffMembers");

            migrationBuilder.DropTable(
                name: "StoreSettings");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Motorcycles");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
