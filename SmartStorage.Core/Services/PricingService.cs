using System;
using System.Collections.Generic;

namespace SmartStorage.Core.Services
{
    public static class PricingService
    {
        // Base prices per storage type and size
        private static readonly Dictionary<string, Dictionary<string, (decimal Min, decimal Max)>> PricingMatrix = new()
        {
            ["self"] = new()
            {
                ["Small"] = (500, 800),
                ["Medium"] = (900, 1300),
                ["Large"] = (1300, 2500),
                ["ExtraLarge"] = (2500, 4000)
            },
            ["climate"] = new()
            {
                ["Small"] = (900, 1200),
                ["Medium"] = (1300, 2000),
                ["Large"] = (2000, 3500),
                ["ExtraLarge"] = (3500, 5000)
            },
            ["vehicle"] = new()
            {
                ["Small"] = (800, 1200),     // Small car
                ["Medium"] = (1200, 1800),   // SUV / Truck
                ["Large"] = (1800, 2800),    // Large truck / Boat
                ["ExtraLarge"] = (2800, 4000) // Boat / RV
            },
            ["business"] = new()
            {
                ["Small"] = (1000, 2000),
                ["Medium"] = (2000, 4000),
                ["Large"] = (4000, 7000),
                ["ExtraLarge"] = (7000, 10000)
            },
            ["household"] = new()
            {
                ["Small"] = (800, 1200),
                ["Medium"] = (1200, 2000),
                ["Large"] = (2000, 3500),
                ["ExtraLarge"] = (3500, 5000)
            }
        };

        // Size mapping based on storage type
        private static readonly Dictionary<string, List<string>> SizeMapping = new()
        {
            ["self"] = new() { "Small", "Medium", "Large", "ExtraLarge" },
            ["climate"] = new() { "Small", "Medium", "Large", "ExtraLarge" },
            ["vehicle"] = new() { "Small", "Medium", "Large", "ExtraLarge" },
            ["business"] = new() { "Small", "Medium", "Large", "ExtraLarge" },
            ["household"] = new() { "Small", "Medium", "Large", "ExtraLarge" }
        };

        public static List<string> GetAvailableSizes(string storageType)
        {
            return SizeMapping.ContainsKey(storageType) ? SizeMapping[storageType] : SizeMapping["self"];
        }

        public static (decimal Min, decimal Max) GetPriceRange(string storageType, string size)
        {
            if (PricingMatrix.TryGetValue(storageType, out var sizes) && sizes.TryGetValue(size, out var range))
            {
                return range;
            }
            return (500, 1000); // Default fallback
        }

        public static decimal CalculateMonthlyPrice(string storageType, string size, string location = "Standard")
        {
            var range = GetPriceRange(storageType, size);
            // For now, use the midpoint. Later can add location-based adjustments
            return (range.Min + range.Max) / 2;
        }

        public static decimal CalculateTotalPrice(string storageType, string size, int months, int discountPercentage = 0)
        {
            var monthlyPrice = CalculateMonthlyPrice(storageType, size);
            var subtotal = monthlyPrice * months;
            var discount = subtotal * (discountPercentage / 100m);
            return subtotal - discount;
        }

        public static int GetDiscountPercentage(int months)
        {
            if (months >= 12) return 20;
            if (months >= 6) return 15;
            if (months >= 3) return 10;
            return 0;
        }

        public static string GetSizeDescription(string storageType, string size)
        {
            return size switch
            {
                "Small" when storageType == "vehicle" => "Small Car (Sedan, Hatchback)",
                "Medium" when storageType == "vehicle" => "SUV / Light Truck",
                "Large" when storageType == "vehicle" => "Large Truck / Small Boat",
                "ExtraLarge" when storageType == "vehicle" => "Boat / RV / Caravan",
                "Small" when storageType == "business" => "Small Office / Inventory (10-20 boxes)",
                "Medium" when storageType == "business" => "Medium Business Stock (20-50 boxes)",
                "Large" when storageType == "business" => "Large Business / Warehouse (50-100 boxes)",
                "ExtraLarge" when storageType == "business" => "Full Warehouse (100+ boxes)",
                "Small" => "Small (5x5) - Up to 50 items / 1 room",
                "Medium" => "Medium (10x10) - 1-bedroom apartment / 100 items",
                "Large" => "Large (10x20) - 2-3 bedroom house / Furniture",
                "ExtraLarge" => "Extra Large (10x30+) - Full house / Business stock",
                _ => "Standard Unit"
            };
        }

        public static string GetFeatureDescription(string storageType)
        {
            return storageType switch
            {
                "climate" => "✓ Temperature controlled (18-22°C) | ✓ Humidity regulated | ✓ Premium security",
                "vehicle" => "✓ 24/7 vehicle access | ✓ Security cameras | ✓ Battery charging station available",
                "business" => "✓ Extended business hours | ✓ Loading dock access | ✓ Pallet storage available",
                "household" => "✓ Easy access | ✓ Security cameras | ✓ On-site manager",
                _ => "✓ 24/7 access | ✓ Security cameras | ✓ Individual alarm available"
            };
        }
    }
}