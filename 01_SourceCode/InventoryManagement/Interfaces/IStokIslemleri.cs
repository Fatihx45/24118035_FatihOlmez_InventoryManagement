namespace InventoryManagement.Interfaces
{
    /// <summary>
    /// Stok hareket ve kritik seviye kontrollerini yöneten sözleşme arayüzü.
    /// OOP İlkesi: Soyutlama (Abstraction)
    /// </summary>
    public interface IStokIslemleri
    {
        int KritikStokSeviyesi { get; set; }
        bool KritikStokMu();
        void StokArttir(int miktar);
        void StokAzalt(int miktar);
    }
}
