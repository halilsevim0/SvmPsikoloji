using EnsoPsikoloji.Models;

namespace EnsoPsikoloji.Services
{
    public interface ISmsService
    {
        Task<bool> SendSmsAsync(string telefon, string mesaj);
        Task<bool> SendRandevuSmsAsync(Randevu randevu, string aliciAdi, string telefon, bool hatirlatmaMi = false);
    }
}

