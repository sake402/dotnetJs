// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// ------------------------------------------------------------------------------
// Changes to this file must follow the https://aka.ms/api-review process.
// ------------------------------------------------------------------------------

namespace Microsoft.Win32.SafeHandles
{
    public sealed partial class SafeAccessTokenHandle : System.Runtime.InteropServices.SafeHandle
    {
        public SafeAccessTokenHandle() : base (default(System.IntPtr), default(bool)) { }
        public SafeAccessTokenHandle(System.IntPtr handle) : base (default(System.IntPtr), default(bool)) { }
        public static Microsoft.Win32.SafeHandles.SafeAccessTokenHandle InvalidHandle { get { throw new System.PlatformNotSupportedException(); } }
        public override bool IsInvalid { get { throw new System.PlatformNotSupportedException(); } }
        protected override bool ReleaseHandle() { throw new System.PlatformNotSupportedException(); }
    }
}
namespace System.Security.Principal
{
    public sealed partial class IdentityNotMappedException : System.SystemException
    {
        public IdentityNotMappedException() { }
        public IdentityNotMappedException(string? message) { }
        public IdentityNotMappedException(string? message, System.Exception? inner) { }
        public System.Security.Principal.IdentityReferenceCollection UnmappedIdentities { get { throw new System.PlatformNotSupportedException(); } }
        [System.ObsoleteAttribute("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.", DiagnosticId = "SYSLIB0051", UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext) { }
    }
    public abstract partial class IdentityReference
    {
        internal IdentityReference() { }
        public abstract string Value { get; }
        public abstract override bool Equals(object? o);
        public abstract override int GetHashCode();
        public abstract bool IsValidTargetType(System.Type targetType);
        public static bool operator ==(System.Security.Principal.IdentityReference? left, System.Security.Principal.IdentityReference? right) { throw new System.PlatformNotSupportedException(); }
        public static bool operator !=(System.Security.Principal.IdentityReference? left, System.Security.Principal.IdentityReference? right) { throw new System.PlatformNotSupportedException(); }
        public abstract override string ToString();
        public abstract System.Security.Principal.IdentityReference Translate(System.Type targetType);
    }
    public partial class IdentityReferenceCollection : System.Collections.Generic.ICollection<System.Security.Principal.IdentityReference>, System.Collections.Generic.IEnumerable<System.Security.Principal.IdentityReference>, System.Collections.IEnumerable
    {
        public IdentityReferenceCollection() { }
        public IdentityReferenceCollection(int capacity) { }
        public int Count { get { throw new System.PlatformNotSupportedException(); } }
        public System.Security.Principal.IdentityReference this[int index] { get { throw new System.PlatformNotSupportedException(); } set { throw new System.PlatformNotSupportedException(); } }
        bool System.Collections.Generic.ICollection<System.Security.Principal.IdentityReference>.IsReadOnly { get { throw new System.PlatformNotSupportedException(); } }
        public void Add(System.Security.Principal.IdentityReference identity) { }
        public void Clear() { }
        public bool Contains(System.Security.Principal.IdentityReference identity) { throw new System.PlatformNotSupportedException(); }
        public void CopyTo(System.Security.Principal.IdentityReference[] array, int offset) { }
        public System.Collections.Generic.IEnumerator<System.Security.Principal.IdentityReference> GetEnumerator() { throw new System.PlatformNotSupportedException(); }
        public bool Remove(System.Security.Principal.IdentityReference identity) { throw new System.PlatformNotSupportedException(); }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { throw new System.PlatformNotSupportedException(); }
        public System.Security.Principal.IdentityReferenceCollection Translate(System.Type targetType) { throw new System.PlatformNotSupportedException(); }
        public System.Security.Principal.IdentityReferenceCollection Translate(System.Type targetType, bool forceSuccess) { throw new System.PlatformNotSupportedException(); }
    }
    public sealed partial class NTAccount : System.Security.Principal.IdentityReference
    {
        public NTAccount(string name) { }
        public NTAccount(string domainName, string accountName) { }
        public override string Value { get { throw new System.PlatformNotSupportedException(); } }
        public override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] object? o) { throw new System.PlatformNotSupportedException(); }
        public override int GetHashCode() { throw new System.PlatformNotSupportedException(); }
        public override bool IsValidTargetType(System.Type targetType) { throw new System.PlatformNotSupportedException(); }
        public static bool operator ==(System.Security.Principal.NTAccount? left, System.Security.Principal.NTAccount? right) { throw new System.PlatformNotSupportedException(); }
        public static bool operator !=(System.Security.Principal.NTAccount? left, System.Security.Principal.NTAccount? right) { throw new System.PlatformNotSupportedException(); }
        public override string ToString() { throw new System.PlatformNotSupportedException(); }
        public override System.Security.Principal.IdentityReference Translate(System.Type targetType) { throw new System.PlatformNotSupportedException(); }
    }
    public sealed partial class SecurityIdentifier : System.Security.Principal.IdentityReference, System.IComparable<System.Security.Principal.SecurityIdentifier>
    {
        public static readonly int MaxBinaryLength;
        public static readonly int MinBinaryLength;
        public SecurityIdentifier(byte[] binaryForm, int offset) { }
        public SecurityIdentifier(System.IntPtr binaryForm) { }
        public SecurityIdentifier(System.Security.Principal.WellKnownSidType sidType, System.Security.Principal.SecurityIdentifier? domainSid) { }
        public SecurityIdentifier(string sddlForm) { }
        public System.Security.Principal.SecurityIdentifier? AccountDomainSid { get { throw new System.PlatformNotSupportedException(); } }
        public int BinaryLength { get { throw new System.PlatformNotSupportedException(); } }
        public override string Value { get { throw new System.PlatformNotSupportedException(); } }
        public int CompareTo(System.Security.Principal.SecurityIdentifier? sid) { throw new System.PlatformNotSupportedException(); }
        public override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhenAttribute(true)] object? o) { throw new System.PlatformNotSupportedException(); }
        public bool Equals(System.Security.Principal.SecurityIdentifier sid) { throw new System.PlatformNotSupportedException(); }
        public void GetBinaryForm(byte[] binaryForm, int offset) { }
        public override int GetHashCode() { throw new System.PlatformNotSupportedException(); }
        public bool IsAccountSid() { throw new System.PlatformNotSupportedException(); }
        public bool IsEqualDomainSid(System.Security.Principal.SecurityIdentifier sid) { throw new System.PlatformNotSupportedException(); }
        public override bool IsValidTargetType(System.Type targetType) { throw new System.PlatformNotSupportedException(); }
        public bool IsWellKnown(System.Security.Principal.WellKnownSidType type) { throw new System.PlatformNotSupportedException(); }
        public static bool operator ==(System.Security.Principal.SecurityIdentifier? left, System.Security.Principal.SecurityIdentifier? right) { throw new System.PlatformNotSupportedException(); }
        public static bool operator !=(System.Security.Principal.SecurityIdentifier? left, System.Security.Principal.SecurityIdentifier? right) { throw new System.PlatformNotSupportedException(); }
        public override string ToString() { throw new System.PlatformNotSupportedException(); }
        public override System.Security.Principal.IdentityReference Translate(System.Type targetType) { throw new System.PlatformNotSupportedException(); }
    }
    [System.FlagsAttribute]
    public enum TokenAccessLevels
    {
        AssignPrimary = 1,
        Duplicate = 2,
        Impersonate = 4,
        Query = 8,
        QuerySource = 16,
        AdjustPrivileges = 32,
        AdjustGroups = 64,
        AdjustDefault = 128,
        AdjustSessionId = 256,
        Read = 131080,
        Write = 131296,
        AllAccess = 983551,
        MaximumAllowed = 33554432,
    }
    public enum WellKnownSidType
    {
        NullSid = 0,
        WorldSid = 1,
        LocalSid = 2,
        CreatorOwnerSid = 3,
        CreatorGroupSid = 4,
        CreatorOwnerServerSid = 5,
        CreatorGroupServerSid = 6,
        NTAuthoritySid = 7,
        DialupSid = 8,
        NetworkSid = 9,
        BatchSid = 10,
        InteractiveSid = 11,
        ServiceSid = 12,
        AnonymousSid = 13,
        ProxySid = 14,
        EnterpriseControllersSid = 15,
        SelfSid = 16,
        AuthenticatedUserSid = 17,
        RestrictedCodeSid = 18,
        TerminalServerSid = 19,
        RemoteLogonIdSid = 20,
        LogonIdsSid = 21,
        LocalSystemSid = 22,
        LocalServiceSid = 23,
        NetworkServiceSid = 24,
        BuiltinDomainSid = 25,
        BuiltinAdministratorsSid = 26,
        BuiltinUsersSid = 27,
        BuiltinGuestsSid = 28,
        BuiltinPowerUsersSid = 29,
        BuiltinAccountOperatorsSid = 30,
        BuiltinSystemOperatorsSid = 31,
        BuiltinPrintOperatorsSid = 32,
        BuiltinBackupOperatorsSid = 33,
        BuiltinReplicatorSid = 34,
        BuiltinPreWindows2000CompatibleAccessSid = 35,
        BuiltinRemoteDesktopUsersSid = 36,
        BuiltinNetworkConfigurationOperatorsSid = 37,
        AccountAdministratorSid = 38,
        AccountGuestSid = 39,
        AccountKrbtgtSid = 40,
        AccountDomainAdminsSid = 41,
        AccountDomainUsersSid = 42,
        AccountDomainGuestsSid = 43,
        AccountComputersSid = 44,
        AccountControllersSid = 45,
        AccountCertAdminsSid = 46,
        AccountSchemaAdminsSid = 47,
        AccountEnterpriseAdminsSid = 48,
        AccountPolicyAdminsSid = 49,
        AccountRasAndIasServersSid = 50,
        NtlmAuthenticationSid = 51,
        DigestAuthenticationSid = 52,
        SChannelAuthenticationSid = 53,
        ThisOrganizationSid = 54,
        OtherOrganizationSid = 55,
        BuiltinIncomingForestTrustBuildersSid = 56,
        BuiltinPerformanceMonitoringUsersSid = 57,
        BuiltinPerformanceLoggingUsersSid = 58,
        BuiltinAuthorizationAccessSid = 59,
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.ObsoleteAttribute("This member has been deprecated and is only maintained for backwards compatability. WellKnownSidType values greater than MaxDefined may be defined in future releases.")]
        MaxDefined = 60,
        WinBuiltinTerminalServerLicenseServersSid = 60,
        WinBuiltinDCOMUsersSid = 61,
        WinBuiltinIUsersSid = 62,
        WinIUserSid = 63,
        WinBuiltinCryptoOperatorsSid = 64,
        WinUntrustedLabelSid = 65,
        WinLowLabelSid = 66,
        WinMediumLabelSid = 67,
        WinHighLabelSid = 68,
        WinSystemLabelSid = 69,
        WinWriteRestrictedCodeSid = 70,
        WinCreatorOwnerRightsSid = 71,
        WinCacheablePrincipalsGroupSid = 72,
        WinNonCacheablePrincipalsGroupSid = 73,
        WinEnterpriseReadonlyControllersSid = 74,
        WinAccountReadonlyControllersSid = 75,
        WinBuiltinEventLogReadersGroup = 76,
        WinNewEnterpriseReadonlyControllersSid = 77,
        WinBuiltinCertSvcDComAccessGroup = 78,
        WinMediumPlusLabelSid = 79,
        WinLocalLogonSid = 80,
        WinConsoleLogonSid = 81,
        WinThisOrganizationCertificateSid = 82,
        WinApplicationPackageAuthoritySid = 83,
        WinBuiltinAnyPackageSid = 84,
        WinCapabilityInternetClientSid = 85,
        WinCapabilityInternetClientServerSid = 86,
        WinCapabilityPrivateNetworkClientServerSid = 87,
        WinCapabilityPicturesLibrarySid = 88,
        WinCapabilityVideosLibrarySid = 89,
        WinCapabilityMusicLibrarySid = 90,
        WinCapabilityDocumentsLibrarySid = 91,
        WinCapabilitySharedUserCertificatesSid = 92,
        WinCapabilityEnterpriseAuthenticationSid = 93,
        WinCapabilityRemovableStorageSid = 94,
    }
    public enum WindowsAccountType
    {
        Normal = 0,
        Guest = 1,
        System = 2,
        Anonymous = 3,
    }
    public enum WindowsBuiltInRole
    {
        Administrator = 544,
        User = 545,
        Guest = 546,
        PowerUser = 547,
        AccountOperator = 548,
        SystemOperator = 549,
        PrintOperator = 550,
        BackupOperator = 551,
        Replicator = 552,
    }
    public partial class WindowsIdentity : System.Security.Claims.ClaimsIdentity, System.IDisposable, System.Runtime.Serialization.IDeserializationCallback, System.Runtime.Serialization.ISerializable
    {
        public new const string DefaultIssuer = "AD AUTHORITY";
        public WindowsIdentity(System.IntPtr userToken) { }
        public WindowsIdentity(System.IntPtr userToken, string type) { }
        public WindowsIdentity(System.IntPtr userToken, string type, System.Security.Principal.WindowsAccountType acctType) { }
        public WindowsIdentity(System.IntPtr userToken, string type, System.Security.Principal.WindowsAccountType acctType, bool isAuthenticated) { }
        [System.ObsoleteAttribute("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.", DiagnosticId = "SYSLIB0051", UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        public WindowsIdentity(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
        protected WindowsIdentity(System.Security.Principal.WindowsIdentity identity) { }
        public WindowsIdentity(string sUserPrincipalName) { }
        public Microsoft.Win32.SafeHandles.SafeAccessTokenHandle AccessToken { get { throw new System.PlatformNotSupportedException(); } }
        public sealed override string? AuthenticationType { get { throw new System.PlatformNotSupportedException(); } }
        public override System.Collections.Generic.IEnumerable<System.Security.Claims.Claim> Claims { get { throw new System.PlatformNotSupportedException(); } }
        public virtual System.Collections.Generic.IEnumerable<System.Security.Claims.Claim> DeviceClaims { get { throw new System.PlatformNotSupportedException(); } }
        public System.Security.Principal.IdentityReferenceCollection? Groups { get { throw new System.PlatformNotSupportedException(); } }
        public System.Security.Principal.TokenImpersonationLevel ImpersonationLevel { get { throw new System.PlatformNotSupportedException(); } }
        public virtual bool IsAnonymous { get { throw new System.PlatformNotSupportedException(); } }
        public override bool IsAuthenticated { get { throw new System.PlatformNotSupportedException(); } }
        public virtual bool IsGuest { get { throw new System.PlatformNotSupportedException(); } }
        public virtual bool IsSystem { get { throw new System.PlatformNotSupportedException(); } }
        public override string Name { get { throw new System.PlatformNotSupportedException(); } }
        public System.Security.Principal.SecurityIdentifier? Owner { get { throw new System.PlatformNotSupportedException(); } }
        public virtual System.IntPtr Token { get { throw new System.PlatformNotSupportedException(); } }
        public System.Security.Principal.SecurityIdentifier? User { get { throw new System.PlatformNotSupportedException(); } }
        public virtual System.Collections.Generic.IEnumerable<System.Security.Claims.Claim> UserClaims { get { throw new System.PlatformNotSupportedException(); } }
        public override System.Security.Claims.ClaimsIdentity Clone() { throw new System.PlatformNotSupportedException(); }
        public void Dispose() { }
        protected virtual void Dispose(bool disposing) { }
        public static System.Security.Principal.WindowsIdentity GetAnonymous() { throw new System.PlatformNotSupportedException(); }
        public static System.Security.Principal.WindowsIdentity GetCurrent() { throw new System.PlatformNotSupportedException(); }
        public static System.Security.Principal.WindowsIdentity? GetCurrent(bool ifImpersonating) { throw new System.PlatformNotSupportedException(); }
        public static System.Security.Principal.WindowsIdentity GetCurrent(System.Security.Principal.TokenAccessLevels desiredAccess) { throw new System.PlatformNotSupportedException(); }
        public static void RunImpersonated(Microsoft.Win32.SafeHandles.SafeAccessTokenHandle safeAccessTokenHandle, System.Action action) { }
        public static System.Threading.Tasks.Task RunImpersonatedAsync(Microsoft.Win32.SafeHandles.SafeAccessTokenHandle safeAccessTokenHandle, System.Func<System.Threading.Tasks.Task> func) { throw new System.PlatformNotSupportedException(); }
        public static System.Threading.Tasks.Task<T> RunImpersonatedAsync<T>(Microsoft.Win32.SafeHandles.SafeAccessTokenHandle safeAccessTokenHandle, System.Func<System.Threading.Tasks.Task<T>> func) { throw new System.PlatformNotSupportedException(); }
        public static T RunImpersonated<T>(Microsoft.Win32.SafeHandles.SafeAccessTokenHandle safeAccessTokenHandle, System.Func<T> func) { throw new System.PlatformNotSupportedException(); }
        void System.Runtime.Serialization.IDeserializationCallback.OnDeserialization(object? sender) { }
        void System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
    }
    public partial class WindowsPrincipal : System.Security.Claims.ClaimsPrincipal
    {
        public WindowsPrincipal(System.Security.Principal.WindowsIdentity ntIdentity) { }
        public virtual System.Collections.Generic.IEnumerable<System.Security.Claims.Claim> DeviceClaims { get { throw new System.PlatformNotSupportedException(); } }
        public override System.Security.Principal.IIdentity Identity { get { throw new System.PlatformNotSupportedException(); } }
        public virtual System.Collections.Generic.IEnumerable<System.Security.Claims.Claim> UserClaims { get { throw new System.PlatformNotSupportedException(); } }
        public virtual bool IsInRole(int rid) { throw new System.PlatformNotSupportedException(); }
        public virtual bool IsInRole(System.Security.Principal.SecurityIdentifier sid) { throw new System.PlatformNotSupportedException(); }
        public virtual bool IsInRole(System.Security.Principal.WindowsBuiltInRole role) { throw new System.PlatformNotSupportedException(); }
        public override bool IsInRole(string role) { throw new System.PlatformNotSupportedException(); }
    }
}
