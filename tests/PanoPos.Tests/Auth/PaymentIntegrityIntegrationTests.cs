using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using PanoPos.Application.Common;
using PanoPos.Application.Payment;
using PanoPos.Infrastructure.Persistence;
using Swashbuckle.AspNetCore.Swagger;

namespace PanoPos.Tests.Auth;

public sealed partial class SessionIntegrationTests
{
    [Fact]
    public void Odeme_swagger_anahtar_ve_fatura_filtresini_yayinlar()
    {
        var doc = app.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("v1");
        var schema = doc.Components.Schemas[nameof(TahsilatOlusturRequestDto)];
        Assert.Contains("islemAnahtari", schema.Required);
        Assert.Equal("uuid", schema.Properties["islemAnahtari"].Format);
        Assert.Contains(doc.Paths["/api/v1/tahsilat"].Operations[OperationType.Get].Parameters,
            p => p.Name == "faturaId" && p.In == ParameterLocation.Query);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Odeme_http_zorunlu_anahtar_ve_kapatma_dogrulanir(bool withEmptyKey)
    {
        await Login();
        var invoice = await Post("/api/v1/fatura/olustur-siparisten", new { SiparisId = await Order() });
        var id = invoice.GetProperty("id").GetInt64();
        object payload = withEmptyKey
            ? new { IslemAnahtari = Guid.Empty, FaturaId = id, OdemeTipi = 1, Tutar = 100, ParaBirimKodu = "TRY", Kur = 1, KasaId = 10 }
            : new { FaturaId = id, OdemeTipi = 1, Tutar = 100, ParaBirimKodu = "TRY", Kur = 1, KasaId = 10 };
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/tahsilat", payload)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync($"/api/v1/fatura/{id}/kapat", new { })).StatusCode);
        await using var db = new PanoPosDbContext(options);
        Assert.False(await db.Tahsilatlar.AnyAsync(x => x.FaturaId == id));
        Assert.Equal(0, (await db.Faturalar.SingleAsync(x => x.Id == id)).OdenenTutar);
    }

    [Fact]
    public async Task Odeme_http_tekrar_conflict_dagilim_ve_tenant_izolasyonu()
    {
        await Login();
        var invoice = await Post("/api/v1/fatura/olustur-siparisten", new { SiparisId = await Order() });
        var id = invoice.GetProperty("id").GetInt64();
        var key = Guid.NewGuid();
        var request = new { IslemAnahtari = key, FaturaId = id, OdemeTipi = 1, Tutar = 600, ParaBirimKodu = "TRY", Kur = 1, KasaId = 10 };
        var first = await Post("/api/v1/tahsilat", request);
        var repeated = await Post("/api/v1/tahsilat", request);
        Assert.Equal(first.GetProperty("id").GetInt64(), repeated.GetProperty("id").GetInt64());
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/v1/tahsilat",
            new { IslemAnahtari = key, FaturaId = id, OdemeTipi = 1, Tutar = 601, ParaBirimKodu = "TRY", Kur = 1, KasaId = 10 })).StatusCode);
        await Post("/api/v1/tahsilat", new
        {
            IslemAnahtari = Guid.NewGuid(),
            FaturaId = id,
            OdemeTipi = 2,
            Tutar = 400,
            ParaBirimKodu = "TRY",
            Kur = 1,
            BankaId = 10
        });
        var distribution = await client.GetFromJsonAsync<SayfaliSonucDto<TahsilatListeItemDto>>(
            $"/api/v1/tahsilat?subeId=1&faturaId={id}");
        Assert.Equal(2, distribution!.ToplamKayit);
        Assert.Equal(1000, distribution.Kayitlar.Sum(x => x.Tutar));
        var foreign = await client.GetFromJsonAsync<SayfaliSonucDto<TahsilatListeItemDto>>("/api/v1/tahsilat?subeId=1&faturaId=20");
        Assert.Empty(foreign!.Kayitlar);
        await Login("5678", 2);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync("/api/v1/tahsilat", request)).StatusCode);
        var scoped = await client.GetFromJsonAsync<SayfaliSonucDto<TahsilatListeItemDto>>($"/api/v1/tahsilat?subeId=2&faturaId={id}");
        Assert.Empty(scoped!.Kayitlar);
        await using var db = new PanoPosDbContext(options);
        Assert.Equal(1, await db.StokHareketleri.CountAsync(x => x.TenantId == tenantA));
        Assert.Equal(2, await db.Tahsilatlar.CountAsync(x => x.FaturaId == id));
    }
}

