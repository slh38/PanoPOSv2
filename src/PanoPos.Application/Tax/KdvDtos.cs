using PanoPos.Application.Common;
namespace PanoPos.Application.Tax;
public sealed class KdvKaydetRequest
{
    public long SubeId { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public decimal Oran { get; set; }
    public bool AktifMi { get; set; } = true;
}
public sealed class KdvDto
{
    public long Id { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public decimal Oran { get; set; }
    public bool AktifMi { get; set; }
}
public interface IKdvServisi
{
    Task<KdvDto> CreateAsync(KdvKaydetRequest request, CancellationToken ct = default);
    Task<KdvDto> UpdateAsync(long id, KdvKaydetRequest request, CancellationToken ct = default);
    Task<KdvDto> GetByIdAsync(long id, long subeId, CancellationToken ct = default);
    Task<SayfaliSonucDto<KdvDto>> GetPagedAsync(long subeId, string? arama, bool? aktifMi, int page, int pageSize, CancellationToken ct = default);
    Task DeleteAsync(long id, long subeId, CancellationToken ct = default);
}
