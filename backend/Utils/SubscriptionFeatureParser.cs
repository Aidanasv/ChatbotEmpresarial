using System.Globalization;
using System.Text;

namespace backend.Utils
{
    public static class SubscriptionFeatureParser
    {
        public static IReadOnlyList<string> ParseRawFeatures(string? rawFeatures)
        {
            if (string.IsNullOrWhiteSpace(rawFeatures))
            {
                return Array.Empty<string>();
            }

            return rawFeatures
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(feature => !string.IsNullOrWhiteSpace(feature))
                .ToList();
        }

        public static bool ContainsFeature(IEnumerable<string> features, string requiredFeature)
        {
            if (string.IsNullOrWhiteSpace(requiredFeature))
            {
                return false;
            }

            var normalizedRequired = Normalize(requiredFeature);
            return features.Any(feature => Normalize(feature) == normalizedRequired);
        }

        private static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var formD = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(formD.Length);

            foreach (var c in formD)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
