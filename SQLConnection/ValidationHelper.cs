using System.Text.RegularExpressions;

namespace SQLConnection
{
    public static class ValidationHelper
    {
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                return Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$");
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidMobile(string mobile)
        {
            if (string.IsNullOrWhiteSpace(mobile)) return false;
            return Regex.IsMatch(mobile, "^\\d{7,15}$");
        }
    }
}
