using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class TenantAyar : BaseEntity
{
    public PanoPos.Domain.Enums.MaliyetYontemi MaliyetYontemi { get; set; } = PanoPos.Domain.Enums.MaliyetYontemi.AgirlikliOrtalama;
    public bool SatisFiyatlariKdvDahilMi { get; set; } = true;
    public bool AlisFiyatlariKdvDahilMi { get; set; }
}
