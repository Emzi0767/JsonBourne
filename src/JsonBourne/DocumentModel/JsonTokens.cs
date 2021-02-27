// This file is a part of JsonBourne project.
// 
// Copyright 2020 Emzi0767
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Buffers;
using System.Buffers.Text;

namespace JsonBourne.DocumentModel
{
    internal static class JsonTokens
    {
        // object stuff
        public const byte OpeningBrace = (byte)'{';
        public const byte ClosingBrace = (byte)'}';

        // array stuff
        public const byte OpeningBracket = (byte)'[';
        public const byte ClosingBracket = (byte)']';

        // separators
        public const byte ItemSeparator = (byte)',';
        public const byte KeyValueSeparator = (byte)':';

        // string stuff
        public const byte QuoteMark = (byte)'"';
        public const byte ReverseSolidus = (byte)'\\';
        public const byte Solidus = (byte)'/';
        public const byte Backspace = (byte)'b';
        public const byte FormFeed = (byte)'f';
        public const byte LineFeed = (byte)'n';
        public const byte CarriageReturn = (byte)'r';
        public const byte HorizontalTab = (byte)'t';
        public const byte UnicodePrefix = (byte)'u';

        public static StandardFormat FormatHex4 { get; } = StandardFormat.Parse("X4");

        public static bool TryUnescape(ReadOnlySpan<byte> chars, out char result, out int consumed)
        {
            consumed = 0;
            result = '\0';
            if (chars[0] != ReverseSolidus)
                return false;

            consumed = 2;
            switch (chars[1])
            {
                case Backspace:
                    result = '\b';
                    return true;

                case FormFeed:
                    result = '\f';
                    return true;

                case LineFeed:
                    result = '\n';
                    return true;

                case CarriageReturn:
                    result = '\r';
                    return true;

                case HorizontalTab:
                    result = '\t';
                    return true;

                case UnicodePrefix:
                    if (!Utf8Parser.TryParse(chars[2..6], out ushort ch16, out var fmtConsumed, 'X') || fmtConsumed != 4)
                        return false;

                    consumed = 6;
                    result = (char)ch16;
                    return true;

                case QuoteMark:
                case ReverseSolidus:
                case Solidus:
                    result = (char)chars[1];
                    return true;

                default:
                    return false;
            }
        }

        public static bool TryEscape(char ch, Span<byte> result, out int written)
        {
            written = 0;
            if (result.Length < 2)
                return false;

            written = 2;
            result[0] = ReverseSolidus;

            switch (ch)
            {
                case '"':
                    result[1] = QuoteMark;
                    return true;

                case '\\':
                    result[1] = ReverseSolidus;
                    return true;

                case '/':
                    result[1] = Solidus;
                    return true;

                case '\b':
                    result[1] = Backspace;
                    return true;

                case '\f':
                    result[1] = FormFeed;
                    return true;

                case '\n':
                    result[1] = LineFeed;
                    return true;

                case '\r':
                    result[1] = CarriageReturn;
                    return true;

                case '\t':
                    result[1] = HorizontalTab;
                    return true;
            }

            if (char.IsControl(ch))
            {
                if (result.Length < 6)
                    return false;

                written = 6;
                result[1] = UnicodePrefix;

                if (!Utf8Formatter.TryFormat((ushort)ch, result[2..6], out var fmtWritten, FormatHex4) || fmtWritten != 4)
                    return false;

                return true;
            }

            written = 0;
            return false;
        }

        // number stuff
        public const byte NumberSign = (byte)'-';
        public const byte DecimalSeparator = (byte)'.';
        public const byte ExponentSmall = (byte)'e';
        public const byte ExponentCapital = (byte)'E';
        public const byte ExponentSignPositive = (byte)'+';
        public const byte Digit0 = (byte)'0';
        public const byte Digit1 = (byte)'1';
        public const byte Digit2 = (byte)'2';
        public const byte Digit3 = (byte)'3';
        public const byte Digit4 = (byte)'4';
        public const byte Digit5 = (byte)'5';
        public const byte Digit6 = (byte)'6';
        public const byte Digit7 = (byte)'7';
        public const byte Digit8 = (byte)'8';
        public const byte Digit9 = (byte)'9';

        // true, false, null
        public const byte TrueFirst = (byte)'t';
        public const byte FalseFirst = (byte)'f';
        public const byte NullFirst = (byte)'n';
        public const int True32 = 0x65757274;
        public const int Fals32 = 0x736C6166;
        public const byte FalseFinal = (byte)'e';
        public const int Null32 = 0x6C6C756E;

        // whitespace
        public const byte WhitespaceSpace = (byte)' ';
        public const byte WhitespaceNewline = (byte)'\n';
        public const byte WhitespaceCarriageReturn = (byte)'\r';
        public const byte WhitespaceHorizontalTab = (byte)'\t';

        // misc
        public static byte[] BOM { get; } = new byte[] { 0xEF, 0xBB, 0xBF };
    }
}
