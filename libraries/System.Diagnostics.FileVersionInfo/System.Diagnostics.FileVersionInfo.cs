// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// ------------------------------------------------------------------------------
// Changes to this file must follow the https://aka.ms/api-review process.
// ------------------------------------------------------------------------------

namespace System.Diagnostics
{
    public sealed partial class FileVersionInfo
    {
        internal FileVersionInfo() { }
        public string? Comments { get { throw new System.PlatformNotSupportedException(); } }
        public string? CompanyName { get { throw new System.PlatformNotSupportedException(); } }
        public int FileBuildPart { get { throw new System.PlatformNotSupportedException(); } }
        public string? FileDescription { get { throw new System.PlatformNotSupportedException(); } }
        public int FileMajorPart { get { throw new System.PlatformNotSupportedException(); } }
        public int FileMinorPart { get { throw new System.PlatformNotSupportedException(); } }
        public string FileName { get { throw new System.PlatformNotSupportedException(); } }
        public int FilePrivatePart { get { throw new System.PlatformNotSupportedException(); } }
        public string? FileVersion { get { throw new System.PlatformNotSupportedException(); } }
        public string? InternalName { get { throw new System.PlatformNotSupportedException(); } }
        public bool IsDebug { get { throw new System.PlatformNotSupportedException(); } }
        public bool IsPatched { get { throw new System.PlatformNotSupportedException(); } }
        public bool IsPreRelease { get { throw new System.PlatformNotSupportedException(); } }
        public bool IsPrivateBuild { get { throw new System.PlatformNotSupportedException(); } }
        public bool IsSpecialBuild { get { throw new System.PlatformNotSupportedException(); } }
        public string? Language { get { throw new System.PlatformNotSupportedException(); } }
        public string? LegalCopyright { get { throw new System.PlatformNotSupportedException(); } }
        public string? LegalTrademarks { get { throw new System.PlatformNotSupportedException(); } }
        public string? OriginalFilename { get { throw new System.PlatformNotSupportedException(); } }
        public string? PrivateBuild { get { throw new System.PlatformNotSupportedException(); } }
        public int ProductBuildPart { get { throw new System.PlatformNotSupportedException(); } }
        public int ProductMajorPart { get { throw new System.PlatformNotSupportedException(); } }
        public int ProductMinorPart { get { throw new System.PlatformNotSupportedException(); } }
        public string? ProductName { get { throw new System.PlatformNotSupportedException(); } }
        public int ProductPrivatePart { get { throw new System.PlatformNotSupportedException(); } }
        public string? ProductVersion { get { throw new System.PlatformNotSupportedException(); } }
        public string? SpecialBuild { get { throw new System.PlatformNotSupportedException(); } }
        public static System.Diagnostics.FileVersionInfo GetVersionInfo(string fileName) { throw new System.PlatformNotSupportedException(); }
        public override string ToString() { throw new System.PlatformNotSupportedException(); }
    }
}
