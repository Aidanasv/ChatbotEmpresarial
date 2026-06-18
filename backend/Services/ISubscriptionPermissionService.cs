using System.Security.Claims;

namespace backend.Services
{
    public record SubscriptionPermissionResult(bool IsAllowed, int StatusCode, string Message);

    public interface ISubscriptionPermissionService
    {
        Task<IReadOnlyList<string>> GetCompanyFeaturesAsync(int companyId, CancellationToken cancellationToken = default);
        Task<int> GetCompanyMaxUsersAsync(int companyId, CancellationToken cancellationToken = default);
        Task<int?> GetDocumentUploadLimitAsync(int companyId, CancellationToken cancellationToken = default);
        Task<bool> CompanyHasFeatureAsync(int companyId, string requiredFeature, CancellationToken cancellationToken = default);
        Task<SubscriptionPermissionResult> ValidateUserFeatureAsync(ClaimsPrincipal user, string requiredFeature, CancellationToken cancellationToken = default);
    }
}
