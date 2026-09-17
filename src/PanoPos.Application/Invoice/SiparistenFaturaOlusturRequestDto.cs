namespace PanoPos.Application.Invoice;

public sealed class SiparistenFaturaOlusturRequestDto
{
    public long SiparisId { get; set; }
    public long? DepoId { get; set; }
    public string? Aciklama { get; set; }
}
