namespace EnsoPsikoloji.ViewModels
{
    public class FinansRaporViewModel
    {
        // Genel Finans
        public decimal ToplamCiro { get; set; }
        public decimal TahsilEdilenUcret { get; set; }
        public decimal BekleyenTahsilat { get; set; }

        public decimal BuAyCiro { get; set; }
        public decimal BuAyTahsilEdilen { get; set; }
        public decimal BuAyBekleyen { get; set; }

        // Seans & Hasta İstatistikleri
        public int ToplamRandevuSayisi { get; set; }
        public int TamamlananRandevuSayisi { get; set; }
        public int IptalRandevuSayisi { get; set; }
        public int ToplamHastaSayisi { get; set; }
        public int ToplamPsikologSayisi { get; set; }

        // Talep / Dönüşüm
        public int ToplamIletisimTalep { get; set; }
        public int AranmisTalepSayisi { get; set; }

        // Psikolog Bazlı Hakediş Listesi
        public List<PsikologFinansOzeti> PsikologOzetleri { get; set; } = new();

        // Son Seans Hareketleri
        public List<SonSeansHareketi> SonSeanslar { get; set; } = new();
    }

    public class PsikologFinansOzeti
    {
        public int PsikologId { get; set; }
        public string AdSoyad { get; set; } = string.Empty;
        public string UzmanlikAlani { get; set; } = string.Empty;
        public int ToplamSeans { get; set; }
        public decimal ToplamUretilenCiro { get; set; }
        public decimal TahsilEdilen { get; set; }
        public decimal Bekleyen { get; set; }
        // Varsayılan %50 hakediş örneği
        public decimal TahminiHakedis => TahsilEdilen * 0.50m;
    }

    public class SonSeansHareketi
    {
        public int RandevuId { get; set; }
        public string DanisanAdi { get; set; } = string.Empty;
        public string PsikologAdi { get; set; } = string.Empty;
        public DateTime Tarih { get; set; }
        public decimal Ucret { get; set; }
        public string OdemeDurumu { get; set; } = string.Empty;
        public string Durum { get; set; } = string.Empty;
    }
}

