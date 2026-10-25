using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EnsoPsikoloji.Models;

namespace EnsoPsikoloji.Models
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Rolleri Tanimlayalim
            string[] roller = { "Admin", "Psikolog", "Sekreter" };
            foreach (var rol in roller)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole(rol));
                }
            }

            // 2. Admin Kullanicisi (admin@ensopsikoloji.com / Admin123!)
            string adminEmail = "admin@ensopsikoloji.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var sonuc = await userManager.CreateAsync(adminUser, "Admin123!");
                if (sonuc.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3. Sekreter Kullanicisi (sekreter@ensopsikoloji.com / Sekreter123!)
            string sekreterEmail = "sekreter@ensopsikoloji.com";
            var sekreterUser = await userManager.FindByEmailAsync(sekreterEmail);
            if (sekreterUser == null)
            {
                sekreterUser = new IdentityUser
                {
                    UserName = sekreterEmail,
                    Email = sekreterEmail,
                    EmailConfirmed = true
                };
                var sekreterSonuc = await userManager.CreateAsync(sekreterUser, "Sekreter123!");
                if (sekreterSonuc.Succeeded)
                {
                    await userManager.AddToRoleAsync(sekreterUser, "Sekreter");
                }
            }

            // 4. Test Psikolog Kullanicisi (psikolog@ensopsikoloji.com / Psikolog123!)
            string psikologEmail = "psikolog@ensopsikoloji.com";
            var psikologUser = await userManager.FindByEmailAsync(psikologEmail);
            if (psikologUser == null)
            {
                psikologUser = new IdentityUser
                {
                    UserName = psikologEmail,
                    Email = psikologEmail,
                    EmailConfirmed = true
                };
                var pSonuc = await userManager.CreateAsync(psikologUser, "Psikolog123!");
                if (pSonuc.Succeeded)
                {
                    await userManager.AddToRoleAsync(psikologUser, "Psikolog");
                }
            }

            // 5. Ornek Verileri Yukle (Idempotent - yoksa ekler)
            await SeedOrnekVerilerAsync(context, psikologUser?.Id);
        }

        public static async Task<string> SeedOrnekVerilerAsync(ApplicationDbContext context, string? psikologUserId = null)
        {
            int eklenenPsikolog = 0;
            int eklenenHasta = 0;
            int eklenenRandevu = 0;

            // 1. Psikologlar
            var ornekPsikologlar = new List<Psikolog>
            {
                new Psikolog
                {
                    AdSoyad = "Elif Karaca",
                    UzmanlikAlani = "Bilisel Davranisci Terapi & Kaygi Bozukluklari",
                    Email = "psikolog@ensopsikoloji.com",
                    Telefon = "0532 111 22 33",
                    UserId = psikologUserId
                },
                new Psikolog
                {
                    AdSoyad = "Caner Yildiz",
                    UzmanlikAlani = "Cift ve Aile Terapisi, Iliski Danismanligi",
                    Email = "caner.yildiz@ensopsikoloji.com",
                    Telefon = "0533 222 33 44"
                },
                new Psikolog
                {
                    AdSoyad = "Dr. Selin Demir",
                    UzmanlikAlani = "EMDR & Yetiskin Travma Terapisi",
                    Email = "selin.demir@ensopsikoloji.com",
                    Telefon = "0535 333 44 55"
                },
                new Psikolog
                {
                    AdSoyad = "Mehmet Aksoy",
                    UzmanlikAlani = "Cocuk, Ergen ve Oyun Terapisi",
                    Email = "mehmet.aksoy@ensopsikoloji.com",
                    Telefon = "0536 444 55 66"
                }
            };

            foreach (var p in ornekPsikologlar)
            {
                if (!await context.Psikologlar.AnyAsync(x => x.AdSoyad == p.AdSoyad))
                {
                    await context.Psikologlar.AddAsync(p);
                    eklenenPsikolog++;
                }
            }
            await context.SaveChangesAsync();

            // 2. Danisanlar (Hastalar)
            var ornekHastalar = new List<Hasta>
            {
                new Hasta { Adi = "Ayse", Soyadi = "Yilmaz", Email = "ayse.yilmaz@gmail.com", Telefon = "0542 555 10 20", KayitTarihi = DateTime.Now.AddDays(-45) },
                new Hasta { Adi = "Burak", Soyadi = "Sahin", Email = "burak.sahin@hotmail.com", Telefon = "0530 444 20 30", KayitTarihi = DateTime.Now.AddDays(-30) },
                new Hasta { Adi = "Ceren", Soyadi = "Demir", Email = "ceren.demir@gmail.com", Telefon = "0555 333 40 50", KayitTarihi = DateTime.Now.AddDays(-20) },
                new Hasta { Adi = "Emre", Soyadi = "Koc", Email = "emre.koc@yahoo.com", Telefon = "0538 222 50 60", KayitTarihi = DateTime.Now.AddDays(-15) },
                new Hasta { Adi = "Merve", Soyadi = "Aydin", Email = "merve.aydin@gmail.com", Telefon = "0544 111 60 70", KayitTarihi = DateTime.Now.AddDays(-10) },
                new Hasta { Adi = "Selim", Soyadi = "Celik", Email = "selim.celik@outlook.com", Telefon = "0533 999 70 80", KayitTarihi = DateTime.Now.AddDays(-5) }
            };

            foreach (var h in ornekHastalar)
            {
                if (!await context.Hastalar.AnyAsync(x => x.Adi == h.Adi && x.Soyadi == h.Soyadi))
                {
                    await context.Hastalar.AddAsync(h);
                    eklenenHasta++;
                }
            }
            await context.SaveChangesAsync();

            // 3. Randevular
            var pList = await context.Psikologlar.ToListAsync();
            var hList = await context.Hastalar.ToListAsync();

            if (pList.Any() && hList.Any() && await context.Randevular.CountAsync() < 5)
            {
                var p1 = pList[0].Id;
                var p2 = pList.Count > 1 ? pList[1].Id : p1;
                var p3 = pList.Count > 2 ? pList[2].Id : p1;

                var h1 = hList[0].Id;
                var h2 = hList.Count > 1 ? hList[1].Id : h1;
                var h3 = hList.Count > 2 ? hList[2].Id : h1;
                var h4 = hList.Count > 3 ? hList[3].Id : h1;
                var h5 = hList.Count > 4 ? hList[4].Id : h1;

                var randevular = new List<Randevu>
                {
                    new Randevu { HastaId = h1, PsikologId = p1, RandevuTarihi = DateTime.Now.AddDays(-14).Date.AddHours(10), Ucret = 2000, OdemeDurumu = "Odendi", Durum = "Tamamlandi", Notlar = "Ilk degerlendirme seansi tamamlandi." },
                    new Randevu { HastaId = h2, PsikologId = p2, RandevuTarihi = DateTime.Now.AddDays(-10).Date.AddHours(14), Ucret = 2500, OdemeDurumu = "Odendi", Durum = "Tamamlandi", Notlar = "Iletisim blokajlari uzerine calisildi." },
                    new Randevu { HastaId = h3, PsikologId = p3, RandevuTarihi = DateTime.Now.AddDays(-7).Date.AddHours(16), Ucret = 2200, OdemeDurumu = "Odendi", Durum = "Tamamlandi", Notlar = "EMDR guvenli yer egzersizi yapildi." },
                    new Randevu { HastaId = h4, PsikologId = p1, RandevuTarihi = DateTime.Now.AddDays(-4).Date.AddHours(11), Ucret = 2000, OdemeDurumu = "Odendi", Durum = "Tamamlandi", Notlar = "Kaygi gunlugu odevleri kontrol edildi." },
                    new Randevu { HastaId = h1, PsikologId = p1, RandevuTarihi = DateTime.Today.AddHours(14), Ucret = 2000, OdemeDurumu = "Odendi", Durum = "Aktif", Notlar = "2. Seans - Otomatik olumsuz dusunceler." },
                    new Randevu { HastaId = h5, PsikologId = p2, RandevuTarihi = DateTime.Today.AddDays(1).AddHours(11), Ucret = 2500, OdemeDurumu = "Bekliyor", Durum = "Aktif", Notlar = "Ilk gorusme planlandi." },
                    new Randevu { HastaId = h2, PsikologId = p2, RandevuTarihi = DateTime.Today.AddDays(2).AddHours(15), Ucret = 2500, OdemeDurumu = "Bekliyor", Durum = "Aktif", Notlar = "Cift danismanligi takip seansi." },
                    new Randevu { HastaId = h3, PsikologId = p3, RandevuTarihi = DateTime.Today.AddDays(3).AddHours(13), Ucret = 2200, OdemeDurumu = "Bekliyor", Durum = "Aktif", Notlar = "Hedef ani calismasi." }
                };

                await context.Randevular.AddRangeAsync(randevular);
                await context.SaveChangesAsync();
                eklenenRandevu = randevular.Count;
            }

            // 4. Iletisim Talepleri
            if (!await context.IletisimTalepleri.AnyAsync())
            {
                var talepler = new List<IletisimTalep>
                {
                    new IletisimTalep { AdSoyad = "Zehra Parlak", Telefon = "0531 888 11 22", TalepDetayi = "EMDR Terapisi hakkinda ucret ve seans detaylarini ogrenmek istiyorum.", OlusturmaTarihi = DateTime.Now.AddHours(-3), ArandiMi = false },
                    new IletisimTalep { AdSoyad = "Kaan Yurt", Telefon = "0543 777 33 44", TalepDetayi = "Cift ve Evlilik Danismanligi icin hafta sonu randevu durumu var mi?", OlusturmaTarihi = DateTime.Now.AddHours(-18), ArandiMi = false }
                };
                await context.IletisimTalepleri.AddRangeAsync(talepler);
                await context.SaveChangesAsync();
            }

            return $"{eklenenPsikolog} psikolog, {eklenenHasta} danisan, {eklenenRandevu} randevu olusturuldu.";
        }
    }
}
