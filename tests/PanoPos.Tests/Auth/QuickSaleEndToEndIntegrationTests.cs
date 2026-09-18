using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PanoPos.Application.Invoice;
using PanoPos.Application.Order;
using PanoPos.Application.Payment;
using PanoPos.Application.Product;
using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;
using PanoPos.Infrastructure.Persistence;

namespace PanoPos.Tests.Auth;

public sealed partial class SessionIntegrationTests
{
    // Reuse the existing real HTTP host, Bearer pipeline and isolated SQLite fixture,
    // without inheriting (and rerunning) the fixture's existing test methods.
    public sealed class QuickSaleEndToEndIntegrationTests : IAsyncLifetime
    {
        private readonly SessionIntegrationTests fixture = new();
        public Task InitializeAsync() => fixture.InitializeAsync();
        public Task DisposeAsync() => fixture.DisposeAsync();

        [Fact]
        public async Task Login_koli_beklet_geri_ac_faturala_parcali_ode_fis_ve_tenant_izolasyonu()
        {
            var login = await fixture.Login();
            Assert.False(string.IsNullOrWhiteSpace(login.OturumToken));
            Assert.Equal(fixture.tenantA, login.TenantId);
            Assert.Equal(1, login.SubeId);
            Assert.Equal(1, login.KullaniciId);
            Assert.Equal(4, login.CihazId);
            Assert.Equal("Bearer", fixture.client.DefaultRequestHeaders.Authorization!.Scheme);
            Assert.Equal(login.OturumToken, fixture.client.DefaultRequestHeaders.Authorization.Parameter);

            var category = await fixture.Post("/api/v1/stok-kategori", new { SubeId = login.SubeId, Kod = "ICECEK", Ad = "Icecek" });
            var group = await fixture.Post("/api/v1/stok-grup", new { SubeId = login.SubeId, Kod = "GAZLI", Ad = "Gazli Icecek" });
            var product = await fixture.Post("/api/v1/stok-kart/tam-kayit", new
            {
                SubeId = login.SubeId, StokKartKodu = "TEST-KOLA", Ad = "Test Kola", KdvId = 4,
                StokKategoriId = category.GetProperty("id").GetInt64(),
                StokGrupId = group.GetProperty("id").GetInt64(),
                SatisBirimleri = new[]
                {
                    new { BirimKodu = "ADET", BirimAdi = "Adet", Katsayi = 1m, VarsayilanMi = true,
                        BarkodNo = "869TEST0001", Fiyatlar = new[]
                        {
                            new { FiyatTipiId = 1L, Fiyat = 10m, ParaBirimKodu = "TRY" },
                            new { FiyatTipiId = 2L, Fiyat = 11m, ParaBirimKodu = "TRY" }
                        } },
                    new { BirimKodu = "KOLI", BirimAdi = "Koli", Katsayi = 24m, VarsayilanMi = false,
                        BarkodNo = "869TEST0024", Fiyatlar = new[]
                        {
                            new { FiyatTipiId = 1L, Fiyat = 220m, ParaBirimKodu = "TRY" },
                            new { FiyatTipiId = 2L, Fiyat = 230m, ParaBirimKodu = "TRY" }
                        } }
                }
            });
            var stockId = product.GetProperty("id").GetInt64();
            var unit = await Get<SatisStokDto>("/api/v1/satis/barkod/869TEST0001?fiyatTipiId=1");
            var box = await Get<SatisStokDto>("/api/v1/satis/barkod/869TEST0024?fiyatTipiId=1");
            Assert.Equal(stockId, unit.StokKartId);
            Assert.Equal("Adet", unit.BirimAdi); Assert.Equal(1m, unit.Katsayi); Assert.Equal(10m, unit.Fiyat);
            Assert.NotEqual(unit.StokKartSatisBirimiId, box.StokKartSatisBirimiId);
            Assert.Equal(stockId, box.StokKartId); Assert.Equal("Test Kola", box.StokKartAdi);
            Assert.Equal("Koli", box.BirimAdi); Assert.Equal(24m, box.Katsayi);
            Assert.Equal(1, box.FiyatTipiId); Assert.Equal("Perakende", box.FiyatTipiAdi);
            Assert.Equal(220m, box.Fiyat); Assert.Equal("TRY", box.FiyatParaBirimKodu);
            Assert.Equal(4, box.KdvId); Assert.Equal(20m, box.KdvOrani);
            Assert.Equal(11m, (await Get<SatisStokDto>("/api/v1/satis/barkod/869TEST0001?fiyatTipiId=2")).Fiyat);
            Assert.Equal(230m, (await Get<SatisStokDto>("/api/v1/satis/barkod/869TEST0024?fiyatTipiId=2")).Fiyat);
            await AssertBeforeInvoice(0, 0);

            var held = await Send<SiparisDto>(HttpMethod.Post, "/api/v1/hizli-satis/beklet", new
            {
                CariId = 10, FiyatTipiId = 1, BelgeParaBirimKodu = "TRY", Kur = 1,
                Satirlar = new[] { new { box.StokKartSatisBirimiId, Miktar = 1m } }
            });
            Assert.Equal(SiparisTipi.HizliSatisBekleyen, held.SiparisTipi);
            Assert.Equal(SiparisDurumu.Bekliyor, held.Durum);
            Assert.Equal(220m, held.NetToplam);
            await AssertBeforeInvoice(1, 1);

            // Check the other tenant while the order is still genuinely pending.
            await fixture.Login("5678", 2);
            await AssertNotFound($"/api/v1/hizli-satis/bekleyen/{held.Id}");
            await AssertNotFound("/api/v1/satis/barkod/869TEST0024?fiyatTipiId=20");
            await fixture.Login();
            var opened = await Get<SiparisDto>($"/api/v1/hizli-satis/bekleyen/{held.Id}");
            var openedLine = Assert.Single(opened.Detaylar);
            AssertOrderLine(openedLine, box, 1m);
            Assert.Equal(held.Surum, opened.Surum);
            Assert.NotEqual(Guid.Empty, opened.Surum);

            var update = new
            {
                Surum = opened.Surum, CariId = 10, FiyatTipiId = 1, BelgeParaBirimKodu = "TRY", Kur = 1,
                NetToplam = 0.01m, AraToplam = 0.01m, ToplamKdv = 9999m,
                Satirlar = new[] { new { SiparisDetayId = openedLine.Id, box.StokKartSatisBirimiId,
                    Miktar = 2m, BirimFiyat = 0.01m, KdvTutari = 9999m, SatirNetToplam = 0.01m } }
            };
            var updated = await Send<SiparisDto>(HttpMethod.Put, $"/api/v1/hizli-satis/bekleyen/{held.Id}", update);
            var orderLine = Assert.Single(updated.Detaylar);
            AssertOrderLine(orderLine, box, 2m);
            Assert.Equal(440m, updated.NetToplam); Assert.Equal(440m, updated.AraToplam);
            Assert.Equal(366.67m, updated.ToplamMatrah); Assert.Equal(73.33m, updated.ToplamKdv);
            Assert.NotEqual(opened.Surum, updated.Surum);
            using (var stale = await fixture.client.PutAsJsonAsync($"/api/v1/hizli-satis/bekleyen/{held.Id}", update))
                Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
            await AssertBeforeInvoice(1, 1);

            var invoice = await Send<FaturaDto>(HttpMethod.Post, "/api/v1/fatura/olustur-siparisten", new { SiparisId = held.Id });
            Assert.Equal(440m, invoice.NetToplam);
            AssertInvoiceLine(Assert.Single(invoice.Detaylar), orderLine);
            using (var duplicate = await fixture.client.PostAsJsonAsync("/api/v1/fatura/olustur-siparisten", new { SiparisId = held.Id }))
                Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
            await AssertStock(invoice.Id, stockId, box.StokKartSatisBirimiId);
            var balance = await Get<JsonElement>($"/api/v1/stok/miktar?depoId=1&stokKartId={stockId}");
            Assert.Equal(-48m, balance.GetProperty("miktar").GetDecimal());

            var cash = decimal.Round(invoice.NetToplam * 0.6m, 2, MidpointRounding.AwayFromZero);
            var card = invoice.NetToplam - cash;
            var cashKey = Guid.NewGuid();
            var cardKey = Guid.NewGuid();
            Assert.NotEqual(cashKey, cardKey);
            var first = await Send<TahsilatDto>(HttpMethod.Post, "/api/v1/tahsilat", new
            {
                IslemAnahtari = cashKey, FaturaId = invoice.Id, OdemeTipi = OdemeTipi.Nakit,
                Tutar = cash, ParaBirimKodu = "TRY", Kur = 1, KasaId = 10
            });
            Assert.Equal(cash, first.FaturaOdenenTutar); Assert.Equal(card, first.FaturaKalanTutar);
            Assert.True(first.FaturaKalanTutar > 0); Assert.Equal(FaturaDurumu.Acik, first.FaturaDurumu);
            await using (var db = new PanoPosDbContext(fixture.options))
            {
                Assert.Single(await db.Tahsilatlar.Where(x => x.FaturaId == invoice.Id).ToListAsync());
                var movement = Assert.Single(await db.KasaHareketleri.Where(x => x.TenantId == fixture.tenantA).ToListAsync());
                Assert.Equal(first.Id, movement.ReferansId); Assert.Equal(nameof(Tahsilat), movement.ReferansTip);
                Assert.Equal(10, movement.KasaId); Assert.Equal(cash, movement.Tutar);
                Assert.Empty(await db.BankaHareketleri.Where(x => x.FaturaId == invoice.Id).ToListAsync());
            }
            var cardRequest = new
            {
                IslemAnahtari = cardKey, FaturaId = invoice.Id, OdemeTipi = OdemeTipi.KrediKarti,
                Tutar = card, ParaBirimKodu = "TRY", Kur = 1, BankaId = 10
            };
            var second = await Send<TahsilatDto>(HttpMethod.Post, "/api/v1/tahsilat", cardRequest);
            Assert.Equal(invoice.NetToplam, second.FaturaOdenenTutar);
            Assert.Equal(0m, second.FaturaKalanTutar); Assert.Equal(FaturaDurumu.Kapali, second.FaturaDurumu);
            await AssertPayments(invoice.Id, second.Id, cash, card);
            var replay = await Send<TahsilatDto>(HttpMethod.Post, "/api/v1/tahsilat", cardRequest);
            Assert.Equal(second.Id, replay.Id); Assert.Equal(second.FaturaOdenenTutar, replay.FaturaOdenenTutar);
            Assert.Equal(0m, replay.FaturaKalanTutar);
            await AssertPayments(invoice.Id, second.Id, cash, card);
            await AssertStock(invoice.Id, stockId, box.StokKartSatisBirimiId);

            var receipt = await Get<FaturaDto>($"/api/v1/fatura/{invoice.Id}");
            Assert.Equal(invoice.FaturaNo, receipt.FaturaNo); Assert.False(string.IsNullOrWhiteSpace(receipt.FaturaNo));
            Assert.Equal(invoice.FaturaTarihi, receipt.FaturaTarihi); Assert.NotEqual(default, receipt.FaturaTarihi);
            Assert.Equal(fixture.tenantA, receipt.TenantId); Assert.Equal("Pano Demo Tenant", receipt.TenantAdi);
            Assert.Equal(1, receipt.SubeId); Assert.Equal("Merkez Sube", receipt.SubeAdi);
            Assert.Equal(1, receipt.KasiyerId); Assert.Equal("Admin Kullanici", receipt.KasiyerAdi);
            Assert.Equal(4, receipt.CihazId); Assert.Equal("POS4", receipt.CihazAdi);
            Assert.Equal(1, receipt.DepoId); Assert.Equal("Merkez Depo", receipt.DepoAdi);
            Assert.Equal(10, receipt.CariId); Assert.Equal("Customer A", receipt.CariAdi);
            Assert.Equal(440m, receipt.NetToplam); Assert.Equal(440m, receipt.OdenenTutar);
            Assert.Equal(0m, receipt.KalanTutar); Assert.Equal(FaturaDurumu.Kapali, receipt.Durum);
            AssertInvoiceLine(Assert.Single(receipt.Detaylar), orderLine);
            Assert.Equal(cash, receipt.NakitToplam); Assert.Equal(card, receipt.KartToplam); Assert.Equal(0m, receipt.VeresiyeToplam);
            Assert.Equal(2, receipt.Odemeler.Count);
            var cashReceipt = Assert.Single(receipt.Odemeler.Where(x => x.OdemeTipi == OdemeTipi.Nakit));
            Assert.Equal(first.Id, cashReceipt.TahsilatId); Assert.Equal(cash, cashReceipt.Tutar); Assert.Equal(10, cashReceipt.KasaId);
            var cardReceipt = Assert.Single(receipt.Odemeler.Where(x => x.OdemeTipi == OdemeTipi.KrediKarti));
            Assert.Equal(second.Id, cardReceipt.TahsilatId); Assert.Equal(card, cardReceipt.Tutar); Assert.Equal(10, cardReceipt.BankaId);

            await fixture.Login("5678", 2);
            await AssertNotFound($"/api/v1/fatura/{invoice.Id}");
            await AssertNotFound($"/api/v1/hizli-satis/bekleyen/{held.Id}");
            await AssertNotFound("/api/v1/satis/barkod/869TEST0024?fiyatTipiId=20");
        }

        private async Task<T> Get<T>(string path)
        {
            using var response = await fixture.client.GetAsync(path);
            Assert.True(response.IsSuccessStatusCode, $"GET {path}: {await response.Content.ReadAsStringAsync()}");
            return (await response.Content.ReadFromJsonAsync<T>())!;
        }

        private async Task<T> Send<T>(HttpMethod method, string path, object body)
        {
            using var request = new HttpRequestMessage(method, path) { Content = JsonContent.Create(body) };
            using var response = await fixture.client.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode, $"{method} {path}: {await response.Content.ReadAsStringAsync()}");
            return (await response.Content.ReadFromJsonAsync<T>())!;
        }

        private async Task AssertNotFound(string path)
        {
            using var response = await fixture.client.GetAsync(path);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private async Task AssertBeforeInvoice(int orders, int lines)
        {
            await using var db = new PanoPosDbContext(fixture.options);
            Assert.Equal(orders, await db.Siparisler.CountAsync(x => x.TenantId == fixture.tenantA));
            Assert.Equal(lines, await db.SiparisDetaylari.CountAsync(x => x.TenantId == fixture.tenantA));
            Assert.False(await db.Faturalar.AnyAsync(x => x.TenantId == fixture.tenantA));
            Assert.False(await db.StokFisleri.AnyAsync(x => x.TenantId == fixture.tenantA));
            Assert.False(await db.StokHareketleri.AnyAsync(x => x.TenantId == fixture.tenantA));
            Assert.False(await db.Tahsilatlar.AnyAsync(x => x.TenantId == fixture.tenantA));
        }

        private static void AssertOrderLine(SiparisDetayDto line, SatisStokDto box, decimal quantity)
        {
            Assert.Equal(box.StokKartId, line.StokKartId); Assert.Equal("Test Kola", line.StokKartAd);
            Assert.Equal(box.StokKartSatisBirimiId, line.StokKartSatisBirimiId);
            Assert.Equal("Koli", line.BirimAdi); Assert.Equal(24m, line.BirimKatsayi); Assert.Equal(quantity, line.Miktar);
            Assert.Equal(220m, line.BirimFiyat); Assert.Equal(1, line.FiyatTipiId); Assert.Equal("Perakende", line.FiyatTipiAdi);
            Assert.Equal("TRY", line.FiyatParaBirimKodu); Assert.Equal(1m, line.FiyatKur);
            Assert.Equal(4, line.KdvId); Assert.Equal(20m, line.KdvOrani); Assert.True(line.KdvDahilMi);
            Assert.Equal(220m * quantity, line.SatirNetToplam);
        }

        private static void AssertInvoiceLine(FaturaDetayDto line, SiparisDetayDto order)
        {
            Assert.Equal(order.StokKartId, line.StokKartId); Assert.Equal("Test Kola", line.StokKartAd);
            Assert.Equal(order.StokKartSatisBirimiId, line.StokKartSatisBirimiId);
            Assert.Equal(order.BirimKodu, line.BirimKodu); Assert.Equal(order.BirimAdi, line.BirimAdi);
            Assert.Equal(order.BirimKatsayi, line.BirimKatsayi); Assert.Equal(2m, line.Miktar);
            Assert.Equal(220m, line.BirimFiyat); Assert.Equal(order.FiyatTipiId, line.FiyatTipiId);
            Assert.Equal(order.FiyatTipiAdi, line.FiyatTipiAdi); Assert.Equal(order.FiyatKur, line.FiyatKur);
            Assert.Equal(order.FiyatParaBirimKodu, line.FiyatParaBirimKodu);
            Assert.Equal(order.KdvId, line.KdvId); Assert.Equal(order.KdvOrani, line.KdvOrani);
            Assert.Equal(order.KdvDahilMi, line.KdvDahilMi); Assert.Equal(order.KdvTutari, line.KdvTutari);
            Assert.Equal(order.Matrah, line.Matrah); Assert.Equal(order.SatirNetToplam, line.SatirNetToplam);
            Assert.Equal(0m, line.BirimMaliyet);
        }

        private async Task AssertStock(long invoiceId, long stockId, long unitId)
        {
            await using var db = new PanoPosDbContext(fixture.options);
            Assert.Single(await db.Faturalar.Where(x => x.TenantId == fixture.tenantA).ToListAsync());
            Assert.Single(await db.FaturaDetaylari.Where(x => x.FaturaId == invoiceId).ToListAsync());
            var voucher = Assert.Single(await db.StokFisleri.Where(x => x.TenantId == fixture.tenantA).ToListAsync());
            Assert.Equal(invoiceId, voucher.FaturaId); Assert.Equal(StokFisTipi.Satis, voucher.StokFisTipi);
            Assert.Equal(1, voucher.DepoId);
            var detail = Assert.Single(await db.Set<StokFisDetay>().Where(x => x.StokFisId == voucher.Id).ToListAsync());
            Assert.Equal(stockId, detail.StokKartId); Assert.Equal(unitId, detail.StokKartSatisBirimiId);
            Assert.Equal(2m, detail.Miktar); Assert.Equal(24m, detail.Katsayi);
            var movement = Assert.Single(await db.StokHareketleri.Where(x => x.TenantId == fixture.tenantA).ToListAsync());
            Assert.Equal(stockId, movement.StokKartId); Assert.Equal(1, movement.DepoId); Assert.Equal(-48m, movement.Miktar);
            Assert.False(await db.Set<StokMaliyet>().AnyAsync(x => x.TenantId == fixture.tenantA && x.StokKartId == stockId));
        }

        private async Task AssertPayments(long invoiceId, long cardPaymentId, decimal cash, decimal card)
        {
            await using var db = new PanoPosDbContext(fixture.options);
            var payments = await db.Tahsilatlar.Where(x => x.FaturaId == invoiceId).ToListAsync();
            Assert.Equal(2, payments.Count); Assert.Equal(cash + card, payments.Sum(x => x.Tutar));
            var movement = Assert.Single(await db.BankaHareketleri.Where(x => x.FaturaId == invoiceId).ToListAsync());
            Assert.Equal(cardPaymentId, movement.TahsilatId); Assert.Equal(card, movement.Tutar); Assert.Equal(10, movement.BankaId);
            Assert.Single(await db.KasaHareketleri.Where(x => x.TenantId == fixture.tenantA).ToListAsync());
            var invoice = await db.Faturalar.SingleAsync(x => x.Id == invoiceId);
            Assert.Equal(cash + card, invoice.OdenenTutar); Assert.Equal(0m, invoice.KalanTutar);
            Assert.Equal(FaturaDurumu.Kapali, invoice.Durum);
        }
    }
}
