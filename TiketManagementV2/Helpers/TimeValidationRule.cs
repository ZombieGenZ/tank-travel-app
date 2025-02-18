using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TiketManagementV2.Helpers
{
    public class TimeValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = value as string;
            if (string.IsNullOrWhiteSpace(input))
                return new ValidationResult(false, "Không được để trống!");

            // Định dạng hợp lệ: HH:mm:ss.fff (ví dụ: 14:30:00.123)
            if (!Regex.IsMatch(input, @"^(?:[01]\d|2[0-3]):[0-5]\d:[0-5]\d\.\d{3}$"))
                return new ValidationResult(false, "Sai định dạng! Đúng: HH:mm:ss.fff");

            return ValidationResult.ValidResult;
        }
    }
}
