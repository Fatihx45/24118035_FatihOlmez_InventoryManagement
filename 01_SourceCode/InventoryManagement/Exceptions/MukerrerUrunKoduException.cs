using System;

namespace InventoryManagement.Exceptions
{
    /// <summary>
    /// Sistemde zaten var olan bir ürün koduyla yeni kayıt eklenmeye çalışıldığında fırlatılır.
    /// </summary>
    public class MukerrerUrunKoduException : Exception
    {
        public string UrunKodu { get; }

        public MukerrerUrunKoduException(string urunKodu)
            : base($"'{urunKodu}' ürün koduna sahip bir kayıt zaten mevcut! Ürün kodları benzersiz olmalıdır.")
        {
            UrunKodu = urunKodu;
        }
    }
}
