namespace gView.Framework.system
{
    public class WildcardEx : global::System.Text.RegularExpressions.Regex
    {
        public WildcardEx(string pattern, global::System.Text.RegularExpressions.RegexOptions options)
            : base(WildcardToRegex(pattern), options)
        {
        }

        private static string WildcardToRegex(string pattern)
        {
            return "^" + global::System.Text.RegularExpressions.Regex.Escape(pattern).
                        Replace("\\*", ".*").Replace("\\?", ".") + "$";
        }
    }
}
