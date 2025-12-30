using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using KLDShop.Data;
using System.Text;
using System.Xml;

namespace KLDShop.Controllers
{
    public class SitemapController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SitemapController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("sitemap.xml")]
        [ResponseCache(Duration = 3600)] // Cache for 1 hour
        public async Task<IActionResult> Index()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var sitemap = new StringBuilder();

            sitemap.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sitemap.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

            // Homepage
            sitemap.AppendLine("  <url>");
            sitemap.AppendLine($"    <loc>{baseUrl}/</loc>");
            sitemap.AppendLine($"    <lastmod>{DateTime.Now:yyyy-MM-dd}</lastmod>");
            sitemap.AppendLine("    <changefreq>daily</changefreq>");
            sitemap.AppendLine("    <priority>1.0</priority>");
            sitemap.AppendLine("  </url>");

            // Products list page
            sitemap.AppendLine("  <url>");
            sitemap.AppendLine($"    <loc>{baseUrl}/Product/List</loc>");
            sitemap.AppendLine($"    <lastmod>{DateTime.Now:yyyy-MM-dd}</lastmod>");
            sitemap.AppendLine("    <changefreq>daily</changefreq>");
            sitemap.AppendLine("    <priority>0.9</priority>");
            sitemap.AppendLine("  </url>");

            // About page
            sitemap.AppendLine("  <url>");
            sitemap.AppendLine($"    <loc>{baseUrl}/Home/About</loc>");
            sitemap.AppendLine($"    <lastmod>{DateTime.Now:yyyy-MM-dd}</lastmod>");
            sitemap.AppendLine("    <changefreq>monthly</changefreq>");
            sitemap.AppendLine("    <priority>0.5</priority>");
            sitemap.AppendLine("  </url>");

            // Contact page
            sitemap.AppendLine("  <url>");
            sitemap.AppendLine($"    <loc>{baseUrl}/Home/Contact</loc>");
            sitemap.AppendLine($"    <lastmod>{DateTime.Now:yyyy-MM-dd}</lastmod>");
            sitemap.AppendLine("    <changefreq>monthly</changefreq>");
            sitemap.AppendLine("    <priority>0.5</priority>");
            sitemap.AppendLine("  </url>");

            // FAQ page
            sitemap.AppendLine("  <url>");
            sitemap.AppendLine($"    <loc>{baseUrl}/Home/Faq</loc>");
            sitemap.AppendLine($"    <lastmod>{DateTime.Now:yyyy-MM-dd}</lastmod>");
            sitemap.AppendLine("    <changefreq>monthly</changefreq>");
            sitemap.AppendLine("    <priority>0.8</priority>");
            sitemap.AppendLine("  </url>");

            // Privacy page
            sitemap.AppendLine("  <url>");
            sitemap.AppendLine($"    <loc>{baseUrl}/Home/Privacy</loc>");
            sitemap.AppendLine($"    <lastmod>{DateTime.Now:yyyy-MM-dd}</lastmod>");
            sitemap.AppendLine("    <changefreq>monthly</changefreq>");
            sitemap.AppendLine("    <priority>0.6</priority>");
            sitemap.AppendLine("  </url>");

            // Shipping Policy
            sitemap.AppendLine("  <url>");
            sitemap.AppendLine($"    <loc>{baseUrl}/Home/ShippingPolicy</loc>");
            sitemap.AppendLine($"    <lastmod>{DateTime.Now:yyyy-MM-dd}</lastmod>");
            sitemap.AppendLine("    <changefreq>monthly</changefreq>");
            sitemap.AppendLine("    <priority>0.6</priority>");
            sitemap.AppendLine("  </url>");

            // Return Policy
            sitemap.AppendLine("  <url>");
            sitemap.AppendLine($"    <loc>{baseUrl}/Home/ReturnPolicy</loc>");
            sitemap.AppendLine($"    <lastmod>{DateTime.Now:yyyy-MM-dd}</lastmod>");
            sitemap.AppendLine("    <changefreq>monthly</changefreq>");
            sitemap.AppendLine("    <priority>0.6</priority>");
            sitemap.AppendLine("  </url>");

            // All active products
            var products = await _context.Products
                .Where(p => p.IsActive)
                .Select(p => new { p.Slug, p.ProductId, p.UpdatedAt })
                .ToListAsync();

            foreach (var product in products)
            {
                var productUrl = !string.IsNullOrEmpty(product.Slug)
                    ? $"{baseUrl}/san-pham/{product.Slug}"
                    : $"{baseUrl}/Product/Details/{product.ProductId}";

                sitemap.AppendLine("  <url>");
                sitemap.AppendLine($"    <loc>{XmlEscape(productUrl)}</loc>");
                sitemap.AppendLine($"    <lastmod>{product.UpdatedAt:yyyy-MM-dd}</lastmod>");
                sitemap.AppendLine("    <changefreq>weekly</changefreq>");
                sitemap.AppendLine("    <priority>0.8</priority>");
                sitemap.AppendLine("  </url>");
            }

            sitemap.AppendLine("</urlset>");

            return Content(sitemap.ToString(), "application/xml", Encoding.UTF8);
        }

        private string XmlEscape(string unescaped)
        {
            var doc = new XmlDocument();
            var node = doc.CreateElement("root");
            node.InnerText = unescaped;
            return node.InnerXml;
        }
    }
}
