using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Common;
using PanoPos.Domain.Entities;
using PanoPos.Infrastructure.Auth;
using PanoPos.Infrastructure.Persistence;
using PanoPos.Infrastructure.Persistence.Seed;

namespace PanoPos.Tests.Auth;

public sealed class AuthServisiTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly PanoPosDbContext _dbContext;
    private readonly AuthServisi _authServisi;

    public AuthServisiTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<PanoPosDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new PanoPosDbContext(options);
        _dbContext.Database.EnsureCreated();

        _authServisi = new AuthServisi(_dbContext, new PinHashServisi(), new BosAuthIslemLogServisi());
    }

    [Fact]
    public async Task Dogru_PIN_ile_login_basarili_olmali()
    {
        var sonuc = await _authServisi.LoginAsync("1234", 1);

        Assert.Equal(1, sonuc.KullaniciId);
        Assert.Equal(1, sonuc.CihazId);
        Assert.Equal(1, sonuc.VarsayilanSubeId);
        Assert.Contains("Admin", sonuc.Roller);
        Assert.Single(sonuc.Subeler);
    }

    [Fact]
    public async Task Yanlis_PIN_ile_login_basarisiz_olmali()
    {
        var ex = await Assert.ThrowsAsync<UygulamaHatasi>(() => _authServisi.LoginAsync("9999", 1));

        Assert.Equal(401, ex.StatusCode);
    }

    [Fact]
    public async Task Pasif_kullanici_giris_yapamamali()
    {
        var kullanici = await _dbContext.Kullanicilar.SingleAsync(x => x.Id == 1);
        kullanici.AktifMi = false;
        await _dbContext.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<UygulamaHatasi>(() => _authServisi.LoginAsync("1234", 1));

        Assert.Equal(403, ex.StatusCode);
        Assert.Equal("user_passive", ex.ErrorCode);
    }

    [Fact]
    public async Task Kilitli_kullanici_giris_yapamamali()
    {
        var kullanici = await _dbContext.Kullanicilar.SingleAsync(x => x.Id == 1);
        kullanici.KilitliMi = true;
        await _dbContext.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<UygulamaHatasi>(() => _authServisi.LoginAsync("1234", 1));

        Assert.Equal(403, ex.StatusCode);
        Assert.Equal("user_locked", ex.ErrorCode);
    }

    [Fact]
    public async Task Login_sonrasi_aktif_oturum_olusmali()
    {
        var sonuc = await _authServisi.LoginAsync("1234", 1);

        var oturum = await _dbContext.KullaniciOturumlari.SingleAsync(x => x.Id == sonuc.OturumId);

        Assert.True(oturum.AktifMi);
        Assert.Null(oturum.CikisTarihi);
    }

    [Fact]
    public async Task Yeni_login_eski_aktif_oturumu_kapatmali()
    {
        var eskiOturum = new KullaniciOturum
        {
            TenantId = SystemSeedData.TenantGuid,
            SubeId = 1,
            KullaniciId = 1,
            CihazId = 1,
            GirisTarihi = DateTime.UtcNow.AddMinutes(-10),
            AktifMi = true,
            SilindiMi = false
        };

        _dbContext.KullaniciOturumlari.Add(eskiOturum);
        await _dbContext.SaveChangesAsync();

        var yeniSonuc = await _authServisi.LoginAsync("1234", 1);

        var oturumlar = await _dbContext.KullaniciOturumlari
            .Where(x => x.KullaniciId == 1)
            .OrderBy(x => x.Id)
            .ToListAsync();

        Assert.Equal(2, oturumlar.Count);
        Assert.False(oturumlar[0].AktifMi);
        Assert.NotNull(oturumlar[0].CikisTarihi);
        Assert.Equal(yeniSonuc.OturumId, oturumlar[1].Id);
        Assert.True(oturumlar[1].AktifMi);
    }

    [Fact]
    public async Task Logout_aktif_oturumu_kapatmali()
    {
        var loginSonuc = await _authServisi.LoginAsync("1234", 1);

        await _authServisi.LogoutAsync(loginSonuc.OturumId);

        var oturum = await _dbContext.KullaniciOturumlari.SingleAsync(x => x.Id == loginSonuc.OturumId);
        Assert.False(oturum.AktifMi);
        Assert.NotNull(oturum.CikisTarihi);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}
