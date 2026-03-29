namespace NetJs.Compiler
{
    public static class LibraryDoctorExtension
    {
        internal static string Comment(this string xml, string toComment)
        {
            var replacement = $"<!--{toComment}-->";
            return xml.Replace(toComment, replacement);
        }

        internal static string InsertAfter(this string xml, string toFind, string toInsert)
        {
            var i = xml.IndexOf(toFind);
            return xml.Substring(0, i + toFind.Length) + toInsert + xml.Substring(i + toFind.Length);
        }

        internal static string CommentTargetFrameworks(this string xml)
        {
            return xml.Replace("<TargetFrameworks>", "<!--<TargetFrameworks>")
                .Replace("</TargetFrameworks>", "</TargetFrameworks>-->");
        }
    }
}
