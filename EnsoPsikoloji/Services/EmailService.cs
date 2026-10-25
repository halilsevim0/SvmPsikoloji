using System.Net;
using System.Net.Mail;
using EnsoPsikoloji.Models;

namespace EnsoPsikoloji.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string bodyHtml)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                return;

            try
            {
                var smtpHost = _configuration["EmailSettings:Host"];
                var smtpPortStr = _configuration["EmailSettings:Port"];
                var smtpUser = _configuration["EmailSettings:UserName"];
                var smtpPass = _configuration["EmailSettings:Password"];
                var fromAddress = _configuration["EmailSettings:From"] ?? "info@ensopsikoloji.com";
                var enableSsl = bool.TryParse(_configuration["EmailSettings:EnableSsl"], out var ssl) && ssl;

                // Eer gerek SMTP ayarlar henz girilmemise log dp simle edelim
                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser))
                {
                    _logger.LogInformation($"[E-Posta Simlasyonu] Kime: {toEmail} | Konu: {subject}");
                    return;
                }

                int.TryParse(smtpPortStr, out int port);
                if (port <= 0) port = 587;

                using var message = new MailMessage
                {
                    From = new MailAddress(fromAddress, "Enso Psikoloji"),
                    Subject = subject,
                    Body = bodyHtml,
                    IsBodyHtml = true
                };
                message.To.Add(toEmail);

                using var client = new SmtpClient(smtpHost, port)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = enableSsl
                };

                await client.SendMailAsync(message);
                _logger.LogInformation($"[E-Posta Baaryla Gnderildi] Kime: {toEmail} | Konu: {subject}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[E-Posta Gnderim Hatas] Kime: {toEmail} | Hata: {ex.Message}");
            }
        }

        public async Task SendRandevuOnayMailiAsync(Randevu randevu, string aliciEmail, string aliciAdi, bool psikologMu = false)
        {
            var baslik = psikologMu ? "Yeni Randevu Bilgisi" : "Randevu Onay Bilgilendirmesi";
            var doktorAdi = randevu.Psikolog != null ? randevu.Psikolog.AdSoyad : "Klinik Uzmanı";
            var danisanAdi = randevu.Hasta != null ? $"{randevu.Hasta.Adi} {randevu.Hasta.Soyadi}" : "Danışan";
            var tarih = randevu.RandevuTarihi.ToString("dd MMMM yyyy, dddd HH:mm");

            var mesajIcerik = psikologMu
                ? $"Sayın <strong>{doktorAdi}</strong>,<br><br>Takviminize yeni bir seans eklendi.<br>Danışan: <strong>{danisanAdi}</strong><br>Tarih & Saat: <strong>{tarih}</strong>"
                : $"Sayın <strong>{aliciAdi}</strong>,<br><br>Enso Psikoloji randevunuz başarıyla oluşturulmuştur.<br>Uzman Psikolog: <strong>{doktorAdi}</strong><br>Tarih & Saat: <strong>{tarih}</strong>";

            var html = GenerateHtmlTemplate(baslik, mesajIcerik, randevu);
            await SendEmailAsync(aliciEmail, $"Enso Psikoloji - {baslik}", html);
        }

        public async Task SendRandevuHatirlatmaMailiAsync(Randevu randevu, string aliciEmail, string aliciAdi)
        {
            var doktorAdi = randevu.Psikolog != null ? randevu.Psikolog.AdSoyad : "Klinik Uzmanı";
            var tarih = randevu.RandevuTarihi.ToString("dd MMMM yyyy, dddd HH:mm");

            var mesajIcerik = $"Sayın <strong>{aliciAdi}</strong>,<br><br>Enso Psikoloji kliniğimizdeki seans randevunuzu hatırlatmak isteriz.<br><br>Uzman Psikolog: <strong>{doktorAdi}</strong><br>Tarih & Saat: <strong>{tarih}</strong><br><br>Lütfen randevu saatinizden 10 dakika önce hazır bulunmanızı rica ederiz.";

            var html = GenerateHtmlTemplate("Randevu Hatırlatması", mesajIcerik, randevu);
            await SendEmailAsync(aliciEmail, "Enso Psikoloji - Randevu Hatırlatması", html);
        }

        private string GenerateHtmlTemplate(string title, string content, Randevu randevu)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8' />
                <style>
                    body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f6f9; margin: 0; padding: 20px; color: #333; }}
                    .card {{ max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.08); }}
                    .header {{ background: linear-gradient(135deg, #2b5876, #4e4376); color: white; padding: 25px; text-align: center; }}
                    .header h1 {{ margin: 0; font-size: 22px; letter-spacing: 0.5px; }}
                    .body {{ padding: 30px; font-size: 15px; line-height: 1.6; }}
                    .details {{ background: #f8fafc; border-left: 4px solid #4e4376; padding: 15px 20px; margin: 20px 0; border-radius: 4px; }}
                    .footer {{ background: #edf2f7; text-align: center; padding: 15px; font-size: 12px; color: #718096; }}
                </style>
            </head>
            <body>
                <div class='card'>
                    <div class='header'>
                        <h1>ENSO PSİKOLOJİ</h1>
                        <p style='margin: 5px 0 0 0; opacity: 0.9;'>{title}</p>
                    </div>
                    <div class='body'>
                        {content}
                        <div class='details'>
                            <strong>Randevu Kodu:</strong> #{randevu.Id}<br />
                            <strong>Durum:</strong> {randevu.Durum ?? "Aktif"}<br />
                            <strong>Ödeme:</strong> {randevu.OdemeDurumu ?? "Bekliyor"} ({randevu.Ucret:C2})
                        </div>
                        <p style='color: #666; font-size: 13px;'>Herhangi bir değişiklik veya iptal talebiniz için lütfen kliniğimiz ile iletişime geçiniz.</p>
                    </div>
                    <div class='footer'>
                        &copy; {DateTime.Now.Year} Enso Psikoloji Danışmanlık Merkezi. Tüm hakları saklıdır.
                    </div>
                </div>
            </body>
            </html>";
        }
    }
}

