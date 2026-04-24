using DevExpress.XtraEditors;
using PanoPos.Desktop.Services;
using PanoPos.Desktop.Session;

namespace PanoPos.Desktop.Forms;

public sealed class MainForm : XtraForm
{
    private readonly IAuthService _authService;
    private readonly AppSession _session;
    private readonly LabelControl lblKullanici;
    private readonly LabelControl lblCihaz;
    private readonly LabelControl lblSube;
    private readonly SimpleButton btnCikis;

    public MainForm(IAuthService authService, AppSession session)
    {
        _authService = authService;
        _session = session;

        Text = "Pano POS";
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1000, 650);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(16)
        };

        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var headerPanel = new PanelControl
        {
            Dock = DockStyle.Fill
        };

        lblKullanici = new LabelControl { Location = new Point(20, 18) };
        lblKullanici.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);

        lblCihaz = new LabelControl { Location = new Point(20, 46) };
        lblCihaz.Appearance.Font = new Font("Segoe UI", 10F);

        lblSube = new LabelControl { Location = new Point(20, 68) };
        lblSube.Appearance.Font = new Font("Segoe UI", 10F);

        headerPanel.Controls.Add(lblKullanici);
        headerPanel.Controls.Add(lblCihaz);
        headerPanel.Controls.Add(lblSube);

        var menuPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            WrapContents = true,
            AutoScroll = true,
            Padding = new Padding(4)
        };

        menuPanel.Controls.Add(CreateMenuButton("Hizli Satis"));
        menuPanel.Controls.Add(CreateMenuButton("Restoran"));
        menuPanel.Controls.Add(CreateMenuButton("Urunler"));
        menuPanel.Controls.Add(CreateMenuButton("Cariler"));
        menuPanel.Controls.Add(CreateMenuButton("Faturalar"));
        menuPanel.Controls.Add(CreateMenuButton("Tahsilatlar"));
        menuPanel.Controls.Add(CreateMenuButton("Loglar"));
        menuPanel.Controls.Add(CreateMenuButton("Ayarlar"));

        btnCikis = CreateMenuButton("Cikis");
        btnCikis.Click -= HandlePlaceholderClick;
        btnCikis.Click += BtnCikis_Click;
        menuPanel.Controls.Add(btnCikis);

        root.Controls.Add(headerPanel, 0, 0);
        root.Controls.Add(menuPanel, 0, 1);

        Controls.Add(root);
        BindSession();
    }

    public event EventHandler? LogoutCompleted;

    private void BindSession()
    {
        lblKullanici.Text = $"AdSoyad: {_session.AdSoyad}";
        lblCihaz.Text = $"CihazId: {_session.CihazId}";
        lblSube.Text = $"SubeId: {_session.VarsayilanSubeId}";
    }

    private SimpleButton CreateMenuButton(string text)
    {
        var button = new SimpleButton
        {
            Text = text,
            Width = 180,
            Height = 90,
            Margin = new Padding(8)
        };
        button.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        button.Click += HandlePlaceholderClick;
        return button;
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
        btnCikis.Enabled = !isBusy;
        UseWaitCursor = isBusy;
    }
}
