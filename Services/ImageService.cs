namespace QA.Services
{
    public interface IImageService
    {
        Task<ServiceResult<string>> SaveAsync(IFormFile file, string folder);
        void Delete(string? imageUrl);
    }

    public class ImageService : IImageService
    {
        private const long MaxFileSize = 5 * 1024 * 1024;
        private const string UploadPrefix = "up-";

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        private readonly IWebHostEnvironment _env;

        public ImageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        // Lưu ảnh vào wwwroot/images/{folder}, trả về đường dẫn web (vd: /images/products/up-xxx.jpg)
        public async Task<ServiceResult<string>> SaveAsync(IFormFile file, string folder)
        {
            if (file.Length == 0)
            {
                return ServiceResult<string>.Fail("File ảnh rỗng.");
            }

            if (file.Length > MaxFileSize)
            {
                return ServiceResult<string>.Fail("Ảnh tối đa 5MB.");
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
            {
                return ServiceResult<string>.Fail("Chỉ chấp nhận ảnh .jpg, .jpeg, .png, .webp.");
            }

            string directory = Path.Combine(_env.WebRootPath, "images", folder);
            Directory.CreateDirectory(directory);

            string fileName = $"{UploadPrefix}{Guid.NewGuid():N}{extension}";

            await using (var stream = new FileStream(Path.Combine(directory, fileName), FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return ServiceResult<string>.Ok($"/images/{folder}/{fileName}");
        }

        // Chỉ xóa ảnh do admin upload (không xóa ảnh mẫu hay link ngoài)
        public void Delete(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl) ||
                !imageUrl.StartsWith("/images/") ||
                !Path.GetFileName(imageUrl).StartsWith(UploadPrefix))
            {
                return;
            }

            string path = Path.GetFullPath(Path.Combine(_env.WebRootPath, imageUrl.TrimStart('/')));

            if (path.StartsWith(_env.WebRootPath) && File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
