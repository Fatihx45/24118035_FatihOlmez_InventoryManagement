using System;

namespace InventoryManagement.Exceptions
{
    /// <summary>
    /// Stok giriş/çıkış işlemlerinde sıfır veya negatif miktar girildiğinde fırlatılır.
    /// </summary>
    public class GecersizMiktarException : Exception
    {
        public GecersizMiktarException(string mesaj) : base(mesaj)
        {
        }
    }
}
