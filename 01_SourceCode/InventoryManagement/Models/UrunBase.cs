using System;
using InventoryManagement.Enums;
using InventoryManagement.Exceptions;
using InventoryManagement.Interfaces;

namespace InventoryManagement.Models
{
    /// <summary>
    /// Tüm ürünlerin türetildiği soyut temel sınıf.
    /// OOP İlkeleri:
    /// - Soyutlama (Abstraction): abstract class ve abstract metotlar
    /// - Kapsülleme (Encapsulation): private alanlar ve kontrollü property erişimleri
    /// </summary>
    public abstract class UrunBase : IUrun, IStokIslemleri
    {
        private static int _idSayac = 1000;

        // Kapsüllenmiş özel (private) alanlar
        private readonly int _id;
        private string _urunKodu = string.Empty;
        private string _urunAdi = string.Empty;
        private UrunKategorisi _kategori;
        private decimal _birimFiyat;
        private int _stokMiktari;
        private int _kritikStokSeviyesi;
        private DateTime _olusturulmaTarihi;

        // Constructor 1 (Parametresiz - Varsayılan)
        protected UrunBase()
        {
            _id = ++_idSayac;
            _olusturulmaTarihi = DateTime.Now;
            _kritikStokSeviyesi = 10;
        }

        // Constructor 2 (Parametreli)
        protected UrunBase(string urunKodu, string urunAdi, UrunKategorisi kategori, decimal birimFiyat, int stokMiktari, int kritikStokSeviyesi = 10)
            : this()
        {
            UrunKodu = urunKodu;
            UrunAdi = urunAdi;
            Kategori = kategori;
            BirimFiyat = birimFiyat;
            StokMiktari = stokMiktari;
            KritikStokSeviyesi = kritikStokSeviyesi;
        }

        #region Kapsüllenmiş Özellikler (Properties)

        public int Id => _id;

        public string UrunKodu
        {
            get => _urunKodu;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Ürün kodu boş bırakılamaz!");
                _urunKodu = value.Trim().ToUpperInvariant();
            }
        }

        public string UrunAdi
        {
            get => _urunAdi;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Ürün adı boş bırakılamaz!");
                if (value.Any(char.IsDigit))
                    throw new ArgumentException("Ürün adı rakam içeremez! Lütfen sadece harflerden oluşan bir ad giriniz.");
                _urunAdi = value.Trim();
            }
        }

        public UrunKategorisi Kategori
        {
            get => _kategori;
            set => _kategori = value;
        }

        public decimal BirimFiyat
        {
            get => _birimFiyat;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Birim fiyat 0'dan küçük olamaz!");
                _birimFiyat = value;
            }
        }

        public int StokMiktari
        {
            get => _stokMiktari;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Stok miktarı negatif olamaz!");
                _stokMiktari = value;
            }
        }

        public int KritikStokSeviyesi
        {
            get => _kritikStokSeviyesi;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Kritik stok seviyesi negatif olamaz!");
                _kritikStokSeviyesi = value;
            }
        }

        public DateTime OlusturulmaTarihi => _olusturulmaTarihi;

        #endregion

        #region Stok İşlemleri ve Çok Biçimlilik (Polymorphism)

        /// <summary>
        /// Seçilen ürüne stok girişi yapar.
        /// </summary>
        public virtual void StokArttir(int miktar)
        {
            if (miktar <= 0)
                throw new GecersizMiktarException("Giriş yapılacak stok miktarı 0'dan büyük olmalıdır!");

            _stokMiktari += miktar;
        }

        /// <summary>
        /// Seçilen üründen stok çıkışı yapar.
        /// </summary>
        public virtual void StokAzalt(int miktar)
        {
            if (miktar <= 0)
                throw new GecersizMiktarException("Çıkış yapılacak stok miktarı 0'dan büyük olmalıdır!");

            if (miktar > _stokMiktari)
                throw new YetersizStokException(_stokMiktari, miktar);

            _stokMiktari -= miktar;
        }

        /// <summary>
        /// Çok Biçimlilik (Polymorphism):
        /// Alt sınıflar kendi özel kural ve dinamiklerine göre bu metodu override eder.
        /// </summary>
        public abstract bool KritikStokMu();

        /// <summary>
        /// Sanal Metot: Toplam envanter maliyet değerini hesaplar.
        /// </summary>
        public virtual decimal ToplamDegerHesapla()
        {
            return _birimFiyat * _stokMiktari;
        }

        /// <summary>
        /// Sanal Metot: Ürünün detaylı özet bilgisini döner.
        /// </summary>
        public virtual string DetayliBilgiGetir()
        {
            return $"[{UrunKodu}] {UrunAdi} | Kategori: {Kategori} | Fiyat: {_birimFiyat:C2} | Stok: {_stokMiktari} Adet";
        }

        #endregion
    }
}
