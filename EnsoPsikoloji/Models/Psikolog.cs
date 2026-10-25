using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnsoPsikoloji.Models
{
    public class Psikolog
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad Soyad alani zorunludur.")]
        [Display(Name = "Ad Soyad")]
        public string AdSoyad { get; set; } = string.Empty;

        [NotMapped]
        public string Ad { get => AdSoyad; set => AdSoyad = value; }

        [NotMapped]
        public string Soyad { get => ""; set { } }

        [Required(ErrorMessage = "Uzmanlik alani zorunludur.")]
        [Display(Name = "Uzmanlik Alani")]
        public string UzmanlikAlani { get; set; } = string.Empty;

        [NotMapped]
        public string UzmanlikAlan { get => UzmanlikAlani; set => UzmanlikAlani = value; }

        [Display(Name = "E-posta")]
        public string? Email { get; set; }

        [Display(Name = "Telefon")]
        public string? Telefon { get; set; }

        // Sisteme giris yapan Identity kullanicisinin (AspNetUsers) Id'si
        public string? UserId { get; set; }

        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}
