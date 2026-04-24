using PanoPos.Desktop.Models;

namespace PanoPos.Desktop.Services;

public interface ITahsilatService
{
    Task<TahsilatResponseModel?> TahsilatYapAsync(TahsilatRequestModel request, CancellationToken cancellationToken = default);
}
