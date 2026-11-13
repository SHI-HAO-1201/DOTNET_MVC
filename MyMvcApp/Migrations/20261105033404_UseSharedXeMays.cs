using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyMvcApp.Migrations
{
    public partial class UseSharedXeMays : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.Motorcycles', N'U') IS NULL OR OBJECT_ID(N'dbo.XeMays', N'U') IS NULL
                    THROW 51001, 'Shared product migration requires dbo.Motorcycles and dbo.XeMays.', 1;

                IF EXISTS
                    (SELECT 1 FROM dbo.Motorcycles
                     WHERE LEN(Code) NOT BETWEEN 1 AND 32
                        OR LEN(Name) > 160 OR LEN(Brand) > 80 OR LEN(Type) > 80
                        OR Price < 0 OR Price > 999999999999
                        OR LEN(CASE WHEN Image LIKE N'photo-%'
                                    THEN CONCAT(N'https://images.unsplash.com/', Image, N'?auto=format&fit=crop&w=1200&q=85')
                                    ELSE Image END) > 1000
                        OR LEN(CONCAT(Engine, N' · ', Power, N' · ', Gear)) > 4000)
                    THROW 51004, 'A legacy product exceeds dbo.XeMays limits; migration stopped to avoid truncating product data.', 1;

                IF OBJECT_ID(N'dbo.TR_Motorcycles_SyncXeMays', N'TR') IS NOT NULL
                    DROP TRIGGER dbo.TR_Motorcycles_SyncXeMays;
                IF OBJECT_ID(N'dbo.TR_XeMays_SyncMotorcycles', N'TR') IS NOT NULL
                    DROP TRIGGER dbo.TR_XeMays_SyncMotorcycles;

                INSERT dbo.XeMays
                    (MaXe, TenXe, HangXe, LoaiXe, PhienBan, GiaNiemYet, GiaBan, SoLuong, HinhAnh, MoTa,
                     MauSac, NamSanXuat, NhienLieu, PhanKho, TinhTrang, BaoHanhThang, DangKinhDoanh, NgayTao)
                SELECT m.Code, LEFT(m.Name, 160), LEFT(m.Brand, 80), LEFT(m.Type, 80), N'Bản tiêu chuẩn',
                       CONVERT(decimal(18,2), m.Price), CONVERT(decimal(18,2), m.Price), m.StockQuantity,
                       CASE WHEN m.Image LIKE N'photo-%'
                            THEN CONCAT(N'https://images.unsplash.com/', m.Image, N'?auto=format&fit=crop&w=1200&q=85')
                            ELSE LEFT(m.Image, 1000) END,
                       LEFT(CONCAT(m.Engine, N' · ', m.Power, N' · ', m.Gear), 4000),
                       N'Đen', YEAR(SYSUTCDATETIME()), N'Xăng', NULL, N'Mới', 12, m.IsActive, SYSUTCDATETIME()
                FROM dbo.Motorcycles AS m
                WHERE LEN(m.Code) BETWEEN 1 AND 32
                  AND NOT EXISTS (SELECT 1 FROM dbo.XeMays AS x WHERE x.MaXe = m.Code);

                IF EXISTS (SELECT MaXe FROM dbo.XeMays GROUP BY MaXe HAVING COUNT(*) > 1)
                    THROW 51002, 'Duplicate MaXe values prevent creating a unique product-code index.', 1;

                IF NOT EXISTS
                    (SELECT 1 FROM sys.indexes
                     WHERE object_id = OBJECT_ID(N'dbo.XeMays') AND name = N'IX_XeMays_MaXe')
                    CREATE UNIQUE INDEX IX_XeMays_MaXe ON dbo.XeMays(MaXe);

                IF NOT EXISTS (SELECT 1 FROM sys.default_constraints d JOIN sys.columns c ON c.object_id = d.parent_object_id AND c.column_id = d.parent_column_id WHERE d.parent_object_id = OBJECT_ID(N'dbo.XeMays') AND c.name = N'PhienBan')
                    ALTER TABLE dbo.XeMays ADD CONSTRAINT DF_XeMays_PhienBan DEFAULT N'Bản tiêu chuẩn' FOR PhienBan;
                IF NOT EXISTS (SELECT 1 FROM sys.default_constraints d JOIN sys.columns c ON c.object_id = d.parent_object_id AND c.column_id = d.parent_column_id WHERE d.parent_object_id = OBJECT_ID(N'dbo.XeMays') AND c.name = N'MauSac')
                    ALTER TABLE dbo.XeMays ADD CONSTRAINT DF_XeMays_MauSac DEFAULT N'Đen' FOR MauSac;
                IF NOT EXISTS (SELECT 1 FROM sys.default_constraints d JOIN sys.columns c ON c.object_id = d.parent_object_id AND c.column_id = d.parent_column_id WHERE d.parent_object_id = OBJECT_ID(N'dbo.XeMays') AND c.name = N'NhienLieu')
                    ALTER TABLE dbo.XeMays ADD CONSTRAINT DF_XeMays_NhienLieu DEFAULT N'Xăng' FOR NhienLieu;
                IF NOT EXISTS (SELECT 1 FROM sys.default_constraints d JOIN sys.columns c ON c.object_id = d.parent_object_id AND c.column_id = d.parent_column_id WHERE d.parent_object_id = OBJECT_ID(N'dbo.XeMays') AND c.name = N'TinhTrang')
                    ALTER TABLE dbo.XeMays ADD CONSTRAINT DF_XeMays_TinhTrang DEFAULT N'Mới' FOR TinhTrang;
                IF NOT EXISTS (SELECT 1 FROM sys.default_constraints d JOIN sys.columns c ON c.object_id = d.parent_object_id AND c.column_id = d.parent_column_id WHERE d.parent_object_id = OBJECT_ID(N'dbo.XeMays') AND c.name = N'BaoHanhThang')
                    ALTER TABLE dbo.XeMays ADD CONSTRAINT DF_XeMays_BaoHanhThang DEFAULT 12 FOR BaoHanhThang;
                IF NOT EXISTS (SELECT 1 FROM sys.default_constraints d JOIN sys.columns c ON c.object_id = d.parent_object_id AND c.column_id = d.parent_column_id WHERE d.parent_object_id = OBJECT_ID(N'dbo.XeMays') AND c.name = N'NgayTao')
                    ALTER TABLE dbo.XeMays ADD CONSTRAINT DF_XeMays_NgayTao DEFAULT SYSUTCDATETIME() FOR NgayTao;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Motorcycles_MotorcycleId",
                table: "OrderItems");
            migrationBuilder.DropForeignKey(
                name: "FK_ProductContactRequests_Motorcycles_MotorcycleId",
                table: "ProductContactRequests");
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Motorcycles_MotorcycleId",
                table: "Reviews");

            migrationBuilder.Sql("""
                IF EXISTS
                    (SELECT 1 FROM dbo.OrderItems AS oi
                     JOIN dbo.Motorcycles AS m ON m.Id = oi.MotorcycleId
                     LEFT JOIN dbo.XeMays AS x ON x.MaXe = m.Code
                     WHERE x.Id IS NULL)
                    OR EXISTS
                    (SELECT 1 FROM dbo.Reviews AS r
                     JOIN dbo.Motorcycles AS m ON m.Id = r.MotorcycleId
                     LEFT JOIN dbo.XeMays AS x ON x.MaXe = m.Code
                     WHERE x.Id IS NULL)
                    OR EXISTS
                    (SELECT 1 FROM dbo.ProductContactRequests AS cr
                     JOIN dbo.Motorcycles AS m ON m.Id = cr.MotorcycleId
                     LEFT JOIN dbo.XeMays AS x ON x.MaXe = m.Code
                     WHERE x.Id IS NULL)
                    THROW 51003, 'A historical product reference has no matching dbo.XeMays.MaXe; migration stopped without changing order history.', 1;

                UPDATE oi SET MotorcycleId = x.Id
                FROM dbo.OrderItems AS oi
                JOIN dbo.Motorcycles AS m ON m.Id = oi.MotorcycleId
                JOIN dbo.XeMays AS x ON x.MaXe = m.Code;

                UPDATE r SET MotorcycleId = x.Id
                FROM dbo.Reviews AS r
                JOIN dbo.Motorcycles AS m ON m.Id = r.MotorcycleId
                JOIN dbo.XeMays AS x ON x.MaXe = m.Code;

                UPDATE cr SET MotorcycleId = x.Id
                FROM dbo.ProductContactRequests AS cr
                JOIN dbo.Motorcycles AS m ON m.Id = cr.MotorcycleId
                JOIN dbo.XeMays AS x ON x.MaXe = m.Code;
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_XeMays_MotorcycleId",
                table: "OrderItems",
                column: "MotorcycleId",
                principalTable: "XeMays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_ProductContactRequests_XeMays_MotorcycleId",
                table: "ProductContactRequests",
                column: "MotorcycleId",
                principalTable: "XeMays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_XeMays_MotorcycleId",
                table: "Reviews",
                column: "MotorcycleId",
                principalTable: "XeMays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_XeMays_MotorcycleId",
                table: "OrderItems");
            migrationBuilder.DropForeignKey(
                name: "FK_ProductContactRequests_XeMays_MotorcycleId",
                table: "ProductContactRequests");
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_XeMays_MotorcycleId",
                table: "Reviews");

            migrationBuilder.Sql("""
                INSERT dbo.Motorcycles
                    (Name, Brand, Type, Price, Engine, Power, Gear, Image, Code, StockQuantity, IsActive, IsHiddenByStock, AvailabilityStatus)
                SELECT LEFT(x.TenXe, 450), LEFT(x.HangXe, 450), LEFT(x.LoaiXe, 450), CONVERT(bigint, x.GiaBan),
                       LEFT(x.MoTa, 450), N'Đang cập nhật', N'Đang cập nhật', x.HinhAnh, x.MaXe,
                       x.SoLuong, x.DangKinhDoanh, 0,
                       CASE WHEN x.SoLuong > 0 THEN N'ForSale' ELSE N'Incoming' END
                FROM dbo.XeMays AS x
                WHERE NOT EXISTS (SELECT 1 FROM dbo.Motorcycles AS m WHERE m.Code = x.MaXe);

                UPDATE oi SET MotorcycleId = m.Id
                FROM dbo.OrderItems AS oi
                JOIN dbo.XeMays AS x ON x.Id = oi.MotorcycleId
                JOIN dbo.Motorcycles AS m ON m.Code = x.MaXe;

                UPDATE r SET MotorcycleId = m.Id
                FROM dbo.Reviews AS r
                JOIN dbo.XeMays AS x ON x.Id = r.MotorcycleId
                JOIN dbo.Motorcycles AS m ON m.Code = x.MaXe;

                UPDATE cr SET MotorcycleId = m.Id
                FROM dbo.ProductContactRequests AS cr
                JOIN dbo.XeMays AS x ON x.Id = cr.MotorcycleId
                JOIN dbo.Motorcycles AS m ON m.Code = x.MaXe;

                IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = N'DF_XeMays_PhienBan') ALTER TABLE dbo.XeMays DROP CONSTRAINT DF_XeMays_PhienBan;
                IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = N'DF_XeMays_MauSac') ALTER TABLE dbo.XeMays DROP CONSTRAINT DF_XeMays_MauSac;
                IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = N'DF_XeMays_NhienLieu') ALTER TABLE dbo.XeMays DROP CONSTRAINT DF_XeMays_NhienLieu;
                IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = N'DF_XeMays_TinhTrang') ALTER TABLE dbo.XeMays DROP CONSTRAINT DF_XeMays_TinhTrang;
                IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = N'DF_XeMays_BaoHanhThang') ALTER TABLE dbo.XeMays DROP CONSTRAINT DF_XeMays_BaoHanhThang;
                IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = N'DF_XeMays_NgayTao') ALTER TABLE dbo.XeMays DROP CONSTRAINT DF_XeMays_NgayTao;
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.XeMays') AND name = N'IX_XeMays_MaXe')
                    DROP INDEX IX_XeMays_MaXe ON dbo.XeMays;
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Motorcycles_MotorcycleId",
                table: "OrderItems",
                column: "MotorcycleId",
                principalTable: "Motorcycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(
                name: "FK_ProductContactRequests_Motorcycles_MotorcycleId",
                table: "ProductContactRequests",
                column: "MotorcycleId",
                principalTable: "Motorcycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Motorcycles_MotorcycleId",
                table: "Reviews",
                column: "MotorcycleId",
                principalTable: "Motorcycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
