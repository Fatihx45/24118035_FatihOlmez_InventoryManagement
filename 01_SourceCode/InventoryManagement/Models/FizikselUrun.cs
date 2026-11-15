using InventoryManagement.Enums;

namespace InventoryManagement.Models
{
    /// <summary>
    /// Standart depo ve lojistik fiziksel ürün sınıfı.
    /// OOP İlkeleri: Kalıtım (Inheritance) ve Çok Biçimlilik (Polymorphism)
    /// </summary>
    public class FizikselUrun : UrunBase
    {
        public string RafKonumu { get; set; } = "A-1";
        public double AgirlikKg { get; set; } = 1.0;

        public FizikselUrun() : base()
        {
        }

        public FizikselUrun(string urunKodu, string urunAdi, UrunKategorisi kategori, decimal birimFiyat, int stokMiktari, string rafKonumu, double agirlikKg, int kritikSeviye = 10)
            : base(urunKodu, urunAdi, kategori, birimFiyat, stokMiktari, kritikSeviye)
        {
            RafKonumu = string.IsNullOrWhiteSpace(rafKonumu) ? "Genel Depo" : rafKonumu;
            AgirlikKg = agirlikKg > 0 ? agirlikKg : 0.5;
        }

        /// <summary>
        /// Çok Biçimlilik: Fiziksel ürünlerde standart stok seviyesi kontrolü.
        /// </summary>
        public override bool KritikStokMu()
        {
            return StokMiktari <= KritikStokSeviyesi;
        }

        public override string DetayliBilgiGetir()
        {
            return base.DetayliBilgiGetir() + $" | Raf: {RafKonumu} | Ağırlık: {AgirlikKg:N2} kg";
        }
    }
}
