// ***********************************************************************
//  Assembly          : RzR.MiddleWares.ETagMW
//  Author            : RzR
//  Created           : 07-05-2026 17:05
// 
//  Last Modified By : RzR
//  Last Modified On : 13-05-2026 23:09
//  ***********************************************************************
//  <copyright file="ResponseBufferingStream.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace RzR.Web.Middleware.ETag.Internal
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Response buffering stream with a bounded in-memory buffer.
    /// </summary>
    /// <seealso cref="T:System.IO.Stream"/>
    /// =================================================================================================
    internal sealed class ResponseBufferingStream : Stream
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable)
        ///     In-memory buffer.
        /// </summary>
        /// =================================================================================================
        private readonly MemoryStream _bufferStream;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable)
        ///     Inner response stream.
        /// </summary>
        /// =================================================================================================
        private readonly Stream _innerStream;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable)
        ///     Max allowed buffer size.
        /// </summary>
        /// =================================================================================================
        private readonly long _maxBufferSize;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     (Immutable)
        ///     Callback invoked when the response starts writing to the inner stream.
        /// </summary>
        /// =================================================================================================
        private readonly Action _onResponseStart;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Flag indicating whether response start was already reported.
        /// </summary>
        /// =================================================================================================
        private bool _responseStarted;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="ResponseBufferingStream" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when one or more arguments are outside the required range.
        /// </exception>
        /// <param name="innerStream">Inner response stream.</param>
        /// <param name="maxBufferSize">Maximum buffer size.</param>
        /// <param name="onResponseStart">(Optional) Response start callback.</param>
        /// =================================================================================================
        internal ResponseBufferingStream(Stream innerStream, long maxBufferSize, Action onResponseStart = null)
        {
            _innerStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));

            if (maxBufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxBufferSize));

            _maxBufferSize = maxBufferSize;
            _onResponseStart = onResponseStart;
            _bufferStream = BufferingStreamManager.GetStream();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets a value indicating whether buffering is still active.
        /// </summary>
        /// <value>
        ///     True if the buffering is enabled, false if not.
        /// </value>
        /// =================================================================================================
        internal bool IsBufferingEnabled { get; private set; } = true;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets in-memory buffer stream.
        /// </summary>
        /// <value>
        ///     The buffer.
        /// </value>
        /// =================================================================================================
        internal MemoryStream Buffer => _bufferStream;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets buffered length.
        /// </summary>
        /// <value>
        ///     The length of the buffered.
        /// </value>
        /// =================================================================================================
        internal long BufferedLength => _bufferStream.Length;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <value>
        ///     true if the stream supports reading; otherwise, false.
        /// </value>
        /// =================================================================================================
        public override bool CanRead => false;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <value>
        ///     true if the stream supports seeking; otherwise, false.
        /// </value>
        /// =================================================================================================
        public override bool CanSeek => false;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <value>
        ///     true if the stream supports writing; otherwise, false.
        /// </value>
        /// =================================================================================================
        public override bool CanWrite => true;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the length in bytes of the stream.
        /// </summary>
        /// <value>
        ///     A long value representing the length of the stream in bytes.
        /// </value>
        /// =================================================================================================
        public override long Length => throw new NotSupportedException();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets the position within the current stream.
        /// </summary>
        /// <exception cref="NotSupportedException">
        ///     Thrown when the requested operation is not supported.
        /// </exception>
        /// <value>
        ///     The current position within the stream.
        /// </value>
        /// =================================================================================================
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Flush stream.
        /// </summary>
        /// =================================================================================================
        public override void Flush()
        {
            if (!IsBufferingEnabled)
                _innerStream.Flush();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Flush stream.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        ///     A Task.
        /// </returns>
        /// =================================================================================================
        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            if (!IsBufferingEnabled)
                await _innerStream.FlushAsync(cancellationToken);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Read is not supported.
        /// </summary>
        /// <exception cref="NotSupportedException">
        ///     Thrown when the requested operation is not supported.
        /// </exception>
        /// <param name="buffer">Buffer.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="count">Count.</param>
        /// <returns>
        ///     An int.
        /// </returns>
        /// =================================================================================================
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Seek is not supported.
        /// </summary>
        /// <exception cref="NotSupportedException">
        ///     Thrown when the requested operation is not supported.
        /// </exception>
        /// <param name="offset">Offset.</param>
        /// <param name="origin">Origin.</param>
        /// <returns>
        ///     A long.
        /// </returns>
        /// =================================================================================================
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Set length is not supported.
        /// </summary>
        /// <exception cref="NotSupportedException">
        ///     Thrown when the requested operation is not supported.
        /// </exception>
        /// <param name="value">Length value.</param>
        /// =================================================================================================
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Write bytes to stream.
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="count">Count.</param>
        /// =================================================================================================
        public override void Write(byte[] buffer, int offset, int count)
        {
            ValidateWriteArguments(buffer, offset, count);

            if (TryWriteToBuffer(buffer, offset, count))
                return;

            DisableBuffering();
            _innerStream.Write(buffer, offset, count);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Write bytes to stream asynchronously.
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="count">Count.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        ///     A Task.
        /// </returns>
        /// =================================================================================================
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ValidateWriteArguments(buffer, offset, count);

            if (TryWriteToBuffer(buffer, offset, count))
                return;

            await DisableBufferingAsync(cancellationToken);
            await _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Write a single byte to stream.
        /// </summary>
        /// <param name="value">Byte value.</param>
        /// =================================================================================================
        public override void WriteByte(byte value)
        {
            if (TryWriteToBuffer(value))
                return;

            DisableBuffering();
            _innerStream.WriteByte(value);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Copy buffered content to the inner stream.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        ///     A Task.
        /// </returns>
        /// =================================================================================================
        internal Task CopyBufferedToInnerAsync(CancellationToken cancellationToken)
        {
            if (!IsBufferingEnabled)
                return Task.CompletedTask;

            NotifyResponseStart();
            return FlushBufferToInnerAsync(cancellationToken);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Dispose stream.
        /// </summary>
        /// <param name="disposing">Dispose flag.</param>
        /// =================================================================================================
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _bufferStream.Dispose();

            base.Dispose(disposing);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Try to write data to the in-memory buffer.
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="count">Count.</param>
        /// <returns>
        ///     True if it succeeds, false if it fails.
        /// </returns>
        /// =================================================================================================
        private bool TryWriteToBuffer(byte[] buffer, int offset, int count)
        {
            if (!IsBufferingEnabled)
                return false;

            if (_bufferStream.Length + count > _maxBufferSize)
                return false;

            _bufferStream.Write(buffer, offset, count);

            return true;
        }

#if !NETSTANDARD2_0
        /// <summary>
        ///     Try to write span data to the in-memory buffer.
        /// </summary>
        /// <param name="buffer">Buffer span</param>
        /// <returns></returns>
        private bool TryWriteToBuffer(ReadOnlySpan<byte> buffer)
        {
            if (!IsBufferingEnabled)
                return false;

            if (_bufferStream.Length + buffer.Length > _maxBufferSize)
                return false;

            _bufferStream.Write(buffer);

            return true;
        }
#endif

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Try to write a single byte to the in-memory buffer.
        /// </summary>
        /// <param name="value">Byte value.</param>
        /// <returns>
        ///     True if it succeeds, false if it fails.
        /// </returns>
        /// =================================================================================================
        private bool TryWriteToBuffer(byte value)
        {
            if (!IsBufferingEnabled)
                return false;

            if (_bufferStream.Length + 1 > _maxBufferSize)
                return false;

            _bufferStream.WriteByte(value);

            return true;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Disable buffering and flush buffered content.
        /// </summary>
        /// =================================================================================================
        internal void DisableBuffering()
        {
            if (!IsBufferingEnabled)
                return;

            IsBufferingEnabled = false;
            NotifyResponseStart();
            FlushBufferToInner();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Disable buffering and flush buffered content asynchronously.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        ///     A Task.
        /// </returns>
        /// =================================================================================================
        internal async Task DisableBufferingAsync(CancellationToken cancellationToken)
        {
            if (!IsBufferingEnabled)
                return;

            IsBufferingEnabled = false;
            NotifyResponseStart();
            await FlushBufferToInnerAsync(cancellationToken);
        }

#if !NETSTANDARD2_0
        /// <summary>
        ///     Write directly to the inner stream after disabling buffering.
        /// </summary>
        /// <param name="buffer">Buffer memory</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        private async ValueTask WriteDirectAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
        {
            await DisableBufferingAsync(cancellationToken);
            await _innerStream.WriteAsync(buffer, cancellationToken);
        }
#endif

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Notify that the response is about to start.
        /// </summary>
        /// =================================================================================================
        private void NotifyResponseStart()
        {
            if (_responseStarted)
                return;

            _responseStarted = true;
            _onResponseStart?.Invoke();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Flush buffered content to inner stream.
        /// </summary>
        /// =================================================================================================
        private void FlushBufferToInner()
        {
            if (_bufferStream.Length == 0)
                return;

            _bufferStream.Position = 0;
            _bufferStream.CopyTo(_innerStream);
            _bufferStream.SetLength(0);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Flush buffered content to inner stream asynchronously.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        ///     A Task.
        /// </returns>
        /// =================================================================================================
        private async Task FlushBufferToInnerAsync(CancellationToken cancellationToken)
        {
            if (_bufferStream.Length == 0)
                return;

            _bufferStream.Position = 0;
            await _bufferStream.CopyToAsync(_innerStream, 81920, cancellationToken);
            _bufferStream.SetLength(0);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Validate write arguments.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when one or more required arguments are null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when one or more arguments are outside the required range.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when one or more arguments have unsupported or illegal values.
        /// </exception>
        /// <param name="buffer">Buffer.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="count">Count.</param>
        /// =================================================================================================
        private static void ValidateWriteArguments(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (buffer.Length - offset < count)
                throw new ArgumentException("Invalid offset and count for provided buffer.", nameof(count));
        }

#if !NETSTANDARD2_0
        /// <summary>
        ///     Write bytes to stream.
        /// </summary>
        /// <param name="buffer">Buffer span</param>
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (TryWriteToBuffer(buffer))
                return;

            DisableBuffering();
            _innerStream.Write(buffer);
        }

        /// <summary>
        ///     Write bytes to stream asynchronously.
        /// </summary>
        /// <param name="buffer">Buffer memory</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (TryWriteToBuffer(buffer.Span))
                return default;

            return WriteDirectAsync(buffer, cancellationToken);
        }
#endif
    }
}