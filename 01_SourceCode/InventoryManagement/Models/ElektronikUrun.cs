using InventoryManagement.Enums;

namespace InventoryManagement.Models
{
    /// <summary>
    /// Teknoloji ve elektronik cihaz sınıfı.
    /// OOP İlkeleri: Kalıtım (Inheritance) ve Çok Biçimlilik (Polymorphism)
    /// </summary>
    public class ElektronikUrun : UrunBase
    {
        public int GarantiSuresiAy { get; set; } = 24;
        public string SeriNoSerisi { get; set; } = "SN-GENEL";

        public ElektronikUrun() : base()
        {
        }

        public ElektronikUrun(string urunKodu, string urunAdi, decimal birimFiyat, int stokMiktari, int garantiSuresiAy, string seriNoSerisi, int kritikSeviye = 5)
            : base(urunKodu, urunAdi, UrunKategorisi.Elektronik, birimFiyat, stokMiktari, kritikSeviye)
        {
            GarantiSuresiAy = garantiSuresiAy > 0 ? garantiSuresiAy : 12;
            SeriNoSerisi = string.IsNullOrWhiteSpace(seriNoSerisi) ? "SN-DEFAULT" : seriNoSerisi.Trim();
        }

        /// <summary>
        /// Çok Biçimlilik: Elektronik ürünlerde tedarik süresi uzun olduğu için güvenlik payı eklenir.
        /// </summary>
        public override bool KritikStokMu()
        {
            return StokMiktari <= (KritikStokSeviyesi + 2);
        }

        public override string DetayliBilgiGetir()
        {
            return base.DetayliBilgiGetir() + $" | Garanti: {GarantiSuresiAy} Ay | Seri Serisi: {SeriNoSerisi}";
        }
    }
}
