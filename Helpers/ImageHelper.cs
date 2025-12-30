namespace KLDShop.Helpers
{
    public static class ImageHelper
    {
        /// <summary>
        /// Generate responsive image tag with lazy loading
        /// </summary>
        public static string GetResponsiveImageTag(string imageUrl, string altText, string cssClass = "", bool isEager = false)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                imageUrl = "/images/no-image.jpg";
            }

            var loading = isEager ? "eager" : "lazy";
            var classes = string.IsNullOrEmpty(cssClass) ? "" : $"class=\"{cssClass}\"";
            
            return $"<img src=\"{imageUrl}\" alt=\"{altText}\" {classes} loading=\"{loading}\" decoding=\"async\" />";
        }

        /// <summary>
        /// Generate placeholder image URL
        /// </summary>
        public static string GetPlaceholderUrl(string productName, int width = 300, int height = 200)
        {
            var encodedName = System.Net.WebUtility.UrlEncode(productName);
            return $"https://via.placeholder.com/{width}x{height}?text={encodedName}";
        }

        /// <summary>
        /// Check if image URL is external
        /// </summary>
        public static bool IsExternalUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return false;
            return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                   url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Get optimized alt text for SEO
        /// </summary>
        public static string GetOptimizedAltText(string productName, string? category = null, string? manufacturer = null)
        {
            var parts = new List<string> { productName };
            
            if (!string.IsNullOrEmpty(manufacturer))
            {
                parts.Add(manufacturer);
            }
            
            if (!string.IsNullOrEmpty(category))
            {
                parts.Add(category);
            }
            
            parts.Add("KLDShop");
            
            return string.Join(" - ", parts);
        }
    }
}
