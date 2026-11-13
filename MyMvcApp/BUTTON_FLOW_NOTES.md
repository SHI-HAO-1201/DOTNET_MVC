# Ghi chú luồng nút và điều hướng — MVC

Trong MVC, link Razor (`asp-controller`/`asp-action`) tạo URL tới action của controller. Form `method="post"` gửi dữ liệu tới action POST; action xử lý rồi thường redirect hoặc trả view. Nút có `data-*` có thể chỉ chạy JavaScript trong trình duyệt, không gọi controller ngay.

## Khách hàng và mua xe

| Nút/thao tác | Đích / handler | File xử lý tiếp |
|---|---|---|
| Logo, trang chủ | `Home/Index` | `Controllers/HomeController.cs` → `Views/Home/Index.cshtml` |
| Khám phá xe, showroom | `Motor/Index` | `Controllers/MotorController.cs` → `Views/Motor/Index.cshtml` |
| Chọn danh mục | `Motor/ByCategory?brand=...` | `MotorController.ByCategory` → `Views/Motor/ByCategory.cshtml` |
| Mở thẻ/chi tiết xe | `Motor/Details/{id}` | `MotorController.Details` → `Views/Motor/Details.cshtml` |
| Thêm xe vào giỏ ở danh sách | `.add-cart[data-id]` | JavaScript `wwwroot/js/site.js`; cập nhật giỏ trong `localStorage`, không gọi controller |
| Xóa giỏ / chỉnh số lượng | `#clear-cart`, nút số lượng được render bởi JS | `wwwroot/js/site.js`; lưu lại `localStorage` |
| Mở trang giỏ / tiếp tục mua | `Shopping/Cart`, `Motor/Index` | `ShoppingController.Cart`, `MotorController.Index` |
| Tiến hành đặt hàng | `Shopping/Checkout` GET | `ShoppingController.Checkout()` → `Views/Shopping/Checkout.cshtml` |
| Gửi đơn đặt xe | form POST `Shopping/Checkout` | `ShoppingController.Checkout(...)`: kiểm tra xe/tồn kho/khuyến mãi, tạo đơn và chi tiết rồi `SaveChangesAsync()` |
| Gửi đánh giá | form POST `Motor/SubmitReview` | `MotorController.SubmitReview` |
| Yêu cầu xe sắp về | form POST `Motor/RequestIncomingProduct` | `MotorController.RequestIncomingProduct` |
| Liên hệ / đặt lịch lái thử | `Home/Contact` GET/POST | `HomeController.Contact`; hiện action POST ghi nhận trạng thái demo, không lưu yêu cầu vào database |

## Tài khoản

- **Đăng nhập:** form `Views/Account/Login.cshtml` → POST `Account/Login` trong `Controllers/AccountController.cs`.
- **Hiện/ẩn mật khẩu:** nút `data-password-toggle` → `wwwroot/js/password-toggle.js`, chỉ đổi kiểu hiển thị input.
- **Đăng ký:** `Account/Register` GET/POST → `AccountController.Register`.
- **Quên mật khẩu:** `Account/ForgotPassword` → action hiện hướng dẫn liên hệ cửa hàng; chưa có quy trình gửi email đặt lại.
- **Cập nhật hồ sơ:** form `Account/Profile` POST → `AccountController.Profile` → cập nhật database.
- **Đơn của tôi:** link `Account/Orders` → `AccountController.Orders` → đọc đơn theo khách đăng nhập.
- **Đăng xuất:** POST `Account/Logout` → `AccountController.Logout`.
- **Đăng nhập nhà quản trị:** trang admin login dùng action `Admin/Login`; đăng xuất admin dùng `Admin/Logout`.

## Admin

Các link menu và nút trong `Views/Admin/*.cshtml` gọi action của `Controllers/AdminController.cs`:

| Màn hình / nút | Action đích | View sau xử lý |
|---|---|---|
| Tổng quan | `Dashboard` | `Views/Admin/Dashboard.cshtml` |
| Danh sách/thêm/sửa sản phẩm | `Products`, `CreateProduct`, `EditProduct` | `Products.cshtml`, `ProductForm.cshtml` |
| Tải CSV sản phẩm | `ExportProducts` | `AdminController` trả file CSV, không mở view |
| Lưu sản phẩm | POST `SaveProduct` | `AdminController.SaveProduct` → redirect `Products` |
| Xóa/ẩn-hiện sản phẩm | POST `DeleteProduct`, `ToggleProductVisibility` | `AdminController` → redirect `Products` |
| Tồn kho | `Inventory`; POST `UpdateStock` | `Inventory.cshtml` → `AdminController.UpdateStock` |
| Đơn hàng | `Orders`; POST `UpdateOrderStatus`, `DeleteOrder` | `Orders.cshtml` → controller → redirect `Orders` |
| Khách hàng | `Customers`, `CreateCustomer`, `EditCustomer`; POST `SaveCustomer`, `DeleteCustomer` | `Customers.cshtml`, `CustomerForm.cshtml` → controller |
| Yêu cầu liên hệ sản phẩm | `ProductContactRequests`; POST `MarkProductContactRequest`, `DeleteProductContactRequest` | `ProductContactRequests.cshtml` → controller |
| Khuyến mãi | `Promotions`, `CreatePromotion`, `EditPromotion`; POST `SavePromotion`, `DeletePromotion` | `Promotions.cshtml`, `PromotionForm.cshtml` → controller |
| Đánh giá | `Reviews`; POST `SaveReviewReply`, `ToggleReviewVisibility`, `DeleteReview` | `Reviews.cshtml` → controller |
| Báo cáo | GET `Reports`; POST/download `ExportReport` | `Reports.cshtml` → controller xuất CSV |
| Nhân viên | `Staff`, `CreateStaff`, `EditStaff`; POST `SaveStaff`, `DeleteStaff` | `Staff.cshtml`, `StaffForm.cshtml` → controller |
| Cài đặt | GET `Settings`; POST `SaveSettings` | `Settings.cshtml` → controller |

Các ô tìm kiếm, lọc bảng và thao tác có `data-*` trong admin thường được xử lý ở `wwwroot/js/admin.js`; chúng lọc/đổi giao diện phía trình duyệt. Nút submit trong form mới gửi request tới action đã nêu ở bảng.

## Nút chỉ chạy ở trình duyệt hoặc trang phụ

- Tìm kiếm/lọc/sắp xếp danh sách xe ở `Views/Motor/Index.cshtml`: hàm `filterProducts()` nằm ngay trong `@section Scripts` của view và lọc các thẻ xe đã render trên trình duyệt. `wwwroot/js/site.js` xử lý riêng nút `.add-cart[data-id]`, cập nhật giỏ trong `localStorage`.
- Điều chỉnh số lượng / xóa dòng / xóa toàn bộ giỏ ở `Views/Shopping/Cart.cshtml`: `wwwroot/js/site.js`; nút đặt hàng mới mở `Shopping/Checkout`.
- Nút hiện/ẩn mật khẩu ở `Views/Account/Login.cshtml`: `wwwroot/js/password-toggle.js`.
- Tìm kiếm, lọc, xác nhận xóa và tương tác dạng `data-*` trong trang admin: `wwwroot/js/admin.js`; nếu đó là submit form, request đi tới action tương ứng ở bảng Admin.
- Nút đăng nhập bằng nhà cung cấp ngoài trong `Views/Account/Login.cshtml` hiện bị `disabled`, chưa chuyển tới file/action nào.
- Nút điều hướng giới thiệu, chính sách, liên hệ nằm trong `Views/Home/*.cshtml`; route tương ứng là `Home/About`, `Home/Privacy`, `Home/Contact`. Các link xe dùng `Motor/Details`, link danh mục dùng `Motor/ByCategory`.

## Cách lần theo một nút trong IDE

1. Mở view đang hiển thị nút.
2. Nếu có `asp-controller`/`asp-action`, mở controller cùng tên và tìm method action.
3. Nếu form không chỉ rõ `asp-action`, kiểm tra form bao quanh nút và method `POST`.
4. Nếu nút dùng `data-*` hoặc `id` mà không có action, tìm selector đó trong `wwwroot/js/*.js`.

## Khi chạy `dotnet watch`: thứ tự khởi động

Chạy lệnh từ thư mục `MyMvcApp`:

```powershell
dotnet watch
```

1. `dotnet watch` theo dõi project và build lại khi file được sửa; file C# đầu tiên của ứng dụng được thực thi là top-level `Program.cs`.
2. `Program.cs` đăng ký MVC, `AppDbContext`, xác thực và phân quyền.
3. App build xong thì `DatabaseInitializer.InitializeAsync` trong `Data/DatabaseInitializer.cs` gọi EF `MigrateAsync()`.
4. `Program.cs` cài middleware HTTPS, routing, authentication/authorization rồi map route mặc định `{controller=Home}/{action=Index}/{id?}`.
5. Browser GET `/` → `HomeController.Index()` → đọc xe từ database → `Views/Home/Index.cshtml`.
6. `Views/_ViewStart.cshtml` gán layout `Views/Shared/_Layout.cshtml`; layout render `@RenderBody()` và nạp `wwwroot/js/site.js`.

`dotnet watch` không chạy `Index.cshtml` đầu tiên một cách độc lập: `Program.cs` khởi động server, còn file được chọn để render phụ thuộc URL browser yêu cầu.

## Ví dụ luồng bấm trang

### Bấm “Xe mô tô” / “Khám phá bộ sưu tập”

`Views/Shared/_Layout.cshtml` hoặc nút trên `Views/Home/Index.cshtml` tạo link `Motor/Index` → routing gọi `Controllers/MotorController.cs` → method `Index()` query database → `return View(...)` chọn `Views/Motor/Index.cshtml` → `Views/_ViewStart.cshtml` chọn `_Layout.cshtml` → layout render trang và tải JS.

Trong `Motor/Index.cshtml`, bấm tên/ảnh xe đi `Motor/Details/{id}` → `MotorController.Details(id)` → `Views/Motor/Details.cshtml`. Bấm nút thêm giỏ không gọi controller: `.add-cart[data-id]` → `wwwroot/js/site.js` → `localStorage`.

### Bấm một thương hiệu

`_Layout.cshtml` hoặc footer tạo link `Motor/ByCategory?brand=Honda` (hoặc thương hiệu khác) → `MotorController.ByCategory(brand)` lọc xe → `Views/Motor/ByCategory.cshtml` → `_ViewStart` → `_Layout`.

### Từ giỏ đi thanh toán

Link `Shopping/Cart` → `ShoppingController.Cart()` → `Views/Shopping/Cart.cshtml`; JS đọc giỏ `localStorage`. Nút đặt hàng → GET `Shopping/Checkout` → `ShoppingController.Checkout()` → `Views/Shopping/Checkout.cshtml`. Nút gửi đơn POST cùng action → overload `Checkout(...)` trong controller xác thực, lưu đơn/chi tiết bằng EF, rồi redirect.

## File đầu tiên và nơi tìm chức năng

### Khi chạy ứng dụng

- File C# bắt đầu chạy là `Program.cs`: đăng ký dịch vụ/EF Core, dựng app, cấu hình middleware và route, rồi gọi `Run()`.
- Khi trình duyệt mở `/`, route mặc định gọi `HomeController.Index()` trong `Controllers/HomeController.cs`; controller chuẩn bị dữ liệu và chọn `Views/Home/Index.cshtml` để render.
- `Views/_ViewStart.cshtml` chọn `Views/Shared/_Layout.cshtml`. Layout tạo header/menu và nạp JavaScript dùng chung. URL khác có thể bắt đầu ở controller/view khác, nhưng server vẫn khởi động từ `Program.cs`.

### Nút được khai báo ở đâu, chạy tới đâu?

Nút/link thường được viết trực tiếp trong `.cshtml`; MVC không có một file chung “tạo tất cả nút”. `asp-controller`/`asp-action` tạo URL tới action; form POST gửi dữ liệu tới action; nút chỉ có `id`, class hoặc `data-*` thường được JavaScript bắt.

| Ví dụ | Nơi khai báo | Handler/đích và nơi có logic |
|---|---|---|
| Tìm xe trong trang danh sách | `Views/Motor/Index.cshtml`, ô tìm kiếm và các bộ lọc | Hàm JavaScript `filterProducts()` trong chính `Views/Motor/Index.cshtml` lọc/sắp xếp các thẻ đang có trên trình duyệt; đây không phải truy vấn mới tới controller/SQL. |
| Tìm xe từ header (nếu có form tìm kiếm) | `Views/Shared/_Layout.cshtml` | Form GET đưa từ khóa vào URL; xem `asp-controller`, `asp-action` hoặc `action` để biết controller nhận. |
| Thêm xe vào giỏ | Nút `.add-cart[data-id]` trong `Views/Motor/Index.cshtml` hoặc trang chi tiết | Listener trong `wwwroot/js/site.js` đọc ID xe và cập nhật `localStorage`; không gọi controller và chưa tạo đơn hàng trong database. |
| Gửi đơn hàng | Form checkout trong `Views/Shopping/Checkout.cshtml` | POST tới `ShoppingController.Checkout(...)` trong `Controllers/ShoppingController.cs`; controller kiểm tra dữ liệu, tạo đơn/chi tiết và gọi EF `SaveChangesAsync()`. |
| Tìm/lọc trong trang admin | Ô tìm kiếm/bộ lọc trong `Views/Admin/*.cshtml` | `wwwroot/js/admin.js` lọc nội dung đã nạp trong trang; nút submit của form mới gửi request tới action admin. |

Để lần theo chức năng: tìm markup nút trong view → đọc `asp-*`, `form action/method`, `id/class/data-*` → nếu là JavaScript thì tìm selector/hàm trong `wwwroot/js` hoặc `@section Scripts` → nếu request tới server thì tìm action trong controller → xem service/DbContext mà action gọi để biết chỗ đọc/ghi dữ liệu.


## File đầu tiên và nơi tìm chức năng

### Khi chạy ứng dụng

- File C# bắt đầu chạy là `Program.cs`: đăng ký dịch vụ/EF Core, dựng app, cấu hình middleware và route, rồi gọi `Run()`.
- Khi trình duyệt mở `/`, route mặc định gọi `HomeController.Index()` trong `Controllers/HomeController.cs`; controller chuẩn bị dữ liệu và chọn `Views/Home/Index.cshtml` để render.
- `Views/_ViewStart.cshtml` chọn `Views/Shared/_Layout.cshtml`. Layout tạo header/menu và nạp các file JavaScript dùng chung. URL khác có thể bắt đầu ở controller/view khác, nhưng server vẫn khởi động từ `Program.cs`.

### Nút được khai báo ở đâu, chạy tới đâu?

Nút/link thường được viết trực tiếp trong `.cshtml`; MVC không có một file chung “tạo tất cả nút”. `asp-controller`/`asp-action` tạo URL tới action; form POST gửi dữ liệu tới action; nút chỉ có `id`, class hoặc `data-*` thường được JavaScript bắt.

| Ví dụ | Nơi khai báo | Handler/đích và nơi có logic |
|---|---|---|
| Tìm xe trong trang danh sách | `Views/Motor/Index.cshtml`, ô tìm kiếm và các bộ lọc | Hàm JavaScript `filterProducts()` trong chính `Views/Motor/Index.cshtml` lọc/sắp xếp các thẻ đang có trên trình duyệt; đây không phải truy vấn mới tới controller/SQL. |
| Tìm xe từ header (nếu dùng form tìm kiếm) | `Views/Shared/_Layout.cshtml` | Form GET đưa từ khóa vào URL; action nhận từ khóa ở controller được chỉ định bởi form. Kiểm tra `asp-controller`, `asp-action` hoặc `action` tại form để biết đích chính xác. |
| Thêm xe vào giỏ | Nút `.add-cart[data-id]` trong `Views/Motor/Index.cshtml` hoặc trang chi tiết | Listener trong `wwwroot/js/site.js` đọc ID xe và cập nhật `localStorage`; không gọi controller và chưa tạo đơn hàng trong database. |
| Gửi đơn hàng | Form checkout trong `Views/Shopping/Checkout.cshtml` | POST tới `ShoppingController.Checkout(...)` trong `Controllers/ShoppingController.cs`; controller kiểm tra dữ liệu, tạo đơn/chi tiết và gọi EF `SaveChangesAsync()`. |
| Tìm/lọc trong trang admin | Ô tìm kiếm/bộ lọc trong `Views/Admin/*.cshtml` | `wwwroot/js/admin.js` lọc nội dung đã nạp trong trang; nút submit của form (nếu có) mới gửi request tới action admin. |

Để lần theo một chức năng: tìm markup nút trong view → đọc `asp-*`, `form action/method`, `id/class/data-*` → nếu là JavaScript thì tìm selector/hàm trong `wwwroot/js` hoặc `@section Scripts` → nếu request tới server thì tìm action cùng tên trong controller → xem service/DbContext mà action gọi để biết chỗ đọc/ghi dữ liệu.