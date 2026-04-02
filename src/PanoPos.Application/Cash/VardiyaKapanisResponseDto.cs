namespace PanoPos.Application.Cash;

public sealed class VardiyaKapanisResponseDto
{
    public long VardiyaId { get; set; }
    public decimal BeklenenNakit { get; set; }
    public decimal SayilanNakit { get; set; }
    public decimal FarkTutar { get; set; }
    public DateTime KapanisTarihi { get; set; }
}
