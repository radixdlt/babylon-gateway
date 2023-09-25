/* Copyright 2021 Radix Publishing Ltd incorporated in Jersey (Channel Islands).
 *
 * Licensed under the Radix License, Version 1.0 (the "License"); you may not use this
 * file except in compliance with the License. You may obtain a copy of the License at:
 *
 * radixfoundation.org/licenses/LICENSE-v1
 *
 * The Licensor hereby grants permission for the Canonical version of the Work to be
 * published, distributed and used under or by reference to the Licensor’s trademark
 * Radix ® and use of any unregistered trade names, logos or get-up.
 *
 * The Licensor provides the Work (and each Contributor provides its Contributions) on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied,
 * including, without limitation, any warranties or conditions of TITLE, NON-INFRINGEMENT,
 * MERCHANTABILITY, or FITNESS FOR A PARTICULAR PURPOSE.
 *
 * Whilst the Work is capable of being deployed, used and adopted (instantiated) to create
 * a distributed ledger it is your responsibility to test and validate the code, together
 * with all logic and performance of that code under all foreseeable scenarios.
 *
 * The Licensor does not make or purport to make and hereby excludes liability for all
 * and any representation, warranty or undertaking in any form whatsoever, whether express
 * or implied, to any entity or person, including any representation, warranty or
 * undertaking, as to the functionality security use, value or other characteristics of
 * any distributed ledger nor in respect the functioning or value of any tokens which may
 * be created stored or transferred using the Work. The Licensor does not warrant that the
 * Work or any use of the Work complies with any law or regulation in any territory where
 * it may be implemented or used or that it will be appropriate for any specific purpose.
 *
 * Neither the licensor nor any current or former employees, officers, directors, partners,
 * trustees, representatives, agents, advisors, contractors, or volunteers of the Licensor
 * shall be liable for any direct or indirect, special, incidental, consequential or other
 * losses of any kind, in tort, contract or otherwise (including but not limited to loss
 * of revenue, income or profits, or loss of use or data, or loss of reputation, or loss
 * of any economic or other opportunity of whatsoever nature or howsoever arising), arising
 * out of or in connection with (without limitation of any use, misuse, of any ledger system
 * or use made or its functionality or any performance or operation of any code or protocol
 * caused by bugs or programming or logic errors or otherwise);
 *
 * A. any offer, purchase, holding, use, sale, exchange or transmission of any
 * cryptographic keys, tokens or assets created, exchanged, stored or arising from any
 * interaction with the Work;
 *
 * B. any failure in a transmission or loss of any token or assets keys or other digital
 * artefacts due to errors in transmission;
 *
 * C. bugs, hacks, logic errors or faults in the Work or any communication;
 *
 * D. system software or apparatus including but not limited to losses caused by errors
 * in holding or transmitting tokens by any third-party;
 *
 * E. breaches or failure of security including hacker attacks, loss or disclosure of
 * password, loss of private key, unauthorised use or misuse of such passwords or keys;
 *
 * F. any losses including loss of anticipated savings or other benefits resulting from
 * use of the Work or any changes to the Work (however implemented).
 *
 * You are solely responsible for; testing, validating and evaluation of all operation
 * logic, functionality, security and appropriateness of using the Work for any commercial
 * or non-commercial purpose and for any reproduction or redistribution by You of the
 * Work. You assume all risks associated with Your use of the Work and the exercise of
 * permissions under this License.
 */

// <auto-generated>
#nullable enable

using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GatewayApi.SlowRequestLogging;

internal abstract class BufferingStream : Stream, IBufferWriter<byte>
{
    private const int MinimumBufferSize = 4096; // 4K
    protected int _bytesBuffered;
    private BufferSegment? _head;
    private BufferSegment? _tail;
    protected Memory<byte> _tailMemory; // remainder of tail memory
    protected int _tailBytesBuffered;
    protected ILogger _logger;
    protected Stream _innerStream;

    public BufferingStream(Stream innerStream, ILogger logger)
    {
        _logger = logger;
        _innerStream = innerStream;
    }

    public override bool CanSeek => _innerStream.CanSeek;

    public override bool CanRead => _innerStream.CanRead;

    public override bool CanWrite => _innerStream.CanWrite;

    public override long Length => _innerStream.Length;

    public override long Position
    {
        get => _innerStream.Position;
        set => _innerStream.Position = value;
    }

    public override int WriteTimeout
    {
        get => _innerStream.WriteTimeout;
        set => _innerStream.WriteTimeout = value;
    }

    public string GetString(Encoding? encoding)
    {
        try
        {
            if (_head == null || _tail == null)
            {
                // nothing written
                return string.Empty;
            }

            if (encoding == null)
            {
                // This method is used only for the response body
                _logger.UnrecognizedMediaType("response");
                return string.Empty;
            }

            // Only place where we are actually using the buffered data.
            // update tail here.
            _tail.End = _tailBytesBuffered;

            var ros = new ReadOnlySequence<byte>(_head, 0, _tail, _tailBytesBuffered);

            var bufferWriter = new ArrayBufferWriter<char>();

            var decoder = encoding.GetDecoder();
            // First calls convert on the entire ReadOnlySequence, with flush: false.
            // flush: false is required as we don't want to write invalid characters that
            // are spliced due to truncation. If we set flush: true, if effectively means
            // we expect EOF in this array, meaning it will try to write any bytes at the end of it.
            decoder.Convert(ros, bufferWriter, flush: false, out var charUsed, out var completed);

            // Afterwards, we need to call convert in a loop until complete is true.
            // The first call to convert many return true, but if it doesn't, we call
            // Convert with a empty ReadOnlySequence and flush: true until we get completed: true.

            // This should never infinite due to the contract for decoders.
            // But for safety, call this only 10 times, throwing a decode failure if it fails.
            for (var i = 0; i < 10; i++)
            {
                if (completed)
                {
                    return new string(bufferWriter.WrittenSpan);
                }
                else
                {
                    EncodingExtensions.Convert(decoder, ReadOnlySequence<byte>.Empty, bufferWriter, flush: true, out charUsed, out completed);
                }
            }

            throw new DecoderFallbackException("Failed to decode after 10 calls to Decoder.Convert");
        }
        catch (DecoderFallbackException ex)
        {
            _logger.DecodeFailure(ex);
            return "<Decoder failure>";
        }
        finally
        {
            Reset();
        }
    }

    public void Advance(int bytes)
    {
        if ((uint)bytes > (uint)_tailMemory.Length)
        {
            ThrowArgumentOutOfRangeException(nameof(bytes));
        }

        _tailBytesBuffered += bytes;
        _bytesBuffered += bytes;
        _tailMemory = _tailMemory.Slice(bytes);
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        AllocateMemory(sizeHint);
        return _tailMemory;
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        AllocateMemory(sizeHint);
        return _tailMemory.Span;
    }

    private void AllocateMemory(int sizeHint)
    {
        if (_head is null)
        {
            // We need to allocate memory to write since nobody has written before
            var newSegment = AllocateSegment(sizeHint);

            // Set all the pointers
            _head = _tail = newSegment;
            _tailBytesBuffered = 0;
        }
        else
        {
            var bytesLeftInBuffer = _tailMemory.Length;

            if (bytesLeftInBuffer == 0 || bytesLeftInBuffer < sizeHint)
            {
                Debug.Assert(_tail != null);

                if (_tailBytesBuffered > 0)
                {
                    // Flush buffered data to the segment
                    _tail.End += _tailBytesBuffered;
                    _tailBytesBuffered = 0;
                }

                var newSegment = AllocateSegment(sizeHint);

                _tail.SetNext(newSegment);
                _tail = newSegment;
            }
        }
    }

    private BufferSegment AllocateSegment(int sizeHint)
    {
        var newSegment = CreateSegment();

        // We can't use the recommended pool so use the ArrayPool
        newSegment.SetOwnedMemory(ArrayPool<byte>.Shared.Rent(GetSegmentSize(sizeHint)));

        _tailMemory = newSegment.AvailableMemory;

        return newSegment;
    }

    private static BufferSegment CreateSegment()
    {
        return new BufferSegment();
    }

    private static int GetSegmentSize(int sizeHint, int maxBufferSize = int.MaxValue)
    {
        // First we need to handle case where hint is smaller than minimum segment size
        sizeHint = Math.Max(MinimumBufferSize, sizeHint);
        // After that adjust it to fit into pools max buffer size
        var adjustedToMaximumSize = Math.Min(maxBufferSize, sizeHint);
        return adjustedToMaximumSize;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Reset();
        }
    }

    public void Reset()
    {
        var segment = _head;
        while (segment != null)
        {
            var returnSegment = segment;
            segment = segment.NextSegment;

            // We haven't reached the tail of the linked list yet, so we can always return the returnSegment.
            returnSegment.ResetMemory();
        }

        _head = _tail = null;

        _bytesBuffered = 0;
        _tailBytesBuffered = 0;
    }

    // Copied from https://github.com/dotnet/corefx/blob/de3902bb56f1254ec1af4bf7d092fc2c048734cc/src/System.Memory/src/System/ThrowHelper.cs
    private static void ThrowArgumentOutOfRangeException(string argumentName) { throw CreateArgumentOutOfRangeException(argumentName); }
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Exception CreateArgumentOutOfRangeException(string argumentName) { return new ArgumentOutOfRangeException(argumentName); }

    public override void Flush()
    {
        _innerStream.Flush();
    }

    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        return _innerStream.FlushAsync(cancellationToken);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return _innerStream.Read(buffer, offset, count);
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _innerStream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        _innerStream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _innerStream.Write(buffer, offset, count);
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return _innerStream.WriteAsync(buffer, cancellationToken);
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        _innerStream.Write(buffer);
    }

    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
    {
        return _innerStream.BeginRead(buffer, offset, count, callback, state);
    }

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
    {
        return _innerStream.BeginWrite(buffer, offset, count, callback, state);
    }

    public override int EndRead(IAsyncResult asyncResult)
    {
        return _innerStream.EndRead(asyncResult);
    }

    public override void EndWrite(IAsyncResult asyncResult)
    {
        _innerStream.EndWrite(asyncResult);
    }

    public override void CopyTo(Stream destination, int bufferSize)
    {
        _innerStream.CopyTo(destination, bufferSize);
    }

    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
        return _innerStream.CopyToAsync(destination, bufferSize, cancellationToken);
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return _innerStream.ReadAsync(buffer, cancellationToken);
    }

    public override ValueTask DisposeAsync()
    {
        return _innerStream.DisposeAsync();
    }

    public override int Read(Span<byte> buffer)
    {
        return _innerStream.Read(buffer);
    }
}
