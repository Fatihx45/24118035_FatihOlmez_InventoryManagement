using System;
using InventoryManagement.Enums;
using InventoryManagement.Interfaces;

namespace InventoryManagement.Models
{
    /// <summary>
    /// Depoda gerçekleşen tüm stok giriş ve çıkış işlemlerinin denetim kaydını tutan sınıf.
    /// OOP İlkesi: Soyutlama (Abstraction - ILoglanabilir)
    /// </summary>
    public class StokHareketi : ILoglanabilir
    {
        private static int _hareketIdSayac = 0;

        public int Id { get; }
        public DateTime IslemZamani { get; }
        public string UrunKodu { get; set; }
        public string UrunAdi { get; set; }
        public IslemTipi IslemTuru { get; set; }
        public int Miktar { get; set; }
        public int OncekiStok { get; set; }
        public int SonrakiStok { get; set; }
        public string Aciklama { get; set; }

        public StokHareketi(string urunKodu, string urunAdi, IslemTipi islemTuru, int miktar, int oncekiStok, int sonrakiStok, string aciklama = "")
        {
            Id = ++_hareketIdSayac;
            IslemZamani = DateTime.Now;
            UrunKodu = urunKodu;
            UrunAdi = urunAdi;
            IslemTuru = islemTuru;
            Miktar = miktar;
            OncekiStok = oncekiStok;
            SonrakiStok = sonrakiStok;
            Aciklama = string.IsNullOrWhiteSpace(aciklama) ? (islemTuru == IslemTipi.Giris ? "Stok Girişi" : "Stok Çıkışı") : aciklama;
        }

        public string LogFormatiOlustur()
        {
            return $"[{IslemZamani:dd.MM.yyyy HH:mm:ss}] {IslemTuru.ToString().ToUpper()} | Kod: {UrunKodu} ({UrunAdi}) | Miktar: {Miktar} | Değişim: {OncekiStok} -> {SonrakiStok} | Not: {Aciklama}";
        }
    }
}
