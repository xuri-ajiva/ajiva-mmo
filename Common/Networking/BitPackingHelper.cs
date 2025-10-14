using System.Buffers;

namespace Common.Networking;

/// <summary>
/// Provides high-performance, allocation-free helper methods for variable-length
/// bitpacking of 64-bit integers using Varint and ZigZag encoding.
/// These methods are designed as extension methods for Span, ReadOnlySpan, and IBufferWriter.
/// </summary>
public static class BitPackingHelper
{
    public const i32 MaxVarLength = 10;

    /// <summary>
    /// Attempts to read a Varint-encoded 64-bit unsigned integer from the start of the span.
    /// This method does not throw an exception on failure.
    /// </summary>
    /// <param name="data">The span of bytes to read from.</param>
    /// <param name="value">When this method returns, contains the decoded ulong value if successful; otherwise, 0.</param>
    /// <param name="bytesRead">When this method returns, contains the number of bytes read if successful; otherwise, 0.</param>
    /// <returns>true if the value was successfully decoded; otherwise, false.</returns>
    public static bool TryReadVarU64(this ReadOnlySpan<u8> data, out u64 value, out i32 bytesRead)
    {
        value = 0;
        var shift = 0;

        for (var i = 0; i < data.Length; i++)
        {
            var b = data[i];

            // A 64-bit varint can be at most 10 bytes long.
            if (i >= MaxVarLength)
            {
                bytesRead = 0;
                value = 0;
                return false; // Malformed: Varint is too long
            }

            value |= (u64)(b & 0x7F) << shift;

            if ((b & 0x80) == 0)
            {
                bytesRead = i + 1;
                return true; // Successfully decoded
            }

            shift += 7;
        }

        // If we exit the loop, it means the data was incomplete.
        bytesRead = 0;
        value = 0;
        return false;
    }

    /// <summary>
    /// Attempts to decode a Varint-encoded 64-bit unsigned integer from the span,
    /// advancing the span past the consumed bytes if successful.
    /// </summary>
    /// <param name="data">The span of bytes to read from. It will be sliced on success.</param>
    /// <param name="value">When this method returns, contains the decoded ulong value if successful.</param>
    /// <returns>true if the value was successfully consumed; otherwise, false.</returns>
    public static bool TryConsumeVarU64(ref this ReadOnlySpan<u8> data, out u64 value)
    {
        if (data.TryReadVarU64(out value, out i32 bytesRead))
        {
            data = data.Slice(bytesRead);
            return true;
        }

        return false;
    }


    /// <summary>
    /// Reads a Varint-encoded 64-bit unsigned integer from the start of the span. Throws on failure.
    /// </summary>
    /// <returns>A tuple containing the decoded ulong value and the number of bytes read.</returns>
    /// <exception cref="InvalidDataException">Thrown if the data is malformed or incomplete.</exception>
    public static u64 ReadVarU64(this ReadOnlySpan<u8> data, out i32 bytesRead)
    {
        if (data.TryReadVarU64(out u64 value, out bytesRead))
        {
            return value;
        }

        throw new InvalidDataException("Malformed or incomplete Varint data.");
    }

    /// <summary>
    /// Decodes a Varint-encoded 64-bit unsigned integer from the span, advancing it. Throws on failure.
    /// </summary>
    /// <returns>The decoded ulong value.</returns>
    /// <exception cref="InvalidDataException">Thrown if the data is malformed or incomplete.</exception>
    public static u64 ConsumeVarU64(ref this ReadOnlySpan<u8> data)
    {
        if (data.TryReadVarU64(out u64 value, out i32 bytesRead))
        {
            data = data.Slice(bytesRead);
            return value;
        }

        throw new InvalidDataException("Malformed or incomplete Varint data.");
    }


    /// <summary>
    /// Encodes a 64-bit unsigned integer using Varint encoding into the destination span.
    /// </summary>
    public static i32 PackVarU64(this Span<u8> destination, u64 value)
    {
        i32 bytesWritten = 0;
        do
        {
            if (bytesWritten >= destination.Length)
            {
                throw new ArgumentException("Destination span is too small.", nameof(destination));
            }

            u8 b = (u8)(value & 0x7F);
            value >>= 7;
            if (value > 0) b |= 0x80;
            destination[bytesWritten++] = b;
        } while (value > 0);

        return bytesWritten;
    }

    /// <summary>
    /// Encodes a 64-bit unsigned integer using Varint and writes it to an IBufferWriter.
    /// </summary>
    public static void PackVarU64(this IBufferWriter<u8> writer, u64 value)
    {
        Span<u8> buffer = writer.GetSpan(MaxVarLength);
        i32 bytesWritten = buffer.PackVarU64(value);
        writer.Advance(bytesWritten);
    }


    /// <summary>
    /// Attempts to read a ZigZag/Varint-encoded 64-bit signed integer from the start of the span.
    /// </summary>
    /// <returns>true if the value was successfully decoded; otherwise, false.</returns>
    public static bool TryReadVarI64(this ReadOnlySpan<u8> data, out long value, out i32 bytesRead)
    {
        if (data.TryReadVarU64(out u64 zigzagValue, out bytesRead))
        {
            // ZigZag decode
            value = (long)(zigzagValue >> 1) ^ -(long)(zigzagValue & 1);
            return true;
        }

        value = 0;
        return false;
    }

    /// <summary>
    /// Attempts to decode a ZigZag/Varint-encoded 64-bit signed integer, advancing the span on success.
    /// </summary>
    public static bool TryConsumeVarI64(ref this ReadOnlySpan<u8> data, out long value)
    {
        if (data.TryReadVarI64(out value, out i32 bytesRead))
        {
            data = data.Slice(bytesRead);
            return true;
        }

        return false;
    }


    /// <summary>
    /// Reads a ZigZag/Varint-encoded 64-bit signed integer from the start of the span. Throws on failure.
    /// </summary>
    public static long ReadVarI64(this ReadOnlySpan<u8> data, out i32 bytesRead)
    {
        if (data.TryReadVarI64(out long value, out bytesRead))
        {
            return value;
        }

        throw new InvalidDataException("Malformed or incomplete ZigZag/Varint data.");
    }

    /// <summary>
    /// Decodes a ZigZag/Varint-encoded 64-bit signed integer, advancing the span. Throws on failure.
    /// </summary>
    public static long ConsumeVarI64(ref this ReadOnlySpan<u8> data)
    {
        if (data.TryReadVarI64(out long value, out i32 bytesRead))
        {
            data = data.Slice(bytesRead);
            return value;
        }

        throw new InvalidDataException("Malformed or incomplete ZigZag/Varint data.");
    }


    /// <summary>
    /// Encodes a 64-bit signed integer using ZigZag and Varint encoding into the destination span.
    /// </summary>
    public static i32 PackVarI64(this Span<u8> destination, long value)
    {
        u64 zigzagValue = (u64)((value << 1) ^ (value >> 63));
        return destination.PackVarU64(zigzagValue);
    }

    /// <summary>
    /// Encodes a 64-bit signed integer using ZigZag/Varint and writes it to an IBufferWriter.
    /// </summary>
    public static void PackVarI64(this IBufferWriter<u8> writer, long value)
    {
        u64 zigzagValue = (u64)((value << 1) ^ (value >> 63));
        writer.PackVarU64(zigzagValue);
    }
}