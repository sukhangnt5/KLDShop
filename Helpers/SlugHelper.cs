using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace KLDShop.Helpers
{
    public static class SlugHelper
    {
        /// <summary>
        /// Chuyển đổi text thành URL-friendly slug
        /// </summary>
        public static string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Chuyển về chữ thường
            text = text.ToLowerInvariant();

            // Chuyển đổi tiếng Việt có dấu sang không dấu
            text = RemoveVietnameseDiacritics(text);

            // Xóa các ký tự đặc biệt, chỉ giữ chữ, số, dấu gạch ngang
            text = Regex.Replace(text, @"[^a-z0-9\s-]", "");

            // Chuyển khoảng trắng thành dấu gạch ngang
            text = Regex.Replace(text, @"\s+", "-");

            // Xóa dấu gạch ngang trùng lặp
            text = Regex.Replace(text, @"-+", "-");

            // Xóa dấu gạch ngang ở đầu và cuối
            text = text.Trim('-');

            return text;
        }

        /// <summary>
        /// Xóa dấu tiếng Việt
        /// </summary>
        private static string RemoveVietnameseDiacritics(string text)
        {
            string[] vietnameseSigns = new string[]
            {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ",
                "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ",
                "ÍÌỊỈĨ",
                "đ",
                "Đ",
                "ýỳỵỷỹ",
                "ÝỲỴỶỸ"
            };

            for (int i = 1; i < vietnameseSigns.Length; i++)
            {
                for (int j = 0; j < vietnameseSigns[i].Length; j++)
                {
                    text = text.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
                }
            }

            return text;
        }

        /// <summary>
        /// Tạo slug unique bằng cách thêm số suffix nếu trùng
        /// </summary>
        public static string GenerateUniqueSlug(string baseSlug, Func<string, bool> checkExists)
        {
            string slug = baseSlug;
            int counter = 1;

            while (checkExists(slug))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }
    }
}
