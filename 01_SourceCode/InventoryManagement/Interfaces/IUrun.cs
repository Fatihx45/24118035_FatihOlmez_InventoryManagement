using InventoryManagement.Enums;

namespace InventoryManagement.Interfaces
{
    /// <summary>
    /// Temel ürün kimlik ve finansal özelliklerini tanımlayan arayüz.
    /// OOP İlkesi: Soyutlama (Abstraction)
    /// </summary>
    public interface IUrun
    {
        int Id { get; }
        string UrunKodu { get; set; }
        string UrunAdi { get; set; }
        UrunKategorisi Kategori { get; set; }
        decimal BirimFiyat { get; set; }
        int StokMiktari { get; set; }

        decimal ToplamDegerHesapla();
        string DetayliBilgiGetir();
    }
}
