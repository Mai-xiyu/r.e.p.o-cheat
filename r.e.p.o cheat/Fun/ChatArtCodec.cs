using System;
using System.Collections.Generic;
using System.Text;

namespace r.e.p.o_cheat;

/// <summary>
/// Converts multiline chat art into a representation that the game's TMP/TTS
/// path treats as one message. The codec is intentionally Unity-independent so
/// its newline, Unicode and chunking behaviour can be unit-tested.
/// </summary>
public static class ChatArtCodec
{
    public const int DefaultMaxPayloadChars = 900;

    public static string NormalizeNewlines(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }
        return text.Replace("\r\n", "\n").Replace('\r', '\n');
    }

    public static bool LooksLikeArt(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        string normalized = NormalizeNewlines(text);
        if (normalized.IndexOf('\n') >= 0 ||
            normalized.IndexOf("  ", StringComparison.Ordinal) >= 0 ||
            normalized.IndexOf('\u3000') >= 0)
        {
            return true;
        }

        int structuralGlyphs = 0;
        for (int i = 0; i < normalized.Length; i++)
        {
            char c = normalized[i];
            bool boxOrBlock = (c >= '\u2500' && c <= '\u259f') ||
                (c >= '\u25a0' && c <= '\u25ff') ||
                (c >= '\u2190' && c <= '\u21ff');
            bool asciiStructure = "|/_\\[](){}<>*#@=+^-~".IndexOf(c) >= 0;
            if (boxOrBlock || asciiStructure)
            {
                structuralGlyphs++;
            }
        }
        return structuralGlyphs >= 3;
    }

    /// <summary>
    /// Converts ASCII printable characters to their fullwidth forms and ASCII
    /// whitespace to U+3000 while preserving LF newlines. This avoids the
    /// game's ASCII-space TTS tokenization and prevents angle brackets from
    /// becoming TMP rich-text tags.
    /// </summary>
    public static string EncodeForGame(string text)
    {
        string normalized = NormalizeNewlines(text);
        if (normalized.Length == 0)
        {
            return string.Empty;
        }

        StringBuilder result = new StringBuilder(normalized.Length);
        for (int i = 0; i < normalized.Length; i++)
        {
            char c = normalized[i];
            if (c == '\n')
            {
                result.Append(c);
            }
            else if (c == ' ' || c == '\t')
            {
                result.Append('\u3000');
            }
            else if (c >= '!' && c <= '~')
            {
                result.Append((char)(c + 0xfee0));
            }
            else if (!char.IsControl(c))
            {
                result.Append(c);
            }
        }
        return result.ToString();
    }

    /// <summary>
    /// Creates one payload for normal-size art. Oversized content is split only
    /// as a transport fallback, preferring hard newlines and never splitting a
    /// UTF-16 surrogate pair.
    /// </summary>
    public static string[] BuildPayloads(string text, int maxPayloadChars = DefaultMaxPayloadChars)
    {
        string normalized = NormalizeNewlines(text);
        if (normalized.Length == 0)
        {
            return Array.Empty<string>();
        }

        string payload = LooksLikeArt(normalized)
            ? EncodeForGame(normalized)
            : RemoveUnsupportedControls(normalized);
        if (payload.Length == 0)
        {
            return Array.Empty<string>();
        }

        return SplitPayload(payload, Math.Max(64, maxPayloadChars));
    }

    private static string RemoveUnsupportedControls(string text)
    {
        StringBuilder result = null;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '\n' || !char.IsControl(c))
            {
                if (result != null)
                {
                    result.Append(c);
                }
                continue;
            }

            if (result == null)
            {
                result = new StringBuilder(text.Length);
                result.Append(text, 0, i);
            }
        }
        return result == null ? text : result.ToString();
    }

    private static string[] SplitPayload(string payload, int maxPayloadChars)
    {
        if (payload.Length <= maxPayloadChars)
        {
            return new[] { payload };
        }

        List<string> chunks = new List<string>();
        int start = 0;
        while (start < payload.Length)
        {
            int end = Math.Min(payload.Length, start + maxPayloadChars);
            int next = end;

            if (end < payload.Length)
            {
                int newline = -1;
                for (int i = end - 1; i > start; i--)
                {
                    if (payload[i] == '\n')
                    {
                        newline = i;
                        break;
                    }
                }

                if (newline > start)
                {
                    end = newline;
                    next = newline + 1;
                }
                else if (end > start && char.IsHighSurrogate(payload[end - 1]) &&
                    char.IsLowSurrogate(payload[end]))
                {
                    end--;
                    next = end;
                }
            }

            if (end <= start)
            {
                end = Math.Min(payload.Length, start + maxPayloadChars);
                next = end;
            }

            string chunk = payload.Substring(start, end - start);
            if (chunk.Length > 0)
            {
                chunks.Add(chunk);
            }
            start = next;
        }
        return chunks.ToArray();
    }
}
