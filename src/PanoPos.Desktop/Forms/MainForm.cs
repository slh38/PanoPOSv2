using DevExpress.XtraEditors;
using PanoPos.Desktop.Services;
using PanoPos.Desktop.Session;

namespace PanoPos.Desktop.Forms;

public sealed class MainForm : XtraForm
{
    private readonly IAuthService _authService;
    private readonly IHizliSatisService _hizliSatisService;
    private readonly AppSession _session;
    private readonly List<SimpleButton> menuButtons = [];

    public MainForm(IAuthService authService, IHizliSatisService hizliSatisService, AppSession session)
    {
        _authService = authService;
        _hizliSatisService = hizliSatisService;
        _session = session;

        Text = "Pano POS";
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1280, 820);
        BackColor = Color.FromArgb(246, 246, 246);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 4,
            BackColor = Color.Transparent
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16F));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68F));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 110F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 560F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));

        var titlePanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent
        };

        var lblTitle = new LabelControl
        {
            Text = "Pano POS",
            Dock = DockStyle.Fill,
            AutoSizeMode = LabelAutoSizeMode.None
        };
        lblTitle.Appearance.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
        lblTitle.Appearance.ForeColor = Color.FromArgb(44, 62, 80);
        lblTitle.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
        lblTitle.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
        titlePanel.Controls.Add(lblTitle);

        var tileHost = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent
        };

        var grid = new TableLayoutPanel
        {
            Size = new Size(960, 540),
            BackColor = Color.Transparent,
            ColumnCount = 3,
            RowCount = 3,
            Anchor = AnchorStyles.None,
            Margin = new Padding(0)
        };

        for (var i = 0; i < 3; i++)
        {
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320F));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 180F));
        }

        grid.Controls.Add(CreateTileCard("Hizli Satis", Color.FromArgb(137, 54, 226), Color.FromArgb(168, 93, 238)), 0, 0);
        grid.Controls.Add(CreateTileCard("Restoran", Color.FromArgb(229, 27, 80), Color.FromArgb(242, 78, 120)), 1, 0);
        grid.Controls.Add(CreateTileCard("Urunler", Color.FromArgb(58, 149, 214), Color.FromArgb(94, 174, 231)), 2, 0);
        grid.Controls.Add(CreateTileCard("Cariler", Color.FromArgb(255, 132, 44), Color.FromArgb(255, 163, 96)), 0, 1);
        grid.Controls.Add(CreateTileCard("Faturalar", Color.FromArgb(55, 198, 107), Color.FromArgb(97, 219, 140)), 1, 1);
        grid.Controls.Add(CreateTileCard("Tahsilatlar", Color.FromArgb(255, 96, 73), Color.FromArgb(255, 132, 111)), 2, 1);
        grid.Controls.Add(CreateTileCard("Loglar", Color.FromArgb(105, 105, 105), Color.FromArgb(131, 131, 131)), 0, 2);
        grid.Controls.Add(CreateTileCard("Ayarlar", Color.FromArgb(161, 161, 229), Color.FromArgb(188, 188, 241)), 1, 2);

        var btnCikis = CreateTileCard("Cikis", Color.FromArgb(58, 74, 94), Color.FromArgb(80, 98, 121));
        btnCikis.Click -= HandlePlaceholderClick;
        btnCikis.Click += BtnCikis_Click;
        grid.Controls.Add(btnCikis, 2, 2);

        tileHost.Controls.Add(grid);
        tileHost.Resize += (_, _) =>
        {
            grid.Left = Math.Max(0, (tileHost.ClientSize.Width - grid.Width) / 2);
            grid.Top = Math.Max(0, (tileHost.ClientSize.Height - grid.Height) / 2);
        };

        var footerPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 0, 18, 14)
        };

        var lblFooter = new LabelControl
        {
            Text = $"Kullanici: {_session.AdSoyad}   |   Sube: {_session.Subeler.FirstOrDefault(x => x.SubeId == _session.VarsayilanSubeId)?.Ad ?? _session.VarsayilanSubeId.ToString()}",
            Dock = DockStyle.Fill,
            AutoSizeMode = LabelAutoSizeMode.None
        };
        lblFooter.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
        lblFooter.Appearance.ForeColor = Color.FromArgb(98, 98, 98);
        lblFooter.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
        lblFooter.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
        footerPanel.Controls.Add(lblFooter);

        root.Controls.Add(titlePanel, 1, 0);
        root.Controls.Add(tileHost, 1, 1);
        root.Controls.Add(footerPanel, 1, 3);

        Controls.Add(root);
    }

    public event EventHandler? LogoutCompleted;

    private SimpleButton CreateTileCard(string text, Color baseColor, Color accentColor)
    {
        var button = new SimpleButton
        {
            Text = text,
            Dock = DockStyle.Fill,
            Margin = new Padding(16),
            Padding = new Padding(18)
        };

        button.Appearance.BackColor = baseColor;
        button.Appearance.ForeColor = Color.White;
        button.Appearance.BorderColor = ControlPaint.Dark(baseColor);
        button.Appearance.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
        button.Appearance.Options.UseBackColor = true;
        button.Appearance.Options.UseForeColor = true;
        button.Appearance.Options.UseBorderColor = true;
        button.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
        button.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;

        button.AppearanceHovered.BackColor = accentColor;
        button.AppearanceHovered.ForeColor = Color.White;
        button.AppearanceHovered.BorderColor = accentColor;
        button.AppearanceHovered.Options.UseBackColor = true;
        button.AppearanceHovered.Options.UseForeColor = true;
        button.AppearanceHovered.Options.UseBorderColor = true;

        button.AppearancePressed.BackColor = ControlPaint.Dark(baseColor);
        button.AppearancePressed.ForeColor = Color.White;
        button.AppearancePressed.BorderColor = ControlPaint.Dark(baseColor);
        button.AppearancePressed.Options.UseBackColor = true;
        button.AppearancePressed.Options.UseForeColor = true;
        button.AppearancePressed.Options.UseBorderColor = true;

        button.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
        button.Click += MenuTile_Click;

        menuButtons.Add(button);
        return button;
    }

    private void MenuTile_Click(object? sender, EventArgs e)
    {
        if (sender is not SimpleButton button)
        {
            return;
        }

        if (string.Equals(button.Text, "Hizli Satis", StringComparison.OrdinalIgnoreCase))
        {
            using var form = new HizliSatisForm(_hizliSatisService, _session);
            form.ShowDialog(this);
            return;
        }

        HandlePlaceholderClick(sender, e);
    }

    private void HandlePlaceholderClick(object? sender, EventArgs e)
    {
        XtraMessageBox.Show(
            "Bu ekran sonraki adimda yapilacak.",
            "Bilgi",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private async void BtnCikis_Click(object? sender, EventArgs e)
    {
        ToggleLogoutState(true);

        try
        {
            await _authService.LogoutAsync();
            LogoutCompleted?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            XtraMessageBox.Show(ex.Message, "Cikis Hatasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            ToggleLogoutState(false);
        }
    }

    private void ToggleLogoutState(bool isBusy)
    {
        foreach (var button in menuButtons)
        {
            button.Enabled = !isBusy;
        }

        UseWaitCursor = isBusy;
    }
}
