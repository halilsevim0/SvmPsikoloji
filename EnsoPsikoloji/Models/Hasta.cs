using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnsoPsikoloji.Models
{
    public class Hasta
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad alani zorunludur.")]
        [Display(Name = "Ad")]
        public string Adi { get; set; } = string.Empty;

        [NotMapped]
        public string Ad { get => Adi; set => Adi = value; }

        [Required(ErrorMessage = "Soyad alani zorunludur.")]
        [Display(Name = "Soyad")]
        public string Soyadi { get; set; } = string.Empty;

        [NotMapped]
        public string Soyad { get => Soyadi; set => Soyadi = value; }

        [Display(Name = "E-posta")]
        public string? Email { get; set; }

        [Display(Name = "Telefon")]
        public string? Telefon { get; set; }

        [Display(Name = "Kayit Tarihi")]
        public DateTime KayitTarihi { get; set; } = DateTime.Now;

        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}
