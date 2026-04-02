namespace PanoPos.Application.Common;

public sealed class UygulamaHatasi : Exception
{
    public UygulamaHatasi(int statusCode, string title, string detail, string? errorCode = null)
        : base(detail)
    {
        StatusCode = statusCode;
        Title = title;
        Detail = detail;
        ErrorCode = errorCode;
    }

    public int StatusCode { get; }
    public string Title { get; }
    public string Detail { get; }
    public string? ErrorCode { get; }
}
