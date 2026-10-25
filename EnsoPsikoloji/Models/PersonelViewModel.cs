using System.ComponentModel.DataAnnotations;

namespace EnsoPsikoloji.Models
{
    public class PersonelListViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Display(Name = "E-Posta")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Kullanici Adi")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "Roller")]
        public string Roller { get; set; } = string.Empty;
    }

    public class PersonelOlusturViewModel
    {
        [Required(ErrorMessage = "E-posta alani zorunludur.")]
        [EmailAddress(ErrorMessage = "Gecerli bir e-posta giriniz.")]
        [Display(Name = "E-Posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sifre zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Sifre")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rol secimi zorunludur.")]
        [Display(Name = "Rol")]
        public string SecilenRol { get; set; } = string.Empty;

        [Display(Name = "Ad Soyad (Psikolog ise)")]
        public string? AdSoyad { get; set; }

        [Display(Name = "Uzmanlik Alani")]
        public string? UzmanlikAlani { get; set; }

        [Display(Name = "Telefon")]
        public string? Telefon { get; set; }
    }
}
