using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using InventoryManagement.Enums;
using InventoryManagement.Models;
using InventoryManagement.Services;

namespace InventoryManagement.Forms
{
    public class MainForm : Form
    {
        private readonly StokManager _manager;

        // Sol Menü Kontrolleri
        private Panel pnlSidebar = null!;
        private Button btnMenuEnvanter = null!;
        private Button btnMenuHareketler = null!;
        private Button btnMenuHakkinda = null!;

        // Sayfa Konteyneri
        private Panel pnlSayfaEnvanter = null!;
        private Panel pnlSayfaHareketler = null!;
        private Panel pnlSayfaHakkinda = null!;

        // KPI Kartları
        private Label lblKpiToplamUrun = null!;
        private Label lblKpiToplamStok = null!;
        private Label lblKpiKritikStok = null!;
        private Label lblKpiToplamDeger = null!;

        // Filtreleme Kontrolleri
        private TextBox txtArama = null!;
        private ComboBox cmbFiltreKategori = null!;
        private CheckBox chkSadeceKritik = null!;

        // Tablolar
        private DataGridView dgvUrunler = null!;
        private DataGridView dgvHareketler = null!;

        // Alt Bilgi Çubuğu
        private Label lblSeciliUrunDetay = null!;
        private StatusStrip statusStrip = null!;
        private ToolStripStatusLabel statusLabelSaat = null!;
        private ToolStripStatusLabel statusLabelBilgi = null!;

        public MainForm()
        {
            _manager = StokManager.Ornek;
            _manager.VeriDegisti += (s, e) =>
            {
                GuncelleKpiKartlari();
                YenileUrunTablosu();
                YenileHareketTablosu();
            };

            InitializeCustomUI();

            // İlk açılışta zengin örnek verileri otomatik yükle (göz boyama / show)
            DemoDataService.OrnekVerileriYukle(_manager);

            GuncelleKpiKartlari();
            YenileUrunTablosu();
            YenileHareketTablosu();
        }

        private void InitializeCustomUI()
        {
            this.Text = "Stok Yönetim Sistemi - Dr. Öğr. Üyesi Tohid YOUSEFİ (.NET Programlama I) - Fatih Ölmez (24118035)";
            this.Size = new Size(1280, 800);
            this.MinimumSize = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(241, 245, 249);
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            // 1. Sol Menü Paneli (Sidebar)
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 240,
                BackColor = Color.FromArgb(15, 23, 42) // Dark Slate
            };

            var pnlLogo = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.FromArgb(15, 23, 42)
            };

            var lblLogoIcon = new Label
            {
                Text = "📦 INVENTORY PRO",
                Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248),
                Location = new Point(15, 22),
                AutoSize = true
            };

            var lblLogoAlt = new Label
            {
                Text = "Stok Yönetim & Takip Sistemi",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                Location = new Point(16, 48),
                AutoSize = true
            };
            pnlLogo.Controls.Add(lblLogoIcon);
            pnlLogo.Controls.Add(lblLogoAlt);

            // Navigasyon Butonları
            btnMenuEnvanter = OlusturMenuButonu("📊  Envanter & Dashboard", 110);
            btnMenuEnvanter.Click += (s, e) => SayfaGoster(pnlSayfaEnvanter, btnMenuEnvanter);

            btnMenuHareketler = OlusturMenuButonu("📜  Stok Hareket Geçmişi", 165);
            btnMenuHareketler.Click += (s, e) => SayfaGoster(pnlSayfaHareketler, btnMenuHareketler);

            btnMenuHakkinda = OlusturMenuButonu("ℹ️  Proje Künyesi & OOP", 220);
            btnMenuHakkinda.Click += (s, e) => SayfaGoster(pnlSayfaHakkinda, btnMenuHakkinda);

            // Sol Alt: Öğrenci Bilgi Rozeti
            var pnlOgrenci = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 150,
                BackColor = Color.FromArgb(30, 41, 59)
            };
            var lblOgrBaslik = new Label
            {
                Text = "🎓 PROJE BİLGİSİ",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248),
                Location = new Point(15, 12),
                AutoSize = true
            };
            var lblOgrAd = new Label
            {
                Text = "Fatih ÖLMEZ\nNo: 24118035\nBGT331 / BST301\nDr. Öğr. Üyesi Tohid YOUSEFİ",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(203, 213, 225),
                Location = new Point(15, 34),
                AutoSize = true
            };
            pnlOgrenci.Controls.Add(lblOgrBaslik);
            pnlOgrenci.Controls.Add(lblOgrAd);

            pnlSidebar.Controls.Add(pnlLogo);
            pnlSidebar.Controls.Add(btnMenuEnvanter);
            pnlSidebar.Controls.Add(btnMenuHareketler);
            pnlSidebar.Controls.Add(btnMenuHakkinda);
            pnlSidebar.Controls.Add(pnlOgrenci);

            // 2. Üst Header Çubuğu
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.White
            };
            var lblHeaderTitle = new Label
            {
                Text = "STOK YÖNETİM SİSTEMİ | Envanter Kontrol Merkezi",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(25, 20),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblHeaderTitle);

            // 3. Alt StatusStrip
            statusStrip = new StatusStrip
            {
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9f)
            };
            statusLabelBilgi = new ToolStripStatusLabel
            {
                Text = "Sistem Hazır. Tüm OOP kuralları ve veri kontrolleri aktif.",
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(71, 85, 105)
            };
            statusLabelSaat = new ToolStripStatusLabel
            {
                Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
                ForeColor = Color.FromArgb(100, 116, 139)
            };
            statusStrip.Items.Add(statusLabelBilgi);
            statusStrip.Items.Add(statusLabelSaat);

            // Sayfaları İnşa Et
            InsaEtSayfaEnvanter();
            InsaEtSayfaHareketler();
            InsaEtSayfaHakkinda();

            // Ana Kontrolleri Ekle
            this.Controls.Add(pnlSayfaEnvanter);
            this.Controls.Add(pnlSayfaHareketler);
            this.Controls.Add(pnlSayfaHakkinda);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlSidebar);
            this.Controls.Add(statusStrip);

            // Varsayılan aktif sayfa
            SayfaGoster(pnlSayfaEnvanter, btnMenuEnvanter);
        }

        private Button OlusturMenuButonu(string metin, int yKonum)
        {
            var btn = new Button
            {
                Text = metin,
                Location = new Point(10, yKonum),
                Size = new Size(220, 46),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(203, 213, 225),
                BackColor = Color.FromArgb(15, 23, 42),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void SayfaGoster(Panel gosterilecek, Button aktifButon)
        {
            pnlSayfaEnvanter.Visible = false;
            pnlSayfaHareketler.Visible = false;
            pnlSayfaHakkinda.Visible = false;

            btnMenuEnvanter.BackColor = Color.FromArgb(15, 23, 42);
            btnMenuHareketler.BackColor = Color.FromArgb(15, 23, 42);
            btnMenuHakkinda.BackColor = Color.FromArgb(15, 23, 42);

            gosterilecek.Visible = true;
            gosterilecek.BringToFront();

            aktifButon.BackColor = Color.FromArgb(37, 99, 235);
        }

        #region Sayfa 1: Envanter & Dashboard

        private void InsaEtSayfaEnvanter()
        {
            pnlSayfaEnvanter = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(241, 245, 249),
                Padding = new Padding(20)
            };

            // KPI Kartları Paneli
            var pnlKpiContainer = new Panel
            {
                Dock = DockStyle.Top,
                Height = 105,
                BackColor = Color.Transparent
            };

            var kpi1 = OlusturKpiKarti("Toplam Çeşit Ürün", "0", Color.FromArgb(37, 99, 235), out lblKpiToplamUrun);
            kpi1.Location = new Point(0, 0);

            var kpi2 = OlusturKpiKarti("Toplam Depo Stoku", "0 Adet", Color.FromArgb(16, 185, 129), out lblKpiToplamStok);
            kpi2.Location = new Point(250, 0);

            var kpi3 = OlusturKpiKarti("⚠️ Kritik Stok Alarmı", "0 Ürün", Color.FromArgb(220, 38, 38), out lblKpiKritikStok);
            kpi3.Location = new Point(500, 0);

            var kpi4 = OlusturKpiKarti("Envanter Mali Değeri", "0,00 ₺", Color.FromArgb(147, 51, 234), out lblKpiToplamDeger);
            kpi4.Location = new Point(750, 0);

            pnlKpiContainer.Controls.Add(kpi1);
            pnlKpiContainer.Controls.Add(kpi2);
            pnlKpiContainer.Controls.Add(kpi3);
            pnlKpiContainer.Controls.Add(kpi4);

            // Toolbar Paneli (Arama, Filtre, İşlem Butonları)
            var pnlToolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            // Arama Kutusu
            txtArama = new TextBox
            {
                Location = new Point(12, 14),
                Size = new Size(200, 28),
                PlaceholderText = "🔍 Ürün Adı veya Kodu..."
            };
            txtArama.TextChanged += (s, e) => YenileUrunTablosu();

            // Kategori Filtresi
            cmbFiltreKategori = new ComboBox
            {
                Location = new Point(222, 14),
                Size = new Size(150, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFiltreKategori.Items.Add("Tüm Kategoriler");
            foreach (var kat in Enum.GetValues(typeof(UrunKategorisi)))
            {
                cmbFiltreKategori.Items.Add(kat);
            }
            cmbFiltreKategori.SelectedIndex = 0;
            cmbFiltreKategori.SelectedIndexChanged += (s, e) => YenileUrunTablosu();

            // Sadece Kritik Filtresi
            chkSadeceKritik = new CheckBox
            {
                Text = "Sadece Kritik Stoklar",
                Location = new Point(382, 17),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(185, 28, 28)
            };
            chkSadeceKritik.CheckedChanged += (s, e) => YenileUrunTablosu();

            // Butonlar
            var btnYeni = OlusturIslemButonu("➕ Yeni", Color.FromArgb(16, 185, 129), 525);
            btnYeni.Width = 85;
            btnYeni.Click += BtnYeni_Click;

            var btnDuzenle = OlusturIslemButonu("✏️ Düzenle", Color.FromArgb(37, 99, 235), 618);
            btnDuzenle.Width = 95;
            btnDuzenle.Click += BtnDuzenle_Click;

            var btnSil = OlusturIslemButonu("🗑️ Sil", Color.FromArgb(239, 68, 68), 721);
            btnSil.Width = 75;
            btnSil.Click += BtnSil_Click;

            var btnHizliStok = OlusturIslemButonu("🔄 Stok Giriş/Çıkış", Color.FromArgb(245, 158, 11), 804);
            btnHizliStok.Width = 155;
            btnHizliStok.Click += BtnHizliStok_Click;

            pnlToolbar.Controls.Add(txtArama);
            pnlToolbar.Controls.Add(cmbFiltreKategori);
            pnlToolbar.Controls.Add(chkSadeceKritik);
            pnlToolbar.Controls.Add(btnYeni);
            pnlToolbar.Controls.Add(btnDuzenle);
            pnlToolbar.Controls.Add(btnSil);
            pnlToolbar.Controls.Add(btnHizliStok);

            // DataGridView (Akıllı Envanter Tablosu)
            dgvUrunler = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false
            };
            dgvUrunler.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            dgvUrunler.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvUrunler.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgvUrunler.ColumnHeadersHeight = 36;
            dgvUrunler.RowTemplate.Height = 32;
            dgvUrunler.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvUrunler.CellFormatting += DgvUrunler_CellFormatting;
            dgvUrunler.SelectionChanged += DgvUrunler_SelectionChanged;
            dgvUrunler.DoubleClick += (s, e) => BtnHizliStok_Click(s, e);

            // Tablo Kolonları
            dgvUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", FillWeight = 30 });
            dgvUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "UrunKodu", HeaderText = "Ürün Kodu", FillWeight = 65 });
            dgvUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "UrunAdi", HeaderText = "Ürün Adı", FillWeight = 135 });
            dgvUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Kategori", HeaderText = "Kategori", FillWeight = 60 });
            dgvUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "BirimFiyat", HeaderText = "Birim Fiyat", FillWeight = 60 });
            dgvUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "StokMiktari", HeaderText = "Mevcut Stok", FillWeight = 55 });
            dgvUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "KritikEsik", HeaderText = "Kritik Eşik", FillWeight = 50 });
            dgvUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Durum", HeaderText = "Stok Durumu", FillWeight = 85 });
            dgvUrunler.Columns.Add(new DataGridViewTextBoxColumn { Name = "OzelBilgi", HeaderText = "Özel Özellikler / Not", FillWeight = 110 });

            // Alt Detay Önizleme Barı
            var pnlSeciliAlt = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 38,
                BackColor = Color.FromArgb(241, 245, 249),
                Padding = new Padding(8, 6, 8, 4)
            };
            lblSeciliUrunDetay = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Bir ürün seçiniz...",
                Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                ForeColor = Color.FromArgb(71, 85, 105)
            };
            pnlSeciliAlt.Controls.Add(lblSeciliUrunDetay);

            pnlSayfaEnvanter.Controls.Add(dgvUrunler);
            pnlSayfaEnvanter.Controls.Add(pnlSeciliAlt);
            pnlSayfaEnvanter.Controls.Add(pnlToolbar);
            pnlSayfaEnvanter.Controls.Add(pnlKpiContainer);
        }

        private Panel OlusturKpiKarti(string baslik, string deger, Color accentColor, out Label outLabelDeger)
        {
            var pnl = new Panel
            {
                Size = new Size(235, 90),
                BackColor = Color.White
            };

            // Üst renkli çizgi
            var strip = new Panel
            {
                Dock = DockStyle.Top,
                Height = 4,
                BackColor = accentColor
            };

            var lblB = new Label
            {
                Text = baslik,
                Location = new Point(14, 15),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139),
                AutoSize = true
            };

            var lblD = new Label
            {
                Text = deger,
                Location = new Point(12, 40),
                Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                AutoSize = true
            };

            outLabelDeger = lblD;
            pnl.Controls.Add(strip);
            pnl.Controls.Add(lblB);
            pnl.Controls.Add(lblD);
            return pnl;
        }

        private Button OlusturIslemButonu(string metin, Color renk, int xKonum)
        {
            var btn = new Button
            {
                Text = metin,
                Location = new Point(xKonum, 10),
                Size = new Size(100, 34),
                BackColor = renk,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        #endregion

        #region Sayfa 2: Stok Hareket Geçmişi (Audit Log)

        private void InsaEtSayfaHareketler()
        {
            pnlSayfaHareketler = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(241, 245, 249),
                Padding = new Padding(20),
                Visible = false
            };

            var pnlUst = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(15)
            };

            var lblBaslik = new Label
            {
                Text = "📜 Depo Stok Hareketleri Denetim Günlüğü (Audit Log)",
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(15, 18),
                AutoSize = true
            };
            pnlUst.Controls.Add(lblBaslik);

            dgvHareketler = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false
            };
            dgvHareketler.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            dgvHareketler.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvHareketler.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgvHareketler.ColumnHeadersHeight = 36;
            dgvHareketler.RowTemplate.Height = 30;
            dgvHareketler.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            dgvHareketler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "#", FillWeight = 30 });
            dgvHareketler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tarih", HeaderText = "Tarih / Saat", FillWeight = 75 });
            dgvHareketler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Islem", HeaderText = "İşlem Türü", FillWeight = 60 });
            dgvHareketler.Columns.Add(new DataGridViewTextBoxColumn { Name = "UrunKodu", HeaderText = "Ürün Kodu", FillWeight = 60 });
            dgvHareketler.Columns.Add(new DataGridViewTextBoxColumn { Name = "UrunAdi", HeaderText = "Ürün Adı", FillWeight = 110 });
            dgvHareketler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Miktar", HeaderText = "İşlem Miktarı", FillWeight = 55 });
            dgvHareketler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Degisim", HeaderText = "Stok Değişimi", FillWeight = 75 });
            dgvHareketler.Columns.Add(new DataGridViewTextBoxColumn { Name = "Aciklama", HeaderText = "Açıklama / Sebep", FillWeight = 120 });

            dgvHareketler.CellFormatting += (s, e) =>
            {
                if (e.RowIndex >= 0 && dgvHareketler.Columns[e.ColumnIndex].Name == "Islem" && e.CellStyle != null)
                {
                    string? val = e.Value?.ToString();
                    if (val == "GİRİŞ")
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(5, 150, 105);
                        e.CellStyle.Font = new Font(dgvHareketler.Font, FontStyle.Bold);
                    }
                    else if (val == "ÇIKIŞ")
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                        e.CellStyle.Font = new Font(dgvHareketler.Font, FontStyle.Bold);
                    }
                }
            };

            pnlSayfaHareketler.Controls.Add(dgvHareketler);
            pnlSayfaHareketler.Controls.Add(pnlUst);
        }

        #endregion

        #region Sayfa 3: Proje Künyesi & OOP Mimarisi Açıklaması

        private void InsaEtSayfaHakkinda()
        {
            pnlSayfaHakkinda = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(30),
                AutoScroll = true,
                Visible = false
            };

            var rtb = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10.5f),
                Text =
@"===================================================================================
.NET PROGRAMLAMA - I | PROJE ÖDEVİ TEKNİK RAPOR VE MİMARİ KÜNYESİ
Stok Yönetim Sistemi (Inventory Management System)
===================================================================================

• Ders Bilgileri: .NET Programlama - I (BGT331 / BST301)
• Öğretim Elemanı: Dr. Öğr. Üyesi Tohid YOUSEFİ
• Geliştirici: Fatih ÖLMEZ (Öğrenci No: 24118035)
• Teslim Tarihi: Güz Dönemi
• Geliştirme Ortamı: Visual Studio & C# Windows Forms (.NET Framework / Windows Desktop)

-----------------------------------------------------------------------------------
1. NESNE TABANLI PROGRAMLAMA (OOP) PRENSİPLERİNİN UYGULANIŞI
-----------------------------------------------------------------------------------

A) KAPSÜLLEME (Encapsulation):
   - Tüm sınıf alanları (field) 'private' erişim belirleyicisiyle korunmuştur (_birimFiyat, _stokMiktari vb.).
   - Kontrollü erişim için property blokları kullanılmış; negatif fiyat, negatif stok veya boş ürün adı girişleri engellenmiştir.

B) KALITIM (Inheritance):
   - 'UrunBase' temel soyut sınıfından 3 adet gerçek hayat senaryosuna uygun alt sınıf türetilmiştir:
     1. FizikselUrun: Raf Konumu ve Ağırlık (kg) bilgilerini barındırır.
     2. BozulabilirUrun: Son Kullanma Tarihi (SKT) ve Saklama Sıcaklığı (°C) bilgilerini barındırır.
     3. ElektronikUrun: Garanti Süresi (Ay) ve Seri Numarası bilgilerini barındırır.

C) ÇOK BİÇİMLİLİK (Polymorphism):
   - 'KritikStokMu()' metodu alt sınıflarda farklı dinamik kurallarla override edilmiştir:
     * BozulabilirUrun sınıfında, stok miktarı yeterli olsa dahi Son Kullanma Tarihine 7 günden az kalmışsa ürün otomatik olarak 'KRİTİK' durumuna geçer.
     * ElektronikUrun sınıfında, uzun tedarik süreleri sebebiyle güvenlik stoğu (+2) payı ile kritik seviye hesaplanır.
     * FizikselUrun sınıfında ise standart eşik değeri kullanılır.
   - 'DetayliBilgiGetir()' ve 'ToplamDegerHesapla()' sanal (virtual) metotları her ürün tipi için özelleştirilmiştir.

D) SOYUTLAMA (Abstraction):
   - 'UrunBase' adında soyut sınıf (abstract class) tanımlanmıştır.
   - 'IUrun', 'IStokIslemleri' ve 'ILoglanabilir' olmak üzere 3 adet fonksiyonel arayüz (interface) tanımlanmış ve uygulanmıştır.

-----------------------------------------------------------------------------------
2. HATA YÖNETİMİ, DEBUGGING VE GÜVENLİK
-----------------------------------------------------------------------------------
- Özel Exception Sınıfları: 'YetersizStokException', 'MukerrerUrunKoduException', 'GecersizMiktarException'.
- Sayısal Alan Kontrolleri: NumericUpDown kontrolleriyle metin/harf girişi engellenmiş, minimum ve maksimum sınırlar çizilmiştir.
- Negatif Stok Engeli: Stok çıkış işlemlerinde depodaki mevcut miktardan daha fazlasının çıkışı engellenmiş, kullanıcıya uyarı verilmiştir.
- Mükerrer Ürün Kodu Koruması: Aynı ürün koduyla ikinci bir kayıt eklenmesi engellenmiştir.
"
            };

            pnlSayfaHakkinda.Controls.Add(rtb);
        }

        #endregion

        #region Veri Yenileme ve Olaylar

        private void GuncelleKpiKartlari()
        {
            lblKpiToplamUrun.Text = _manager.ToplamUrunSayisi.ToString();
            lblKpiToplamStok.Text = $"{_manager.ToplamStokMiktari:N0} Adet";
            lblKpiKritikStok.Text = $"{_manager.KritikUrunSayisi} Ürün";
            lblKpiToplamDeger.Text = $"{_manager.ToplamEnvanterDegeri:C2}";

            // Kritik stok varsa kart rengini daha belirgin yap
            if (_manager.KritikUrunSayisi > 0)
                lblKpiKritikStok.ForeColor = Color.FromArgb(220, 38, 38);
            else
                lblKpiKritikStok.ForeColor = Color.FromArgb(16, 185, 129);
        }

        private void YenileUrunTablosu()
        {
            dgvUrunler.Rows.Clear();

            string arama = txtArama?.Text ?? "";
            UrunKategorisi? secilenKat = null;
            if (cmbFiltreKategori != null && cmbFiltreKategori.SelectedIndex > 0)
            {
                secilenKat = (UrunKategorisi)cmbFiltreKategori.SelectedItem!;
            }
            bool sadeceKritik = chkSadeceKritik?.Checked ?? false;

            var liste = _manager.Filtrele(arama, secilenKat, sadeceKritik);

            foreach (var u in liste)
            {
                bool kritik = u.KritikStokMu(); // Çok biçimli kontrol
                string durumMetni = kritik ? "⚠️ KRİTİK SEVİYE" : "✓ NORMAL";

                string ozelBilgi = "";
                if (u is BozulabilirUrun b)
                    ozelBilgi = $"SKT: {b.SonKullanmaTarihi:dd.MM.yyyy} ({b.KalanGunSayisi()} gün) | {b.SaklamaSicakligi}°C";
                else if (u is ElektronikUrun e)
                    ozelBilgi = $"Garanti: {e.GarantiSuresiAy} Ay | Seri: {e.SeriNoSerisi}";
                else if (u is FizikselUrun f)
                    ozelBilgi = $"Raf: {f.RafKonumu} | Ağırlık: {f.AgirlikKg:N2} kg";

                int rowIndex = dgvUrunler.Rows.Add(
                    u.Id,
                    u.UrunKodu,
                    u.UrunAdi,
                    u.Kategori.ToString(),
                    u.BirimFiyat.ToString("C2"),
                    u.StokMiktari,
                    u.KritikStokSeviyesi,
                    durumMetni,
                    ozelBilgi
                );

                dgvUrunler.Rows[rowIndex].Tag = u;
            }

            statusLabelBilgi.Text = $"Listelenen Ürün Sayısı: {liste.Count} / {_manager.ToplamUrunSayisi}";
        }

        private void YenileHareketTablosu()
        {
            dgvHareketler.Rows.Clear();
            var hareketler = _manager.TumHareketleriGetir();

            foreach (var h in hareketler)
            {
                dgvHareketler.Rows.Add(
                    h.Id,
                    h.IslemZamani.ToString("dd.MM.yyyy HH:mm"),
                    h.IslemTuru == IslemTipi.Giris ? "GİRİŞ" : "ÇIKIŞ",
                    h.UrunKodu,
                    h.UrunAdi,
                    h.Miktar,
                    $"{h.OncekiStok} ➔ {h.SonrakiStok}",
                    h.Aciklama
                );
            }
        }

        private void DgvUrunler_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvUrunler.Rows.Count) return;

            var urun = dgvUrunler.Rows[e.RowIndex].Tag as UrunBase;
            if (urun == null || e.CellStyle == null) return;

            if (urun.KritikStokMu())
            {
                // Kritik stok satırını açık kırmızı/turuncu tonla vurgula
                if (dgvUrunler.Columns[e.ColumnIndex].Name == "Durum")
                {
                    e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                    e.CellStyle.Font = new Font(dgvUrunler.Font, FontStyle.Bold);
                }
                else if (dgvUrunler.Columns[e.ColumnIndex].Name == "StokMiktari")
                {
                    e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                    e.CellStyle.Font = new Font(dgvUrunler.Font, FontStyle.Bold);
                }
            }
            else
            {
                if (dgvUrunler.Columns[e.ColumnIndex].Name == "Durum")
                {
                    e.CellStyle.ForeColor = Color.FromArgb(5, 150, 105);
                    e.CellStyle.Font = new Font(dgvUrunler.Font, FontStyle.Bold);
                }
            }
        }

        private void DgvUrunler_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvUrunler.SelectedRows.Count > 0)
            {
                var urun = dgvUrunler.SelectedRows[0].Tag as UrunBase;
                if (urun != null)
                {
                    lblSeciliUrunDetay.Text = $"Seçili Ürün: {urun.DetayliBilgiGetir()} | Toplam Değer: {urun.ToplamDegerHesapla():C2}";
                    lblSeciliUrunDetay.ForeColor = Color.FromArgb(30, 41, 59);
                    return;
                }
            }
            lblSeciliUrunDetay.Text = "Bir ürün seçiniz...";
            lblSeciliUrunDetay.ForeColor = Color.FromArgb(100, 116, 139);
        }

        private UrunBase? GetSeciliUrun()
        {
            if (dgvUrunler.SelectedRows.Count > 0)
            {
                return dgvUrunler.SelectedRows[0].Tag as UrunBase;
            }
            return null;
        }

        private void BtnYeni_Click(object? sender, EventArgs e)
        {
            using var form = new FormUrunEkleDuzenle(_manager);
            form.ShowDialog(this);
        }

        private void BtnDuzenle_Click(object? sender, EventArgs e)
        {
            var secili = GetSeciliUrun();
            if (secili == null)
            {
                MessageBox.Show("Lütfen düzenlemek istediğiniz ürünü tablodan seçiniz!", "Ürün Seçilmedi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var form = new FormUrunEkleDuzenle(_manager, secili);
            form.ShowDialog(this);
        }

        private void BtnSil_Click(object? sender, EventArgs e)
        {
            var secili = GetSeciliUrun();
            if (secili == null)
            {
                MessageBox.Show("Lütfen silmek istediğiniz ürünü tablodan seçiniz!", "Ürün Seçilmedi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var dialogResult = MessageBox.Show(
                $"[{secili.UrunKodu}] '{secili.UrunAdi}' isimli ürünü silmek istediğinize emin misiniz?\n\nBu işlem geri alınamaz!",
                "Ürün Silme Onayı",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (dialogResult == DialogResult.Yes)
            {
                _manager.UrunSil(secili.UrunKodu);
                MessageBox.Show("Ürün başarıyla silindi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnHizliStok_Click(object? sender, EventArgs e)
        {
            var secili = GetSeciliUrun();
            if (secili == null)
            {
                MessageBox.Show("Lütfen stok işlemi yapmak istediğiniz ürünü tablodan seçiniz!", "Ürün Seçilmedi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var form = new FormStokIslem(secili, _manager);
            form.ShowDialog(this);
        }

        #endregion
    }
}
