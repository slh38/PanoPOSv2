using PanoPos.Domain.Entities;
using PanoPos.Domain.Enums;

namespace PanoPos.Application.Stock;

public interface IStokMaliyetServisi
{
    Task AlistanGuncelleAsync(AlisFatura fatura, CancellationToken ct = default);
    Task SatisSnapshotAsync(Fatura fatura, IReadOnlyCollection<FaturaDetay> detaylar, CancellationToken ct = default);
    Task<StokMaliyetDto> GetAsync(long subeId, long depoId, long stokKartId, long? varyantId, CancellationToken ct = default);
    Task<MaliyetYontemi> YontemDegistirAsync(long subeId, MaliyetYontemi yontem, CancellationToken ct = default);
}

public sealed class StokMaliyetDto
{
    public long DepoId { get; set; }
    public long StokKartId { get; set; }
    public long? StokKartVaryantId { get; set; }
    public string ParaBirimKodu { get; set; } = string.Empty;
    public decimal SonAlisMaliyeti { get; set; }
    public decimal AgirlikliOrtalamaMaliyet { get; set; }
    public MaliyetYontemi MaliyetYontemi { get; set; }
    public decimal SeciliMaliyet { get; set; }
}

public sealed class MaliyetYontemiRequest
{
    public long SubeId { get; set; }
    public MaliyetYontemi MaliyetYontemi { get; set; }
}
