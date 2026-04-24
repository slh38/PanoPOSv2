using System.ComponentModel;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using PanoPos.Desktop.Models;
using PanoPos.Desktop.Services;
using PanoPos.Desktop.Session;

namespace PanoPos.Desktop.Forms;

public sealed class HizliSatisForm : XtraForm
{
    private readonly IHizliSatisService _hizliSatisService;
    private readonly AppSession _session;
    private readonly ToolTip _toolTip = new();
    private readonly BindingList<SepetSatirModel> _sepetSatirlari = [];
    private readonly List<UrunKartModel> _urunler = [];
    private readonly FlowLayoutPanel _urunKartPanel;
    private readonly TextEdit _txtBarkod;
    private readonly LabelControl _lblAraToplam;
    private readonly LabelControl _lblIndirim;
    private readonly LabelControl _lblToplam;
    private readonly GridView _gridView;
    private readonly FlowLayoutPanel _kategoriPanel;
    private string? _aktifKategori;

    public HizliSatisForm(IHizliSatisService hizliSatisService, AppSession session)
    {
        _hizliSatisService = hizliSatisService;
        _session = session;

        Text = "Hizli Satis";
        StartPosition = FormStartPosition.CenterParent;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1450, 860);
        BackColor = Color.FromArgb(247, 247, 247);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = Color.Transparent
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 700));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        root.Controls.Add(BuildMenuPanel(), 0, 0);
        root.Controls.Add(BuildCartPanel(out _gridView, out _txtBarkod, out _lblAraToplam, out _lblIndirim, out _lblToplam), 1, 0);
        root.Controls.Add(BuildKeypadPanel(), 2, 0);
        root.Controls.Add(BuildProductsPanel(out _kategoriPanel, out _urunKartPanel), 3, 0);

        Controls.Add(root);

        _sepetSatirlari.ListChanged += (_, _) => UpdateTotals();
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _txtBarkod.Focus();
        await LoadProductsAsync();
    }

    private Control BuildMenuPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(247, 247, 247),
            Padding = new Padding(12)
        };

        var buttonHost = new Panel
        {
            Dock = DockStyle.Top,
            Height = 288,
            BackColor = Color.Transparent
        };

        buttonHost.Controls.Add(CreateMenuButtonSlot(CreateLeftMenuButton("Bekleyen\nSatislar", Color.FromArgb(116, 97, 195)), 108));
        buttonHost.Controls.Add(CreateMenuButtonSlot(CreateLeftMenuButton("Beklet", Color.FromArgb(57, 138, 201), BtnBeklet_Click), 78));
        buttonHost.Controls.Add(CreateMenuButtonSlot(CreateLeftMenuButton("Borc", Color.FromArgb(47, 179, 127)), 78));

        var statusPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 90,
            BackColor = Color.Transparent,
            Padding = new Padding(4, 0, 4, 8)
        };

        var lblOnline = new LabelControl
        {
            Text = "ONLINE",
            Dock = DockStyle.Top,
            Height = 28,
            AutoSizeMode = LabelAutoSizeMode.None
        };
        lblOnline.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblOnline.Appearance.ForeColor = Color.FromArgb(95, 220, 120);

        var lblShift = new LabelControl
        {
            Text = "Vardiya: -",
            Dock = DockStyle.Top,
            Height = 24,
            AutoSizeMode = LabelAutoSizeMode.None
        };
        lblShift.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
        lblShift.Appearance.ForeColor = Color.FromArgb(92, 92, 92);

        statusPanel.Controls.Add(lblShift);
        statusPanel.Controls.Add(lblOnline);

        panel.Controls.Add(statusPanel);
        panel.Controls.Add(buttonHost);
        return panel;
    }

    private Control CreateMenuButtonSlot(SimpleButton button, int height)
    {
        var slot = new Panel
        {
            Dock = DockStyle.Top,
            Height = height + 8,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 0, 0, 8)
        };

        button.Dock = DockStyle.Fill;
        slot.Controls.Add(button);
        return slot;
    }

    private SimpleButton CreateLeftMenuButton(string text, Color backColor, EventHandler? click = null)
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
        button.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        button.Appearance.Options.UseBackColor = true;
        button.Appearance.Options.UseForeColor = true;
        button.Appearance.Options.UseBorderColor = true;
        button.Appearance.Options.UseTextOptions = true;
        button.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
        button.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
        button.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
        button.AppearanceHovered.BackColor = ControlPaint.Light(backColor);
        button.AppearanceHovered.BorderColor = ControlPaint.Light(backColor);
        button.AppearanceHovered.ForeColor = Color.White;
        button.AppearanceHovered.Options.UseBackColor = true;
        button.AppearanceHovered.Options.UseBorderColor = true;
        button.AppearanceHovered.Options.UseForeColor = true;
        button.AppearancePressed.BackColor = ControlPaint.Dark(backColor);
        button.AppearancePressed.BorderColor = ControlPaint.Dark(backColor);
        button.AppearancePressed.ForeColor = Color.White;
        button.AppearancePressed.Options.UseBackColor = true;
        button.AppearancePressed.Options.UseBorderColor = true;
        button.AppearancePressed.Options.UseForeColor = true;
        button.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
        button.Click += click ?? PlaceholderClick;
        return button;
    }

    private Control BuildCartPanel(out GridView gridView, out TextEdit txtBarkod, out LabelControl lblAraToplam, out LabelControl lblIndirim, out LabelControl lblToplam)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(16)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 210));

        var satisToolbar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 6)
        };
        satisToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        satisToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 54));
        satisToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 54));
        satisToolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 12));

        var cmbSatisTipi = new ComboBoxEdit
        {
            Dock = DockStyle.Fill
        };
        cmbSatisTipi.Properties.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
        cmbSatisTipi.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
        cmbSatisTipi.Properties.Items.AddRange(new[]
        {
            "Perakende Satis",
            "Cariye Satis",
            "Cariye Borc"
        });
        cmbSatisTipi.SelectedIndex = 0;

        var btnCariSec = CreateMiniActionButton("K");
        btnCariSec.Click += PlaceholderClick;
        _toolTip.SetToolTip(btnCariSec, "Cari Secin");

        var btnCariEkle = CreateMiniActionButton("+");
        btnCariEkle.Click += PlaceholderClick;
        _toolTip.SetToolTip(btnCariEkle, "Cari Ekleyin");

        var gridControl = new GridControl
        {
            Dock = DockStyle.Fill,
            DataSource = _sepetSatirlari
        };

        gridView = new GridView(gridControl);
        gridView.OptionsView.ShowGroupPanel = false;
        gridView.OptionsView.ShowIndicator = false;
        gridView.OptionsView.RowAutoHeight = false;
        gridView.VertScrollVisibility = DevExpress.XtraGrid.Views.Base.ScrollVisibility.Auto;
        gridView.Appearance.Row.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
        gridView.Appearance.Row.Options.UseFont = true;
        gridView.Appearance.HeaderPanel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        gridView.Appearance.HeaderPanel.Options.UseFont = true;
        gridView.RowHeight = 42;
        gridView.OptionsBehavior.Editable = true;
        gridView.OptionsCustomization.AllowColumnMoving = false;
        gridView.OptionsCustomization.AllowFilter = false;
        gridView.OptionsCustomization.AllowSort = false;
        gridView.OptionsSelection.EnableAppearanceFocusedCell = false;
        gridView.KeyDown += GridView_KeyDown;
        gridView.CellValueChanged += GridView_CellValueChanged;

        var qtyEditor = new RepositoryItemSpinEdit
        {
            AutoHeight = false,
            IsFloatValue = true,
            MinValue = 0,
            MaxValue = 9999
        };
        qtyEditor.EditMask = "n2";
        gridControl.RepositoryItems.Add(qtyEditor);

        var urunAdiColumn = gridView.Columns.AddVisible(nameof(SepetSatirModel.UrunAdi), "UrunAdi");
        urunAdiColumn.OptionsColumn.AllowEdit = false;
        urunAdiColumn.MinWidth = 300;
        urunAdiColumn.Width = 340;
        var miktarColumn = gridView.Columns.AddVisible(nameof(SepetSatirModel.Miktar), "Miktar");
        miktarColumn.ColumnEdit = qtyEditor;
        miktarColumn.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
        miktarColumn.DisplayFormat.FormatString = "n2";
        miktarColumn.Width = 70;

        var birimFiyatColumn = gridView.Columns.AddVisible(nameof(SepetSatirModel.BirimFiyat), "BirimFiyat");
        birimFiyatColumn.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
        birimFiyatColumn.DisplayFormat.FormatString = "n2";
        birimFiyatColumn.OptionsColumn.AllowEdit = false;
        birimFiyatColumn.Width = 85;

        var indirimColumn = gridView.Columns.AddVisible(nameof(SepetSatirModel.IndirimTutari), "IndirimTutari");
        indirimColumn.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
        indirimColumn.DisplayFormat.FormatString = "n2";
        indirimColumn.OptionsColumn.AllowEdit = false;
        indirimColumn.Width = 80;

        var netColumn = gridView.Columns.AddVisible(nameof(SepetSatirModel.SatirNetToplam), "SatirNetToplam");
        netColumn.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
        netColumn.DisplayFormat.FormatString = "n2";
        netColumn.OptionsColumn.AllowEdit = false;
        netColumn.Width = 95;

        gridControl.MainView = gridView;

        txtBarkod = new TextEdit
        {
            Dock = DockStyle.Fill
        };
        txtBarkod.Properties.NullValuePrompt = "Barkod okutun veya yazin";
        txtBarkod.Properties.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Regular);
        txtBarkod.KeyDown += TxtBarkod_KeyDown;

        var totalPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(248, 249, 251),
            Padding = new Padding(12)
        };

        var totalLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4
        };
        totalLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        totalLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        totalLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        totalLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        totalLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        totalLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        totalLayout.Controls.Add(CreateTotalCaption("AraToplam"), 0, 0);
        lblAraToplam = CreateTotalValue(Color.FromArgb(64, 64, 64), 12F);
        totalLayout.Controls.Add(lblAraToplam, 1, 0);

        totalLayout.Controls.Add(CreateTotalCaption("Indirim"), 0, 1);
        lblIndirim = CreateTotalValue(Color.FromArgb(64, 64, 64), 12F);
        totalLayout.Controls.Add(lblIndirim, 1, 1);

        totalLayout.Controls.Add(CreateTotalCaption("Toplam"), 0, 2);
        lblToplam = CreateTotalValue(Color.FromArgb(35, 108, 224), 20F);
        totalLayout.Controls.Add(lblToplam, 1, 2);

        var paymentLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(0, 12, 0, 0)
        };
        paymentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        paymentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        paymentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        paymentLayout.Controls.Add(CreatePaymentButton("KREDI KARTI"), 0, 0);
        paymentLayout.Controls.Add(CreatePaymentButton("PARCALI"), 1, 0);
        paymentLayout.Controls.Add(CreatePaymentButton("NAKIT"), 2, 0);
        totalLayout.Controls.Add(paymentLayout, 0, 3);
        totalLayout.SetColumnSpan(paymentLayout, 2);

        totalPanel.Controls.Add(totalLayout);

        satisToolbar.Controls.Add(cmbSatisTipi, 0, 0);
        satisToolbar.Controls.Add(btnCariSec, 1, 0);
        satisToolbar.Controls.Add(btnCariEkle, 2, 0);

        layout.Controls.Add(satisToolbar, 0, 0);
        layout.Controls.Add(gridControl, 0, 1);
        layout.Controls.Add(txtBarkod, 0, 2);
        layout.Controls.Add(totalPanel, 0, 3);
        panel.Controls.Add(layout);

        return panel;
    }

    private SimpleButton CreateMiniActionButton(string text)
    {
        var button = new SimpleButton
        {
            Text = text,
            Dock = DockStyle.Fill,
            Margin = new Padding(6, 0, 0, 0)
        };
        button.Appearance.BackColor = Color.White;
        button.Appearance.ForeColor = Color.FromArgb(52, 73, 94);
        button.Appearance.BorderColor = Color.FromArgb(214, 219, 224);
        button.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        button.Appearance.Options.UseBackColor = true;
        button.Appearance.Options.UseForeColor = true;
        button.Appearance.Options.UseBorderColor = true;
        button.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
        return button;
    }

    private LabelControl CreateTotalCaption(string text)
    {
        var label = new LabelControl
        {
            Text = text,
            Dock = DockStyle.Fill,
            AutoSizeMode = LabelAutoSizeMode.None
        };
        label.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        label.Appearance.ForeColor = Color.FromArgb(70, 70, 70);
        return label;
    }

    private LabelControl CreateTotalValue(Color color, float size)
    {
        var label = new LabelControl
        {
            Dock = DockStyle.Fill,
            AutoSizeMode = LabelAutoSizeMode.None,
            Text = "0,00"
        };
        label.Appearance.Font = new Font("Segoe UI", size, FontStyle.Bold);
        label.Appearance.ForeColor = color;
        label.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
        return label;
    }

    private SimpleButton CreatePaymentButton(string text)
    {
        var button = new SimpleButton
        {
            Text = text,
            Dock = DockStyle.Fill,
            Margin = new Padding(6, 6, 6, 0)
        };
        button.Appearance.BackColor = Color.FromArgb(71, 93, 116);
        button.Appearance.ForeColor = Color.White;
        button.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        button.Appearance.Options.UseBackColor = true;
        button.Appearance.Options.UseForeColor = true;
        button.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
        button.Click += PlaceholderClick;
        return button;
    }

    private Control BuildKeypadPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(240, 242, 245),
            Padding = new Padding(10)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 12
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        for (var i = 0; i < 12; i++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 8.33F));
        }

        var keys = new[]
        {
            "0",
            ",",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "Sil"
        };

        for (var row = 0; row < keys.Length; row++)
        {
            layout.Controls.Add(CreateKeypadButton(keys[row]), 0, row);
        }

        panel.Controls.Add(layout);
        return panel;
    }

    private SimpleButton CreateKeypadButton(string text)
    {
        var button = new SimpleButton
        {
            Text = text,
            Dock = DockStyle.Fill,
            Margin = new Padding(6)
        };
        button.Appearance.BackColor = text == "Sil" ? Color.FromArgb(226, 97, 83) : Color.White;
        button.Appearance.ForeColor = text == "Sil" ? Color.White : Color.FromArgb(52, 73, 94);
        button.Appearance.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        button.Appearance.Options.UseBackColor = true;
        button.Appearance.Options.UseForeColor = true;
        button.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
        return button;
    }

    private Control BuildProductsPanel(out FlowLayoutPanel kategoriPanel, out FlowLayoutPanel urunKartPanel)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(250, 250, 250),
            Padding = new Padding(14)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        kategoriPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = true,
            AutoScroll = false,
            Padding = new Padding(0, 4, 0, 0)
        };

        foreach (var kategori in new[]
                 {
                     "Sicak Icecekler",
                     "Soguk Icecekler",
                     "Firin",
                     "Tatli",
                     "Sandvic",
                     "Perakende"
                 })
        {
            kategoriPanel.Controls.Add(CreateKategoriButton(kategori));
        }

        urunKartPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = true,
            Padding = new Padding(0)
        };

        layout.Controls.Add(kategoriPanel, 0, 0);
        layout.Controls.Add(urunKartPanel, 0, 1);
        panel.Controls.Add(layout);
        return panel;
    }

    private SimpleButton CreateKategoriButton(string kategori)
    {
        var button = new SimpleButton
        {
            Text = kategori,
            Width = 160,
            Height = 42,
            Margin = new Padding(0, 0, 10, 10),
            Tag = kategori
        };
        button.Appearance.BackColor = Color.FromArgb(39, 76, 119);
        button.Appearance.ForeColor = Color.White;
        button.Appearance.BorderColor = Color.FromArgb(39, 76, 119);
        button.Appearance.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        button.Appearance.Options.UseBackColor = true;
        button.Appearance.Options.UseForeColor = true;
        button.Appearance.Options.UseBorderColor = true;
        button.AppearanceHovered.BackColor = Color.FromArgb(52, 152, 219);
        button.AppearanceHovered.ForeColor = Color.White;
        button.AppearanceHovered.BorderColor = Color.FromArgb(52, 152, 219);
        button.AppearanceHovered.Options.UseBackColor = true;
        button.AppearanceHovered.Options.UseForeColor = true;
        button.AppearanceHovered.Options.UseBorderColor = true;
        button.Click += KategoriButton_Click;
        return button;
    }

    private SimpleButton CreateUrunButton(UrunKartModel urun)
    {
        var button = new SimpleButton
        {
            Width = 170,
            Height = 108,
            Margin = new Padding(0, 0, 12, 12),
            Tag = urun,
            Text = $"{urun.UrunAdi}\r\n{urun.Fiyat:n2} TRY"
        };
        button.Appearance.BackColor = Color.White;
        button.Appearance.ForeColor = Color.FromArgb(45, 59, 72);
        button.Appearance.BorderColor = Color.FromArgb(214, 219, 224);
        button.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        button.Appearance.Options.UseBackColor = true;
        button.Appearance.Options.UseForeColor = true;
        button.Appearance.Options.UseBorderColor = true;
        button.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
        button.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
        button.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
        button.Click += UrunButton_Click;
        return button;
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            var urunler = await _hizliSatisService.GetUrunlerAsync();
            _urunler.Clear();
            _urunler.AddRange(urunler);
            RenderUrunButtons();
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show(ex.Message, "Urunler Yuklenemedi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RenderUrunButtons()
    {
        _urunKartPanel.SuspendLayout();
        _urunKartPanel.Controls.Clear();

        var list = string.IsNullOrWhiteSpace(_aktifKategori)
            ? _urunler
            : _urunler.Where(x => string.Equals(x.KategoriAdi, _aktifKategori, StringComparison.OrdinalIgnoreCase)).ToList();

        foreach (var urun in list)
        {
            _urunKartPanel.Controls.Add(CreateUrunButton(urun));
        }

        _urunKartPanel.ResumeLayout();
    }

    private void KategoriButton_Click(object? sender, EventArgs e)
    {
        if (sender is not SimpleButton button)
        {
            return;
        }

        var kategori = button.Tag?.ToString() ?? string.Empty;
        _aktifKategori = string.Equals(_aktifKategori, kategori, StringComparison.OrdinalIgnoreCase)
            ? null
            : kategori;

        foreach (SimpleButton item in _kategoriPanel.Controls)
        {
            var isActive = string.Equals(item.Tag?.ToString(), _aktifKategori, StringComparison.OrdinalIgnoreCase);
            item.Appearance.BackColor = isActive ? Color.FromArgb(52, 152, 219) : Color.FromArgb(39, 76, 119);
            item.Appearance.ForeColor = Color.White;
            item.Appearance.BorderColor = isActive ? Color.FromArgb(52, 152, 219) : Color.FromArgb(39, 76, 119);
        }

        RenderUrunButtons();
    }

    private void UrunButton_Click(object? sender, EventArgs e)
    {
        if (sender is not SimpleButton button || button.Tag is not UrunKartModel urun)
        {
            return;
        }

        AddOrUpdateSepetSatiri(new SepetSatirModel
        {
            UrunId = urun.UrunId,
            UrunAdi = urun.UrunAdi,
            Miktar = 1,
            BirimFiyat = urun.Fiyat,
            IndirimTutari = 0m,
            SatirNetToplam = urun.Fiyat
        });
    }

    private async void TxtBarkod_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }

        e.Handled = true;
        e.SuppressKeyPress = true;

        var barkodNo = _txtBarkod.Text.Trim();
        if (string.IsNullOrWhiteSpace(barkodNo))
        {
            return;
        }

        try
        {
            ToggleBusy(true);
            var satir = await _hizliSatisService.GetSepetSatiriByBarcodeAsync(barkodNo);
            if (satir is null)
            {
                XtraMessageBox.Show("Barkod ile urun bulunamadi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            AddOrUpdateSepetSatiri(satir);
            _txtBarkod.Text = string.Empty;
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show(ex.Message, "Barkod Hatasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            ToggleBusy(false);
            _txtBarkod.Focus();
        }
    }

    private void AddOrUpdateSepetSatiri(SepetSatirModel yeniSatir)
    {
        var mevcut = _sepetSatirlari.FirstOrDefault(x =>
            x.UrunId == yeniSatir.UrunId &&
            x.UrunVaryantId == yeniSatir.UrunVaryantId);

        if (mevcut is null)
        {
            RecalculateLine(yeniSatir);
            _sepetSatirlari.Add(yeniSatir);
        }
        else
        {
            mevcut.Miktar += yeniSatir.Miktar;
            RecalculateLine(mevcut);
            _gridView.RefreshData();
        }

        UpdateTotals();
    }

    private void RecalculateLine(SepetSatirModel satir)
    {
        if (satir.Miktar < 0)
        {
            satir.Miktar = 0;
        }

        satir.SatirNetToplam = (satir.Miktar * satir.BirimFiyat) - satir.IndirimTutari;
        if (satir.SatirNetToplam < 0)
        {
            satir.SatirNetToplam = 0;
        }
    }

    private void GridView_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
    {
        if (e.RowHandle < 0)
        {
            return;
        }

        var satir = _gridView.GetRow(e.RowHandle) as SepetSatirModel;
        if (satir is null)
        {
            return;
        }

        if (satir.Miktar <= 0)
        {
            satir.Miktar = 1;
        }

        RecalculateLine(satir);
        _gridView.RefreshRow(e.RowHandle);
        UpdateTotals();
    }

    private void GridView_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Delete)
        {
            return;
        }

        var satir = _gridView.GetFocusedRow() as SepetSatirModel;
        if (satir is null)
        {
            return;
        }

        _sepetSatirlari.Remove(satir);
        UpdateTotals();
    }

    private async void BtnBeklet_Click(object? sender, EventArgs e)
    {
        try
        {
            ToggleBusy(true);
            var siparisId = await _hizliSatisService.BekletAsync(_sepetSatirlari);
            _sepetSatirlari.Clear();
            UpdateTotals();
            XtraMessageBox.Show($"Satis beklemeye alindi. SiparisId: {siparisId}", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show(ex.Message, "Bekletilemedi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            ToggleBusy(false);
            _txtBarkod.Focus();
        }
    }

    private void UpdateTotals()
    {
        var araToplam = _sepetSatirlari.Sum(x => x.Miktar * x.BirimFiyat);
        var indirim = _sepetSatirlari.Sum(x => x.IndirimTutari);
        var toplam = _sepetSatirlari.Sum(x => x.SatirNetToplam);

        _lblAraToplam.Text = $"{araToplam:n2} TRY";
        _lblIndirim.Text = $"{indirim:n2} TRY";
        _lblToplam.Text = $"{toplam:n2} TRY";
    }

    private void ToggleBusy(bool isBusy)
    {
        UseWaitCursor = isBusy;
        Enabled = !isBusy;
    }

    private void PlaceholderClick(object? sender, EventArgs e)
    {
        XtraMessageBox.Show("Sonraki adimda yapilacak.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
