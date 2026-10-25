using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnsoPsikoloji.Models
{
    public class Randevu
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Hasta secimi zorunludur.")]
        [Display(Name = "Hasta")]
        public int HastaId { get; set; }
        public Hasta? Hasta { get; set; }

        [Required(ErrorMessage = "Psikolog secimi zorunludur.")]
        [Display(Name = "Psikolog")]
        public int PsikologId { get; set; }
        public Psikolog? Psikolog { get; set; }

        [Required(ErrorMessage = "Randevu tarihi ve saati zorunludur.")]
        [Display(Name = "Randevu Tarihi")]
        public DateTime RandevuTarihi { get; set; }

        [Display(Name = "Seans Notlari")]
        public string? Notlar { get; set; }

        [Display(Name = "Durum")]
        public string? Durum { get; set; } = "Aktif";

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Ucret (TL)")]
        public decimal? Ucret { get; set; } = 0;

        [Display(Name = "Odeme Durumu")]
        public string? OdemeDurumu { get; set; } = "Bekliyor"; // "Odendi" veya "Bekliyor"

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}
