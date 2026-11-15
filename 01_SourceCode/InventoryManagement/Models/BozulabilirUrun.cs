using System;
using InventoryManagement.Enums;

namespace InventoryManagement.Models
{
    /// <summary>
    /// Gıda ve hızlı tüketim gibi son kullanma tarihi olan ürün sınıfı.
    /// OOP İlkeleri: Kalıtım (Inheritance) ve Çok Biçimlilik (Polymorphism)
    /// </summary>
    public class BozulabilirUrun : UrunBase
    {
        public DateTime SonKullanmaTarihi { get; set; }
        public double SaklamaSicakligi { get; set; } = 4.0; // Santigrat derece

        public BozulabilirUrun() : base()
        {
            SonKullanmaTarihi = DateTime.Today.AddDays(30);
        }

        public BozulabilirUrun(string urunKodu, string urunAdi, decimal birimFiyat, int stokMiktari, DateTime sonKullanmaTarihi, double saklamaSicakligi = 4.0, int kritikSeviye = 15)
            : base(urunKodu, urunAdi, UrunKategorisi.Gida, birimFiyat, stokMiktari, kritikSeviye)
        {
            SonKullanmaTarihi = sonKullanmaTarihi;
            SaklamaSicakligi = saklamaSicakligi;
        }

        /// <summary>
        /// Çok Biçimlilik (Polymorphism - Override):
        /// Bozulabilir ürünlerde sadece stok miktarı değil, son kullanma tarihine 7 günden az kalması da ürünü kritik duruma sokar!
        /// </summary>
        public override bool KritikStokMu()
        {
            bool stokKritik = StokMiktari <= KritikStokSeviyesi;
            bool sktKritik = (SonKullanmaTarihi.Date - DateTime.Today).TotalDays <= 7;
            return stokKritik || sktKritik;
        }

        /// <summary>
        /// Kalan gün sayısını hesaplar.
        /// </summary>
        public int KalanGunSayisi()
        {
            return (int)Math.Ceiling((SonKullanmaTarihi.Date - DateTime.Today).TotalDays);
        }

        public override string DetayliBilgiGetir()
        {
            int kalanGun = KalanGunSayisi();
            string sktDurumu = kalanGun < 0 ? "SÜRESİ GEÇMİŞ!" : $"{kalanGun} gün kaldı";
            return base.DetayliBilgiGetir() + $" | SKT: {SonKullanmaTarihi:dd.MM.yyyy} ({sktDurumu}) | Sıcaklık: {SaklamaSicakligi}°C";
        }
    }
}
