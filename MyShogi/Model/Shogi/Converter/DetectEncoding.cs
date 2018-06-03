using System.Text;
using SMath = System.Math;

namespace MyShogi.Model.Shogi.Converter
{
    public static class DetectEncoding
    {
        public enum DetectBit
        {
            NONE = 0,
            ASCII = 1,
            ShiftJIS = 2,
            UTF8 = 4,
            ALL = (ASCII | ShiftJIS | UTF8),
        }
        public static DetectBit check(byte[] text) => check(text, text.LongLength);
        public static DetectBit check(byte[] text, long limitlen)
        {
            if (IsAscii(text, limitlen)) return DetectBit.ALL;
            DetectBit detect = DetectBit.NONE;
            if (IsShiftJis(text, limitlen)) detect |= DetectBit.ShiftJIS;
            if (IsUtf8(text, limitlen)) detect |= DetectBit.UTF8;
            return detect;
        }
        public static Encoding getEncoding_sj(byte[] text) => getEncoding_sj(text, text.LongLength);
        public static Encoding getEncoding_sj(byte[] text, long limitlen)
        {
            var detect = check(text, limitlen);
            if (IsShiftJis(detect)) return Encoding.GetEncoding(932);
            if (IsUtf8(detect)) return Encoding.UTF8;
            return null;
        }
        public static Encoding getEncoding_u8(byte[] text) => getEncoding_u8(text, text.LongLength);
        public static Encoding getEncoding_u8(byte[] text, long limitlen)
        {
            var detect = check(text, limitlen);
            if (IsUtf8(detect)) return Encoding.UTF8;
            if (IsShiftJis(detect)) return Encoding.GetEncoding(932);
            return null;
        }
        public static string getString_sj(byte[] text) => getString_sj(text, text.LongLength);
        public static string getString_sj(byte[] text, long limitlen)
        {
            var enc = getEncoding_sj(text, limitlen);
            if (enc == null) return string.Empty;
            return enc.GetString(text);
        }
        public static string getString_u8(byte[] text) => getString_u8(text, text.LongLength);
        public static string getString_u8(byte[] text, long limitlen)
        {
            var enc = getEncoding_u8(text, limitlen);
            if (enc == null) return string.Empty;
            return enc.GetString(text);
        }
        public static bool IsAscii(DetectBit bit) => ((bit & DetectBit.ASCII) != 0);
        public static bool IsAscii(byte[] text) => IsAscii(text, text.LongLength);
        public static bool IsAscii(byte[] text, long _limitlen)
        {
            long limitlen = SMath.Min(text.LongLength, _limitlen);
            for (long i = 0; i < limitlen; ++i)
            {
                if (text[i] >= 0x80) return false;
            }
            return true;
        }
        public static bool IsShiftJis(DetectBit bit) => ((bit & DetectBit.ShiftJIS) != 0);
        public static bool IsShiftJis(byte[] text) => IsShiftJis(text, text.LongLength);
        public static bool IsShiftJis(byte[] text, long _limitlen)
        {
            long limitlen = SMath.Min(text.LongLength, _limitlen);
            for (long i = 0; i < limitlen; ++i)
            {
                byte b0 = text[i];
                if (b0 <= 0x80 || b0 >= 0xa0 && b0 <= 0xdf || b0 >= 0xfd) continue;
                if (i + 1 >= limitlen) return true;
                byte b1 = text[++i];
                if (b1 < 0x40 || b1 == 0x7f || b1 > 0xfc) return false;
            }
            return true;
        }
        public static bool IsUtf8(DetectBit bit) => ((bit & DetectBit.UTF8) != 0);
        public static bool IsUtf8(byte[] text) => IsUtf8(text, text.LongLength);
        public static bool IsUtf8(byte[] text, long _limitlen)
        {
            long limitlen = SMath.Min(text.LongLength, _limitlen);
            for (long i = 0; i < limitlen; ++i)
            {
                byte b0 = text[i];
                // thru U+0000 ~ U+007F
                if (b0 <= 0x7f) continue;
                // ↑ 1byte 以内で表現できる範囲
                // b0:0xc0~0xc1 を使用すると U+0000 ~ U+007F の冗長な表現にあたるので invalid
                if (b0 <= 0xc1) return false;
                if (++i >= limitlen) return true;
                byte b1 = text[i];
                if (b0 <= 0xdf)
                {
                    // thru U+0080 - U+07FF
                    if (b1 < 0x80 || b0 > 0xbf) return false;
                    continue;
                }
                // ↑ 2byte 以内で表現できる範囲
                if (++i >= limitlen) return true;
                byte b2 = text[i];
                if (b0 == 0xe0)
                {
                    // thru U+0800 ~ U+0FFF
                    if (b1 < 0xa0 || b1 > 0xbf) return false;
                    if (b2 < 0x80 || b2 > 0xbf) return false;
                    continue;
                }
                if (b0 <= 0xec)
                {
                    // thru U+1000 ~ U+CFFF
                    if (b1 < 0x80 || b1 > 0xbf) return false;
                    if (b2 < 0x80 || b2 > 0xbf) return false;
                    continue;
                }
                if (b0 == 0xed)
                {
                    // thru U+D000 ~ U+D7FF
                    // Surrogate Code Point (U+D800 ~ U+DFFF, b0:0xed, b1:0xa0~0xbf, b2:0x80~0xbf) は UTF-8 では invalid
                    // Surrogate Code Point は、 U+10000 ~ U+10FFFF の文字を UTF-16 において4byteで表現するための領域で、 UTF-8 で用いてはならない
                    if (b1 < 0x80 || b1 > 0x9f) return false;
                    if (b2 < 0x80 || b2 > 0xbf) return false;
                    continue;
                }
                if (b0 <= 0xef)
                {
                    // thru U+E000 ~ U+FFFF
                    if (b1 < 0x80 || b1 > 0xbf) return false;
                    if (b2 < 0x80 || b2 > 0xbf) return false;
                    // check BOM (U+FEFF, b0:0xef, b1:0xbb, b2:0xbf)
                    if (i == 0 && b0 == 0xef && b1 == 0xbb && b2 == 0xbf) return true;
                    continue;
                }
                // ↑ 3byte 以内で表現できる範囲
                if (++i >= limitlen) return true;
                byte b3 = text[i];
                if (b0 == 0xf0)
                {
                    // thru U+10000 ~ U+3FFFF
                    // Plane 1 (U+10000 ~ U+1FFFF): 追加多言語面
                    // Plane 2 (U+20000 ~ U+2FFFF): 追加漢字面
                    // Plane 3 (U+30000 ~ U+3FFFF): 第三漢字面
                    if (b1 < 0x90 || b1 > 0xbf) return false;
                    if (b2 < 0x80 || b2 > 0xbf) return false;
                    if (b3 < 0x80 || b3 > 0xbf) return false;
                    continue;
                }
                if (b0 <= 0xf3)
                {
                    // thru U+40000 ~ U+FFFFF
                    // Plane 4~13 (U+40000 ~ U+DFFFF): unassigned
                    // Plane 14 (U+E0000 ~ U+EFFFF): 追加特殊用途面
                    // Plane 15 (U+F0000 ~ U+FFFFF): 私用面A
                    if (b1 < 0x80 || b1 > 0xbf) return false;
                    if (b2 < 0x80 || b2 > 0xbf) return false;
                    if (b3 < 0x80 || b3 > 0xbf) return false;
                }
                if (b0 == 0xf4)
                {
                    // thru U+100000 ~ U+10FFFF
                    // Plane 16 (U+100000 ~ U+10FFFF): 私用面B
                    // U+110000 ~ U+13FFFF (b0:0xf4, b1:0x90~0xbf, b2:0x80~0xbf, b3:0x80~0xbf) は UTF-8 では invalid
                    if (b1 < 0x80 || b1 > 0x8f) return false;
                    if (b2 < 0x80 || b2 > 0xbf) return false;
                    if (b3 < 0x80 || b3 > 0xbf) return false;
                }
                // U+140000 以降は UTF-8 では invalid
                if (b0 >= 0xf5) return false;
            }
            return true;
        }

    }
}
