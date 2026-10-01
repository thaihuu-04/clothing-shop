using System.ComponentModel.DataAnnotations;

namespace QA.ViewModels
{
    public class ProfileViewModel
    {
        // Chỉ hiển thị, không cho sửa (không bắt buộc khi submit)
        [Display(Name = "Tên đăng nhập")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [Display(Name = "Giới tính")]
        public string? Gender { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateOnly? DateOfBirth { get; set; }

        public DateTime CreatedAt { get; set; }

        public static readonly string[] Genders = { "Nam", "Nữ", "Khác" };
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string CurrentPassword { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới")]
        [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu nhập lại không khớp")]
        [DataType(DataType.Password)]
        [Display(Name = "Nhập lại mật khẩu mới")]
        public string ConfirmPassword { get; set; } = "";
    }

    public class AddressViewModel
    {
        public int AddressId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên người nhận")]
        [StringLength(100)]
        [Display(Name = "Tên người nhận")]
        public string ReceiverName { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập Tỉnh/Thành phố")]
        [StringLength(100)]
        [Display(Name = "Tỉnh / Thành phố")]
        public string Province { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập Quận/Huyện")]
        [StringLength(100)]
        [Display(Name = "Quận / Huyện")]
        public string District { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập Phường/Xã")]
        [StringLength(100)]
        [Display(Name = "Phường / Xã")]
        public string Ward { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ cụ thể")]
        [StringLength(255)]
        [Display(Name = "Số nhà, tên đường")]
        public string AddressDetail { get; set; } = "";

        [Display(Name = "Đặt làm địa chỉ mặc định")]
        public bool IsDefault { get; set; }
    }
}
