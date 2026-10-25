using EnsoPsikoloji.Models;

namespace EnsoPsikoloji.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string bodyHtml);
        Task SendRandevuOnayMailiAsync(Randevu randevu, string aliciEmail, string aliciAdi, bool psikologMu = false);
        Task SendRandevuHatirlatmaMailiAsync(Randevu randevu, string aliciEmail, string aliciAdi);
    }
}

