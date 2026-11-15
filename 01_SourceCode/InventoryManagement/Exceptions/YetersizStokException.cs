using System;

namespace InventoryManagement.Exceptions
{
    /// <summary>
    /// Depoda mevcut olandan daha fazla stok çıkışı yapılmak istendiğinde fırlatılır.
    /// </summary>
    public class YetersizStokException : Exception
    {
        public int MevcutStok { get; }
        public int TalepEdilenMiktar { get; }

        public YetersizStokException(int mevcutStok, int talepEdilenMiktar)
            : base($"Yetersiz stok! Mevcut stok miktarı: {mevcutStok}, çıkışı istenen miktar: {talepEdilenMiktar}.")
        {
            MevcutStok = mevcutStok;
            TalepEdilenMiktar = talepEdilenMiktar;
        }
    }
}
