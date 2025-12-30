using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KLDShop.Models;
using KLDShop.Data;
using KLDShop.Helpers;

namespace KLDShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang chủ - Danh sách sản phẩm nổi bật
        public async Task<IActionResult> Index(int page = 1, string search = "", string sortBy = "latest")
        {
            const int pageSize = 6;
            var query = _context.Products.Where(p => p.IsActive);

            // Search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.ProductName.Contains(search) || (p.Description != null && p.Description.Contains(search)));
            }

            // Sorting
            query = sortBy switch
            {
                "popular" => query.OrderByDescending(p => p.Views),
                "price-asc" => query.OrderBy(p => p.Price),
                "price-desc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchTerm = search;
            ViewBag.SortBy = sortBy;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(products);
        }

        // Danh sách sản phẩm với filter
        public async Task<IActionResult> List(int page = 1, int categoryId = 0, decimal? minPrice = null, decimal? maxPrice = null, string sortBy = "latest", string search = "")
        {
            const int pageSize = 12;
            var query = _context.Products.Where(p => p.IsActive);

            // Search filter (case-insensitive)
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(p => p.ProductName.ToLower().Contains(search) || (p.Description != null && p.Description.ToLower().Contains(search)));
            }

            // Category filter
            if (categoryId > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            // Price range filter
            if (minPrice.HasValue && minPrice > 0)
            {
                query = query.Where(p => p.Price >= minPrice);
            }

            if (maxPrice.HasValue && maxPrice > 0)
            {
                query = query.Where(p => p.Price <= maxPrice);
            }

            // Sorting
            query = sortBy switch
            {
                "popular" => query.OrderByDescending(p => p.Views),
                "price-asc" => query.OrderBy(p => p.Price),
                "price-desc" => query.OrderByDescending(p => p.Price),
                "rating" => query.OrderByDescending(p => p.ProductId), // TODO: Add rating column
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get all categories for dropdown
            var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            
            ViewBag.SearchTerm = search;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.Categories = categories;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SortBy = sortBy;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            // SEO Pagination
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var queryParams = new Dictionary<string, string>();
            
            if (categoryId > 0) queryParams["categoryId"] = categoryId.ToString();
            if (minPrice.HasValue) queryParams["minPrice"] = minPrice.Value.ToString();
            if (maxPrice.HasValue) queryParams["maxPrice"] = maxPrice.Value.ToString();
            if (!string.IsNullOrEmpty(sortBy) && sortBy != "latest") queryParams["sortBy"] = sortBy;
            if (!string.IsNullOrEmpty(search)) queryParams["search"] = search;

            // Canonical URL (without page parameter for page 1)
            var canonicalUrl = baseUrl;
            if (page > 1)
            {
                queryParams["page"] = page.ToString();
            }
            if (queryParams.Any())
            {
                canonicalUrl += "?" + string.Join("&", queryParams.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
            }
            ViewData["CanonicalUrl"] = canonicalUrl;

            // Previous page link
            if (page > 1)
            {
                var prevParams = new Dictionary<string, string>(queryParams);
                if (page > 2)
                {
                    prevParams["page"] = (page - 1).ToString();
                }
                else
                {
                    prevParams.Remove("page"); // Page 1 doesn't need page parameter
                }
                var prevUrl = baseUrl + (prevParams.Any() ? "?" + string.Join("&", prevParams.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}")) : "");
                ViewData["PrevPageUrl"] = prevUrl;
            }

            // Next page link
            if (page < totalPages)
            {
                var nextParams = new Dictionary<string, string>(queryParams);
                nextParams["page"] = (page + 1).ToString();
                var nextUrl = baseUrl + "?" + string.Join("&", nextParams.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
                ViewData["NextPageUrl"] = nextUrl;
            }

            // SEO Meta
            ViewData["Title"] = page > 1 ? $"Danh Sách Sản Phẩm - Trang {page}" : "Danh Sách Sản Phẩm";
            ViewData["MetaDescription"] = $"Khám phá {totalItems} sản phẩm chất lượng cao tại KLDShop. Giao hàng nhanh, giá tốt nhất thị trường.";

            return View(products);
        }

        // Chi tiết sản phẩm - By ID (legacy support)
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.ProductId == id && p.IsActive);

            if (product == null)
            {
                return NotFound();
            }

            // Redirect to slug URL if slug exists
            if (!string.IsNullOrEmpty(product.Slug))
            {
                return RedirectToActionPermanent("DetailsBySlug", new { slug = product.Slug });
            }

            return await RenderProductDetails(product);
        }

        // Chi tiết sản phẩm - By Slug (SEO-friendly)
        [HttpGet]
        [Route("san-pham/{slug}")]
        public async Task<IActionResult> DetailsBySlug(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);

            if (product == null)
            {
                return NotFound();
            }

            return await RenderProductDetails(product);
        }

        // Helper method to render product details with SEO
        private async Task<IActionResult> RenderProductDetails(Product product)
        {
            // Update views count
            product.Views++;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            // Get related products (same category or manufacturer)
            var relatedProducts = await _context.Products
                .Where(p => (p.CategoryId == product.CategoryId || p.Manufacturer == product.Manufacturer) 
                    && p.ProductId != product.ProductId && p.IsActive)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;

            // SEO Meta Tags
            var productUrl = Url.Action("DetailsBySlug", "Product", new { slug = product.Slug }, Request.Scheme);
            var productImage = !string.IsNullOrEmpty(product.Image) 
                ? $"{Request.Scheme}://{Request.Host}{product.Image}" 
                : $"{Request.Scheme}://{Request.Host}/images/no-image.jpg";

            ViewData["Title"] = product.ProductName;
            ViewData["MetaDescription"] = product.MetaDescription ?? 
                (product.Description?.Length > 160 ? product.Description.Substring(0, 157) + "..." : product.Description) ??
                $"Mua {product.ProductName} giá tốt tại KLDShop. Giao hàng nhanh, uy tín.";
            ViewData["MetaKeywords"] = product.MetaKeywords ?? 
                $"{product.ProductName}, {product.Category?.CategoryName ?? "sản phẩm"}, mua online";
            ViewData["CanonicalUrl"] = productUrl;

            // Open Graph tags for Facebook sharing
            ViewData["OgType"] = "product";
            ViewData["OgUrl"] = productUrl;
            ViewData["OgTitle"] = product.ProductName;
            ViewData["OgDescription"] = ViewData["MetaDescription"];
            ViewData["OgImage"] = productImage;

            return View("Details", product);
        }

        // API: Lấy danh sách sản phẩm (AJAX)
        [HttpGet]
        [Route("api/products")]
        public async Task<IActionResult> GetProducts(int page = 1, int pageSize = 12, string search = "", string sortBy = "latest")
        {
            var query = _context.Products.Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.ProductName.Contains(search) || (p.Description != null && p.Description.Contains(search)));
            }

            query = sortBy switch
            {
                "popular" => query.OrderByDescending(p => p.Views),
                "price-asc" => query.OrderBy(p => p.Price),
                "price-desc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    id = p.ProductId,
                    name = p.ProductName,
                    price = p.Price,
                    image = p.Image,
                    description = p.Description
                })
                .ToListAsync();

            return Json(new { success = true, data = products });
        }

        // API: Lấy chi tiết sản phẩm (AJAX)
        [HttpGet]
        [Route("api/products/{id}")]
        public async Task<IActionResult> GetProductDetail(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id && p.IsActive);

            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });
            }

            return Json(new
            {
                success = true,
                data = new
                {
                    id = product.ProductId,
                    name = product.ProductName,
                    description = product.Description,
                    price = product.Price,
                    discountPrice = product.DiscountPrice,
                    quantity = product.Quantity,
                    image = product.Image,
                    sku = product.SKU,
                    manufacturer = product.Manufacturer,
                    warranty = product.WarrantyPeriod
                }
            });
        }
    }
}
