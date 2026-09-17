using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using PanoPos.Application;
using PanoPos.Application.Auth;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure;
using PanoPos.Infrastructure.Auth;
using PanoPos.Infrastructure.Persistence;
using PanoPos.Infrastructure.Persistence.Seed;
using PanoPos.WebApi.Controllers;
using PanoPos.WebApi.Extensions;
using Serilog;

namespace PanoPos.Tests.Auth;

public sealed partial class SessionIntegrationTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private readonly Guid tenantA = SystemSeedData.TenantGuid;
    private readonly Guid tenantB = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private DbContextOptions<PanoPosDbContext> options = null!;
    private WebApplication app = null!;
    private HttpClient client = null!;

    public async Task InitializeAsync()
    {
        await connection.OpenAsync();
        options = new DbContextOptionsBuilder<PanoPosDbContext>().UseSqlite(connection).Options;
        await using (var db = new PanoPosDbContext(options))
        {
            await db.Database.EnsureCreatedAsync();
            db.AddRange(
                new Tenant { Id = 2, TenantId = tenantB, SubeId = 2, Ad = "B", Kod = "B" },
                new Sube { Id = 2, TenantId = tenantB, SubeId = 2, Ad = "B", Kod = "B" },
                new Sube { Id = 3, TenantId = tenantA, SubeId = 3, Ad = "A2", Kod = "A2" },
                new Cihaz { Id = 4, TenantId = tenantA, SubeId = 1, Ad = "POS4", Kod = "POS4" },
                new Cihaz { Id = 2, TenantId = tenantB, SubeId = 2, Ad = "B", Kod = "B" },
                new Kullanici { Id = 2, TenantId = tenantB, SubeId = 2, Ad = "B", Soyad = "User", PinHash = new PinHashServisi().Hashle("5678") },
                new KullaniciSube { Id = 2, TenantId = tenantB, SubeId = 2, KullaniciId = 2, BagliSubeId = 2 },
                new Depo { Id = 2, TenantId = tenantB, SubeId = 2, DepoKodu = "MERKEZ", Ad = "B", VarsayilanMi = true },
                new Depo { Id = 3, TenantId = tenantA, SubeId = 3, DepoKodu = "MERKEZ", Ad = "A2", VarsayilanMi = true },
                new Kdv { Id = 20, TenantId = tenantB, SubeId = 2, Kod = "B", Ad = "B", Oran = 0, AktifMi = true },
                new StokKategori { Id = 20, TenantId = tenantB, SubeId = 2, Kod = "B", Ad = "B" },
                new StokGrup { Id = 20, TenantId = tenantB, SubeId = 2, Kod = "B", Ad = "B" },
                new Renk { Id = 20, TenantId = tenantB, SubeId = 2, Kod = "B", Ad = "B" },
                new StokKartVaryant { Id = 20, TenantId = tenantB, SubeId = 2, StokKartId = 20, RenkId = 20, VaryantKodu = "B" },
                new FiyatTipi { Id = 20, TenantId = tenantB, SubeId = 2, Kod = "B", Ad = "B" },
                new StokKartFiyat { Id = 20, TenantId = tenantB, SubeId = 2, StokKartSatisBirimiId = 20, FiyatTipiId = 20, Fiyat = 10, ParaBirimKodu = "TRY" },
                new StokKartFiyat { Id = 10, TenantId = tenantA, SubeId = 1, StokKartSatisBirimiId = 10, FiyatTipiId = 1, Fiyat = 1000, ParaBirimKodu = "TRY" },
                new StokKart { Id = 10, TenantId = tenantA, SubeId = 1, StokKartKodu = "A", Ad = "Stock A", KdvId = 1 },
                new StokKart { Id = 20, TenantId = tenantB, SubeId = 2, StokKartKodu = "B", Ad = "Stock B", KdvId = 20 },
                new StokKartSatisBirimi { Id = 10, TenantId = tenantA, SubeId = 1, StokKartId = 10, BirimKodu = "ADET", BirimAdi = "Adet", Katsayi = 1, VarsayilanMi = true },
                new StokKartSatisBirimi { Id = 20, TenantId = tenantB, SubeId = 2, StokKartId = 20, BirimKodu = "ADET", BirimAdi = "Adet", Katsayi = 1, VarsayilanMi = true },
                new Barkod { TenantId = tenantA, SubeId = 1, StokKartId = 10, StokKartSatisBirimiId = 10, BarkodNo = "SAME", BarkodTipi = BarkodTipi.Ean },
                new Barkod { TenantId = tenantB, SubeId = 2, StokKartId = 20, StokKartSatisBirimiId = 20, BarkodNo = "SAME", BarkodTipi = BarkodTipi.Ean },
                new CariKart { Id = 10, TenantId = tenantA, SubeId = 1, Ad = "Customer A", CariKodu = "A", Tip = CariTipi.Alici },
                new CariKart { Id = 20, TenantId = tenantB, SubeId = 2, Ad = "Customer B", CariKodu = "B", Tip = CariTipi.Alici },
                new Kasa { Id = 10, TenantId = tenantA, SubeId = 1, Ad = "A" },
                new Kasa { Id = 30, TenantId = tenantA, SubeId = 3, Ad = "A2" },
                new Banka { Id = 10, TenantId = tenantA, SubeId = 1, Ad = "A", Kod = "A" },
                new Fatura { Id = 20, TenantId = tenantB, SubeId = 2, DepoId = 2, FaturaNo = "B", ParaBirimKodu = "TRY", Kur = 1, NetToplam = 1000, KalanTutar = 1000, Durum = FaturaDurumu.Acik },
                new Siparis { Id = 20, TenantId = tenantB, SubeId = 2, SiparisNo = "B", ParaBirimKodu = "TRY", Kur = 1, SiparisTipi = SiparisTipi.HizliSatisBekleyen, Durum = SiparisDurumu.Bekliyor },
                new Tahsilat { Id = 20, TenantId = tenantB, SubeId = 2, FaturaId = 20, TahsilatFisNo = "B", Tutar = 1, ParaBirimKodu = "TRY", Kur = 1, OdemeTipi = OdemeTipi.Nakit },
                new StokFis { Id = 20, TenantId = tenantB, SubeId = 2, DepoId = 2, FisNo = "B", FisTarihi = DateTime.UtcNow, StokFisTipi = StokFisTipi.Devir });
            await db.SaveChangesAsync();
        }

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.Logging.ClearProviders();
        builder.Host.UseSerilog((_, config) => config.MinimumLevel.Fatal());
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:PanoPos"] = "unused" });
        builder.Services.AddApplication().AddInfrastructure(builder.Configuration).AddWebApiServices();
        builder.Services.RemoveAll<PanoPosDbContext>();
        builder.Services.AddScoped(sp => new PanoPosDbContext(options, sp.GetRequiredService<IIslemBaglami>()));
        builder.Services.AddControllers().AddApplicationPart(typeof(AuthController).Assembly).AddApplicationPart(typeof(BaglamProbeController).Assembly);
        app = builder.Build();
        app.Urls.Add("http://127.0.0.1:0");
        app.UseWebApiPipeline();
        await app.StartAsync();
        client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
    }

    [Theory]
    [InlineData("/api/v1/satis/barkod/SAME?fiyatTipiId=1")]
    [InlineData("/api/v1/satis/fiyat?stokKartSatisBirimiId=10&fiyatTipiId=1")]
    [InlineData("/api/v1/fiyat-tipi")]
    [InlineData("/api/v1/stok-kart/10/satis-birimleri")]
    public async Task Satis_lookup_endpointleri_token_gerektirir(string path)
        => Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(path)).StatusCode);

    [Theory] [InlineData("1234", 4, 1, 10, 1000)] [InlineData("5678", 2, 20, 20, 10)]
    public async Task Satis_lookup_ayni_barkodu_oturum_tenantindan_cozer(string pin, long device, long type, long stock, decimal price)
    {
        await Login(pin, device);
        var response = await client.GetAsync($"/api/v1/satis/barkod/SAME?fiyatTipiId={type}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var data = (await response.Content.ReadFromJsonAsync<PanoPos.Application.Product.SatisStokDto>())!;
        Assert.Equal(stock, data.StokKartId); Assert.Equal(price, data.Fiyat);
        var manual = await client.GetFromJsonAsync<PanoPos.Application.Product.SatisStokDto>($"/api/v1/satis/fiyat?stokKartSatisBirimiId={stock}&fiyatTipiId={type}");
        Assert.Equal(data.Fiyat, manual!.Fiyat);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/fiyat-tipi")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/v1/stok-kart/{stock}/satis-birimleri")).StatusCode);
    }

    [Fact] public async Task Satis_lookup_baska_tenant_birimini_cozemez()
    {
        await Login();
        Assert.False((await client.GetAsync("/api/v1/satis/fiyat?stokKartSatisBirimiId=20&fiyatTipiId=20")).IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/v1/stok-kart/20/satis-birimleri")).StatusCode);
    }

    [Theory] [InlineData(true)] [InlineData(false)]
    public async Task Koli_barkodu_doviz_kdv_snapshot_stok_ve_perakende_kart_tam_akis(bool included)
    {
        await using (var setup = new PanoPosDbContext(options))
        {
            var price = await setup.StokKartFiyatlari.SingleAsync(x => x.Id == 10);
            price.Fiyat = 10; price.ParaBirimKodu = " usd ";
            (await setup.StokKartler.SingleAsync(x => x.Id == 10)).KdvId = 4;
            var unit = await setup.StokKartSatisBirimleri.SingleAsync(x => x.Id == 10);
            unit.BirimKodu = "KOLI"; unit.BirimAdi = "Koli"; unit.Katsayi = 24;
            (await setup.TenantAyarlari.SingleAsync(x => x.TenantId == tenantA)).SatisFiyatlariKdvDahilMi = included;
            await setup.SaveChangesAsync();
        }
        await Login();
        var types = (await client.GetFromJsonAsync<PanoPos.Application.Common.SayfaliSonucDto<PanoPos.Application.Product.FiyatTipiListeDto>>("/api/v1/fiyat-tipi"))!;
        var type = Assert.Single(types.Kayitlar, x => x.Id == 1);
        var lookup = (await client.GetFromJsonAsync<PanoPos.Application.Product.SatisStokDto>($"/api/v1/satis/barkod/SAME?fiyatTipiId={type.Id}"))!;
        Assert.Equal(10, lookup.Fiyat); Assert.Equal("USD", lookup.FiyatParaBirimKodu); Assert.Equal(24, lookup.Katsayi);
        var created = await Post("/api/v1/siparis", new { SiparisTipi = 2, ParaBirimKodu = " try ", Kur = 1 });
        var id = created.GetProperty("id").GetInt64();
        await Post($"/api/v1/siparis/{id}/satir", new {
            lookup.StokKartId, lookup.StokKartSatisBirimiId, FiyatTipiId = type.Id,
            Miktar = 1, BirimFiyat = 0.01m, FiyatParaBirimKodu = "EUR", FiyatKur = 42.50m });
        var order = (await client.GetFromJsonAsync<PanoPos.Application.Order.SiparisDto>($"/api/v1/siparis/{id}"))!;
        var line = Assert.Single(order.Detaylar);
        Assert.Equal("TRY", order.ParaBirimKodu); Assert.Equal(425m, line.BirimFiyat);
        Assert.Equal("USD", line.FiyatParaBirimKodu); Assert.Equal(42.50m, line.FiyatKur);
        Assert.Equal(type.Id, line.FiyatTipiId); Assert.Equal(24, line.BirimKatsayi);
        Assert.Equal(20, line.KdvOrani); Assert.Equal(included, line.KdvDahilMi);
        Assert.Equal(included ? 354.17m : 425m, line.Matrah);
        Assert.Equal(included ? 70.83m : 85m, line.KdvTutari);
        Assert.Equal(included ? 425m : 510m, order.NetToplam);
        await using (var setup = new PanoPosDbContext(options))
        {
            (await setup.StokKartFiyatlari.SingleAsync(x => x.Id == 10)).Fiyat = 999;
            (await setup.Kdvler.SingleAsync(x => x.Id == 4)).Oran = 25;
            (await setup.StokKartSatisBirimleri.SingleAsync(x => x.Id == 10)).Katsayi = 48;
            await setup.SaveChangesAsync();
        }
        var unchanged = (await client.GetFromJsonAsync<PanoPos.Application.Order.SiparisDto>($"/api/v1/siparis/{id}"))!;
        Assert.Equal(line.BirimFiyat, unchanged.Detaylar[0].BirimFiyat);
        Assert.Equal(line.KdvOrani, unchanged.Detaylar[0].KdvOrani);
        var invoiceJson = await Post("/api/v1/fatura/olustur-siparisten", new { SiparisId = id });
        var invoiceId = invoiceJson.GetProperty("id").GetInt64();
        var invoice = (await client.GetFromJsonAsync<PanoPos.Application.Invoice.FaturaDto>($"/api/v1/fatura/{invoiceId}"))!;
        var detail = Assert.Single(invoice.Detaylar);
        Assert.Equal(line.BirimFiyat, detail.BirimFiyat); Assert.Equal(line.FiyatKur, detail.FiyatKur);
        Assert.Equal(line.FiyatParaBirimKodu, detail.FiyatParaBirimKodu); Assert.Equal(line.FiyatTipiId, detail.FiyatTipiId);
        Assert.Equal(line.KdvOrani, detail.KdvOrani); Assert.Equal(line.KdvDahilMi, detail.KdvDahilMi);
        Assert.Equal(line.KdvTutari, detail.KdvTutari); Assert.Equal(line.Matrah, detail.Matrah);
        Assert.Equal(line.BirimKatsayi, detail.BirimKatsayi);
        await Post("/api/v1/tahsilat", new { FaturaId = invoiceId, OdemeTipi = OdemeTipi.KrediKarti,
            BankaId = 10, Tutar = invoice.NetToplam, ParaBirimKodu = "TRY", Kur = 1 });
        var paid = (await client.GetFromJsonAsync<PanoPos.Application.Invoice.FaturaDto>($"/api/v1/fatura/{invoiceId}"))!;
        Assert.Equal(0, paid.KalanTutar); Assert.Equal(425, paid.Detaylar[0].BirimFiyat);
        Assert.Equal(1, paid.Detaylar[0].FiyatTipiId);
        await using var verify = new PanoPosDbContext(options);
        var fis = await verify.StokFisleri.SingleAsync(x => x.FaturaId == invoiceId);
        var movement = await verify.StokHareketleri.SingleAsync(x => x.StokFisId == fis.Id);
        Assert.Equal(-24, movement.Miktar);
    }

    [Theory] [InlineData("1234", 4, 10, 20)] [InlineData("5678", 2, 20, 10)]
    public async Task Kategori_filtreli_katalog_oturum_tenantini_asamaz(string pin, long device, long own, long foreign)
    {
        await using (var setup = new PanoPosDbContext(options))
        {
            setup.Add(new StokKategori { Id = 10, TenantId = tenantA, SubeId = 1, Kod = "A", Ad = "A" });
            (await setup.StokKartler.SingleAsync(x => x.Id == 10)).StokKategoriId = 10;
            (await setup.StokKartler.SingleAsync(x => x.Id == 20)).StokKategoriId = 20;
            await setup.SaveChangesAsync();
        }
        await Login(pin, device);
        var ownResult = (await client.GetFromJsonAsync<PanoPos.Application.Common.SayfaliSonucDto<PanoPos.Application.Product.StokKartListeItemDto>>($"/api/v1/stok-kart?kategoriId={own}&arama=Stock&page=1&pageSize=1&aktifMi=true"))!;
        Assert.Equal(1, ownResult.ToplamKayit); Assert.Equal(own, Assert.Single(ownResult.Kayitlar).Id);
        var foreignResult = (await client.GetFromJsonAsync<PanoPos.Application.Common.SayfaliSonucDto<PanoPos.Application.Product.StokKartListeItemDto>>($"/api/v1/stok-kart?kategoriId={foreign}&page=1&pageSize=1"))!;
        Assert.Equal(0, foreignResult.ToplamKayit); Assert.Empty(foreignResult.Kayitlar);
    }

    [Fact] public void Satis_swagger_yollari_ve_filtreleri_yayinlanir()
    {
        var swagger = app.Services.GetRequiredService<Swashbuckle.AspNetCore.Swagger.ISwaggerProvider>().GetSwagger("v1");
        foreach (var path in new[] { "/api/v1/satis/barkod/{barkodNo}", "/api/v1/satis/fiyat", "/api/v1/fiyat-tipi", "/api/v1/stok-kart/{id}/satis-birimleri" })
            Assert.True(swagger.Paths[path].Operations.ContainsKey(Microsoft.OpenApi.Models.OperationType.Get));
        var lookup = swagger.Paths["/api/v1/satis/fiyat"].Operations[Microsoft.OpenApi.Models.OperationType.Get];
        Assert.Contains(lookup.Parameters, x => x.Name == "fiyatTipiId");
        Assert.Contains(lookup.Parameters, x => x.Name == "stokKartSatisBirimiId");
        Assert.DoesNotContain(lookup.Parameters, x => x.Name == "tenantId");
        var list = swagger.Paths["/api/v1/stok-kart"].Operations[Microsoft.OpenApi.Models.OperationType.Get];
        foreach (var name in new[] { "kategoriId", "grupId", "arama", "page", "pageSize" })
            Assert.Contains(list.Parameters, x => x.Name == name);
    }

    private async Task<LoginResponseDto> Login(string pin = "1234", long device = 4)
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { Pin = pin, CihazId = device });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var login = (await response.Content.ReadFromJsonAsync<LoginResponseDto>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.OturumToken);
        return login;
    }

    private async Task<JsonElement> Post(string url, object value)
    {
        var response = await client.PostAsJsonAsync(url, value);
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private async Task<long> Order()
    {
        var order = await Post("/api/v1/siparis", new { SiparisTipi = 2, ParaBirimKodu = "TRY", Kur = 1 });
        var id = order.GetProperty("id").GetInt64();
        await Post($"/api/v1/siparis/{id}/satir", new { StokKartId = 10, StokKartSatisBirimiId = 10, FiyatTipiId = 1, Miktar = 1, BirimFiyat = 1000 });
        return id;
    }

    [Fact]
    public async Task Public_login_token_hash_ve_dogrulanmis_baglam()
    {
        var login = await Login();
        Assert.Equal(64, login.OturumToken.Length);
        Assert.Equal(tenantA, login.TenantId);
        Assert.Equal(1, login.SubeId);
        await using var db = new PanoPosDbContext(options);
        var stored = await db.KullaniciOturumlari.SingleAsync();
        Assert.NotEqual(login.OturumToken, stored.OturumTokenHash);
        Assert.Equal(64, stored.OturumTokenHash!.Length);
        var ctx = await client.GetFromJsonAsync<JsonElement>("/api/test/baglam");
        Assert.Equal(tenantA, ctx.GetProperty("tenantId").GetGuid());
        Assert.Equal(1, ctx.GetProperty("subeId").GetInt64());
        Assert.Equal(1, ctx.GetProperty("kullaniciId").GetInt64());
        Assert.Equal(4, ctx.GetProperty("cihazId").GetInt64());
        Assert.Equal(login.OturumId, ctx.GetProperty("kullaniciOturumId").GetInt64());
    }

    [Theory]
    [InlineData("stok-kart")] [InlineData("barkod/SAME")] [InlineData("stok-kategori")]
    [InlineData("stok-grup")] [InlineData("kdv")] [InlineData("carikart")]
    [InlineData("siparis")] [InlineData("fatura")] [InlineData("tahsilat")]
    [InlineData("depo")] [InlineData("stok-fis")] [InlineData("kasa")] [InlineData("banka")]
    [InlineData("vardiya/aktif")] [InlineData("stok/miktar")] [InlineData("stok-maliyet")]
    public async Task Ticari_endpoint_tokensiz_401(string path)
    {
        var response = await client.GetAsync("/api/v1/" + path);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains("session_invalid", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Gecersiz_token_401()
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", new string('A', 64));
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/kasa")).StatusCode);
    }

    [Fact]
    public async Task Logout_bodydeki_oturumu_degil_tokeni_kapatir()
    {
        var login = await Login();
        var response = await client.PostAsJsonAsync("/api/v1/auth/logout", new { KullaniciOturumId = 999 });
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/kasa")).StatusCode);
        await using var db = new PanoPosDbContext(options);
        Assert.False((await db.KullaniciOturumlari.SingleAsync(x => x.Id == login.OturumId)).AktifMi);
    }

    [Theory]
    [InlineData("user")] [InlineData("locked")] [InlineData("branch")] [InlineData("device")]
    [InlineData("tenant")] [InlineData("permission")] [InlineData("deleted")] [InlineData("device-branch")]
    public async Task Oturum_baglantisi_bozulursa_401(string reason)
    {
        await Login();
        await using (var db = new PanoPosDbContext(options))
        {
            switch (reason)
            {
                case "user": (await db.Kullanicilar.SingleAsync(x => x.Id == 1)).AktifMi = false; break;
                case "locked": (await db.Kullanicilar.SingleAsync(x => x.Id == 1)).KilitliMi = true; break;
                case "deleted": db.Kullanicilar.Remove(await db.Kullanicilar.SingleAsync(x => x.Id == 1)); break;
                case "branch": (await db.Subeler.SingleAsync(x => x.Id == 1)).AktifMi = false; break;
                case "device": (await db.Cihazlar.SingleAsync(x => x.Id == 4)).AktifMi = false; break;
                case "tenant": (await db.Tenantler.SingleAsync(x => x.Id == 1)).AktifMi = false; break;
                case "permission": (await db.KullaniciSubeleri.SingleAsync(x => x.KullaniciId == 1)).AktifMi = false; break;
                case "device-branch": (await db.Cihazlar.SingleAsync(x => x.Id == 4)).SubeId = 3; break;
            }
            await db.SaveChangesAsync();
        }
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/kasa")).StatusCode);
    }

    [Fact]
    public async Task Barkod_iki_tenantta_ayni_olabilir_lookup_izole()
    {
        await Login();
        Assert.Equal(10, (await client.GetFromJsonAsync<JsonElement>("/api/v1/barkod/SAME")).GetProperty("stokKartId").GetInt64());
        await Login("5678", 2);
        Assert.Equal(20, (await client.GetFromJsonAsync<JsonElement>("/api/v1/barkod/SAME")).GetProperty("stokKartId").GetInt64());
    }

    [Theory]
    [InlineData("stok-kart", 1)] [InlineData("carikart", 1)]
    [InlineData("siparis", 0)] [InlineData("fatura", 0)] [InlineData("tahsilat", 0)]
    [InlineData("depo", 1)] [InlineData("stok-fis", 0)]
    public async Task Dapper_listeleri_oturum_tenant_sube_izolasyonlu(string path, int count)
    {
        await Login();
        var result = await client.GetFromJsonAsync<JsonElement>("/api/v1/" + path);
        Assert.Equal(count, result.GetProperty("toplamKayit").GetInt32());
        Assert.DoesNotContain(result.GetProperty("kayitlar").EnumerateArray(), x => x.GetProperty("id").GetInt64() == 20);
    }

    [Theory]
    [InlineData("stok-kart/20")] [InlineData("carikart/20")] [InlineData("siparis/20")]
    [InlineData("fatura/20")] [InlineData("tahsilat/20")] [InlineData("depo/2")]
    public async Task Baska_tenant_id_okunamaz(string path)
    {
        await Login();
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/v1/" + path)).StatusCode);
    }

    [Fact]
    public async Task Client_baska_sube_ve_cihaz_secemez()
    {
        await Login();
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/v1/carikart?subeId=2")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/v1/vardiya/aktif?cihazId=1")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/v1/siparis", new { SubeId = 3, SiparisTipi = 2, Kur = 1 })).StatusCode);
    }

    [Fact]
    public async Task Baska_tenant_cari_siparise_baglanamaz()
    {
        await Login();
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync("/api/v1/siparis", new { CariId = 20, SiparisTipi = 2, Kur = 1 })).StatusCode);
    }

    [Theory]
    [InlineData(20, 20)] [InlineData(10, 20)]
    public async Task Baska_tenant_stok_veya_birim_satira_giremez(long stock, long unit)
    {
        await Login();
        var id = await Order();
        var response = await client.PostAsJsonAsync($"/api/v1/siparis/{id}/satir", new { StokKartId = stock, StokKartSatisBirimiId = unit, Miktar = 1, BirimFiyat = 1 });
        Assert.Contains(response.StatusCode, new[] { HttpStatusCode.BadRequest, HttpStatusCode.NotFound });
        await using var db = new PanoPosDbContext(options);
        Assert.Single(await db.SiparisDetaylari.Where(x => x.SiparisId == id).ToListAsync());
    }

    [Theory]
    [InlineData(2)] [InlineData(3)]
    public async Task Baska_tenant_veya_sube_deposu_faturada_kullanilamaz(long depot)
    {
        await Login();
        var id = await Order();
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/v1/fatura/olustur-siparisten", new { SiparisId = id, DepoId = depot })).StatusCode);
        await using var db = new PanoPosDbContext(options);
        Assert.False(await db.Faturalar.AnyAsync(x => x.TenantId == tenantA));
    }

    [Fact]
    public async Task Baska_tenant_siparis_ve_faturasi_islenemez()
    {
        await Login();
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync("/api/v1/fatura/olustur-siparisten", new { SiparisId = 20 })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync("/api/v1/tahsilat", new { FaturaId = 20, OdemeTipi = 1, KasaId = 10, Tutar = 1, ParaBirimKodu = "TRY", Kur = 1 })).StatusCode);
    }

    [Fact]
    public async Task Baska_sube_kasasi_nakit_hareketi_uretemez()
    {
        await Login();
        var invoice = await Post("/api/v1/fatura/olustur-siparisten", new { SiparisId = await Order() });
        var response = await client.PostAsJsonAsync("/api/v1/tahsilat", new { FaturaId = invoice.GetProperty("id").GetInt64(), OdemeTipi = 1, KasaId = 30, Tutar = 1, ParaBirimKodu = "TRY", Kur = 1 });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await using var db = new PanoPosDbContext(options);
        Assert.False(await db.Tahsilatlar.AnyAsync(x => x.TenantId == tenantA));
    }

    [Fact]
    public async Task Login_siparis_fatura_stok_parcali_tahsilat_gercek_context_audit_outbox()
    {
        await Login();
        var invoice = await Post("/api/v1/fatura/olustur-siparisten", new { SiparisId = await Order() });
        var id = invoice.GetProperty("id").GetInt64();
        await Post("/api/v1/tahsilat", new { FaturaId = id, OdemeTipi = 1, KasaId = 10, Tutar = 600, ParaBirimKodu = "TRY", Kur = 1 });
        var last = await Post("/api/v1/tahsilat", new { FaturaId = id, OdemeTipi = 2, BankaId = 10, Tutar = 400, ParaBirimKodu = "TRY", Kur = 1 });
        Assert.Equal(0, last.GetProperty("faturaKalanTutar").GetDecimal());
        await using var db = new PanoPosDbContext(options);
        Assert.Single(await db.KasaHareketleri.ToListAsync());
        Assert.Single(await db.BankaHareketleri.ToListAsync());
        Assert.Equal(-1, (await db.StokHareketleri.SingleAsync()).Miktar);
        Assert.All(await db.OutboxOlaylari.ToListAsync(), x => { Assert.Equal(4, x.CihazId); Assert.Equal(tenantA, x.TenantId); });
        Assert.All(await db.IslemLoglari.Where(x => x.ModulAdi == "Fatura").ToListAsync(), x => { Assert.Equal(4, x.CihazId); Assert.Equal(1, x.KullaniciId); });
        Assert.Equal(1, (await db.Faturalar.SingleAsync(x => x.Id == id)).OlusturanKullaniciId);
        var stock = await client.GetFromJsonAsync<JsonElement>("/api/v1/stok/miktar?depoId=1&stokKartId=10");
        Assert.Equal(-1, stock.GetProperty("miktar").GetDecimal());
    }

    public async Task DisposeAsync()
    {
        client?.Dispose();
        if (app != null) await app.DisposeAsync();
        await connection.DisposeAsync();
    }

    [Fact]
    public async Task Health_public_kalir()
        => Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/system/health")).StatusCode);

    [Theory]
    [InlineData("kdv")] [InlineData("kategori")] [InlineData("grup")]
    public async Task Baska_tenant_master_iliskisi_reddedilir(string field)
    {
        await Login();
        var response = await client.PostAsJsonAsync("/api/v1/stok-kart", new {
            Ad = "Invalid", KdvId = field == "kdv" ? 20 : 1,
            StokKategoriId = field == "kategori" ? (long?)20 : null,
            StokGrupId = field == "grup" ? (long?)20 : null });
        Assert.False(response.IsSuccessStatusCode);
        await using var db = new PanoPosDbContext(options);
        Assert.False(await db.StokKartler.AnyAsync(x => x.Ad == "Invalid"));
    }

    [Fact]
    public async Task Baska_tenant_fiyat_guncellenemez()
    {
        await Login();
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync("/api/v1/stok-kart-fiyat/20",
            new { Fiyat = 999, ParaBirimKodu = "USD", AktifMi = true })).StatusCode);
        await using var db = new PanoPosDbContext(options);
        Assert.Equal(10, (await db.StokKartFiyatlari.SingleAsync(x => x.Id == 20)).Fiyat);
    }

    [Theory]
    [InlineData("varyant")] [InlineData("fiyatTipi")]
    public async Task Baska_tenant_varyant_ve_fiyat_tipi_siparise_giremez(string field)
    {
        await Login();
        var id = await Order();
        var response = await client.PostAsJsonAsync($"/api/v1/siparis/{id}/satir", new {
            StokKartId = 10, StokKartSatisBirimiId = 10, Miktar = 1, BirimFiyat = 1,
            StokKartVaryantId = field == "varyant" ? (long?)20 : null,
            FiyatTipiId = field == "fiyatTipi" ? (long?)20 : null });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData(2)] [InlineData(3)]
    public async Task Stok_sorgusu_baska_depodan_miktar_okuyamaz(long depot)
    {
        await Login();
        Assert.False((await client.GetAsync($"/api/v1/stok/miktar?depoId={depot}&stokKartId=10")).IsSuccessStatusCode);
    }

    [Fact]
    public async Task Dogrulanmamis_baglam_serviste_de_reddedilir()
    {
        await using var db = new PanoPosDbContext(options, new IslemBaglami());
        var service = new PanoPos.Infrastructure.Product.StokKartServisi(db);
        var error = await Assert.ThrowsAsync<PanoPos.Application.Common.UygulamaHatasi>(() => service.StokKartDetayGetirAsync(10));
        Assert.Equal(401, error.StatusCode);
    }
}

[ApiController]
[Route("api/test/baglam")]
public sealed class BaglamProbeController(IIslemBaglami context) : ControllerBase
{
    [HttpGet]
    public object Get() => new { context.TenantId, context.SubeId, context.KullaniciId, context.CihazId, context.KullaniciOturumId };
}
