using dotnetJs;

namespace System.Text.RegularExpressions
{
    [dotnetJs.External]
    [dotnetJs.Name("RegExp")]
    [dotnetJs.Convention(Member = dotnetJs.ConventionMember.Field | dotnetJs.ConventionMember.Method | dotnetJs.ConventionMember.Property, Notation = dotnetJs.Notation.CamelCase)]
    public class RegExp
    {
        [Template("new RegExp({pattern})")]
        public extern RegExp(string pattern);

        [Template("new RegExp({pattern}, {flags})")]
        public extern RegExp(string pattern, string flags);


        public extern int LastIndex
        {
            get;
            set;
        }

        public extern bool Global
        {
            get;
        }

        public extern bool IgnoreCase
        {
            get;
        }

        public extern bool Multiline
        {
            get;
        }

        public extern string Source
        {
            get;
        }

        public extern RegexMatch Exec(string? s);

        public extern bool Test(string? s);
    }

    [dotnetJs.External]
    [dotnetJs.Name("RegexMatch")]
    [dotnetJs.Convention(Member = dotnetJs.ConventionMember.Field | dotnetJs.ConventionMember.Method | dotnetJs.ConventionMember.Property, Notation = dotnetJs.Notation.CamelCase)]
    public class RegexMatch
    {
        public int Index { get; set; }

        public int Length { get; set; }

        public string Input { get; set; }

        public string this[int index] { get { return null; } set { } }

        public static extern implicit operator string[] (RegexMatch rm);

        public static extern explicit operator RegexMatch(string[] a);
    }
}