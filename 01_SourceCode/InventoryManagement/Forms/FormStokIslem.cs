using System;
using System.Drawing;
using System.Windows.Forms;
using InventoryManagement.Enums;
using InventoryManagement.Exceptions;
using InventoryManagement.Models;
using InventoryManagement.Services;

namespace InventoryManagement.Forms
{
    public class FormStokIslem : Form
    {
        private readonly UrunBase _urun;
        private readonly StokManager _manager;

        private Label lblBaslik = null!;
        private Label lblUrunBilgi = null!;
        private Label lblMevcutStok = null!;
        private RadioButton rbGiris = null!;
        private RadioButton rbCikis = null!;
        private Label lblMiktar = null!;
        private NumericUpDown numMiktar = null!;
        private Label lblAciklama = null!;
        private TextBox txtAciklama = null!;
        private Label lblTahminiStok = null!;
        private Button btnKaydet = null!;
        private Button btnIptal = null!;

        public FormStokIslem(UrunBase urun, StokManager manager, IslemTipi varsayilanIslem = IslemTipi.Giris)
        {
            _urun = urun ?? throw new ArgumentNullException(nameof(urun));
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));

            InitializeCustomComponents();
            if (varsayilanIslem == IslemTipi.Cikis)
                rbCikis.Checked = true;
            else
                rbGiris.Checked = true;

            GuncelleTahminiStok();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Hızlı Stok Hareketi İşlemi";
            this.Size = new Size(480, 420);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9.5f);

            var panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(30, 41, 59)
            };

            lblBaslik = new Label
            {
                Text = "📦 Stok Giriş / Çıkış Hareketi",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            };

            lblUrunBilgi = new Label
            {
                Text = $"Ürün: [{_urun.UrunKodu}] {_urun.UrunAdi}",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(203, 213, 225),
                Location = new Point(22, 42),
                AutoSize = true
            };

            panelHeader.Controls.Add(lblBaslik);
            panelHeader.Controls.Add(lblUrunBilgi);

            lblMevcutStok = new Label
            {
                Text = $"Mevcut Stok: {_urun.StokMiktari} Adet",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = _urun.KritikStokMu() ? Color.FromArgb(220, 38, 38) : Color.FromArgb(16, 185, 129),
                Location = new Point(30, 90),
                AutoSize = true
            };

            var grpIslem = new GroupBox
            {
                Text = "İşlem Türü",
                Location = new Point(30, 125),
                Size = new Size(405, 60),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };

            rbGiris = new RadioButton
            {
                Text = "➕ Stok Girişi (Depoya Ekle)",
                Location = new Point(20, 24),
                AutoSize = true,
                Checked = true,
                ForeColor = Color.FromArgb(5, 150, 105),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            rbGiris.CheckedChanged += (s, e) => GuncelleTahminiStok();

            rbCikis = new RadioButton
            {
                Text = "➖ Stok Çıkışı (Depodan Düş)",
                Location = new Point(210, 24),
                AutoSize = true,
                ForeColor = Color.FromArgb(220, 38, 38),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            rbCikis.CheckedChanged += (s, e) => GuncelleTahminiStok();

            grpIslem.Controls.Add(rbGiris);
            grpIslem.Controls.Add(rbCikis);

            lblMiktar = new Label
            {
                Text = "İşlem Miktarı:",
                Location = new Point(30, 202),
                AutoSize = true
            };

            numMiktar = new NumericUpDown
            {
                Location = new Point(140, 200),
                Size = new Size(130, 28),
                Minimum = 1,
                Maximum = 100000,
                Value = 1
            };
            numMiktar.ValueChanged += (s, e) => GuncelleTahminiStok();

            lblTahminiStok = new Label
            {
                Text = "İşlem Sonrası: 0 Adet",
                Location = new Point(285, 202),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Italic),
                ForeColor = Color.FromArgb(71, 85, 105)
            };

            lblAciklama = new Label
            {
                Text = "Açıklama / Not:",
                Location = new Point(30, 245),
                AutoSize = true
            };

            txtAciklama = new TextBox
            {
                Location = new Point(140, 242),
                Size = new Size(295, 28),
                PlaceholderText = "Örn: Müşteri Satışı, Tedarikçi İadesi vb."
            };

            btnKaydet = new Button
            {
                Text = "✓ İşlemi Onayla ve Kaydet",
                Location = new Point(140, 310),
                Size = new Size(180, 42),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnKaydet.FlatAppearance.BorderSize = 0;
            btnKaydet.Click += BtnKaydet_Click;

            btnIptal = new Button
            {
                Text = "İptal",
                Location = new Point(330, 310),
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
            this.Controls.Add(lblMevcutStok);
            this.Controls.Add(grpIslem);
            this.Controls.Add(lblMiktar);
            this.Controls.Add(numMiktar);
            this.Controls.Add(lblTahminiStok);
            this.Controls.Add(lblAciklama);
            this.Controls.Add(txtAciklama);
            this.Controls.Add(btnKaydet);
            this.Controls.Add(btnIptal);
        }

        private void GuncelleTahminiStok()
        {
            int miktar = (int)numMiktar.Value;
            int yeniStok = rbGiris.Checked ? (_urun.StokMiktari + miktar) : (_urun.StokMiktari - miktar);

            lblTahminiStok.Text = $"İşlem Sonrası: {yeniStok} Adet";
            if (yeniStok < 0)
            {
                lblTahminiStok.ForeColor = Color.FromArgb(220, 38, 38);
                lblTahminiStok.Text += " (YETERSİZ STOK!)";
            }
            else
            {
                lblTahminiStok.ForeColor = Color.FromArgb(71, 85, 105);
            }
        }

        private void BtnKaydet_Click(object? sender, EventArgs e)
        {
            int miktar = (int)numMiktar.Value;
            string aciklama = txtAciklama.Text.Trim();

            try
            {
                if (rbGiris.Checked)
                {
                    _manager.StokGirisYap(_urun.UrunKodu, miktar, aciklama);
                    MessageBox.Show(
                        $"Stok girişi başarıyla tamamlandı!\n\n" +
                        $"Ürün: [{_urun.UrunKodu}] {_urun.UrunAdi}\n" +
                        $"Eklenen Miktar: +{miktar} Adet\n" +
                        $"Güncel Stok: {_urun.StokMiktari} Adet",
                        "Başarılı Stok Girişi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                else
                {
                    _manager.StokCikisYap(_urun.UrunKodu, miktar, aciklama);
                    MessageBox.Show(
                        $"Stok çıkışı başarıyla tamamlandı!\n\n" +
                        $"Ürün: [{_urun.UrunKodu}] {_urun.UrunAdi}\n" +
                        $"Düşülen Miktar: -{miktar} Adet\n" +
                        $"Güncel Stok: {_urun.StokMiktari} Adet",
                        "Başarılı Stok Çıkışı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (YetersizStokException yEx)
            {
                MessageBox.Show(
                    $"İşlem gerçekleştirilemedi!\n\n{yEx.Message}",
                    "Yetersiz Stok Uyarısı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
            catch (GecersizMiktarException gEx)
            {
                MessageBox.Show(
                    $"Geçersiz miktar!\n\n{gEx.Message}",
                    "Hatalı Giriş",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Beklenmedik bir hata oluştu: {ex.Message}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
