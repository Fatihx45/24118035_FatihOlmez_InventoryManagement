using System;
using System.Collections.Generic;
using System.Linq;
using InventoryManagement.Enums;
using InventoryManagement.Exceptions;
using InventoryManagement.Models;

namespace InventoryManagement.Services
{
    /// <summary>
    /// Stok yönetim iş mantığını yürüten ve verileri bellekte (in-memory) generic List koleksiyonlarında tutan servis sınıfı.
    /// Şartname Gereksinimi:
    /// - List<T> generic koleksiyon kullanımı
    /// - Arayüz ve iş mantığı ayrımı
    /// - Hata ve validasyon yönetimi
    /// </summary>
    public class StokManager
    {
        private readonly List<UrunBase> _urunler;
        private readonly List<StokHareketi> _hareketler;

        // Singleton deseni veya tekil yönetici örneği
        private static StokManager? _ornek;
        public static StokManager Ornek => _ornek ??= new StokManager();

        public event EventHandler? VeriDegisti;

        public StokManager()
        {
            _urunler = new List<UrunBase>();
            _hareketler = new List<StokHareketi>();
        }

        #region Ürün CRUD İşlemleri

        public IReadOnlyList<UrunBase> TumUrunleriGetir()
        {
            return _urunler.AsReadOnly();
        }

        public IReadOnlyList<StokHareketi> TumHareketleriGetir()
        {
            return _hareketler.OrderByDescending(h => h.IslemZamani).ToList().AsReadOnly();
        }

        /// <summary>
        /// Yeni ürün ekler. Benzersiz ürün kodu kuralını denetler.
        /// </summary>
        public void UrunEkle(UrunBase yeniUrun)
        {
            if (yeniUrun == null)
                throw new ArgumentNullException(nameof(yeniUrun), "Eklenecek ürün boş olamaz!");

            if (_urunler.Any(u => u.UrunKodu.Equals(yeniUrun.UrunKodu, StringComparison.OrdinalIgnoreCase)))
            {
                throw new MukerrerUrunKoduException(yeniUrun.UrunKodu);
            }

            _urunler.Add(yeniUrun);

            // İlk stok varsa hareket kaydı oluştur
            if (yeniUrun.StokMiktari > 0)
            {
                _hareketler.Add(new StokHareketi(
                    yeniUrun.UrunKodu,
                    yeniUrun.UrunAdi,
                    IslemTipi.Giris,
                    yeniUrun.StokMiktari,
                    0,
                    yeniUrun.StokMiktari,
                    "Yeni Ürün Kaydı Açılış Stoku"
                ));
            }

            VeriDegisti?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Seçilen ürünün bilgilerini günceller.
        /// </summary>
        public void UrunGuncelle(int urunId, UrunBase guncelUrun)
        {
            if (guncelUrun == null)
                throw new ArgumentNullException(nameof(guncelUrun));

            var mevcut = _urunler.FirstOrDefault(u => u.Id == urunId);
            if (mevcut == null)
                throw new KeyNotFoundException($"ID: {urunId} olan ürün bulunamadı!");

            // Ürün kodunun başka bir üründe olup olmadığını kontrol et
            if (_urunler.Any(u => u.Id != urunId && u.UrunKodu.Equals(guncelUrun.UrunKodu, StringComparison.OrdinalIgnoreCase)))
            {
                throw new MukerrerUrunKoduException(guncelUrun.UrunKodu);
            }

            mevcut.UrunKodu = guncelUrun.UrunKodu;
            mevcut.UrunAdi = guncelUrun.UrunAdi;
            mevcut.Kategori = guncelUrun.Kategori;
            mevcut.BirimFiyat = guncelUrun.BirimFiyat;
            mevcut.KritikStokSeviyesi = guncelUrun.KritikStokSeviyesi;

            // Özel sınıf alanlarını güncelle
            if (mevcut is FizikselUrun fMevcut && guncelUrun is FizikselUrun fGuncel)
            {
                fMevcut.RafKonumu = fGuncel.RafKonumu;
                fMevcut.AgirlikKg = fGuncel.AgirlikKg;
            }
            else if (mevcut is BozulabilirUrun bMevcut && guncelUrun is BozulabilirUrun bGuncel)
            {
                bMevcut.SonKullanmaTarihi = bGuncel.SonKullanmaTarihi;
                bMevcut.SaklamaSicakligi = bGuncel.SaklamaSicakligi;
            }
            else if (mevcut is ElektronikUrun eMevcut && guncelUrun is ElektronikUrun eGuncel)
            {
                eMevcut.GarantiSuresiAy = eGuncel.GarantiSuresiAy;
                eMevcut.SeriNoSerisi = eGuncel.SeriNoSerisi;
            }

            VeriDegisti?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Belirtilen ürün koduna göre ürünü siler.
        /// </summary>
        public bool UrunSil(string urunKodu)
        {
            var urun = _urunler.FirstOrDefault(u => u.UrunKodu.Equals(urunKodu, StringComparison.OrdinalIgnoreCase));
            if (urun == null) return false;

            _urunler.Remove(urun);
            VeriDegisti?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public UrunBase? UrunBul(string urunKodu)
        {
            return _urunler.FirstOrDefault(u => u.UrunKodu.Equals(urunKodu, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region Stok Giriş ve Çıkış İşlemleri

        /// <summary>
        /// Seçilen ürüne stok girişi yapar ve hareket günlüğüne kaydeder.
        /// </summary>
        public void StokGirisYap(string urunKodu, int miktar, string aciklama = "")
        {
            var urun = UrunBul(urunKodu);
            if (urun == null)
                throw new KeyNotFoundException($"'{urunKodu}' kodlu ürün bulunamadı!");

            int oncekiStok = urun.StokMiktari;
            urun.StokArttir(miktar);
            int yeniStok = urun.StokMiktari;

            _hareketler.Add(new StokHareketi(urun.UrunKodu, urun.UrunAdi, IslemTipi.Giris, miktar, oncekiStok, yeniStok, aciklama));
            VeriDegisti?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Seçilen üründen stok çıkışı yapar ve hareket günlüğüne kaydeder.
        /// </summary>
        public void StokCikisYap(string urunKodu, int miktar, string aciklama = "")
        {
            var urun = UrunBul(urunKodu);
            if (urun == null)
                throw new KeyNotFoundException($"'{urunKodu}' kodlu ürün bulunamadı!");

            int oncekiStok = urun.StokMiktari;
            urun.StokAzalt(miktar);
            int yeniStok = urun.StokMiktari;

            _hareketler.Add(new StokHareketi(urun.UrunKodu, urun.UrunAdi, IslemTipi.Cikis, miktar, oncekiStok, yeniStok, aciklama));
            VeriDegisti?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Arama, Filtreleme ve İstatistikler

        /// <summary>
        /// Çok kriterli ürün arama ve filtreleme motoru.
        /// </summary>
        public List<UrunBase> Filtrele(string? aramaMetni, UrunKategorisi? kategori, bool sadeceKritik)
        {
            IEnumerable<UrunBase> sorgu = _urunler;

            if (!string.IsNullOrWhiteSpace(aramaMetni))
            {
                string aranan = aramaMetni.Trim().ToLowerInvariant();
                sorgu = sorgu.Where(u => u.UrunKodu.ToLowerInvariant().Contains(aranan) ||
                                         u.UrunAdi.ToLowerInvariant().Contains(aranan));
            }

            if (kategori.HasValue)
            {
                sorgu = sorgu.Where(u => u.Kategori == kategori.Value);
            }

            if (sadeceKritik)
            {
                // Çok biçimli kritik stok kuralı çalışır (Polymorphism)
                sorgu = sorgu.Where(u => u.KritikStokMu());
            }

            return sorgu.ToList();
        }

        public int ToplamUrunSayisi => _urunler.Count;

        public int ToplamStokMiktari => _urunler.Sum(u => u.StokMiktari);

        public int KritikUrunSayisi => _urunler.Count(u => u.KritikStokMu());

        public decimal ToplamEnvanterDegeri => _urunler.Sum(u => u.ToplamDegerHesapla());

        public List<UrunBase> KritikUrunleriGetir()
        {
            return _urunler.Where(u => u.KritikStokMu()).ToList();
        }

        #endregion
    }
}
