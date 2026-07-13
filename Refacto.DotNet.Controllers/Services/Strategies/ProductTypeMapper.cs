using Refacto.DotNet.Controllers.Entities;

namespace Refacto.DotNet.Controllers.Services.Strategies
{
    public static class ProductTypeMapper
    {
        private static readonly Dictionary<string, ProductType> Map = new(StringComparer.Ordinal)
        {
            ["NORMAL"] = ProductType.Normal,
            ["SEASONAL"] = ProductType.Seasonal,
            ["EXPIRABLE"] = ProductType.Expirable
        };

        public static bool TryMap(string? rawType, out ProductType productType)
        {
            if (rawType is not null && Map.TryGetValue(rawType, out productType))
            {
                return true;
            }
            productType = default;
            return false;
        }
    }
}
