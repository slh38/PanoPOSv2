using System.Globalization;
using DevExpress.XtraEditors;
using PanoPos.Desktop.Models;
using PanoPos.Desktop.Services;
using PanoPos.Desktop.Session;

namespace PanoPos.Desktop.Forms;

public sealed class TahsilatForm : XtraForm
{
    private readonly ITahsilatService _tahsilatService;
    private readonly AppSession _session;
    private readonly CultureInfo _culture = new("tr-TR");
    private readonly TextEdit _txtGirilenTutar;
    private readonly TextEdit _txtToplam;
    private readonly TextEdit _txtIndirimToplami;
    private readonly TextEdit _txtGenelToplam;
    private readonly TextEdit _txtOdenen;
    private readonly TextEdit _txtParaUstu;
    private string _girilenTutar = string.Empty;

    public TahsilatForm(ITahsilatService tahsilatService, AppSession session, FaturaResponseModel fatura, OdemeTipiModel? varsayilanOdemeTipi = null)
    {
        _tahsilatService = tahsilatService;
        _session = session;
        CurrentFatura = fatura;
        VarsayilanOdemeTipi = varsayilanOdemeTipi;

        Text = "Tahsilat";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(1180, 780);
        BackColor = Color.FromArgb(241, 244, 247);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 170));

        var topBar = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Black
        };

        var lblHeader = new LabelControl
        {
            Dock = DockStyle.Fill,
            Text = "Tahsilat",
            AutoSizeMode = LabelAutoSizeMode.None
        };
        lblHeader.Appearance.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblHeader.Appearance.ForeColor = Color.White;
        lblHeader.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
        lblHeader.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
        topBar.Controls.Add(lblHeader);

        var middleLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Padding = new Padding(10, 4, 10, 4)
        };
        middleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320));
        middleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var leftPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(18)
        };

        var leftLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 7
        };
        leftLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));
        leftLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56));
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var lblTitle = new LabelControl
        {
            Text = "Tahsilat",
            Dock = DockStyle.Fill,
            AutoSizeMode = LabelAutoSizeMode.None
        };
        lblTitle.Appearance.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
        lblTitle.Appearance.ForeColor = Color.FromArgb(35, 59, 128);
        lblTitle.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
        lblTitle.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
        leftLayout.Controls.Add(lblTitle, 0, 0);
        leftLayout.SetColumnSpan(lblTitle, 2);

        leftLayout.Controls.Add(CreateSummaryLabel("Toplam"), 0, 1);
        _txtToplam = CreateSummaryValue(Color.FromArgb(22, 28, 36), 18F);
        leftLayout.Controls.Add(_txtToplam, 1, 1);

        leftLayout.Controls.Add(CreateSummaryLabel("Indirim Toplami"), 0, 2);
        _txtIndirimToplami = CreateSummaryValue(Color.FromArgb(22, 28, 36), 18F);
        leftLayout.Controls.Add(_txtIndirimToplami, 1, 2);

        leftLayout.Controls.Add(CreateSummaryLabel("Genel Toplam"), 0, 3);
        _txtGenelToplam = CreateSummaryValue(Color.FromArgb(37, 99, 235), 20F);
        leftLayout.Controls.Add(_txtGenelToplam, 1, 3);

        leftLayout.Controls.Add(CreateSummaryLabel("Odenen"), 0, 4);
        _txtOdenen = CreateSummaryValue(Color.FromArgb(22, 28, 36), 18F);
        leftLayout.Controls.Add(_txtOdenen, 1, 4);

        leftLayout.Controls.Add(CreateSummaryLabel("Para Ustu"), 0, 5);
        _txtParaUstu = CreateSummaryValue(Color.FromArgb(220, 38, 38), 18F);
        leftLayout.Controls.Add(_txtParaUstu, 1, 5);

        leftPanel.Controls.Add(leftLayout);

        var rightPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(16)
        };

        var rightLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var inputLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2
        };
        inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var lblSelect = new LabelControl
        {
            Text = "Seciniz...",
            Dock = DockStyle.Fill,
            AutoSizeMode = LabelAutoSizeMode.None
        };
        lblSelect.Appearance.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblSelect.Appearance.ForeColor = Color.FromArgb(35, 59, 128);
        lblSelect.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
        lblSelect.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;

        _txtGirilenTutar = new TextEdit
        {
            Dock = DockStyle.Fill
        };
        _txtGirilenTutar.Properties.ReadOnly = true;
        _txtGirilenTutar.Properties.Appearance.BackColor = Color.White;
        _txtGirilenTutar.Properties.Appearance.ForeColor = Color.FromArgb(22, 28, 36);
        _txtGirilenTutar.Properties.Appearance.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
        _txtGirilenTutar.Properties.Appearance.Options.UseBackColor = true;
        _txtGirilenTutar.Properties.Appearance.Options.UseForeColor = true;
        _txtGirilenTutar.Properties.Appearance.Options.UseFont = true;
        _txtGirilenTutar.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;

        inputLayout.Controls.Add(lblSelect, 0, 0);
        inputLayout.Controls.Add(_txtGirilenTutar, 1, 0);

        var keypadLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 7,
            RowCount = 4,
            Margin = new Padding(0, 8, 0, 0)
        };

        for (var i = 0; i < 7; i++)
        {
            keypadLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.2857F));
        }

        for (var i = 0; i < 4; i++)
        {
            keypadLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        }

        AddKeyButton(keypadLayout, 0, 0, "1/2", Color.FromArgb(61, 90, 128), RatioButton_Click, 2m);
        AddKeyButton(keypadLayout, 1, 0, "7", Color.FromArgb(85, 85, 85), DigitButton_Click, "7");
        AddKeyButton(keypadLayout, 2, 0, "8", Color.FromArgb(85, 85, 85), DigitButton_Click, "8");
        AddKeyButton(keypadLayout, 3, 0, "9", Color.FromArgb(85, 85, 85), DigitButton_Click, "9");
        AddKeyButton(keypadLayout, 4, 0, "0", Color.FromArgb(85, 85, 85), DigitButton_Click, "0");
        AddKeyButton(keypadLayout, 5, 0, "5", Color.FromArgb(194, 65, 12), QuickAmountButton_Click, 5m);
        AddKeyButton(keypadLayout, 6, 0, "10", Color.FromArgb(194, 65, 12), QuickAmountButton_Click, 10m);

        AddKeyButton(keypadLayout, 0, 1, "1/3", Color.FromArgb(61, 90, 128), RatioButton_Click, 3m);
        AddKeyButton(keypadLayout, 1, 1, "4", Color.FromArgb(85, 85, 85), DigitButton_Click, "4");
        AddKeyButton(keypadLayout, 2, 1, "5", Color.FromArgb(85, 85, 85), DigitButton_Click, "5");
        AddKeyButton(keypadLayout, 3, 1, "6", Color.FromArgb(85, 85, 85), DigitButton_Click, "6");
        AddKeyButton(keypadLayout, 4, 1, ",", Color.FromArgb(68, 68, 68), DecimalButton_Click, null);
        AddKeyButton(keypadLayout, 5, 1, "20", Color.FromArgb(194, 65, 12), QuickAmountButton_Click, 20m);
        AddKeyButton(keypadLayout, 6, 1, "50", Color.FromArgb(194, 65, 12), QuickAmountButton_Click, 50m);

        AddKeyButton(keypadLayout, 0, 2, "1/4", Color.FromArgb(61, 90, 128), RatioButton_Click, 4m);
        AddKeyButton(keypadLayout, 1, 2, "1", Color.FromArgb(85, 85, 85), DigitButton_Click, "1");
        AddKeyButton(keypadLayout, 2, 2, "2", Color.FromArgb(85, 85, 85), DigitButton_Click, "2");
        AddKeyButton(keypadLayout, 3, 2, "3", Color.FromArgb(85, 85, 85), DigitButton_Click, "3");
        AddKeyButton(keypadLayout, 4, 2, "CL", Color.FromArgb(38, 70, 83), ClearButton_Click, null);
        AddKeyButton(keypadLayout, 5, 2, "100", Color.FromArgb(194, 65, 12), QuickAmountButton_Click, 100m);
        AddKeyButton(keypadLayout, 6, 2, "200", Color.FromArgb(194, 65, 12), QuickAmountButton_Click, 200m);

        AddKeyButton(keypadLayout, 0, 3, "1/n", Color.FromArgb(61, 90, 128), PlaceholderButton_Click, "1/n");
        AddKeyButton(keypadLayout, 1, 3, "Odeme iptal", Color.FromArgb(67, 97, 238), PlaceholderButton_Click, "Odeme iptal");
        keypadLayout.SetColumnSpan(keypadLayout.Controls[keypadLayout.Controls.Count - 1], 2);
        AddKeyButton(keypadLayout, 3, 3, "Tahsilat indirim", Color.FromArgb(46, 196, 182), PlaceholderButton_Click, "Tahsilat indirim");
        keypadLayout.SetColumnSpan(keypadLayout.Controls[keypadLayout.Controls.Count - 1], 2);
        AddKeyButton(keypadLayout, 5, 3, "CL", Color.FromArgb(45, 45, 45), ClearButton_Click, null);
        keypadLayout.SetColumnSpan(keypadLayout.Controls[keypadLayout.Controls.Count - 1], 2);

        rightLayout.Controls.Add(inputLayout, 0, 0);
        rightLayout.Controls.Add(keypadLayout, 0, 1);
        rightPanel.Controls.Add(rightLayout);

        var bottomLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 2,
            Padding = new Padding(10, 0, 10, 10)
        };
        bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
        bottomLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        bottomLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

        var btnKrediKarti = CreateBottomButton("Kredi Karti", Color.FromArgb(126, 34, 206), BtnKrediKarti_Click);
        var btnNakit = CreateBottomButton("Nakit", Color.FromArgb(22, 163, 74), BtnNakit_Click);
        var btnFatura = CreateBottomButton("Fatura", Color.FromArgb(67, 97, 238), PlaceholderButton_Click);
        var btnAcikHesap = CreateBottomButton("Acik Hesap", Color.FromArgb(64, 64, 64), BtnAcikHesap_Click);
        var btnKapat = CreateBottomButton("Kapat / Cikis", Color.FromArgb(220, 38, 38), BtnKapat_Click);

        bottomLayout.Controls.Add(btnKrediKarti, 0, 0);
        bottomLayout.SetColumnSpan(btnKrediKarti, 2);
        bottomLayout.Controls.Add(btnNakit, 2, 0);
        bottomLayout.Controls.Add(btnFatura, 0, 1);
        bottomLayout.Controls.Add(btnAcikHesap, 1, 1);
        bottomLayout.Controls.Add(btnKapat, 2, 1);

        middleLayout.Controls.Add(leftPanel, 0, 0);
        middleLayout.Controls.Add(rightPanel, 1, 0);

        root.Controls.Add(topBar, 0, 0);
        root.Controls.Add(middleLayout, 0, 1);
        root.Controls.Add(bottomLayout, 0, 2);

        Controls.Add(root);

        RefreshSummary();

        if (VarsayilanOdemeTipi.HasValue)
        {
            _girilenTutar = FormatAmount(Math.Max(0m, CurrentFatura.KalanTutar));
            RefreshInput();
        }
    }

    public FaturaResponseModel CurrentFatura { get; private set; }

    public bool IsCompleted { get; private set; }

    public OdemeTipiModel? VarsayilanOdemeTipi { get; }

    private LabelControl CreateSummaryLabel(string text)
    {
        var label = new LabelControl
        {
            Text = text,
            Dock = DockStyle.Fill,
            AutoSizeMode = LabelAutoSizeMode.None
        };
        label.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        label.Appearance.ForeColor = Color.FromArgb(22, 28, 36);
        label.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
        label.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
        return label;
    }

    private TextEdit CreateSummaryValue(Color color, float size)
    {
        var edit = new TextEdit
        {
            Dock = DockStyle.Fill
        };
        edit.Properties.ReadOnly = true;
        edit.Properties.Appearance.BackColor = Color.White;
        edit.Properties.Appearance.ForeColor = color;
        edit.Properties.Appearance.Font = new Font("Segoe UI", size, FontStyle.Bold);
        edit.Properties.Appearance.Options.UseBackColor = true;
        edit.Properties.Appearance.Options.UseForeColor = true;
        edit.Properties.Appearance.Options.UseFont = true;
        edit.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
        edit.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
        return edit;
    }

    private SimpleButton CreateBottomButton(string text, Color backColor, EventHandler click)
    {
        var button = new SimpleButton
        {
            Text = text,
            Dock = DockStyle.Fill,
            Margin = new Padding(0)
        };
        button.Appearance.BackColor = backColor;
        button.Appearance.ForeColor = Color.White;
        button.Appearance.BorderColor = backColor;
        button.Appearance.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        button.Appearance.Options.UseBackColor = true;
        button.Appearance.Options.UseForeColor = true;
        button.Appearance.Options.UseBorderColor = true;
        button.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
        button.Click += click;
        return button;
    }

    private void AddKeyButton(TableLayoutPanel layout, int column, int row, string text, Color backColor, EventHandler click, object? tag)
    {
        var button = new SimpleButton
        {
            Text = text,
            Dock = DockStyle.Fill,
            Margin = new Padding(1),
            Tag = tag
        };
        button.Appearance.BackColor = backColor;
        button.Appearance.ForeColor = Color.White;
        button.Appearance.BorderColor = backColor;
        button.Appearance.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
        button.Appearance.Options.UseBackColor = true;
        button.Appearance.Options.UseForeColor = true;
        button.Appearance.Options.UseBorderColor = true;
        button.Appearance.Options.UseTextOptions = true;
        button.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
        button.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
        button.Click += click;
        layout.Controls.Add(button, column, row);
    }

    private void DigitButton_Click(object? sender, EventArgs e)
    {
        if (sender is not SimpleButton button || button.Tag is not string value)
        {
            return;
        }

        _girilenTutar += value;
        RefreshInput();
    }

    private void DecimalButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_girilenTutar))
        {
            _girilenTutar = "0,";
        }
        else if (!_girilenTutar.Contains(','))
        {
            _girilenTutar += ",";
        }

        RefreshInput();
    }

    private void QuickAmountButton_Click(object? sender, EventArgs e)
    {
        if (sender is not SimpleButton button || button.Tag is not decimal amount)
        {
            return;
        }

        _girilenTutar = FormatAmount(amount);
        RefreshInput();
    }

    private void RatioButton_Click(object? sender, EventArgs e)
    {
        if (sender is not SimpleButton button || button.Tag is not decimal divisor || divisor <= 0)
        {
            return;
        }

        var kalan = Math.Max(0m, CurrentFatura.KalanTutar);
        var tutar = Math.Round(kalan / divisor, 2, MidpointRounding.AwayFromZero);
        _girilenTutar = FormatAmount(tutar);
        RefreshInput();
    }

    private void ClearButton_Click(object? sender, EventArgs e)
    {
        _girilenTutar = string.Empty;
        RefreshInput();
    }

    private void PlaceholderButton_Click(object? sender, EventArgs e)
    {
        var caption = sender is SimpleButton button && !string.IsNullOrWhiteSpace(button.Text)
            ? button.Text
            : "Bilgi";

        XtraMessageBox.Show($"{caption} sonraki adimda yapilacak.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async void BtnKrediKarti_Click(object? sender, EventArgs e)
    {
        await ProcessPaymentAsync(OdemeTipiModel.KrediKarti);
    }

    private async void BtnNakit_Click(object? sender, EventArgs e)
    {
        await ProcessPaymentAsync(OdemeTipiModel.Nakit);
    }

    private async void BtnAcikHesap_Click(object? sender, EventArgs e)
    {
        await ProcessPaymentAsync(OdemeTipiModel.Veresiye);
    }

    private void BtnKapat_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private async Task ProcessPaymentAsync(OdemeTipiModel odemeTipi)
    {
        if (CurrentFatura.KalanTutar <= 0)
        {
            XtraMessageBox.Show("Bu fatura zaten tamamen odendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var tutar = ResolveInputAmount();
        if (tutar <= 0)
        {
            XtraMessageBox.Show("Gecerli bir tahsilat tutari girin.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (tutar > CurrentFatura.KalanTutar)
        {
            XtraMessageBox.Show("Tahsilat tutari kalan tutari gecemez.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            ToggleBusy(true);

            var response = await _tahsilatService.TahsilatYapAsync(new TahsilatRequestModel
            {
                SubeId = _session.VarsayilanSubeId,
                FaturaId = CurrentFatura.Id,
                OdemeTipi = odemeTipi,
                KullaniciId = _session.KullaniciId,
                CihazId = _session.CihazId,
                Tutar = tutar,
                ParaBirimKodu = CurrentFatura.ParaBirimKodu,
                Kur = CurrentFatura.Kur
            });

            CurrentFatura.OdenenTutar = response?.FaturaOdenenTutar ?? (CurrentFatura.OdenenTutar + tutar);
            CurrentFatura.KalanTutar = response?.FaturaKalanTutar ?? Math.Max(0m, CurrentFatura.NetToplam - CurrentFatura.OdenenTutar);
            CurrentFatura.Durum = response?.FaturaDurumu ?? CurrentFatura.Durum;

            _girilenTutar = string.Empty;
            RefreshInput();
            RefreshSummary();

            if (CurrentFatura.KalanTutar <= 0)
            {
                IsCompleted = true;
                XtraMessageBox.Show("Tahsilat tamamlandi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
                return;
            }
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show(ex.Message, "Tahsilat Hatasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            ToggleBusy(false);
        }
    }

    private decimal ResolveInputAmount()
    {
        if (string.IsNullOrWhiteSpace(_girilenTutar))
        {
            return Math.Max(0m, CurrentFatura.KalanTutar);
        }

        var normalized = _girilenTutar.Replace('.', ',');
        return decimal.TryParse(normalized, NumberStyles.Number, _culture, out var result)
            ? result
            : 0m;
    }

    private void RefreshInput()
    {
        _txtGirilenTutar.Text = string.IsNullOrWhiteSpace(_girilenTutar)
            ? "0,00"
            : _girilenTutar;
    }

    private void RefreshSummary()
    {
        _txtToplam.Text = FormatAmount(CurrentFatura.AraToplam);
        _txtIndirimToplami.Text = FormatAmount(CurrentFatura.GenelIndirimTutari);
        _txtGenelToplam.Text = FormatAmount(CurrentFatura.NetToplam);
        _txtOdenen.Text = FormatAmount(CurrentFatura.OdenenTutar);
        _txtParaUstu.Text = FormatAmount(0m);
        _txtParaUstu.Properties.Appearance.ForeColor = CurrentFatura.KalanTutar < 0 ? Color.FromArgb(220, 38, 38) : Color.FromArgb(22, 28, 36);
    }

    private string FormatAmount(decimal amount)
    {
        return amount.ToString("n2", _culture);
    }

    private void ToggleBusy(bool isBusy)
    {
        UseWaitCursor = isBusy;
        Enabled = !isBusy;
    }
}
