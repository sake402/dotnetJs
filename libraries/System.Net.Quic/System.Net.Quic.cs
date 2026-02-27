// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// ------------------------------------------------------------------------------
// Changes to this file must follow the https://aka.ms/api-review process.
// ------------------------------------------------------------------------------

namespace System.Net.Quic
{
    [System.FlagsAttribute]
    public enum QuicAbortDirection
    {
        Read = 1,
        Write = 2,
        Both = 3,
    }
    public sealed partial class QuicClientConnectionOptions : System.Net.Quic.QuicConnectionOptions
    {
        public QuicClientConnectionOptions() { }
        public System.Net.Security.SslClientAuthenticationOptions ClientAuthenticationOptions { get { throw new System.PlatformNotSupportedException(); } set { throw new System.PlatformNotSupportedException(); } }
        public System.Net.IPEndPoint? LocalEndPoint { get { throw new System.PlatformNotSupportedException(); } set { throw new System.PlatformNotSupportedException(); } }
        public System.Net.EndPoint RemoteEndPoint { get { throw new System.PlatformNotSupportedException(); } set { throw new System.PlatformNotSupportedException(); } }
    }
    public sealed partial class QuicConnection : System.IAsyncDisposable
    {
        internal QuicConnection() { }
        [Runtime.Versioning.SupportedOSPlatformGuard("windows")]
        [Runtime.Versioning.SupportedOSPlatformGuard("linux")]
        [Runtime.Versioning.SupportedOSPlatformGuard("osx")]
        public static bool IsSupported { get { throw new System.PlatformNotSupportedException(); } }
        public System.Net.IPEndPoint LocalEndPoint { get { throw new System.PlatformNotSupportedException(); } }
        public System.Net.Security.SslApplicationProtocol NegotiatedApplicationProtocol { get { throw new System.PlatformNotSupportedException(); } }
        [System.CLSCompliantAttribute(false)]
        public System.Net.Security.TlsCipherSuite NegotiatedCipherSuite { get { throw new System.PlatformNotSupportedException(); } }
        public System.Security.Authentication.SslProtocols SslProtocol { get { throw new System.PlatformNotSupportedException(); } }
        public System.Security.Cryptography.X509Certificates.X509Certificate? RemoteCertificate { get { throw new System.PlatformNotSupportedException(); } }
        public System.Net.IPEndPoint RemoteEndPoint { get { throw new System.PlatformNotSupportedException(); } }
        public string TargetHostName { get { throw new System.PlatformNotSupportedException(); } }
        public System.Threading.Tasks.ValueTask<System.Net.Quic.QuicStream> AcceptInboundStreamAsync(System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)) { throw new System.PlatformNotSupportedException(); }
        public System.Threading.Tasks.ValueTask CloseAsync(long errorCode, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)) { throw new System.PlatformNotSupportedException(); }
        public static System.Threading.Tasks.ValueTask<System.Net.Quic.QuicConnection> ConnectAsync(System.Net.Quic.QuicClientConnectionOptions options, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)) { throw new System.PlatformNotSupportedException(); }
        public System.Threading.Tasks.ValueTask DisposeAsync() { throw new System.PlatformNotSupportedException(); }
        public System.Threading.Tasks.ValueTask<System.Net.Quic.QuicStream> OpenOutboundStreamAsync(System.Net.Quic.QuicStreamType type, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)) { throw new System.PlatformNotSupportedException(); }
        public override string ToString() { throw new System.PlatformNotSupportedException(); }
    }
    public abstract partial class QuicConnectionOptions
    {
        internal QuicConnectionOptions() { }
        public long DefaultCloseErrorCode { get { throw new System.PlatformNotSupportedException(); } set { throw new System.PlatformNotSupportedException(); } }
        public long DefaultStreamErrorCode { get { throw new System.PlatformNotSupportedException(); } set { throw new System.PlatformNotSupportedException(); } }
        public System.TimeSpan HandshakeTimeout { get { throw new System.PlatformNotSupportedException(); } set { throw new System.PlatformNotSupportedException(); } }
        public System.TimeSpan IdleTimeout { get { throw new System.PlatformNotSupportedException(); } set { throw new System.PlatformNotSupportedException(); } }
        public System.Net.Quic.QuicReceiveWindowSizes InitialReceiveWindowSizes { get { throw new System.PlatformNotSupportedException(); } set { throw new System.PlatformNotSupportedException(); } }
        public System.TimeSpan KeepAliveInterval { get { throw new System.PlatformNotSupportedException(); } set { throw new System.PlatformNotSupportedException(); } }
        public int MaxInboundBidirectionalStreams { get { throw new System.PlatformNotSupportedException(); } set { throw new System.PlatformNotSupportedException(); } }
        public int MaxInboundUnidirectionalStreams { get { throw new System.PlatformNotSupportedException(); } set { throw new System.PlatformNotSupportedException(); } }
        public System.Action<System.Net.Quic.QuicConnection, System.Net.Quic.QuicStreamCapacityChangedArgs>? StreamCapacityCallback { get { throw new System.PlatformNotSupportedException(); } set { throw new System.PlatformNotSupportedException(); } }
    }
    public enum QuicError
    {
        Success = 0,
        InternalError = 1,
        ConnectionAborted = 2,
        StreamAborted = 3,
        ConnectionTimeout = 6,
        ConnectionRefused = 8,
        VersionNegotiationError = 9,
        ConnectionIdle = 10,
        OperationAborted = 12,
        AlpnInUse = 13,
        TransportError = 14,
        CallbackError = 15,
    }
    public sealed partial class QuicException : System.IO.IOException
    {
        public QuicException(System.Net.Quic.QuicError error, long? applicationErrorCode, string message) { }
        public long? ApplicationErrorCode { get { throw new System.PlatformNotSupportedException(); } }
        public System.Net.Quic.QuicError QuicError { get { throw new System.PlatformNotSupportedException(); } }
        public long? TransportErrorCode { get { throw new System.PlatformNotSupportedException(); } }
    }
    public sealed partial class QuicListener : System.IAsyncDisposable
    {
        internal QuicListener() { }
        [Runtime.Versioning.SupportedOSPlatformGuard("windows")]
        [Runtime.Versioning.SupportedOSPlatformGuard("linux")]
        [Runtime.Versioning.SupportedOSPlatformGuard("osx")]
        public static bool IsSupported { get { throw new System.PlatformNotSupportedException(); } }
        public System.Net.IPEndPoint LocalEndPoint { get { throw new System.PlatformNotSupportedException(); } }
        public System.Threading.Tasks.ValueTask<System.Net.Quic.QuicConnection> AcceptConnectionAsync(System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)) { throw new System.PlatformNotSupportedException(); }
        public System.Threading.Tasks.ValueTask DisposeAsync() { throw new System.PlatformNotSupportedException(); }
        public static System.Threading.Tasks.ValueTask<System.Net.Quic.QuicListener> ListenAsync(System.Net.Quic.QuicListenerOptions options, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)) { throw new System.PlatformNotSupportedException(); }
        public override string ToString() { throw new System.PlatformNotSupportedException(); }
    }
    public sealed partial class QuicListenerOptions
    {
        public QuicListenerOptions() { }
        public System.Collections.Generic.List<System.Net.Security.SslApplicationProtocol> ApplicationProtocols { get { throw new System.PlatformNotSupportedException(); } set { throw new System.PlatformNotSupportedException(); } }
        public System.Func<System.Net.Quic.QuicConnection, System.Net.Security.SslClientHelloInfo, System.Threading.CancellationToken, System.Threading.Tasks.ValueTask<System.Net.Quic.QuicServerConnectionOptions>> ConnectionOptionsCallback { get { throw new System.PlatformNotSupportedException(); } set { throw new System.PlatformNotSupportedException(); } }
        public int ListenBacklog { get { throw new System.PlatformNotSupportedException(); } set { throw new System.PlatformNotSupportedException(); } }
        public System.Net.IPEndPoint ListenEndPoint { get { throw new System.PlatformNotSupportedException(); } set { throw new System.PlatformNotSupportedException(); } }
    }
    public sealed partial class QuicReceiveWindowSizes
    {
        public QuicReceiveWindowSizes() { }
        public int Connection { get { throw new System.PlatformNotSupportedException(); } set { throw new System.PlatformNotSupportedException(); } }
        public int LocallyInitiatedBidirectionalStream { get { throw new System.PlatformNotSupportedException(); } set { throw new System.PlatformNotSupportedException(); } }
        public int RemotelyInitiatedBidirectionalStream { get { throw new System.PlatformNotSupportedException(); } set { throw new System.PlatformNotSupportedException(); } }
        public int UnidirectionalStream { get { throw new System.PlatformNotSupportedException(); } set { throw new System.PlatformNotSupportedException(); } }
    }
    public sealed partial class QuicServerConnectionOptions : System.Net.Quic.QuicConnectionOptions
    {
        public QuicServerConnectionOptions() { }
        public System.Net.Security.SslServerAuthenticationOptions ServerAuthenticationOptions { get { throw new System.PlatformNotSupportedException(); } set { throw new System.PlatformNotSupportedException(); } }
    }
    public sealed partial class QuicStream : System.IO.Stream
    {
        internal QuicStream() { }
        public override bool CanRead { get { throw new System.PlatformNotSupportedException(); } }
        public override bool CanSeek { get { throw new System.PlatformNotSupportedException(); } }
        public override bool CanTimeout { get { throw new System.PlatformNotSupportedException(); } }
        public override bool CanWrite { get { throw new System.PlatformNotSupportedException(); } }
        public long Id { get { throw new System.PlatformNotSupportedException(); } }
        public override long Length { get { throw new System.PlatformNotSupportedException(); } }
        public override long Position { get { throw new System.PlatformNotSupportedException(); } set { throw new System.PlatformNotSupportedException(); } }
        public System.Threading.Tasks.Task ReadsClosed { get { throw new System.PlatformNotSupportedException(); } }
        public override int ReadTimeout { get { throw new System.PlatformNotSupportedException(); } set { throw new System.PlatformNotSupportedException(); } }
        public System.Net.Quic.QuicStreamType Type { get { throw new System.PlatformNotSupportedException(); } }
        public System.Threading.Tasks.Task WritesClosed { get { throw new System.PlatformNotSupportedException(); } }
        public override int WriteTimeout { get { throw new System.PlatformNotSupportedException(); } set { throw new System.PlatformNotSupportedException(); } }
        public void Abort(System.Net.Quic.QuicAbortDirection abortDirection, long errorCode) { }
        public override System.IAsyncResult BeginRead(byte[] buffer, int offset, int count, System.AsyncCallback? callback, object? state) { throw new System.PlatformNotSupportedException(); }
        public override System.IAsyncResult BeginWrite(byte[] buffer, int offset, int count, System.AsyncCallback? callback, object? state) { throw new System.PlatformNotSupportedException(); }
        public void CompleteWrites() { }
        protected override void Dispose(bool disposing) { }
        public override System.Threading.Tasks.ValueTask DisposeAsync() { throw new System.PlatformNotSupportedException(); }
        public override int EndRead(System.IAsyncResult asyncResult) { throw new System.PlatformNotSupportedException(); }
        public override void EndWrite(System.IAsyncResult asyncResult) { }
        public override void Flush() { }
        public override System.Threading.Tasks.Task FlushAsync(System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)) { throw new System.PlatformNotSupportedException(); }
        public override int Read(byte[] buffer, int offset, int count) { throw new System.PlatformNotSupportedException(); }
        public override int Read(System.Span<byte> buffer) { throw new System.PlatformNotSupportedException(); }
        public override System.Threading.Tasks.Task<int> ReadAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)) { throw new System.PlatformNotSupportedException(); }
        public override System.Threading.Tasks.ValueTask<int> ReadAsync(System.Memory<byte> buffer, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)) { throw new System.PlatformNotSupportedException(); }
        public override int ReadByte() { throw new System.PlatformNotSupportedException(); }
        public override long Seek(long offset, System.IO.SeekOrigin origin) { throw new System.PlatformNotSupportedException(); }
        public override void SetLength(long value) { }
        public override string ToString() { throw new System.PlatformNotSupportedException(); }
        public override void Write(byte[] buffer, int offset, int count) { }
        public override void Write(System.ReadOnlySpan<byte> buffer) { }
        public override System.Threading.Tasks.Task WriteAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)) { throw new System.PlatformNotSupportedException(); }
        public System.Threading.Tasks.ValueTask WriteAsync(System.ReadOnlyMemory<byte> buffer, bool completeWrites, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)) { throw new System.PlatformNotSupportedException(); }
        public override System.Threading.Tasks.ValueTask WriteAsync(System.ReadOnlyMemory<byte> buffer, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken)) { throw new System.PlatformNotSupportedException(); }
        public override void WriteByte(byte value) { }
    }
    public readonly partial struct QuicStreamCapacityChangedArgs
    {
        private readonly object _dummy;
        private readonly int _dummyPrimitive;
        public int BidirectionalIncrement { get { throw new System.PlatformNotSupportedException(); } init { } }
        public int UnidirectionalIncrement { get { throw new System.PlatformNotSupportedException(); } init { } }
    }
    public enum QuicStreamType
    {
        Unidirectional = 0,
        Bidirectional = 1,
    }
}
