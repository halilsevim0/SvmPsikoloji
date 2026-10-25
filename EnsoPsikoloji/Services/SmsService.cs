using EnsoPsikoloji.Models;

namespace EnsoPsikoloji.Services
{
    public class SmsService : ISmsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmsService> _logger;

        public SmsService(IConfiguration configuration, ILogger<SmsService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendSmsAsync(string telefon, string mesaj)
        {
            if (string.IsNullOrWhiteSpace(telefon) || string.IsNullOrWhiteSpace(mesaj))
                return false;

            try
            {
                // SMS API Entegrasyonu (rnein NetGsm, MutluCell, Twilio vb.)
                var smsApiKey = _configuration["SmsSettings:ApiKey"];
                var smsBaslik = _configuration["SmsSettings:Header"] ?? "ENSO PSIKOLOJI";

                if (string.IsNullOrEmpty(smsApiKey))
                {
                    // Entegrasyon anahtar henz girilmemise konsola/loga simle eder
                    _logger.LogInformation($"[SMS Simlasyonu] Kime: {telefon} | Balk: {smsBaslik} | Mesaj: {mesaj}");
                    await Task.CompletedTask;
                    return true;
                }

                // Gerek HTTP POST istei burada tetiklenebilir
                _logger.LogInformation($"[SMS Gnderildi] Kime: {telefon} | Mesaj: {mesaj}");
                await Task.CompletedTask;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[SMS Hatas] Kime: {telefon} | Hata: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendRandevuSmsAsync(Randevu randevu, string aliciAdi, string telefon, bool hatirlatmaMi = false)
        {
            var doktorAdi = randevu.Psikolog != null ? randevu.Psikolog.AdSoyad : "Klinik Uzmanı";
            var tarih = randevu.RandevuTarihi.ToString("dd.MM.yyyy HH:mm");

            string mesaj = hatirlatmaMi
                ? $"Sn. {aliciAdi}, Enso Psikoloji kliniğimizdeki seansınız {tarih} tarihindedir. 10 dk önce hazır olmanızı rica ederiz."
                : $"Sn. {aliciAdi}, Enso Psikoloji randevunuz {tarih} tarihine oluşturulmuştur. Uzman: {doktorAdi}. Detay: info@ensopsikoloji.com";

            return await SendSmsAsync(telefon, mesaj);
        }
    }
}

