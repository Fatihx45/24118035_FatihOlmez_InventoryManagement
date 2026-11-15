using System;
using InventoryManagement.Enums;
using InventoryManagement.Models;

namespace InventoryManagement.Services
{
    /// <summary>
    /// Sistemin ilk açılışında veya kullanıcı isteğinde zengin örnek veriler yükleyen servis.
    /// Farklı alt sınıfların (FizikselUrun, BozulabilirUrun, ElektronikUrun) çok biçimli davranışlarını sergiler.
    /// </summary>
    public static class DemoDataService
    {
        public static void OrnekVerileriYukle(StokManager manager)
        {
            if (manager.ToplamUrunSayisi > 0) return; // Zaten veri varsa tekrar ekleme

            // 1. Elektronik Ürünler
            manager.UrunEkle(new ElektronikUrun(
                "ELK-101",
                "Logitech MX Master Kablosuz Mouse",
                3250.00m,
                18,
                24,
                "SN-LOGI-8821",
                kritikSeviye: 5
            ));

            manager.UrunEkle(new ElektronikUrun(
                "ELK-102",
                "Dell UltraSharp Profesyonel Monitor",
                18500.00m,
                3, // Kritik stok seviyesinde (5 + 2 = 7 eşiği altında)
                36,
                "SN-DELL-4412",
                kritikSeviye: 5
            ));

            manager.UrunEkle(new ElektronikUrun(
                "ELK-103",
                "Samsung Shield Tasinabilir Bellek",
                4100.00m,
                25,
                24,
                "SN-SMSG-9910",
                kritikSeviye: 8
            ));

            // 2. Bozulabilir (Gıda) Ürünler
            manager.UrunEkle(new BozulabilirUrun(
                "GID-201",
                "Dogal Suzme Cam Bali Kavanoz",
                420.00m,
                45,
                DateTime.Today.AddDays(180),
                saklamaSicakligi: 18.0,
                kritikSeviye: 10
            ));

            manager.UrunEkle(new BozulabilirUrun(
                "GID-202",
                "Taze Organik Gunluk Sut",
                38.50m,
                60,
                DateTime.Today.AddDays(4), // Son kullanma tarihine 4 gün kaldığı için çok biçimli kural gereği KRİTİK!
                saklamaSicakligi: 4.0,
                kritikSeviye: 15
            ));

            manager.UrunEkle(new BozulabilirUrun(
                "GID-203",
                "Eski Kasar Peyniri Blok",
                260.00m,
                8, // Stok miktarı 12'den az olduğu için kritik
                DateTime.Today.AddDays(45),
                saklamaSicakligi: 4.0,
                kritikSeviye: 12
            ));

            // 3. Fiziksel Depo Ürünleri (Tekstil & Kırtasiye)
            manager.UrunEkle(new FizikselUrun(
                "TEK-301",
                "Pamuklu Is Guvenligi Polar Ceket",
                UrunKategorisi.Tekstil,
                890.00m,
                40,
                rafKonumu: "B-2-04",
                agirlikKg: 0.85,
                kritikSeviye: 10
            ));

            manager.UrunEkle(new FizikselUrun(
                "KIR-401",
                "Beyaz Fotokopi Kagidi Koli",
                UrunKategorisi.Kirtasiye,
                650.00m,
                7, // Kritik seviyede (15'ten az)
                rafKonumu: "C-1-01",
                agirlikKg: 12.5,
                kritikSeviye: 15
            ));

            manager.UrunEkle(new FizikselUrun(
                "KIR-402",
                "Stabilo Boss Fosforlu Kalem Seti",
                UrunKategorisi.Kirtasiye,
                295.00m,
                85,
                rafKonumu: "C-2-10",
                agirlikKg: 0.35,
                kritikSeviye: 20
            ));

            // Örnek stok hareketleri yap
            manager.StokGirisYap("ELK-101", 10, "Distribütörden yeni sevkiyat");
            manager.StokCikisYap("GID-201", 5, "Müşteri siparişi çıkışı");
            manager.StokGirisYap("KIR-402", 20, "Toptancı ilave teslimat");
        }
    }
}
