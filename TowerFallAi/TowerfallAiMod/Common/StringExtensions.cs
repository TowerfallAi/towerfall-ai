namespace TowerfallAi.Common {
  public static class StringExtensions {
    public static string Format(this string s, params object[] args) {
      return string.Format(s, args);
    }

    public static string FirstLower(this string s) {
      if (string.IsNullOrWhiteSpace(s)) {
        return s;
      }
      return char.ToLowerInvariant(s[0]) + s.Substring(1, s.Length - 1);
    }
  }
}
