using PanoPos.Domain.Common;

namespace PanoPos.Domain.Entities;

public sealed class TenantAyar : BaseEntity
{
    public bool SatisFiyatlariKdvDahilMi { get; set; } = true;
    public bool AlisFiyatlariKdvDahilMi { get; set; }
}
