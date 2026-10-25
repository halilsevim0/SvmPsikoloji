using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnsoPsikoloji.Models
{
    [Table("SeansNotlari")]
    public class SeansNotu
    {
        public int Id { get; set; }

        [Required]
        public int HastaId { get; set; }

        [ForeignKey("HastaId")]
        public virtual Hasta? Hasta { get; set; }

        [Required]
        public int PsikologId { get; set; }

        [ForeignKey("PsikologId")]
        public virtual Psikolog? Psikolog { get; set; }

        public int? RandevuId { get; set; }

        [ForeignKey("RandevuId")]
        public virtual Randevu? Randevu { get; set; }

        [Required(ErrorMessage = "Seans notu içeriği boş bırakılamaz.")]
        [Display(Name = "Klinik / Seans Notu")]
        public string NotIcerigi { get; set; } = string.Empty;

        [Display(Name = "Gizli Not (Sadece Ben Göreyim)")]
        public bool GizliNotMu { get; set; } = false;

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;

        [Display(Name = "Güncellenme Tarihi")]
        public DateTime? GuncellenmeTarihi { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}