using System;
using System.ComponentModel.DataAnnotations;

namespace EnsoPsikoloji.Models
{
    public class IletisimTalep
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad Soyad alani zorunludur.")]
        [Display(Name = "Ad Soyad")]
        public string AdSoyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefon numarasi zorunludur.")]
        [Phone(ErrorMessage = "Gecerli bir telefon numarasi giriniz.")]
        [Display(Name = "Telefon Numarasi")]
        public string Telefon { get; set; } = string.Empty;

        [Display(Name = "Gorusmek Istediginiz Alan veya Psikolog")]
        public string? TalepDetayi { get; set; }

        [Display(Name = "Talep Tarihi")]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        [Display(Name = "Ulasildi mi?")]
        public bool ArandiMi { get; set; } = false;
    }
}
