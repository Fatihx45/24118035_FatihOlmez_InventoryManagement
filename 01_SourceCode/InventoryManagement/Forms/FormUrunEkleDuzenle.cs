using System;
using System.Drawing;
using System.Windows.Forms;
using InventoryManagement.Enums;
using InventoryManagement.Exceptions;
using InventoryManagement.Models;
using InventoryManagement.Services;

namespace InventoryManagement.Forms
{
    public class FormUrunEkleDuzenle : Form
    {
        private readonly StokManager _manager;
        private readonly UrunBase? _mevcutUrun;
        private readonly bool _duzenlemeModu;

        // UI Kontrolleri
        private Label lblBaslik = null!;
        private TextBox txtUrunKodu = null!;
        private TextBox txtUrunAdi = null!;
        private ComboBox cmbKategori = null!;
        private NumericUpDown numBirimFiyat = null!;
        private NumericUpDown numStokMiktari = null!;
        private NumericUpDown numKritikSeviye = null!;

        // Dinamik Paneller
        private GroupBox grpElektronik = null!;
        private NumericUpDown numGarantiSuresi = null!;
        private TextBox txtSeriNo = null!;

        private GroupBox grpBozulabilir = null!;
        private DateTimePicker dtpSKT = null!;
        private NumericUpDown numSicaklik = null!;

        private GroupBox grpFiziksel = null!;
        private TextBox txtRafKonumu = null!;
        private NumericUpDown numAgirlik = null!;

        private Button btnKaydet = null!;
        private Button btnIptal = null!;

        public FormUrunEkleDuzenle(StokManager manager, UrunBase? mevcutUrun = null)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _mevcutUrun = mevcutUrun;
            _duzenlemeModu = mevcutUrun != null;

            InitializeCustomComponents();
            if (_duzenlemeModu)
            {
                MevcutBilgileriYukle();
            }
            else
            {
                cmbKategori.SelectedIndex = 0;
            }
        }

        private void InitializeCustomComponents()
        {
            this.Text = _duzenlemeModu ? "Ürün Bilgilerini Güncelle" : "Yeni Ürün Tanımla";
            this.Size = new Size(540, 640);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9.5f);

            var panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = _duzenlemeModu ? Color.FromArgb(37, 99, 235) : Color.FromArgb(16, 185, 129)
            };

            lblBaslik = new Label
            {
                Text = _duzenlemeModu ? "✏️ Ürün Bilgilerini Düzenle" : "➕ Yeni Ürün Kartı Aç",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 18),
                AutoSize = true
            };
            panelHeader.Controls.Add(lblBaslik);

            int startY = 85;
            int labelX = 35;
            int controlX = 175;
            int controlWidth = 300;
            int rowHeight = 38;

            // 1. Ürün Kodu
            var lblKod = new Label { Text = "Ürün Kodu *:", Location = new Point(labelX, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            txtUrunKodu = new TextBox { Location = new Point(controlX, startY - 2), Size = new Size(controlWidth, 27), PlaceholderText = "Örn: PRD-001" };

            // 2. Ürün Adı
            startY += rowHeight;
            var lblAd = new Label { Text = "Ürün Adı *:", Location = new Point(labelX, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            txtUrunAdi = new TextBox { Location = new Point(controlX, startY - 2), Size = new Size(controlWidth, 27), PlaceholderText = "Örn: Kablosuz Klavye" };

            // 3. Kategori
            startY += rowHeight;
            var lblKat = new Label { Text = "Kategori *:", Location = new Point(labelX, startY), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            cmbKategori = new ComboBox
            {
                Location = new Point(controlX, startY - 2),
                Size = new Size(controlWidth, 27),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            foreach (var item in Enum.GetValues(typeof(UrunKategorisi)))
            {
                cmbKategori.Items.Add(item);
            }
            cmbKategori.SelectedIndexChanged += CmbKategori_SelectedIndexChanged;

            // 4. Birim Fiyat
            startY += rowHeight;
            var lblFiyat = new Label { Text = "Birim Fiyat (₺) *:", Location = new Point(labelX, startY), AutoSize = true };
            numBirimFiyat = new NumericUpDown
            {
                Location = new Point(controlX, startY - 2),
                Size = new Size(controlWidth, 27),
                DecimalPlaces = 2,
                Maximum = 1000000,
                Value = 100.00m
            };

            // 5. Başlangıç / Mevcut Stok
            startY += rowHeight;
            var lblStok = new Label { Text = "Mevcut Stok *:", Location = new Point(labelX, startY), AutoSize = true };
            numStokMiktari = new NumericUpDown
            {
                Location = new Point(controlX, startY - 2),
                Size = new Size(controlWidth, 27),
                Maximum = 100000,
                Value = 10
            };
            if (_duzenlemeModu)
            {
                // Düzenleme modunda stok miktarı doğrudan elle değil, stok giriş/çıkış ekranından değiştirilmeli
                numStokMiktari.Enabled = false;
            }

            // 6. Kritik Stok Seviyesi
            startY += rowHeight;
            var lblKritik = new Label { Text = "Kritik Eşik Seviyesi:", Location = new Point(labelX, startY), AutoSize = true };
            numKritikSeviye = new NumericUpDown
            {
                Location = new Point(controlX, startY - 2),
                Size = new Size(controlWidth, 27),
                Maximum = 10000,
                Value = 10
            };

            // Dinamik Kategori Panelleri (OOP Alt Sınıfları İçin)
            startY += rowHeight + 10;

            // Panel: Elektronik
            grpElektronik = new GroupBox
            {
                Text = "⚡ Elektronik Ürün Ek Özellikleri",
                Location = new Point(labelX, startY),
                Size = new Size(440, 110),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Visible = false
            };
            var lblGaranti = new Label { Text = "Garanti (Ay):", Location = new Point(15, 30), AutoSize = true, Font = new Font("Segoe UI", 9f) };
            numGarantiSuresi = new NumericUpDown { Location = new Point(140, 28), Size = new Size(270, 25), Maximum = 120, Value = 24, Font = new Font("Segoe UI", 9f) };
            var lblSeri = new Label { Text = "Seri No / Seri:", Location = new Point(15, 68), AutoSize = true, Font = new Font("Segoe UI", 9f) };
            txtSeriNo = new TextBox { Location = new Point(140, 65), Size = new Size(270, 25), PlaceholderText = "Örn: SN-2024-X1", Font = new Font("Segoe UI", 9f) };
            grpElektronik.Controls.Add(lblGaranti);
            grpElektronik.Controls.Add(numGarantiSuresi);
            grpElektronik.Controls.Add(lblSeri);
            grpElektronik.Controls.Add(txtSeriNo);

            // Panel: Bozulabilir (Gıda)
            grpBozulabilir = new GroupBox
            {
                Text = "🍃 Bozulabilir (Gıda) Ek Özellikleri",
                Location = new Point(labelX, startY),
                Size = new Size(440, 110),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Visible = false
            };
            var lblSKT = new Label { Text = "Son Kullanma:", Location = new Point(15, 30), AutoSize = true, Font = new Font("Segoe UI", 9f) };
            dtpSKT = new DateTimePicker { Location = new Point(140, 28), Size = new Size(270, 25), Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(30), Font = new Font("Segoe UI", 9f) };
            var lblSicaklik = new Label { Text = "Saklama Sıcaklığı (°C):", Location = new Point(15, 68), AutoSize = true, Font = new Font("Segoe UI", 9f) };
            numSicaklik = new NumericUpDown { Location = new Point(140, 65), Size = new Size(270, 25), Minimum = -30, Maximum = 50, Value = 4, Font = new Font("Segoe UI", 9f) };
            grpBozulabilir.Controls.Add(lblSKT);
            grpBozulabilir.Controls.Add(dtpSKT);
            grpBozulabilir.Controls.Add(lblSicaklik);
            grpBozulabilir.Controls.Add(numSicaklik);

            // Panel: Fiziksel Depo (Tekstil / Kırtasiye / Diğer)
            grpFiziksel = new GroupBox
            {
                Text = "📦 Depo & Lojistik Ek Özellikleri",
                Location = new Point(labelX, startY),
                Size = new Size(440, 110),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Visible = true
            };
            var lblRaf = new Label { Text = "Raf / Depo Konumu:", Location = new Point(15, 30), AutoSize = true, Font = new Font("Segoe UI", 9f) };
            txtRafKonumu = new TextBox { Location = new Point(140, 28), Size = new Size(270, 25), PlaceholderText = "Örn: Raf B-3, Koridor 2", Font = new Font("Segoe UI", 9f) };
            var lblAgirlik = new Label { Text = "Birim Ağırlık (kg):", Location = new Point(15, 68), AutoSize = true, Font = new Font("Segoe UI", 9f) };
            numAgirlik = new NumericUpDown { Location = new Point(140, 65), Size = new Size(270, 25), DecimalPlaces = 2, Maximum = 500, Value = 1.00m, Font = new Font("Segoe UI", 9f) };
            grpFiziksel.Controls.Add(lblRaf);
            grpFiziksel.Controls.Add(txtRafKonumu);
            grpFiziksel.Controls.Add(lblAgirlik);
            grpFiziksel.Controls.Add(numAgirlik);

            // Alt Butonlar
            btnKaydet = new Button
            {
                Text = _duzenlemeModu ? "✓ Değişiklikleri Kaydet" : "✓ Ürünü Kaydet",
                Location = new Point(controlX, 535),
                Size = new Size(185, 42),
                BackColor = _duzenlemeModu ? Color.FromArgb(37, 99, 235) : Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnKaydet.FlatAppearance.BorderSize = 0;
            btnKaydet.Click += BtnKaydet_Click;

            btnIptal = new Button
            {
                Text = "Vazgeç",
                Location = new Point(controlX + 195, 535),
                Size = new Size(105, 42),
                BackColor = Color.FromArgb(226, 232, 240),
                ForeColor = Color.FromArgb(51, 65, 85),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btnIptal.FlatAppearance.BorderSize = 0;
            btnIptal.Click += (s, e) => this.Close();

            this.Controls.Add(panelHeader);
            this.Controls.Add(lblKod);
            this.Controls.Add(txtUrunKodu);
            this.Controls.Add(lblAd);
            this.Controls.Add(txtUrunAdi);
            this.Controls.Add(lblKat);
            this.Controls.Add(cmbKategori);
            this.Controls.Add(lblFiyat);
            this.Controls.Add(numBirimFiyat);
            this.Controls.Add(lblStok);
            this.Controls.Add(numStokMiktari);
            this.Controls.Add(lblKritik);
            this.Controls.Add(numKritikSeviye);
            this.Controls.Add(grpElektronik);
            this.Controls.Add(grpBozulabilir);
            this.Controls.Add(grpFiziksel);
            this.Controls.Add(btnKaydet);
            this.Controls.Add(btnIptal);
        }

        private void CmbKategori_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbKategori.SelectedItem is UrunKategorisi kategori)
            {
                grpElektronik.Visible = kategori == UrunKategorisi.Elektronik;
                grpBozulabilir.Visible = kategori == UrunKategorisi.Gida;
                grpFiziksel.Visible = (kategori != UrunKategorisi.Elektronik && kategori != UrunKategorisi.Gida);
            }
        }

        private void MevcutBilgileriYukle()
        {
            if (_mevcutUrun == null) return;

            txtUrunKodu.Text = _mevcutUrun.UrunKodu;
            txtUrunAdi.Text = _mevcutUrun.UrunAdi;
            cmbKategori.SelectedItem = _mevcutUrun.Kategori;
            numBirimFiyat.Value = _mevcutUrun.BirimFiyat;
            numStokMiktari.Value = _mevcutUrun.StokMiktari;
            numKritikSeviye.Value = _mevcutUrun.KritikStokSeviyesi;

            if (_mevcutUrun is ElektronikUrun elk)
            {
                numGarantiSuresi.Value = elk.GarantiSuresiAy;
                txtSeriNo.Text = elk.SeriNoSerisi;
            }
            else if (_mevcutUrun is BozulabilirUrun bozul)
            {
                dtpSKT.Value = bozul.SonKullanmaTarihi;
                numSicaklik.Value = (decimal)bozul.SaklamaSicakligi;
            }
            else if (_mevcutUrun is FizikselUrun fiz)
            {
                txtRafKonumu.Text = fiz.RafKonumu;
                numAgirlik.Value = (decimal)fiz.AgirlikKg;
            }
        }

        private void BtnKaydet_Click(object? sender, EventArgs e)
        {
            string kod = txtUrunKodu.Text.Trim();
            string ad = txtUrunAdi.Text.Trim();

            if (string.IsNullOrWhiteSpace(kod))
            {
                MessageBox.Show("Lütfen ürün kodunu giriniz!", "Eksik Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUrunKodu.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(ad))
            {
                MessageBox.Show("Lütfen ürün adını giriniz!", "Eksik Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUrunAdi.Focus();
                return;
            }

            if (ad.Any(char.IsDigit))
            {
                MessageBox.Show("Ürün adı rakam içeremez! Lütfen sadece harflerden oluşan bir ürün adı giriniz.", "Geçersiz Ürün Adı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUrunAdi.Focus();
                txtUrunAdi.SelectAll();
                return;
            }

            if (cmbKategori.SelectedItem == null)
            {
                MessageBox.Show("Lütfen bir kategori seçiniz!", "Eksik Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            UrunKategorisi secilenKategori = (UrunKategorisi)cmbKategori.SelectedItem;
            decimal fiyat = numBirimFiyat.Value;
            int stok = (int)numStokMiktari.Value;
            int kritik = (int)numKritikSeviye.Value;

            try
            {
                UrunBase olusturulanUrun;

                if (secilenKategori == UrunKategorisi.Elektronik)
                {
                    olusturulanUrun = new ElektronikUrun(
                        kod,
                        ad,
                        fiyat,
                        stok,
                        (int)numGarantiSuresi.Value,
                        txtSeriNo.Text.Trim(),
                        kritik
                    );
                }
                else if (secilenKategori == UrunKategorisi.Gida)
                {
                    olusturulanUrun = new BozulabilirUrun(
                        kod,
                        ad,
                        fiyat,
                        stok,
                        dtpSKT.Value,
                        (double)numSicaklik.Value,
                        kritik
                    );
                }
                else
                {
                    olusturulanUrun = new FizikselUrun(
                        kod,
                        ad,
                        secilenKategori,
                        fiyat,
                        stok,
                        txtRafKonumu.Text.Trim(),
                        (double)numAgirlik.Value,
                        kritik
                    );
                }

                if (_duzenlemeModu && _mevcutUrun != null)
                {
                    // ID eşleştirmesi ile güncelleme
                    _manager.UrunGuncelle(_mevcutUrun.Id, olusturulanUrun);
                    MessageBox.Show("Ürün bilgileri başarıyla güncellendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _manager.UrunEkle(olusturulanUrun);
                    MessageBox.Show("Yeni ürün başarıyla kaydedildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (MukerrerUrunKoduException mEx)
            {
                MessageBox.Show(mEx.Message, "Mükerrer Ürün Kodu Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUrunKodu.Focus();
                txtUrunKodu.SelectAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kayıt sırasında bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
