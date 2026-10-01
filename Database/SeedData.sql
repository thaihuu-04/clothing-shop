-- =========================================================
-- DỮ LIỆU MẪU CHO FASHION STORE
-- Chạy được nhiều lần (chỉ thêm sản phẩm nếu chưa tồn tại)
-- Ảnh nằm trong wwwroot/images (nguồn: Unsplash - miễn phí)
-- =========================================================
USE ClothingStoreDB;
GO

-- Mô tả danh mục
UPDATE Categories SET Description = N'Áo thun, áo polo, sơ mi cho mọi phong cách' WHERE CategoryName = N'Áo' AND Description IS NULL;
UPDATE Categories SET Description = N'Quần jean, quần short, quần jogger' WHERE CategoryName = N'Quần' AND Description IS NULL;
UPDATE Categories SET Description = N'Váy, đầm nữ thanh lịch' WHERE CategoryName = N'Váy' AND Description IS NULL;
UPDATE Categories SET Description = N'Áo khoác, hoodie, vest' WHERE CategoryName = N'Áo khoác' AND Description IS NULL;
UPDATE Categories SET Description = N'Balo, túi xách và phụ kiện' WHERE CategoryName = N'Phụ kiện' AND Description IS NULL;
GO

DECLARE @Ao INT = (SELECT CategoryId FROM Categories WHERE CategoryName = N'Áo');
DECLARE @Quan INT = (SELECT CategoryId FROM Categories WHERE CategoryName = N'Quần');
DECLARE @Vay INT = (SELECT CategoryId FROM Categories WHERE CategoryName = N'Váy');
DECLARE @AoKhoac INT = (SELECT CategoryId FROM Categories WHERE CategoryName = N'Áo khoác');
DECLARE @PhuKien INT = (SELECT CategoryId FROM Categories WHERE CategoryName = N'Phụ kiện');

DECLARE @Nike INT = (SELECT BrandId FROM Brands WHERE BrandName = N'Nike');
DECLARE @Adidas INT = (SELECT BrandId FROM Brands WHERE BrandName = N'Adidas');
DECLARE @Uniqlo INT = (SELECT BrandId FROM Brands WHERE BrandName = N'Uniqlo');
DECLARE @Local INT = (SELECT BrandId FROM Brands WHERE BrandName = N'Local Brand');

DECLARE @Seed TABLE (
    ProductName NVARCHAR(200), CategoryId INT, BrandId INT, Description NVARCHAR(MAX),
    Price DECIMAL(18,2), SalePrice DECIMAL(18,2) NULL, Stock INT,
    Size NVARCHAR(50), Color NVARCHAR(50), Material NVARCHAR(100),
    ImageUrl NVARCHAR(500), IsFeatured BIT);

INSERT INTO @Seed VALUES
(N'Áo Thun Trắng Essential', @Ao, @Uniqlo, N'Áo thun trắng cổ tròn, chất cotton mềm mịn, dễ phối đồ hằng ngày.', 220000, NULL, 120, N'S,M,L,XL', N'Trắng', N'Cotton 100%', N'/images/products/aothun-trang.jpg', 1),
(N'Áo Thun Graphic Original', @Ao, @Local, N'Áo thun in hình phong cách streetwear, form rộng thoải mái.', 290000, 259000, 75, N'M,L,XL', N'Be', N'Cotton', N'/images/products/aothun-graphic.jpg', 0),
(N'Áo Thun Skeleton Đen', @Ao, @Local, N'Áo thun đen in hoạ tiết bàn tay xương cá tính.', 260000, NULL, 60, N'S,M,L', N'Đen', N'Cotton', N'/images/products/aothun-skeleton.jpg', 0),
(N'Áo Sơ Mi Công Sở', @Ao, @Uniqlo, N'Áo sơ mi dài tay lịch sự, ít nhăn, phù hợp đi làm.', 420000, 379000, 55, N'S,M,L,XL', N'Xanh navy', N'Cotton pha', N'/images/products/somi-congso.jpg', 1),
(N'Áo Khoác Da Biker', @AoKhoac, @Local, N'Áo khoác da dáng biker cá tính, lót trong ấm áp.', 1250000, 990000, 25, N'M,L,XL', N'Đen', N'Da PU', N'/images/products/aokhoac-da.jpg', 1),
(N'Áo Khoác Bomber', @AoKhoac, @Nike, N'Áo khoác bomber nhẹ, chống gió, phong cách năng động.', 790000, NULL, 40, N'M,L,XL', N'Nâu cam', N'Polyester', N'/images/products/aokhoac-bomber.jpg', 0),
(N'Áo Khoác Kaki Field', @AoKhoac, @Uniqlo, N'Áo khoác kaki nhiều túi, phong cách military.', 690000, 590000, 35, N'S,M,L', N'Xanh rêu', N'Kaki', N'/images/products/aokhoac-kaki.jpg', 0),
(N'Bộ Vest Nam Slim Fit', @AoKhoac, @Local, N'Bộ vest nam ôm dáng, thích hợp sự kiện và công sở.', 1890000, 1590000, 15, N'M,L,XL', N'Xanh navy', N'Polyester pha len', N'/images/products/vest-nam.jpg', 0),
(N'Áo Poncho Len Đan', @AoKhoac, @Local, N'Áo choàng len đan tua rua nhẹ nhàng, nữ tính.', 450000, NULL, 30, N'Free size', N'Kem', N'Len', N'/images/products/poncho-len.jpg', 0),
(N'Quần Jean Rách Patch', @Quan, @Adidas, N'Quần jean rách phối patch phong cách đường phố.', 590000, 520000, 45, N'29,30,31,32', N'Xanh đậm', N'Denim', N'/images/products/jean-patch.jpg', 0),
(N'Quần Jean Nữ Ống Đứng', @Quan, @Uniqlo, N'Quần jean nữ lưng cao, ống đứng tôn dáng.', 480000, NULL, 50, N'26,27,28,29', N'Xanh nhạt', N'Denim co giãn', N'/images/products/jean-nu.jpg', 1),
(N'Quần Jogger Nữ', @Quan, @Nike, N'Quần jogger nữ chất liệu mềm, bo gấu, thoải mái vận động.', 390000, 340000, 65, N'S,M,L', N'Hồng be', N'Polyester', N'/images/products/jogger-nu.jpg', 0),
(N'Quần Jean Đen Slim', @Quan, @Local, N'Quần jean đen dáng slim, dễ phối với mọi loại áo.', 520000, NULL, 4, N'29,30,31,32', N'Đen', N'Denim', N'/images/products/jean-den.jpg', 0),
(N'Váy Hoa Đỏ Vintage', @Vay, @Local, N'Váy hoa đỏ dáng xoè phong cách vintage, kèm thắt lưng.', 690000, 620000, 20, N'S,M,L', N'Đỏ', N'Voan', N'/images/products/vay-hoa-do.jpg', 1),
(N'Đầm Maxi Đỏ Dạ Tiệc', @Vay, @Local, N'Đầm maxi dáng dài thướt tha cho các buổi tiệc.', 950000, NULL, 12, N'S,M', N'Đỏ', N'Lụa', N'/images/products/dam-maxi-do.jpg', 0),
(N'Balo Basic Navy', @PhuKien, @Adidas, N'Balo tối giản, ngăn laptop 15 inch, chống nước nhẹ.', 550000, 490000, 3, N'Free size', N'Xanh navy', N'Polyester', N'/images/products/balo-basic.jpg', 0);

INSERT INTO Products (ProductName, CategoryId, BrandId, Description, Price, SalePrice, StockQuantity,
                      Size, Color, Material, ImageUrl, IsFeatured, IsActive, CreatedAt)
SELECT s.ProductName, s.CategoryId, s.BrandId, s.Description, s.Price, s.SalePrice, s.Stock,
       s.Size, s.Color, s.Material, s.ImageUrl, s.IsFeatured, 1, GETDATE()
FROM @Seed s
WHERE NOT EXISTS (SELECT 1 FROM Products p WHERE p.ProductName = s.ProductName);
GO

-- Bổ sung mô tả / chất liệu cho 6 sản phẩm gốc nếu còn trống
UPDATE Products SET Description = N'Áo thun basic form regular, chất cotton thoáng mát.' WHERE ProductName = N'Áo Thun Basic' AND Description IS NULL;
UPDATE Products SET Description = N'Hoodie nỉ bông dày dặn, mũ trùm và túi kangaroo.' WHERE ProductName = N'Áo Hoodie Basic' AND Description IS NULL;
UPDATE Products SET Description = N'Quần jean nam dáng straight cổ điển, bền màu.' WHERE ProductName = N'Quần Jean Nam' AND Description IS NULL;
UPDATE Products SET Description = N'Váy trắng trễ vai thanh lịch, nhẹ nhàng cho mùa hè.' WHERE ProductName = N'Váy Nữ Thanh Lịch' AND Description IS NULL;
UPDATE Products SET Description = N'Áo polo nam lịch lãm, cổ bẻ, vải cá sấu thoáng khí.' WHERE ProductName = N'Áo Polo Nam' AND Description IS NULL;
UPDATE Products SET Description = N'Quần short denim năng động cho ngày hè.' WHERE ProductName = N'Quần Short Nam' AND Description IS NULL;
GO
