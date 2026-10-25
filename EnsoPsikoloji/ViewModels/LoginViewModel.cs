using System.ComponentModel.DataAnnotations;

namespace EnsoPsikoloji.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "E-posta alani zorunludur.")]
        [EmailAddress(ErrorMessage = "Gecerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sifre alani zorunludur.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Beni Hatirla")]
        public bool RememberMe { get; set; }
    }
}
