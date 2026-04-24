using PanoPos.Desktop.Forms;
using PanoPos.Desktop.Services;

namespace PanoPos.Desktop.Session;

public sealed class DesktopApplicationContext : ApplicationContext
{
    private readonly IAuthService _authService;

    public DesktopApplicationContext(IAuthService authService)
    {
        _authService = authService;
        ShowLoginForm();
    }

    private void ShowLoginForm()
    {
        var loginForm = new LoginForm(_authService);
        loginForm.LoginSucceeded += HandleLoginSucceeded;
        loginForm.FormClosed += HandleLoginFormClosed;
        MainForm = loginForm;
        loginForm.Show();
    }

    private void HandleLoginSucceeded(object? sender, EventArgs e)
    {
        if (sender is not LoginForm loginForm)
        {
            return;
        }

        loginForm.LoginSucceeded -= HandleLoginSucceeded;
        loginForm.FormClosed -= HandleLoginFormClosed;

        var mainForm = new MainForm(_authService, AppSession.Current);
        mainForm.LogoutCompleted += HandleLogoutCompleted;
        mainForm.FormClosed += HandleMainFormClosed;
        MainForm = mainForm;

        loginForm.Hide();
        mainForm.Show();
        loginForm.Dispose();
    }

    private void HandleLogoutCompleted(object? sender, EventArgs e)
    {
        if (sender is MainForm mainForm)
        {
            mainForm.LogoutCompleted -= HandleLogoutCompleted;
            mainForm.FormClosed -= HandleMainFormClosed;
            mainForm.Hide();
            mainForm.Dispose();
        }

        ShowLoginForm();
    }

    private void HandleLoginFormClosed(object? sender, FormClosedEventArgs e)
    {
        if (!AppSession.Current.IsAuthenticated)
        {
            ExitThread();
        }
    }

    private void HandleMainFormClosed(object? sender, FormClosedEventArgs e)
    {
        ExitThread();
    }
}
