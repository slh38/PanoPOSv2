using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Outbox;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Outbox;
using PanoPos.Infrastructure.Persistence;
using PanoPos.Infrastructure.Persistence.Seed;

namespace PanoPos.Tests.Outbox;

public sealed class OutboxServisiTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly PanoPosDbContext _dbContext;
    private readonly OutboxServisi _outboxServisi;

    public OutboxServisiTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<PanoPosDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new PanoPosDbContext(options);
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        _outboxServisi = new OutboxServisi(_dbContext);
    }

    [Fact]
    public async Task Olay_eklenir()
    {
        var olay = await _outboxServisi.OlayEkleAsync(new OutboxOlayEkleRequestDto
        {
            TenantId = SystemSeedData.TenantGuid,
            SubeId = 1,
            CihazId = 1,
            OlayTipi = "SiparisOlusturuldu",
            KaynakTablo = "Siparis",
            KaynakId = 10,
            PayloadJson = "{\"id\":10}"
        });

        Assert.True(olay.Id > 0);
        Assert.Equal(OutboxDurumu.Bekliyor, olay.Durum);
    }

    [Fact]
    public async Task Bekleyenler_filtrelenir()
    {
        await _outboxServisi.OlayEkleAsync(new OutboxOlayEkleRequestDto
        {
            TenantId = SystemSeedData.TenantGuid,
            SubeId = 1,
            CihazId = 1,
            OlayTipi = "SiparisOlusturuldu",
            KaynakTablo = "Siparis",
            KaynakId = 11,
            PayloadJson = "{\"id\":11}"
        });

        var hata = await _outboxServisi.OlayEkleAsync(new OutboxOlayEkleRequestDto
        {
            TenantId = SystemSeedData.TenantGuid,
            SubeId = 1,
            CihazId = 1,
            OlayTipi = "TahsilatOlusturuldu",
            KaynakTablo = "Tahsilat",
            KaynakId = 12,
            PayloadJson = "{\"id\":12}"
        });

        await _outboxServisi.HataIsaretleAsync(hata.Id, "Baglanti yok");

        var liste = await _outboxServisi.BekleyenleriListeleAsync(1, OutboxDurumu.Bekliyor, 1, 10);

        var kayit = Assert.Single(liste.Kayitlar);
        Assert.Equal(OutboxDurumu.Bekliyor, kayit.Durum);
        Assert.Equal("Siparis", kayit.KaynakTablo);
    }

    [Fact]
    public async Task Gonderildi_isaretlenir()
    {
        var olay = await _outboxServisi.OlayEkleAsync(new OutboxOlayEkleRequestDto
        {
            TenantId = SystemSeedData.TenantGuid,
            SubeId = 1,
            CihazId = 1,
            OlayTipi = "FaturaSiparistenOlusturuldu",
            KaynakTablo = "Fatura",
            KaynakId = 20,
            PayloadJson = "{\"id\":20}"
        });

        var gonderildi = await _outboxServisi.GonderildiIsaretleAsync(olay.Id);

        Assert.Equal(OutboxDurumu.Gonderildi, gonderildi.Durum);
        Assert.NotNull(gonderildi.GonderimTarihi);
    }

    [Fact]
    public async Task Hata_isaretlenir()
    {
        var olay = await _outboxServisi.OlayEkleAsync(new OutboxOlayEkleRequestDto
        {
            TenantId = SystemSeedData.TenantGuid,
            SubeId = 1,
            CihazId = 1,
            OlayTipi = "TahsilatOlusturuldu",
            KaynakTablo = "Tahsilat",
            KaynakId = 30,
            PayloadJson = "{\"id\":30}"
        });

        var hata = await _outboxServisi.HataIsaretleAsync(olay.Id, "Timeout");

        Assert.Equal(OutboxDurumu.Hata, hata.Durum);
        Assert.Equal(1, hata.DenemeSayisi);
        Assert.Equal("Timeout", hata.SonHataMesaji);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}
