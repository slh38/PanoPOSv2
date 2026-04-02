namespace PanoPos.Application.Auth;

public interface IPinHashServisi
{
    string Hashle(string pin);
    bool Dogrula(string pin, string hash);
}
