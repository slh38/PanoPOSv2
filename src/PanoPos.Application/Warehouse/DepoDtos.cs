using PanoPos.Application.Common;

namespace PanoPos.Application.Warehouse;

public sealed class DepoDto
{
    public long Id { get; set; }
    public Guid TenantId { get; set; }
    public long SubeId { get; set; }
    public string DepoKodu { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public bool VarsayilanMi { get; set; }
    public bool AktifMi { get; set; }
}

public sealed class DepoKaydetRequestDto
{
    public long SubeId { get; set; }
    public string DepoKodu { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public bool VarsayilanMi { get; set; }
    public bool AktifMi { get; set; } = true;
}

public interface IDepoServisi
{
    Task<DepoDto> CreateAsync(DepoKaydetRequestDto request, CancellationToken cancellationToken = default);
    Task<DepoDto> UpdateAsync(long id, DepoKaydetRequestDto request, CancellationToken cancellationToken = default);
    Task<DepoDto> GetByIdAsync(long id, long subeId, CancellationToken cancellationToken = default);
    Task<SayfaliSonucDto<DepoDto>> GetPagedAsync(long subeId, string? arama, bool? aktifMi, int page, int pageSize, CancellationToken cancellationToken = default);
    Task DeleteAsync(long id, long subeId, CancellationToken cancellationToken = default);
    Task<DepoDto> SetDefaultAsync(long id, long subeId, CancellationToken cancellationToken = default);
    Task EnsureDefaultDepoAsync(Guid tenantId, long subeId, CancellationToken cancellationToken = default);
}
