using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeScriptToCSharp.CSharp.Formatter
{
    public static class CommentFormatter
    {
        static int CountLeftSpace(string str)
        {
            int i = 0;
            int c = 0;
            while (i < str.Length)
            {
                if (str[i] == ' ')
                {
                    c++;
                }
                else if (str[i] == '\t')
                {
                    c += 4;
                }
                else
                    break;
                i++;
            }
            return c;
        }

        public static void Format(this string comment, ICSharpFormatter formatter)
        {
            string[] split = comment.Split(new char[] { '\n' });
            int minLeftSpace = split.Min(v => CountLeftSpace(v));
            for (int i = 0; i < split.Length; i++)
            {
                if ((i == 0 || i == split.Length - 1) && string.IsNullOrEmpty(split[i].Trim()))
                    continue;
                split[i] = split[i].Substring(minLeftSpace).TrimEnd();
                formatter.WriteLine($"///{split[i]}");
//                formatter.WriteLine($"///<para>{split[i]}</para>");
            }
        }

    }
}
