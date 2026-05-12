using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

using Microsoft.Extensions.ObjectPool;

namespace UnfoldedCircle.Server.Logging;

/// <summary>
/// Redacts sensitive fields from a UTF-8 encoded JSON payload before it is written to a log sink.
/// </summary>
internal static class SensitiveJsonRedactor
{
    /// <summary>
    /// Default property names whose scalar value is replaced with a mask.
    /// </summary>
    internal static readonly string[] DefaultRedactedProperties =
    [
        "token",
        "pin",
        "secret",
        "api_key",
        "apiKey",
        "privateKey",
        "PrivateKey",
        "private_key",
        "publicKey",
        "PublicKey",
        "public_key"
    ];

    /// <summary>
    /// Default property names whose entire value (including nested objects/arrays and JSON-in-string
    /// payloads) is replaced with a single mask.
    /// </summary>
    internal static readonly string[] DefaultMaskWholeValueProperties =
    [
        "password",
        "restore_data",
        "backup_data"
    ];

    private static readonly byte[] Mask = "***"u8.ToArray();

    private static readonly ObjectPool<ArrayBufferWriter<byte>> BufferPool =
        new DefaultObjectPool<ArrayBufferWriter<byte>>(new BufferWriterPolicy(), 8);

    /// <summary>
    /// Merges <paramref name="defaults"/> with optional <paramref name="extra"/> property names, returning a
    /// pre-encoded UTF-8 byte array suitable for repeated calls to
    /// <see cref="Redact(ReadOnlySpan{byte}, byte[][], byte[][], int, bool, int)"/>.
    /// </summary>
    internal static byte[][] BuildPropertyList(IReadOnlyCollection<string> defaults, IEnumerable<string>? extra)
    {
        if (extra is null)
            return ToUtf8(defaults);

        var merged = new HashSet<string>(defaults, StringComparer.Ordinal);
        foreach (var item in extra)
        {
            if (!string.IsNullOrEmpty(item))
                merged.Add(item);
        }

        var result = new byte[merged.Count][];
        var i = 0;
        foreach (var item in merged)
            result[i++] = Encoding.UTF8.GetBytes(item);
        return result;
    }

    private static byte[][] ToUtf8(IReadOnlyCollection<string> source)
    {
        var result = new byte[source.Count][];
        var i = 0;
        foreach (var item in source)
            result[i++] = Encoding.UTF8.GetBytes(item);
        return result;
    }

    public static string Redact(
        ReadOnlySpan<byte> utf8Json,
        byte[][] redactedProperties,
        byte[][] maskWholeValueProperties,
        int maxBytes = 1000,
        bool nestedJsonRedaction = true,
        int maxRecursionDepth = 3)
    {
        if (utf8Json.IsEmpty)
            return string.Empty;

        var bw = BufferPool.Get();
        try
        {
            var cfg = new RedactConfig(redactedProperties, maskWholeValueProperties, maxBytes, nestedJsonRedaction, maxRecursionDepth);
            var truncated = RedactCore(utf8Json, in cfg, recursionDepth: 0, bw);
            var written = bw.WrittenMemory;
            if (!truncated)
                return Encoding.UTF8.GetString(written.Span);

            const string suffix = "…";
            return string.Create(written.Length + suffix.Length, (Buffer: written, Suffix: suffix), static (span, state) =>
            {
                Encoding.UTF8.GetChars(state.Buffer.Span, span);
                var charCount = Encoding.UTF8.GetCharCount(state.Buffer.Span);
                state.Suffix.CopyTo(span[charCount..]);
            });
        }
        catch (JsonException)
        {
            return FallbackPlainTruncate(utf8Json, maxBytes);
        }
        finally
        {
            bw.ResetWrittenCount();
            BufferPool.Return(bw);
        }
    }

    private static bool RedactCore(
        ReadOnlySpan<byte> utf8Json,
        in RedactConfig cfg,
        int recursionDepth,
        ArrayBufferWriter<byte> bw)
    {
        var reader = new Utf8JsonReader(utf8Json, new JsonReaderOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        });

        // Bitmap of open container kinds (0 = object, 1 = array) keyed by depth. Up to 64 levels.
        var containerIsArray = 0UL;
        var depth = 0;
        var redactNextValue = false;
        var maskWholeNextValue = false;
        var truncated = false;

        Span<byte> scratch = stackalloc byte[256];

        using var writer = new Utf8JsonWriter(bw, new JsonWriterOptions { SkipValidation = true });
        while (reader.Read())
        {
            if (writer.BytesCommitted + writer.BytesPending >= cfg.MaxBytes)
            {
                truncated = true;
                break;
            }

            if (TryHandleMaskWhole(ref reader, writer, ref maskWholeNextValue))
                continue;

            DispatchToken(ref reader, writer, scratch, in cfg, recursionDepth, ref depth, ref containerIsArray, ref redactNextValue, ref maskWholeNextValue);
        }

        CloseOpenScopes(writer, ref depth, containerIsArray);

        return truncated;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DispatchToken(
        ref Utf8JsonReader reader,
        Utf8JsonWriter writer,
        scoped Span<byte> scratch,
        in RedactConfig cfg,
        int recursionDepth,
        ref int depth,
        ref ulong containerIsArray,
        ref bool redactNextValue,
        ref bool maskWholeNextValue)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.StartObject: HandleStartObject(writer, ref depth, ref containerIsArray); break;
            case JsonTokenType.EndObject: HandleEndObject(writer, ref depth); break;
            case JsonTokenType.StartArray: HandleStartArray(writer, ref depth, ref containerIsArray); break;
            case JsonTokenType.EndArray: HandleEndArray(writer, ref depth); break;
            case JsonTokenType.PropertyName:
                HandlePropertyName(ref reader, writer, scratch, in cfg, ref redactNextValue, ref maskWholeNextValue);
                break;
            case JsonTokenType.String:
                HandleStringValue(ref reader, writer, scratch, in cfg, recursionDepth, ref redactNextValue);
                break;
            case JsonTokenType.Number: HandleNumber(ref reader, writer, ref redactNextValue); break;
            case JsonTokenType.True: HandleBool(writer, value: true, ref redactNextValue); break;
            case JsonTokenType.False: HandleBool(writer, value: false, ref redactNextValue); break;
            case JsonTokenType.Null: HandleNull(writer, ref redactNextValue); break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryHandleMaskWhole(ref Utf8JsonReader reader, Utf8JsonWriter writer, ref bool maskWholeNextValue)
    {
        if (!maskWholeNextValue)
            return false;

        if (reader.TokenType is JsonTokenType.StartObject or JsonTokenType.StartArray)
            reader.Skip();
        writer.WriteStringValue(Mask);
        maskWholeNextValue = false;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void HandleStartObject(Utf8JsonWriter writer, ref int depth, ref ulong containerIsArray)
    {
        writer.WriteStartObject();
        if (depth < 64) containerIsArray &= ~(1UL << depth);
        depth++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void HandleStartArray(Utf8JsonWriter writer, ref int depth, ref ulong containerIsArray)
    {
        writer.WriteStartArray();
        if (depth < 64) containerIsArray |= 1UL << depth;
        depth++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void HandleEndObject(Utf8JsonWriter writer, ref int depth)
    {
        writer.WriteEndObject();
        if (depth > 0) depth--;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void HandleEndArray(Utf8JsonWriter writer, ref int depth)
    {
        writer.WriteEndArray();
        if (depth > 0) depth--;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void HandlePropertyName(
        ref Utf8JsonReader reader,
        Utf8JsonWriter writer,
        scoped Span<byte> scratch,
        in RedactConfig cfg,
        ref bool redactNextValue,
        ref bool maskWholeNextValue)
    {
        foreach (var name in cfg.RedactedProperties)
        {
            if (reader.ValueTextEquals(name.AsSpan()))
            {
                redactNextValue = true;
                break;
            }
        }
        if (!redactNextValue)
        {
            foreach (var name in cfg.MaskWholeValueProperties)
            {
                if (reader.ValueTextEquals(name.AsSpan()))
                {
                    maskWholeNextValue = true;
                    break;
                }
            }
        }
        WriteName(ref reader, writer, scratch);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void HandleStringValue(
        ref Utf8JsonReader reader,
        Utf8JsonWriter writer,
        scoped Span<byte> scratch,
        in RedactConfig cfg,
        int recursionDepth,
        ref bool redactNextValue)
    {
        if (redactNextValue)
        {
            writer.WriteStringValue(Mask);
        }
        else if (cfg.NestedJsonRedaction
                 && recursionDepth < cfg.MaxRecursionDepth
                 && TryWriteNestedJsonString(ref reader, writer, scratch, in cfg, recursionDepth + 1))
        {
            // helper wrote redacted value
        }
        else
        {
            WriteStringValueRaw(ref reader, writer, scratch);
        }
        redactNextValue = false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void HandleNumber(ref Utf8JsonReader reader, Utf8JsonWriter writer, ref bool redactNextValue)
    {
        if (redactNextValue) writer.WriteStringValue(Mask);
        else writer.WriteRawValue(reader.ValueSpan, skipInputValidation: true);
        redactNextValue = false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void HandleBool(Utf8JsonWriter writer, bool value, ref bool redactNextValue)
    {
        if (redactNextValue) writer.WriteStringValue(Mask);
        else writer.WriteBooleanValue(value);
        redactNextValue = false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void HandleNull(Utf8JsonWriter writer, ref bool redactNextValue)
    {
        if (redactNextValue) writer.WriteStringValue(Mask);
        else writer.WriteNullValue();
        redactNextValue = false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CloseOpenScopes(Utf8JsonWriter writer, ref int depth, ulong containerIsArray)
    {
        while (depth > 0)
        {
            depth--;
            if (depth < 64 && (containerIsArray & (1UL << depth)) != 0)
                writer.WriteEndArray();
            else
                writer.WriteEndObject();
        }
    }

    private static bool TryWriteNestedJsonString(
        ref Utf8JsonReader reader,
        Utf8JsonWriter writer,
        scoped Span<byte> scratch,
        in RedactConfig cfg,
        int recursionDepth)
    {
        scoped ReadOnlySpan<byte> decoded;
        byte[]? rented = null;

        if (!reader.ValueIsEscaped && !reader.HasValueSequence)
        {
            decoded = reader.ValueSpan;
        }
        else
        {
            var upper = reader.HasValueSequence ? checked((int)reader.ValueSequence.Length) : reader.ValueSpan.Length;
            if (upper <= scratch.Length)
            {
                var n = reader.CopyString(scratch);
                decoded = scratch[..n];
            }
            else
            {
                rented = ArrayPool<byte>.Shared.Rent(upper);
                var n = reader.CopyString(rented);
                decoded = rented.AsSpan(0, n);
            }
        }

        try
        {
            if (!LooksLikeJsonObject(decoded))
                return false;

            var innerBw = BufferPool.Get();
            try
            {
                var remaining = Math.Max(64, cfg.MaxBytes - (int)writer.BytesCommitted - writer.BytesPending);
                var innerCfg = new RedactConfig(cfg.RedactedProperties, cfg.MaskWholeValueProperties, remaining, cfg.NestedJsonRedaction, cfg.MaxRecursionDepth);
                try
                {
                    RedactCore(decoded, in innerCfg, recursionDepth, innerBw);
                }
                catch (JsonException)
                {
                    return false;
                }
                writer.WriteStringValue(innerBw.WrittenSpan);
                return true;
            }
            finally
            {
                innerBw.ResetWrittenCount();
                BufferPool.Return(innerBw);
            }
        }
        finally
        {
            if (rented is not null)
                ArrayPool<byte>.Shared.Return(rented);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool LooksLikeJsonObject(ReadOnlySpan<byte> utf8)
    {
        foreach (var b in utf8)
        {
            if (b is (byte)' ' or (byte)'\t' or (byte)'\n' or (byte)'\r')
                continue;
            return b == (byte)'{';
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteName(ref Utf8JsonReader reader, Utf8JsonWriter writer, scoped Span<byte> scratch)
    {
        if (!reader.ValueIsEscaped)
        {
            writer.WritePropertyName(reader.ValueSpan);
            return;
        }

        var upperBound = reader.HasValueSequence ? checked((int)reader.ValueSequence.Length) : reader.ValueSpan.Length;
        if (upperBound <= scratch.Length)
        {
            var n = reader.CopyString(scratch);
            writer.WritePropertyName(scratch[..n]);
        }
        else
        {
            var rented = ArrayPool<byte>.Shared.Rent(upperBound);
            try
            {
                var n = reader.CopyString(rented);
                writer.WritePropertyName(rented.AsSpan(0, n));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteStringValueRaw(ref Utf8JsonReader reader, Utf8JsonWriter writer, scoped Span<byte> scratch)
    {
        if (!reader.ValueIsEscaped)
        {
            writer.WriteStringValue(reader.ValueSpan);
            return;
        }

        var upperBound = reader.HasValueSequence ? checked((int)reader.ValueSequence.Length) : reader.ValueSpan.Length;
        if (upperBound <= scratch.Length)
        {
            var n = reader.CopyString(scratch);
            writer.WriteStringValue(scratch[..n]);
        }
        else
        {
            var rented = ArrayPool<byte>.Shared.Rent(upperBound);
            try
            {
                var n = reader.CopyString(rented);
                writer.WriteStringValue(rented.AsSpan(0, n));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }
    }

    private static string FallbackPlainTruncate(ReadOnlySpan<byte> utf8Json, int maxBytes)
    {
        var slice = utf8Json.Length > maxBytes ? utf8Json[..maxBytes] : utf8Json;
        return $"<non-json> {Encoding.UTF8.GetString(slice)}";
    }

    private readonly struct RedactConfig(
        byte[][] redactedProperties,
        byte[][] maskWholeValueProperties,
        int maxBytes,
        bool nestedJsonRedaction,
        int maxRecursionDepth)
    {
        public readonly byte[][] RedactedProperties = redactedProperties;
        public readonly byte[][] MaskWholeValueProperties = maskWholeValueProperties;
        public readonly int MaxBytes = maxBytes;
        public readonly bool NestedJsonRedaction = nestedJsonRedaction;
        public readonly int MaxRecursionDepth = maxRecursionDepth;
    }

    private sealed class BufferWriterPolicy : IPooledObjectPolicy<ArrayBufferWriter<byte>>
    {
        public ArrayBufferWriter<byte> Create() => new(1024);
        public bool Return(ArrayBufferWriter<byte> obj) => obj.Capacity <= 64 * 1024;
    }
}
