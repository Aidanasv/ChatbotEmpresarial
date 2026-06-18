using System.Security.Claims;
using backend.Data;
using backend.Utils;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class SubscriptionPermissionService : ISubscriptionPermissionService
    {
        private readonly AppDbContext _context;

        public SubscriptionPermissionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<string>> GetCompanyFeaturesAsync(int companyId, CancellationToken cancellationToken = default)
        {
            var rawFeatures = await _context.Companies
                .Where(company => company.Id == companyId)
                .Select(company => company.Subscription.Features)
                .FirstOrDefaultAsync(cancellationToken);

            return SubscriptionFeatureParser.ParseRawFeatures(rawFeatures);
        }

        public async Task<int> GetCompanyMaxUsersAsync(int companyId, CancellationToken cancellationToken = default)
        {
            return await _context.Companies
                .Where(company => company.Id == companyId)
                .Select(company => company.Subscription.MaxUsers)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> CompanyHasFeatureAsync(int companyId, string requiredFeature, CancellationToken cancellationToken = default)
        {
            var features = await GetCompanyFeaturesAsync(companyId, cancellationToken);
            return SubscriptionFeatureParser.ContainsFeature(features, requiredFeature);
        }

        public async Task<int?> GetDocumentUploadLimitAsync(int companyId, CancellationToken cancellationToken = default)
        {
            var features = await GetCompanyFeaturesAsync(companyId, cancellationToken);

            if (SubscriptionFeatureParser.ContainsFeature(features, SubscriptionFeatureCatalog.UnlimitedDocumentUpload))
            {
                return null;
            }

            if (SubscriptionFeatureParser.ContainsFeature(features, SubscriptionFeatureCatalog.LimitedDocumentUpload))
            {
                return 5;
            }

            return 0;
        }

        public async Task<SubscriptionPermissionResult> ValidateUserFeatureAsync(ClaimsPrincipal user, string requiredFeature, CancellationToken cancellationToken = default)
        {
            if (user.IsInRole("SuperAdmin"))
            {
                return new SubscriptionPermissionResult(true, StatusCodes.Status200OK, string.Empty);
            }

            var companyIdClaim = user.FindFirst("companyId")?.Value;
            if (!int.TryParse(companyIdClaim, out var companyId))
            {
                return new SubscriptionPermissionResult(false, StatusCodes.Status401Unauthorized, "No se pudo identificar la empresa del usuario.");
            }

            var hasFeature = await CompanyHasFeatureAsync(companyId, requiredFeature, cancellationToken);
            if (!hasFeature)
            {
                return new SubscriptionPermissionResult(false, StatusCodes.Status403Forbidden, $"Tu suscripcion no incluye la caracteristica requerida: {requiredFeature}.");
            }

            return new SubscriptionPermissionResult(true, StatusCodes.Status200OK, string.Empty);
        }
    }
}
