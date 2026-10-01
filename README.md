# clothing-shop

Website bán quần áo **Fashion Store** viết bằng ASP.NET Core MVC.

## Công nghệ
- .NET 10, ASP.NET Core MVC, Razor Views
- Entity Framework Core 10 (Database-First) + SQL Server
- Bootstrap 5, Bootstrap Icons, Chart.js
- Xác thực bằng Cookie Authentication, phân quyền Admin / User

## Chức năng
**Khách hàng**
- Trang chủ: danh mục, sản phẩm nổi bật, hàng mới, đang giảm giá
- Danh sách sản phẩm: tìm kiếm, lọc theo danh mục / thương hiệu / khoảng giá / khuyến mãi, sắp xếp, phân trang
- Chi tiết sản phẩm, sản phẩm liên quan
- Giỏ hàng (thêm bằng AJAX, kiểm tra tồn kho), mua ngay
- Thanh toán: chọn địa chỉ, COD hoặc chuyển khoản, phí ship (miễn phí từ 500.000đ)
- Đơn hàng của tôi: theo dõi trạng thái, hủy đơn khi đang chờ xác nhận
- Tài khoản: hồ sơ, đổi mật khẩu, sổ địa chỉ

**Quản trị (`/Admin`)**
- Dashboard: doanh thu, biểu đồ 12 tháng, đơn theo trạng thái, sản phẩm bán chạy, sắp hết hàng
- Quản lý sản phẩm (upload ảnh, ẩn/hiện, nổi bật), danh mục, thương hiệu
- Quản lý đơn hàng: lọc, cập nhật trạng thái (tự hoàn kho khi hủy), trạng thái thanh toán
- Quản lý người dùng: khóa/mở khóa, đổi quyền

## Cài đặt & chạy
1. Tạo database `ClothingStoreDB` trên SQL Server.
2. Sửa chuỗi kết nối `ConnectionStrings:DefaultConnection` trong `appsettings.json`.
3. Thêm dữ liệu mẫu (dùng `-f 65001` để giữ tiếng Việt):
   ```
   sqlcmd -S <server> -U <user> -P <password> -f 65001 -i Database\SeedData.sql
   ```
4. Chạy ứng dụng:
   ```
   dotnet run --launch-profile https
   ```
   Mở https://localhost:7200 (hoặc http://localhost:5225).

## Tài khoản thử
| Vai trò | Tên đăng nhập | Mật khẩu |
|---|---|---|
| Admin | `admin` | `123456` |
| Khách | `user01` | `123456` |

## Ghi chú
Ảnh sản phẩm minh hoạ lấy từ [Unsplash](https://unsplash.com) (miễn phí).
