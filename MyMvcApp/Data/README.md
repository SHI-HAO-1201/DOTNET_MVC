# SQL Server and Entity Framework Core

The `Data` folder contains the SQL Server `AppDbContext`, its EF Core design-time
factory, database initializer, migrations, database workflow scripts, and
database entity files under `Data/Models`. Entity mappings and seed data are
configured with Fluent API in `AppDbContext.OnModelCreating`. MVC input models
and view models remain in the root `Models` folder.

## Kết nối database chung

MyMvcApp, MyRazorXe_Xemay và BlazorXe_may cùng đọc connection string
`ConnectionStrings:DefaultConnection` trong `appsettings.json`. Cả ba hiện trỏ
tới database `MotoSpeedDb` trên SQL Server `localhost`. EF Core design-time
factory cũng đọc cấu hình này.

## Code First

Install the matching EF Core CLI tool once:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.13
```

From the project directory, create and apply a migration:

```powershell
dotnet ef migrations add MigrationName --output-dir Migrations
dotnet ef database update
```

Project này đã có migration và đang dùng database có sẵn. Khi chạy ứng dụng,
EF Core chỉ áp dụng các migration còn thiếu; không xóa hoặc tạo lại database.
Trước khi chạy lệnh migration thủ công, xác nhận connection string đang trỏ
đến `MotoSpeedDb`:

```powershell
dotnet ef database update
```

The initial migration creates the tables and seeds 21 motorcycles. Other tables
start empty. Add a new migration only after changing the EF model or its
`HasData` seed records. Normal records created through the application are saved
to the database by `SaveChanges` and do not need a migration.

Alternatively, run `Data\Use-CodeFirst.ps1 -MigrationName MigrationName`.
Pending migrations are also applied when the application starts.

## Database First

`Data\Use-DatabaseFirst.ps1` scaffolds an existing SQL Server database into
`Data\DatabaseFirst` and `Data\Models\DatabaseFirst`, so it does
not overwrite the Code First context or current models. It is an alternative
workflow; do not use both workflows to manage the same schema.

## Tạo admin dùng chung

Từ thư mục project:

```powershell
dotnet run --no-launch-profile -- --create-admin
```

Lệnh này tạo hoặc đặt lại tài khoản trong các bảng Identity dùng chung. Nhập
cùng email và mật khẩu ở ba ứng dụng để đăng nhập bằng tài khoản đó.

## Ảnh xe

URL ảnh nằm trong `dbo.Motorcycles.Image` (MVC/MyRazor) hoặc
`dbo.XeMays.HinhAnh` (Blazor). Hai bảng được đồng bộ theo mã xe; ảnh là URL,
còn file ảnh được lưu trên máy chủ ảnh hoặc trong `wwwroot/images` nếu dùng ảnh
nội bộ.
