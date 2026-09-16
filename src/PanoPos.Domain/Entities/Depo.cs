using PanoPos.Domain.Common;
namespace PanoPos.Domain.Entities;
public sealed class Depo : BaseEntity { public string DepoKodu { get; set; } = string.Empty; public string Ad { get; set; } = string.Empty; public bool VarsayilanMi { get; set; } public Tenant Tenant { get; set; } = null!; public Sube Sube { get; set; } = null!; }
