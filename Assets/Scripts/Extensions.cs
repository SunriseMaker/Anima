namespace Extensions
{
    static class Extensions
    {
        /// <summary>
        /// Get substring of specified number of characters on the right.
        /// </summary>
        public static string Right(this string value, int length)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            return value.Length <= length ? value : value.Substring(value.Length - length);
        }

        /// <summary>
        /// Get substring of specified number of characters on the left.
        /// </summary>
        public static string Left(this string value, int length)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            return value.Length <= length ? value : value.Substring(0, value.Length - length);
        }
    }
}
