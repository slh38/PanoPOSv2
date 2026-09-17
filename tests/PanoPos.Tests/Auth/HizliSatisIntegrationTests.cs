using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PanoPos.Application.Common;
using PanoPos.Application.Order;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Tests.Auth;

public sealed partial class SessionIntegrationTests
{
    [Theory] [InlineData("beklet")] [InlineData("siparis")]
    public async Task Hizli_satis_HTTP_sahte_fiyat_toplam_kullanmaz_ve_toplu_satis_faturalasir(string action)
    {
        await Login();
        await using (var db = new PanoPosDbContext(options)) Assert.Empty(await db.Siparisler.Where(x => x.TenantId == tenantA).ToListAsync());
        var response = await client.PostAsJsonAsync("/api/v1/hizli-satis/" + action, new {
            CariId = 10, FiyatTipiId = 1, BelgeParaBirimKodu = "try", NetToplam = 0.01m, AraToplam = 0.01m,
            Satirlar = new[] { new { StokKartSatisBirimiId = 10, Miktar = 1, BirimFiyat = 0.01m, KdvTutari = 9999 } } });
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        var saved = (await response.Content.ReadFromJsonAsync<SiparisDto>())!;
        Assert.Equal(1000m, saved.NetToplam); Assert.Equal(1000m, saved.Detaylar[0].BirimFiyat);
        var list = (await client.GetFromJsonAsync<SayfaliSonucDto<BekleyenHizliSatisDto>>("/api/v1/hizli-satis/bekleyen?arama=Customer"))!;
        Assert.Equal(saved.Id, Assert.Single(list.Kayitlar).Id); Assert.Equal("Customer A", list.Kayitlar[0].CariAdi);
        var opened = (await client.GetFromJsonAsync<SiparisDto>($"/api/v1/hizli-satis/bekleyen/{saved.Id}"))!;
        var body = new { Surum = opened.Surum, FiyatTipiId = 1, BelgeParaBirimKodu = "TRY", GenelIndirimOrani = 10,
            NetToplam = 1, Satirlar = new[] { new { SiparisDetayId = opened.Detaylar[0].Id,
                StokKartSatisBirimiId = 10, Miktar = 2, IndirimOrani = 10, BirimFiyat = 0.01m } } };
        var updatedResponse = await client.PutAsJsonAsync($"/api/v1/hizli-satis/bekleyen/{saved.Id}", body);
        Assert.True(updatedResponse.IsSuccessStatusCode, await updatedResponse.Content.ReadAsStringAsync());
        var updated = (await updatedResponse.Content.ReadFromJsonAsync<SiparisDto>())!;
        Assert.Equal(1620m, updated.NetToplam);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PutAsJsonAsync($"/api/v1/hizli-satis/bekleyen/{saved.Id}", body)).StatusCode);
        var invoice = await Post("/api/v1/fatura/olustur-siparisten", new { SiparisId = saved.Id });
        Assert.Equal(1620m, invoice.GetProperty("netToplam").GetDecimal());
        Assert.Equal(HttpStatusCode.Conflict, (await client.GetAsync($"/api/v1/hizli-satis/bekleyen/{saved.Id}")).StatusCode);
    }

    [Theory] [InlineData("GET", "bekleyen")] [InlineData("POST", "beklet")]
    [InlineData("POST", "siparis")] [InlineData("GET", "bekleyen/1")] [InlineData("PUT", "bekleyen/1")]
    public async Task Hizli_satis_endpointleri_oturum_gerektirir(string method, string path)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), "/api/v1/hizli-satis/" + path);
        request.Content = JsonContent.Create(new { });
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.SendAsync(request)).StatusCode);
    }

    [Theory] [InlineData(20, false)] [InlineData(30, true)]
    public async Task Hizli_satis_HTTP_baska_tenant_sube_kayitlarini_gizler(long id, bool branch)
    {
        if (branch) {
            await using var db = new PanoPosDbContext(options);
            db.Add(new PanoPos.Domain.Entities.Siparis { Id = 30, TenantId = tenantA, SubeId = 3,
                SiparisNo = "A2", SiparisTipi = PanoPos.Domain.Enums.SiparisTipi.HizliSatisBekleyen,
                Durum = PanoPos.Domain.Enums.SiparisDurumu.Bekliyor, Kur = 1 }); await db.SaveChangesAsync();
        }
        await Login();
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/v1/hizli-satis/bekleyen/{id}")).StatusCode);
        var edit = new { Surum = Guid.NewGuid(), FiyatTipiId = 1, Satirlar = new[] { new { StokKartSatisBirimiId = 10, Miktar = 1 } } };
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync($"/api/v1/hizli-satis/bekleyen/{id}", edit)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync($"/api/v1/siparis/{id}/iptal", new { })).StatusCode);
    }

    [Fact] public void Hizli_satis_swagger_toplu_sozlesmeleri_yayinlar()
    {
        var doc = app.Services.GetRequiredService<Swashbuckle.AspNetCore.Swagger.ISwaggerProvider>().GetSwagger("v1");
        foreach (var path in new[] { "/api/v1/hizli-satis/beklet", "/api/v1/hizli-satis/siparis" })
            Assert.True(doc.Paths[path].Operations.ContainsKey(Microsoft.OpenApi.Models.OperationType.Post));
        Assert.True(doc.Paths["/api/v1/hizli-satis/bekleyen"].Operations.ContainsKey(Microsoft.OpenApi.Models.OperationType.Get));
        Assert.Equal(2, doc.Paths["/api/v1/hizli-satis/bekleyen/{id}"].Operations.Count);
        var props = doc.Components.Schemas[nameof(HizliSatisKaydetRequestDto)].Properties;
        Assert.Contains("surum", props.Keys); Assert.DoesNotContain("netToplam", props.Keys);
        Assert.DoesNotContain("birimFiyat", doc.Components.Schemas[nameof(HizliSatisSatirRequestDto)].Properties.Keys);
    }
}
