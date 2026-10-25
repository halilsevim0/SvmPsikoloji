using System.ComponentModel.DataAnnotations;

namespace EnsoPsikoloji.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "E-posta alani zorunludur.")]
        [EmailAddress(ErrorMessage = "Gecerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sifre alani zorunludur.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Sifre Tekrar")]
        [Compare("Password", ErrorMessage = "Sifreler birbiriyle uyusmuyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
