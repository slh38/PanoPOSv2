using DevExpress.XtraEditors;
using PanoPos.Desktop.Services;

namespace PanoPos.Desktop.Forms;

public sealed class LoginForm : XtraForm
{
    private readonly IAuthService _authService;
    private readonly TextEdit txtPin;
    private readonly SimpleButton btnGiris;
    private readonly LabelControl lblDurum;
    private readonly List<SimpleButton> keypadButtons = [];

    public LoginForm(IAuthService authService)
    {
        _authService = authService;

        Text = "Pano POS - PIN Girisi";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(1040, 720);
        BackColor = Color.FromArgb(244, 127, 57);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(18),
            BackColor = Color.Transparent
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54));

        var leftPanel = BuildWelcomePanel();
        var rightPanel = BuildLoginPanel(out txtPin, out lblDurum, out btnGiris);

        root.Controls.Add(leftPanel, 0, 0);
        root.Controls.Add(rightPanel, 1, 0);

        Controls.Add(root);
    }

    public event EventHandler? LoginSucceeded;

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        txtPin.Focus();
    }

    private Panel BuildWelcomePanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(31, 66, 135),
            Margin = new Padding(0, 0, 16, 0),
            Padding = new Padding(34)
        };

        var accentTop = new Panel
        {
            Dock = DockStyle.Top,
            Height = 12,
            BackColor = Color.FromArgb(255, 205, 86)
        };

        var title = new LabelControl
        {
            Text = "PANO POS"
        };
        title.Appearance.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
        title.Appearance.ForeColor = Color.White;
        title.Location = new Point(34, 80);

        var subtitle = new LabelControl
        {
            Text = "Dokunmatik kasa deneyimi icin hizli PIN girisi"
        };
        subtitle.Appearance.Font = new Font("Segoe UI", 14F, FontStyle.Regular);
        subtitle.Appearance.ForeColor = Color.FromArgb(232, 240, 255);
        subtitle.Location = new Point(34, 145);

        var infoCard = new Panel
        {
            Size = new Size(340, 190),
            Location = new Point(34, 250),
            BackColor = Color.FromArgb(255, 255, 255)
        };

        var infoTitle = new LabelControl
        {
            Text = "Hizli Baslangic"
        };
        infoTitle.Appearance.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
        infoTitle.Appearance.ForeColor = Color.FromArgb(31, 66, 135);
        infoTitle.Location = new Point(22, 22);

        var infoText = new LabelControl
        {
            AutoSizeMode = LabelAutoSizeMode.Vertical,
            Text = "PIN alanina sadece rakam girilir. Sag taraftaki buyuk tus takimi dokunmatik ekranlar icin hazirlandi."
        };
        infoText.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
        infoText.Appearance.ForeColor = Color.FromArgb(72, 82, 102);
        infoText.Location = new Point(22, 64);
        infoText.Size = new Size(276, 90);

        infoCard.Controls.Add(infoTitle);
        infoCard.Controls.Add(infoText);

        panel.Controls.Add(accentTop);
        panel.Controls.Add(title);
        panel.Controls.Add(subtitle);
        panel.Controls.Add(infoCard);

        return panel;
    }

    private Panel BuildLoginPanel(out TextEdit pinEditor, out LabelControl statusLabel, out SimpleButton loginButton)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(255, 248, 242),
            Padding = new Padding(36)
        };

        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(36, 24, 36, 24)
        };

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6
        };
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 320));

        var header = new LabelControl
        {
            Text = "PIN ile Giris",
            Dock = DockStyle.Fill
        };
        header.Appearance.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
        header.Appearance.ForeColor = Color.FromArgb(40, 40, 40);

        var helper = new LabelControl
        {
            Text = "Kullanici PIN'inizi tuslayin",
            Dock = DockStyle.Fill
        };
        helper.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
        helper.Appearance.ForeColor = Color.FromArgb(112, 112, 112);

        pinEditor = new TextEdit
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 8, 0, 8)
        };
        pinEditor.Properties.UseSystemPasswordChar = true;
        pinEditor.Properties.MaxLength = 6;
        pinEditor.Properties.Appearance.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
        pinEditor.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
        pinEditor.KeyPress += TxtPin_KeyPress;
        pinEditor.KeyDown += TxtPin_KeyDown;

        loginButton = new SimpleButton
        {
            Text = "Giris Yap",
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 6, 0, 4)
        };
        loginButton.Appearance.BackColor = Color.FromArgb(26, 163, 114);
        loginButton.Appearance.ForeColor = Color.White;
        loginButton.Appearance.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        loginButton.Appearance.Options.UseBackColor = true;
        loginButton.Appearance.Options.UseForeColor = true;
        loginButton.Click += BtnGiris_Click;

        statusLabel = new LabelControl
        {
            Dock = DockStyle.Fill,
            AutoSizeMode = LabelAutoSizeMode.None
        };
        statusLabel.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        statusLabel.Appearance.ForeColor = Color.FromArgb(196, 55, 55);

        var keypad = BuildKeypad();
        keypad.Dock = DockStyle.Top;

        content.Controls.Add(header, 0, 0);
        content.Controls.Add(helper, 0, 1);
        content.Controls.Add(pinEditor, 0, 2);
        content.Controls.Add(loginButton, 0, 3);
        content.Controls.Add(statusLabel, 0, 4);
        content.Controls.Add(keypad, 0, 5);

        card.Controls.Add(content);
        panel.Controls.Add(card);

        return panel;
    }

    private TableLayoutPanel BuildKeypad()
    {
        var keypad = new TableLayoutPanel
        {
            RowCount = 4,
            ColumnCount = 3,
            Size = new Size(360, 320),
            BackColor = Color.Transparent
        };

        for (var i = 0; i < 3; i++)
        {
            keypad.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        }

        for (var i = 0; i < 4; i++)
        {
            keypad.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        }

        var keys = new[,]
        {
            { "1", "2", "3" },
            { "4", "5", "6" },
            { "7", "8", "9" },
            { "Temizle", "0", "Sil" }
        };

        for (var row = 0; row < 4; row++)
        {
            for (var col = 0; col < 3; col++)
            {
                var text = keys[row, col];
                var button = CreateKeypadButton(text);
                keypad.Controls.Add(button, col, row);
                keypadButtons.Add(button);
            }
        }

        return keypad;
    }

    private SimpleButton CreateKeypadButton(string text)
    {
        var button = new SimpleButton
        {
            Text = text,
            Dock = DockStyle.Fill,
            Margin = new Padding(6)
        };

        var isActionButton = text is "Temizle" or "Sil";
        button.Appearance.BackColor = isActionButton ? Color.FromArgb(255, 234, 222) : Color.FromArgb(255, 255, 255);
        button.Appearance.ForeColor = isActionButton ? Color.FromArgb(199, 91, 37) : Color.FromArgb(37, 52, 86);
        button.Appearance.BorderColor = Color.FromArgb(239, 216, 203);
        button.Appearance.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        button.Appearance.Options.UseBackColor = true;
        button.Appearance.Options.UseBorderColor = true;
        button.Appearance.Options.UseForeColor = true;
        button.Click += KeypadButton_Click;

        return button;
    }

    private void KeypadButton_Click(object? sender, EventArgs e)
    {
        if (sender is not SimpleButton button)
        {
            return;
        }

        lblDurum.Text = string.Empty;

        switch (button.Text)
        {
            case "Temizle":
                txtPin.Text = string.Empty;
                break;
            case "Sil":
                if (txtPin.Text.Length > 0)
                {
                    txtPin.Text = txtPin.Text[..^1];
                }
                break;
            default:
                if (txtPin.Text.Length < txtPin.Properties.MaxLength)
                {
                    txtPin.Text += button.Text;
                }
                break;
        }

        txtPin.SelectionStart = txtPin.Text.Length;
        txtPin.Focus();
    }

    private async void BtnGiris_Click(object? sender, EventArgs e)
    {
        await TryLoginAsync();
    }

    private void TxtPin_KeyPress(object? sender, KeyPressEventArgs e)
    {
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
        {
            e.Handled = true;
        }
    }

    private async void TxtPin_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }

        e.Handled = true;
        e.SuppressKeyPress = true;
        await TryLoginAsync();
    }

    private async Task TryLoginAsync()
    {
        lblDurum.Text = string.Empty;

        if (string.IsNullOrWhiteSpace(txtPin.Text))
        {
            lblDurum.Text = "PIN girmeniz gerekiyor.";
            XtraMessageBox.Show("PIN girmeniz gerekiyor.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtPin.Focus();
            return;
        }

        ToggleBusyState(true);

        try
        {
            await _authService.LoginAsync(txtPin.Text);
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            lblDurum.Text = ex.Message;
            XtraMessageBox.Show(ex.Message, "Giris Hatasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            txtPin.SelectAll();
            txtPin.Focus();
        }
        finally
        {
            ToggleBusyState(false);
        }
    }

    private void ToggleBusyState(bool isBusy)
    {
        txtPin.Enabled = !isBusy;
        btnGiris.Enabled = !isBusy;

        foreach (var button in keypadButtons)
        {
            button.Enabled = !isBusy;
        }

        UseWaitCursor = isBusy;
    }
}
