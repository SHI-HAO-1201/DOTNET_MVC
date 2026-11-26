# Bản đồ file và luồng nút trong MyMvcApp

Tài liệu này mô tả những file ảnh hưởng trực tiếp đến trang, nút, biểu mẫu và request trong ứng dụng MVC. Đây là hướng dẫn để lần theo hoặc bổ sung chức năng; khi code thay đổi, hãy cập nhật lại mục tương ứng.

## 1. Hiểu luồng MVC trước khi sửa nút

Trong project này, phần lớn thao tác gửi lên server là **HTTP request tới MVC action**, chưa phải API REST trả JSON. Link thường gửi `GET`; form thường gửi `POST`; action kiểm tra dữ liệu, đọc/ghi SQL qua Entity Framework rồi trả view hoặc redirect sang trang khác. Các nút giỏ hàng, yêu thích, lọc và một số tương tác admin chỉ chạy JavaScript tại trình duyệt.

```text
Người dùng bấm
  ├─ <a asp-controller/asp-action> ── GET URL ──> Controller action ──> View/Redirect
  ├─ <form method="post"> + submit ── POST URL + form fields ──> Controller action
  │                                                      └─> Model validation / SQL / SaveChanges
  │                                                          └─> Redirect hoặc render lại View
  └─ <button type="button" data-...> ── JavaScript listener ──> DOM/localStorage
                                                    └─ chỉ gọi server nếu code JS thực sự dùng fetch()
```

`asp-controller`, `asp-action`, `asp-route-*`, `asp-for` được Razor Tag Helpers xử lý. Với form POST có Tag Helper, ASP.NET Core thường sinh token chống giả mạo; action POST ở controller cũng đánh dấu `[ValidateAntiForgeryToken]`.

**“Nút gọi API” trong code hiện tại:** không thấy frontend gọi `fetch()`/XHR tới API JSON. Checkout, đăng nhập, đánh giá, tồn kho… gửi form tới MVC action. Giỏ hàng/yêu thích dùng `localStorage`, chưa lưu SQL. Các link điều hướng cũng là request GET, không phải thao tác lưu dữ liệu.

## 2. File nền tảng: ứng dụng chạy và dùng dữ liệu thế nào

| File | Ý nghĩa và liên quan tới nút/luồng |
|---|---|
| `Program.cs` | Điểm khởi động. Đăng ký MVC, `AppDbContext`, cookie authentication, authorization và `SharedAdminStore`; cấu hình middleware, route mặc định `{controller=Home}/{action=Index}/{id?}`. Gọi `DatabaseInitializer.InitializeAsync()` trước khi nhận request. Không chứa markup nút. |
| `MyMvcApp.csproj` | Khai báo project web, phiên bản .NET, nullable/implicit usings và EF Core SQL Server. |
| `appsettings.json` | Cấu hình dùng chung, gồm SQL connection string và logging. Không đặt logic nút ở đây. |
| `appsettings.Development.json` | Cấu hình bổ sung khi chạy môi trường Development. |
| `Properties/launchSettings.json` | Profile `http`/`https`, URL localhost và môi trường khi chạy local. |
| `Views/_ViewImports.cshtml` | Import namespace cho view và bật MVC Tag Helpers, nên các thuộc tính `asp-action`, `asp-for` hoạt động. |
| `Views/_ViewStart.cshtml` | Layout mặc định `_Layout.cshtml` cho trang khách hàng. |
| `Views/Admin/_ViewStart.cshtml` | Layout `_AdminLayout.cshtml` cho view trong khu vực admin. |
| `Views/Shared/_Layout.cshtml` | Header/footer, menu thương hiệu, menu tài khoản, link giỏ hàng; nạp Bootstrap, `site.js`, `password-toggle.js`. Đây là nơi sửa menu toàn site. |
| `Views/Shared/_AdminLayout.cshtml` | Sidebar admin, link tới các màn hình quản trị, nút đăng xuất và menu mobile; nạp `admin.js`. Sửa menu quản trị ở đây. |
| `Views/Shared/Error.cshtml` | Trang lỗi dùng bởi `Home/Error`. |
| `Views/Shared/_ValidationScriptsPartial.cshtml` | Nạp thư viện validation client-side cho form dùng partial này. Validation phía server vẫn do controller/model quyết định. |
| `wwwroot/js/site.js` | Giỏ hàng, số lượng trên icon, yêu thích, render giỏ/trang yêu thích, chuẩn bị JSON giỏ hàng trước khi submit checkout. Không gọi controller cho các thao tác local này. |
| `wwwroot/js/password-toggle.js` | Bắt nút `data-password-toggle`, đổi input giữa `password`/`text`; không gửi request. |
| `wwwroot/js/admin.js` | Menu mobile, toast, tìm/lọc bảng, lọc trạng thái đơn, tương tác chart và các nút demo. Chỉ các form/link có URL tương ứng mới gửi request server. |
| `wwwroot/css/site.css`, `wwwroot/css/admin.css`, `Views/Shared/_Layout.cshtml.css` | Giao diện/kiểu nút; không định nghĩa xử lý nghiệp vụ. |
| `wwwroot/lib/*` | Thư viện Bootstrap, jQuery validation, unobtrusive validation. Không phải controller/API của project. |

## 3. Controller, service, model và database

### Controllers

| File | Vai trò |
|---|---|
| `Controllers/HomeController.cs` | Trang chủ, giới thiệu, chính sách, liên hệ và lỗi. `Index` đọc 3 xe đang kinh doanh. `Contact` POST hiện chỉ đặt trạng thái thông báo demo, **chưa lưu yêu cầu liên hệ vào SQL**. |
| `Controllers/MotorController.cs` | Danh sách xe, chi tiết, lọc thương hiệu/loại xe, gửi đánh giá và yêu cầu liên hệ khi xe hết hàng. Hai POST cuối ghi dữ liệu vào SQL. |
| `Controllers/ShoppingController.cs` | GET giỏ/checkout và POST đặt hàng. POST checkout kiểm tra lại sản phẩm, tồn kho, mã khuyến mãi, khách hàng, cập nhật tồn kho và tạo đơn/chi tiết trong transaction. |
| `Controllers/AccountController.cs` | Đăng nhập admin/khách, đăng ký, hồ sơ, đơn hàng, yêu thích, đăng xuất và màn quên mật khẩu. Hồ sơ/đơn yêu cầu đăng nhập khách bằng kiểm tra trong action. |
| `Controllers/AdminController.cs` | Các action quản trị; cả controller có `[Authorize(Roles = "Admin")]`. POST cập nhật sản phẩm, đơn, khách, tồn, khuyến mãi, đánh giá, nhân viên và cài đặt. |
| `Controllers/SharedAdminStore.cs` | Dù file đặt trong `Controllers`, namespace là `MyMvcApp.Services`. Service này xác thực/tạo lại tài khoản quản trị dùng chung. `AccountController` gọi nó khi đăng nhập admin; `Program.cs` đăng ký singleton. Không phải controller/view. |

### Models

| File | Ý nghĩa |
|---|---|
| `Models/AccountProfileViewModel.cs` | Dữ liệu hồ sơ khách khi hiển thị/cập nhật. |
| `Models/AdminProductInput.cs` | Input form tạo/sửa sản phẩm. |
| `Models/AdminCustomerInput.cs` | Input form tạo/sửa khách hàng trong admin. |
| `Models/AdminPromotionInput.cs` | Input tạo/sửa chương trình khuyến mãi. |
| `Models/AdminStaffInput.cs` | Input tạo/sửa nhân viên. |
| `Models/StoreSettingsInput.cs` | Input thông tin/cờ cài đặt cửa hàng. |
| `Models/ReviewInput.cs` | Dữ liệu form gửi đánh giá xe. |
| `Models/ProductContactRequestInput.cs` | Dữ liệu khách để lại khi xe hết hàng. |
| `Models/AdminStatisticsViewModel.cs` | Số liệu dashboard/báo cáo. |
| `Models/AdminOrdersViewModel.cs` | Dữ liệu màn đơn hàng admin. |
| `Models/AdminCustomersViewModel.cs` | Dữ liệu màn khách hàng admin. |
| `Models/AdminPromotionsViewModel.cs` | Dữ liệu màn khuyến mãi admin. |
| `Models/AdminReviewsViewModel.cs` | Dữ liệu màn đánh giá admin. |
| `Models/AdminStaffViewModel.cs` | Dữ liệu màn nhân viên admin. |
| `Models/ErrorViewModel.cs` | Request ID cho trang lỗi. |

### Data / Entity Framework

| File | Ý nghĩa |
|---|---|
| `Data/AppDbContext.cs` | Khai báo DbSet và ánh xạ Entity ↔ SQL. Xe được ánh xạ tới `dbo.XeMays`; các bảng nghiệp vụ như `Orders`, `Customers`, `Reviews`, `Promotions`… được map tại đây. |
| `Data/AppDbContextFactory.cs` | Tạo DbContext cho lệnh EF design-time/migration, đọc cấu hình connection string. |
| `Data/DatabaseInitializer.cs` | Khi app khởi động, gọi EF `MigrateAsync()` để áp migration còn thiếu. |
| `Data/Motorcycle.cs` | Entity xe; `ImageUrl` biến chuỗi ảnh trong DB thành URL dùng trong view hoặc ảnh placeholder. |
| `Data/Customer.cs` | Entity khách hàng/tài khoản khách. |
| `Data/MotorcycleOrder.cs`, `Data/MotorcycleOrderItem.cs` | Entity đơn hàng và dòng xe trong đơn. |
| `Data/MotorcycleReview.cs` | Entity đánh giá. |
| `Data/ProductContactRequest.cs` | Yêu cầu liên hệ khi xe chưa có hàng. |
| `Data/Promotion.cs` | Mã khuyến mãi và lượt dùng. |
| `Data/StaffMember.cs` | Hồ sơ nhân viên quản trị. |
| `Data/StoreSetting.cs` | Cài đặt cửa hàng dạng key/value. |
| `Data/MotorcycleCatalog.cs` | Danh sách mẫu xe seed trong migration/catalog; dữ liệu hiển thị thực tế do `AppDbContext` đọc SQL. |
| `Data/Use-CodeFirst.ps1`, `Data/Use-DatabaseFirst.ps1`, `Data/README.md` | Script và ghi chú quy trình database/migration. Không trực tiếp xử lý nút trên trang. |
| `Migrations/*.cs` | Lịch sử schema/seed của EF Core. Thêm migration khi thay đổi schema/model, không cần migration chỉ để sửa giá trị ảnh/tên/giá trong bản ghi hiện có. |

## 4. Luồng theo trang khách hàng

| Trang / file chứa nút | Nút/thao tác → đích | Xử lý tiếp theo và dữ liệu |
|---|---|---|
| `Views/Shared/_Layout.cshtml` | Logo/Trang chủ → `Home/Index`; Xe mô tô → `Motor/Index`; thương hiệu → `Motor/ByCategory?brand=...`; Về chúng tôi/Liên hệ → `Home/About`, `Home/Contact`; giỏ → `Shopping/Cart`. | GET tới controller rồi render view. Thay menu site-wide ở file này. |
| `_Layout.cshtml` menu tài khoản | Đăng nhập/đăng ký/hồ sơ/đơn/yêu thích → `Account/*`; đăng xuất là form POST `Account/Logout`. | Đăng xuất xóa cookie; các mục hồ sơ/đơn được controller yêu cầu khách đăng nhập. Menu được JavaScript/CSS hiển thị theo role. |
| `Views/Home/Index.cshtml` | Xem xe/ảnh → `Motor/Details/{id}`; nút thêm giỏ `.add-cart[data-id]` được `site.js` chèn hoặc bắt. | HomeController lấy 3 xe; nút giỏ ghi `motospeed-cart` trong localStorage. |
| `Views/Motor/Index.cshtml` | Tìm, lọc hãng/loại/giá, sắp xếp, xóa lọc. | JavaScript trong `@section Scripts` ẩn/sắp xếp các card đã tải; không query SQL lại. |
| `Motor/Index.cshtml` | Ảnh/tên xe → `Motor/Details/{id}`; thêm giỏ → `.add-cart[data-id]`; xe hết hàng → `Details#incoming-contact`. | Link mở trang; giỏ ghi localStorage; để lại thông tin mở form ở chi tiết. |
| `Views/Motor/ByCategory.cshtml` | Chọn xe hoặc thêm giỏ. | `MotorController.ByCategory(brand)` tìm theo Brand; nếu không có brand nào khớp mà lại khớp Type thì lọc theo loại. Trang dùng cùng cách thêm giỏ/localStorage như danh sách. |
| `Views/Motor/Details.cshtml` | Thêm giỏ/yêu thích; gửi đánh giá; form liên hệ xe sắp về. | Giỏ/yêu thích chạy `site.js` trong trình duyệt. Form đánh giá POST `Motor/SubmitReview` → kiểm tra `ReviewInput` → ghi `Reviews` → redirect về chi tiết. Form liên hệ POST `Motor/RequestIncomingProduct` → kiểm tra xe active và tồn kho 0 → ghi `ProductContactRequests` → redirect về chi tiết. |
| `Views/Shopping/Cart.cshtml` | Tăng/giảm số lượng, xóa dòng, xóa giỏ. | Nút được `site.js` tạo/bắt theo `data-quantity-id`, `data-remove`, `#clear-cart`; cập nhật `motospeed-cart`, tổng tiền và badge ở trình duyệt. |
| `Cart.cshtml` | Tiến hành đặt hàng → GET `Shopping/Checkout`; tiếp tục mua → `Motor/Index`. | Chỉ điều hướng; chưa tạo đơn. |
| `Views/Shopping/Checkout.cshtml` | Chọn cách thanh toán và Gửi đơn đặt xe. | `site.js` chặn submit nếu local cart trống, đưa danh sách `{id, quantity}` vào hidden `cartJson`. POST `Shopping/Checkout` kiểm tra form, cài đặt thanh toán, xe/tồn kho và promotion; ghi/cập nhật `Customers`, `Orders`, `OrderItems`, `XeMays.SoLuong`, lượt khuyến mãi trong transaction; redirect về Checkout và xóa cart local sau khi thành công. Giá client không được dùng làm giá có thẩm quyền; controller tính từ DB. |
| `Views/Account/Login.cshtml` | Đăng nhập POST `Account/Login`; quên mật khẩu/đăng ký là GET link; hiện mật khẩu là JS. | Controller thử xác thực admin qua `SharedAdminStore`, sau đó thử khách trong `Customers`; thành công phát cookie role phù hợp và redirect. Nút đăng nhập ngoài hiện disabled, chưa cấu hình OAuth. |
| `Views/Account/Register.cshtml` | Tạo tài khoản POST `Account/Register`; hiện mật khẩu là JS. | Controller validate, hash mật khẩu, lưu khách SQL, phát cookie khách và redirect Profile. Checkbox điều khoản là kiểm tra trình duyệt; controller vẫn kiểm tra dữ liệu đăng ký. |
| `Views/Account/Profile.cshtml` | Lưu thay đổi POST `Account/Profile`; link tab hồ sơ/đơn/yêu thích/giỏ; đăng xuất POST. | Hồ sơ đọc/ghi `Customers`; yêu cầu khách đăng nhập. |
| `Views/Account/Orders.cshtml` | Tab đơn/hồ sơ/yêu thích, khám phá xe khi trống. | `Account/Orders` đọc đơn theo customer ID/email đang đăng nhập. |
| `Views/Account/Saved.cshtml` | Khám phá xe hoặc xóa mẫu yêu thích bằng nút được tạo trong JS. | `site.js` đọc `motospeed-saved` localStorage và render thẻ xe. Danh sách yêu thích hiện **chỉ nằm trên trình duyệt**, không đồng bộ giữa thiết bị và không lưu SQL. |
| `Views/Account/ForgotPassword.cshtml` | Gửi hướng dẫn POST `Account/ForgotPassword`. | Hiện chỉ trả thông báo liên hệ cửa hàng; chưa gửi email, chưa tạo token/đặt lại mật khẩu. |
| `Views/Home/Contact.cshtml` | Gửi form POST `Home/Contact`. | Hiện chỉ hiển thị trạng thái gửi trên view; chưa lưu SQL/gửi email. Cần thêm entity, DbSet/migration hoặc dịch vụ email nếu muốn thành luồng liên hệ thật. |
| `Views/Home/About.cshtml`, `Privacy.cshtml` | Link điều hướng trong nội dung/footer. | Trang tĩnh; controller trả view, không có thao tác ghi dữ liệu. |

## 5. Luồng từng màn hình quản trị

Menu admin nằm trong `Views/Shared/_AdminLayout.cshtml`; mọi màn hình đi tới action `AdminController`. Controller được bảo vệ bằng role Admin. Hành động lưu/xóa thường là POST rồi redirect về danh sách để tránh gửi lại form khi refresh.

| Màn hình / view | Nút hoặc thao tác | Đích và kết quả |
|---|---|---|
| `Views/Admin/Dashboard.cshtml` | Làm mới `data-reload-page`; link xem đơn/sản phẩm; bấm tháng trong chart. | Reload và điều hướng là browser. Chart được `admin.js` tương tác từ dữ liệu đã render; không gọi API. |
| `Views/Admin/Products.cshtml` | Thêm → GET `CreateProduct`; Sửa → GET `EditProduct/{id}`; Lưu trong form → POST `SaveProduct`; Ngừng bán → POST `DeleteProduct`; Ẩn/hiện → POST `ToggleProductVisibility`; Xuất CSV → GET `ExportProducts`. | Save tạo/sửa `dbo.XeMays`; DeleteProduct ngừng kinh doanh, không xóa lịch sử đơn; Toggle đổi khả năng hiển thị; export trả file. Tìm/lọc bảng chạy JS tại client. |
| `Views/Admin/ProductForm.cshtml` | Lưu/submit → POST `SaveProduct`; Hủy/quay lại → GET `Products`. | `AdminProductInput` được validate, action lưu thông tin sản phẩm/ảnh vào `XeMays`, rồi redirect hoặc trả lại form lỗi. |
| `Views/Admin/Inventory.cshtml` | Lưu tồn → POST `UpdateStock`; Trạng thái/Sửa → GET `EditProduct`; bộ lọc tình trạng. | UpdateStock ghi `XeMays.SoLuong`; lọc tình trạng chạy JS cục bộ. |
| `Views/Admin/Orders.cshtml` | Select trạng thái đổi bằng `onchange` submit → POST `UpdateOrderStatus`; xóa → POST `DeleteOrder`; lọc trạng thái/checkbox nếu có. | Controller cập nhật đơn, kiểm tra chuyển trạng thái và các quy tắc tồn kho; lọc bảng chạy `admin.js` không gọi server. Xóa đơn chỉ dành cho nghiệp vụ admin và ảnh hưởng bản ghi đơn/chi tiết theo logic controller. |
| `Views/Admin/Customers.cshtml` | Thêm → GET `CreateCustomer`; Sửa → GET `EditCustomer`; lưu form → POST `SaveCustomer`; xóa → POST `DeleteCustomer`. | Đọc/ghi `Customers`; nếu khách có đơn thì controller có thể từ chối xóa để giữ quan hệ/lịch sử. Tìm/lọc là client-side. |
| `Views/Admin/CustomerForm.cshtml` | Lưu → `SaveCustomer`; Hủy/quay lại → `Customers`. | Bind vào `AdminCustomerInput`; server validate rồi ghi DB. |
| `Views/Admin/ProductContactRequests.cshtml` | Đánh dấu đã liên hệ/chưa liên hệ → POST `MarkProductContactRequest`; xóa → POST `DeleteProductContactRequest`. | Ghi cờ liên hệ hoặc xóa yêu cầu trong `ProductContactRequests`; không phải gửi tin nhắn tự động. |
| `Views/Admin/Promotions.cshtml` | Tạo → GET `CreatePromotion`; sửa → GET `EditPromotion`; xóa → POST `DeletePromotion`. | Đọc/ghi `Promotions`; xóa có kiểm tra/ràng buộc sử dụng. Lọc/tìm bảng là JavaScript. |
| `Views/Admin/PromotionForm.cshtml` | Lưu → POST `SavePromotion`; Hủy/quay lại → `Promotions`. | Bind `AdminPromotionInput`; validate mã, ngày, lượt dùng rồi ghi SQL. Checkout đọc bảng này để áp mã. |
| `Views/Admin/Reviews.cshtml` | Lưu phản hồi → POST `SaveReviewReply`; ẩn/hiện → POST `ToggleReviewVisibility`; xóa → POST `DeleteReview`. | Ghi/xóa `Reviews`; trạng thái hiển thị quyết định review xuất hiện trong `Motor/Details`. |
| `Views/Admin/Reports.cshtml` | Áp dụng/Xóa lọc → GET `Reports?start=...&end=...`; Xuất CSV → GET `ExportReport`. | Controller tính số liệu từ các đơn theo khoảng ngày; export trả file. Đây là GET vì không thay đổi dữ liệu. |
| `Views/Admin/Staff.cshtml` | Thêm → GET `CreateStaff`; sửa → GET `EditStaff`; xóa → POST `DeleteStaff`. | Đọc/ghi `StaffMembers`. |
| `Views/Admin/StaffForm.cshtml` | Lưu → POST `SaveStaff`; Hủy/quay lại → `Staff`. | Bind `AdminStaffInput`; server lưu nhân viên. |
| `Views/Admin/Settings.cshtml` | Lưu thay đổi → POST `SaveSettings`. | Ghi `StoreSettings`; checkout đọc các cờ `BankTransfer`/`PayOnDelivery` để bật/tắt lựa chọn thanh toán. |

### Nút admin chỉ là giao diện minh họa

`wwwroot/js/admin.js` xử lý `data-demo-action`, lọc/tìm phía client, chart, sidebar và một số tab. Khi handler chỉ gọi `showToast()` hoặc đổi class, dữ liệu **không** được lưu. Các nút checkbox hàng loạt, phân trang và tab bộ lọc minh họa không phải API nếu không có form/link tới action. Đừng nhầm toast “đã làm” với thao tác SQL.

## 6. Danh sách toàn bộ view và trang tương ứng

| File view | Trang/action thường render |
|---|---|
| `Views/Home/Index.cshtml`, `About.cshtml`, `Privacy.cshtml`, `Contact.cshtml` | Trang chủ, giới thiệu, chính sách, liên hệ. |
| `Views/Motor/Index.cshtml`, `ByCategory.cshtml`, `Details.cshtml` | Danh sách, lọc hãng/loại, chi tiết xe. |
| `Views/Shopping/Cart.cshtml`, `Checkout.cshtml` | Giỏ trình duyệt và quy trình tạo đơn trong SQL. |
| `Views/Account/Login.cshtml`, `Register.cshtml`, `ForgotPassword.cshtml`, `Profile.cshtml`, `Orders.cshtml`, `Saved.cshtml` | Xác thực, hồ sơ, đơn, yêu thích local. |
| `Views/Admin/Dashboard.cshtml`, `Products.cshtml`, `ProductForm.cshtml`, `Inventory.cshtml`, `Orders.cshtml`, `Customers.cshtml`, `CustomerForm.cshtml`, `ProductContactRequests.cshtml`, `Promotions.cshtml`, `PromotionForm.cshtml`, `Reviews.cshtml`, `Reports.cshtml`, `Staff.cshtml`, `StaffForm.cshtml`, `Settings.cshtml` | Các màn hình quản trị; xem bảng màn hình ở mục 5 để biết action cụ thể. |

Razor mặc định tìm view cùng tên action trong thư mục controller. Một số action dùng `View("ProductForm", ...)`, `View("CustomerForm", ...)`… nên nhiều action dùng chung một file form.

## 7. Cách thêm hoặc sửa một nút có ghi dữ liệu

Ví dụ thêm nút “Đánh dấu đã xử lý” cho bản ghi admin:

1. **View:** đặt nút trong `<form asp-controller="Admin" asp-action="MarkDone" method="post">`, truyền ID bằng `<input type="hidden" name="id" value="...">`.
2. **Controller:** thêm `[HttpPost]` action `MarkDone(int id)`, kiểm tra quyền/đầu vào/tồn tại, cập nhật entity và `await database.SaveChangesAsync()`; trả `RedirectToAction(...)`.
3. **Entity/DbContext:** nếu trạng thái cần lưu mà schema chưa có cột/bảng, cập nhật entity và `AppDbContext`, sau đó tạo/apply migration phù hợp. Nếu cột đã tồn tại thì không cần migration.
4. **Input model:** dùng model input riêng và validation attributes khi form có nhiều trường; kiểm tra `ModelState.IsValid` ở server. Không tin giá/ID/trạng thái do browser gửi mà chưa xác nhận lại từ SQL.
5. **Liên kết màn hình:** thêm link vào `_Layout.cshtml` hoặc `_AdminLayout.cshtml` nếu chức năng cần xuất hiện trong menu.
6. **Thông báo/lỗi:** dùng `TempData` sau redirect hoặc trả lại view với `ModelState` khi cần hiển thị lỗi.

## 8. Cách thêm nút chỉ thao tác trình duyệt

Dùng `<button type="button" data-my-action="...">` để tránh submit form ngoài ý muốn; thêm listener trong `wwwroot/js/site.js` hoặc `admin.js`. Nếu cần giữ dữ liệu qua lần tải lại, quyết định rõ dùng `localStorage` (chỉ trên máy người dùng) hay gửi lên server để lưu SQL. Không lưu mật khẩu/thông tin nhạy cảm vào localStorage.

## 9. Nếu chức năng thực sự cần API JSON

Cần chủ động tạo endpoint trả `Ok(...)`/`BadRequest(...)` (hoặc controller API riêng), định nghĩa input/output rõ ràng, validation + authorization + antiforgery phù hợp; sau đó gọi endpoint bằng `fetch()` trong JavaScript và xử lý lỗi/trạng thái tải. Hiện các luồng ở trên chủ yếu là MVC form/link, nên không cần thêm API chỉ để điều hướng trang hoặc submit form thông thường.

## 10. Cách lần theo một nút khi tự chỉnh

1. Tìm chữ nút hoặc `id`/class/`data-*` bằng `rg` trong `Views` và `wwwroot/js`.
2. Nếu là `<a>`, xem `asp-controller`, `asp-action`, `asp-route-*` hoặc `href`.
3. Nếu là `<button>`, xem nó nằm trong form nào, `type` là `submit` hay `button`, có `data-*` không.
4. Với `submit`, tìm action có tên tương ứng trong `Controllers/*.cs`, đọc attribute `[HttpPost]`, validation, `SaveChangesAsync`, `RedirectToAction`.
5. Với `data-*`, tìm selector trong `wwwroot/js/*.js` hoặc `@section Scripts` của chính view.
6. Lần dữ liệu xuống `Models/*Input.cs`, entity `Data/*.cs`, ánh xạ `Data/AppDbContext.cs`, rồi bảng SQL.
7. Để biết vì sao trang render dữ liệu gì, lần ngược action GET/controller → `return View(model)` → `@model` trong `.cshtml`.

## 11. Khi chạy `dotnet watch`

Chạy lệnh từ thư mục `MyMvcApp`. `dotnet watch` khởi động server từ `Program.cs`, không chạy một view cố định. Browser gọi URL mặc định `/` → route chọn `HomeController.Index()` → controller đọc xe SQL → `Views/Home/Index.cshtml` → `_ViewStart.cshtml` chọn `_Layout.cshtml` → layout render menu/nội dung và nạp JavaScript dùng chung. URL khác sẽ chạy controller/action tương ứng.
