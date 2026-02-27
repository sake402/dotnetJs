using System;
using System.Collections.Generic;
using System.Text;

namespace System.IO
{
    public abstract partial class Stream
    {
        [dotnetJs.MemberReplace(nameof(HasOverriddenBeginEndRead))]
        private bool HasOverriddenBeginEndReadImpl()
        {
            return GetType().GetMethod(nameof(BeginEndReadAsync))!.DeclaringType != GetType();
        }

        [dotnetJs.MemberReplace(nameof(HasOverriddenBeginEndWrite))]
        private bool HasOverriddenBeginEndWriteImpl()
        {
            return GetType().GetMethod(nameof(BeginEndWriteAsync))!.DeclaringType != GetType();
        }
    }
}
