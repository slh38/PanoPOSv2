using PanoPos.Application.Common;
using PanoPos.Domain.Enums;

namespace PanoPos.Application.Stock;

public sealed class StokFisOlusturRequest
{
    public long SubeId { get; set; }
    public string FisNo { get; set; } = string.Empty;
    public DateTime FisTarihi { get; set; } = DateTime.UtcNow;
    public long? DepoId { get; set; }
    public long? KaynakDepoId { get; set; }
    public long? HedefDepoId { get; set; }
    public string? Aciklama { get; set; }
    public List<StokFisSatirRequest> Detaylar { get; set; } = new();
}

public sealed class StokFisSatirRequest
{
    public long StokKartId { get; set; }
    public long? StokKartVaryantId { get; set; }
    public long StokKartSatisBirimiId { get; set; }
    // For counts this is the physically counted quantity in the selected sales unit.
    public decimal Miktar { get; set; }
    public string? Aciklama { get; set; }
}

public class StokFisListeDto
{
    public long Id { get; set; }
    public StokFisTipi StokFisTipi { get; set; }
    public string FisNo { get; set; } = string.Empty;
    public DateTime FisTarihi { get; set; }
    public long? DepoId { get; set; }
    public long? KaynakDepoId { get; set; }
    public long? HedefDepoId { get; set; }
    public string? Aciklama { get; set; }
}

public sealed class StokFisDto : StokFisListeDto
{
    public Guid TenantId { get; set; }
    public long SubeId { get; set; }
    public List<StokFisDetayDto> Detaylar { get; set; } = new();
}

public sealed class StokFisDetayDto
{
    public long Id { get; set; }
    public long StokKartId { get; set; }
    public long? StokKartVaryantId { get; set; }
    public long StokKartSatisBirimiId { get; set; }
    public string BirimKodu { get; set; } = string.Empty;
    public string BirimAdi { get; set; } = string.Empty;
    public decimal Katsayi { get; set; }
    public decimal Miktar { get; set; }
    public decimal? SistemMiktari { get; set; }
    public decimal? SayilanMiktar { get; set; }
    public decimal? FarkMiktari { get; set; }
    public string? Aciklama { get; set; }
}

public sealed class StokFisFiltre
{
    public long SubeId { get; set; }
    public StokFisTipi? StokFisTipi { get; set; }
    public long? DepoId { get; set; }
    public string? Arama { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public sealed record StokMiktarDto(long DepoId, long StokKartId, long? StokKartVaryantId, decimal Miktar);

public interface IStokServisi
{
    Task<StokFisDto> DevirAsync(StokFisOlusturRequest request, CancellationToken ct = default);
    Task<StokFisDto> SayimAsync(StokFisOlusturRequest request, CancellationToken ct = default);
    Task<StokFisDto> TransferAsync(StokFisOlusturRequest request, CancellationToken ct = default);
    Task<StokFisDto> GetByIdAsync(long id, long subeId, CancellationToken ct = default);
    Task<SayfaliSonucDto<StokFisListeDto>> GetPagedAsync(StokFisFiltre request, CancellationToken ct = default);
    Task<StokMiktarDto> GetMiktarAsync(long subeId, long depoId, long stokKartId, long? varyantId, CancellationToken ct = default);
}
