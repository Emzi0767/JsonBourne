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
using System.Buffers.Text;
using Emzi0767.Types;
using JsonBourne.DocumentModel;

namespace JsonBourne.DocumentReader
{
    // BEHOLD, MY MIGHTY STATE MACHINE COROUTINE

    internal sealed class JsonNumberReader : IJsonValueReader<double>
    {
        private MemoryBuffer<byte> Buffer { get; set; }

        private NumberStructure _currentStructure = NumberStructure.None;
        private NumberPart _lastPart = NumberPart.None;

        public JsonNumberReader()
        {
            this.Buffer = null;
        }

        public ValueParseResult TryParse(ReadOnlyMemory<byte> buffer, out double result, out int consumedLength)
            => this.TryParse(buffer.Span, out result, out consumedLength);

        public ValueParseResult TryParse(ReadOnlySpan<byte> readerSpan, out double result, out int consumedLength)
        {
            result = double.NaN;
            consumedLength = 0;

            // if span is empty, and no parsing occured, signal EOF immediately
            if (readerSpan.Length <= 0 && this._lastPart == NumberPart.None)
                return ValueParseResult.EOF;

            // if we are not continuing, check what we're parsing
            if (this.Buffer == null)
            {
                switch (readerSpan[consumedLength++])
                {
                    // a number in JSON can begin with - or digits 0-9
                    case JsonTokens.NumberSign:
                        this._currentStructure = NumberStructure.HasSign;
                        this._lastPart = NumberPart.NumberSign;
                        break;

                    // digit zero is a bit special in that if it's the first digit in a number, it becomes the only
                    // legal digit before decimal point, hence special handling for it
                    case JsonTokens.Digit0:
                        this._currentStructure = NumberStructure.LeadingZero;
                        this._lastPart = NumberPart.FirstDigit;
                        break;

                    // digits 1-9 are also valid as starting characters of a number, and unlike 0, they do not
                    // restrict pre-decimal point digit count (though IEEE754 64-bit binary float limits still apply)
                    case JsonTokens.Digit1:
                    case JsonTokens.Digit2:
                    case JsonTokens.Digit3:
                    case JsonTokens.Digit4:
                    case JsonTokens.Digit5:
                    case JsonTokens.Digit6:
                    case JsonTokens.Digit7:
                    case JsonTokens.Digit8:
                    case JsonTokens.Digit9:
                        this._currentStructure = NumberStructure.LeadingNonzero;
                        this._lastPart = NumberPart.FirstDigit;
                        break;

                    // not a legal character
                    default:
                        return ValueParseResult.Failure;
                }
            }

            // if we got empty when previous parsing occured, just don't parse, it's an end-of-content marker
            var completedParsing = false;
            if (readerSpan.Length > 0)
            {
                var offByOne = false;

                // try reading the number
                while (consumedLength < readerSpan.Length)
                {
                    switch (readerSpan[consumedLength++])
                    {
                        // digit 0 is special
                        // if it's the first digit in the non-fractional part, it is the only legal digit before decimal point
                        // otherwise it behaves like a regular digit
                        // this means it can appear:
                        // - as first digit before decimal point
                        // - as non-first digit before decimal point, if first digit was not a 0
                        // - as a digit after decimal point before exponent mark
                        // - as a digit after exponent mark or exponent sign
                        // see: https://www.json.org/img/number.png
                        case JsonTokens.Digit0:
                            if (this._lastPart == NumberPart.FirstDigit && this._currentStructure.HasFlag(NumberStructure.LeadingZero))
                                return _cleanup(this, ValueParseResult.Failure);

                            if (this._lastPart == NumberPart.NumberSign)
                            {
                                this._currentStructure |= NumberStructure.LeadingZero;
                                this._lastPart = NumberPart.FirstDigit;
                            }
                            else
                            {
                                this._lastPart = this._lastPart switch
                                {
                                    NumberPart.FirstDigit => NumberPart.Digit,
                                    NumberPart.FractionDot => NumberPart.FractionDigit,
                                    NumberPart.ExponentMarker or NumberPart.ExponentSign => NumberPart.ExponentDigit,
                                    _ => this._lastPart
                                };
                            }
                            break;

                        // non-0 digits can appear:
                        // - as first digit before decimal points
                        // - as non-first digit before decimal point, if first digit was not a 0
                        // - as a digit after decimal point before exponent mark
                        // - as a digit after exponent mark or exponent sign
                        // see: https://www.json.org/img/number.png
                        case JsonTokens.Digit1:
                        case JsonTokens.Digit2:
                        case JsonTokens.Digit3:
                        case JsonTokens.Digit4:
                        case JsonTokens.Digit5:
                        case JsonTokens.Digit6:
                        case JsonTokens.Digit7:
                        case JsonTokens.Digit8:
                        case JsonTokens.Digit9:
                            if (this._lastPart == NumberPart.FirstDigit && this._currentStructure.HasFlag(NumberStructure.LeadingZero))
                                return _cleanup(this, ValueParseResult.Failure);

                            if (this._lastPart == NumberPart.NumberSign)
                            {
                                this._currentStructure |= NumberStructure.LeadingNonzero;
                                this._lastPart = NumberPart.FirstDigit;
                            }
                            else
                            {
                                this._lastPart = this._lastPart switch
                                {
                                    NumberPart.FirstDigit => NumberPart.Digit,
                                    NumberPart.FractionDot => NumberPart.FractionDigit,
                                    NumberPart.ExponentMarker or NumberPart.ExponentSign => NumberPart.ExponentDigit,
                                    _ => this._lastPart
                                };
                            }
                            break;

                        // decimal separator can appear only after at least one digit, and only once
                        case JsonTokens.DecimalSeparator:
                            if (this._lastPart != NumberPart.Digit && this._lastPart != NumberPart.FirstDigit)
                                return _cleanup(this, ValueParseResult.Failure);

                            this._currentStructure |= NumberStructure.Fraction;
                            this._lastPart = NumberPart.FractionDot;
                            break;

                        // exponent marker can appear only after at least one digit, or at least one digit after
                        // decimal point, and only once, regardless of variety
                        case JsonTokens.ExponentSmall:
                        case JsonTokens.ExponentCapital:
                            if (this._lastPart != NumberPart.FirstDigit && this._lastPart != NumberPart.Digit && this._lastPart != NumberPart.FractionDigit)
                                return _cleanup(this, ValueParseResult.Failure);

                            this._currentStructure |= NumberStructure.Exponent;
                            this._lastPart = NumberPart.ExponentMarker;

                            if (this._currentStructure.HasFlag(NumberStructure.Fraction))
                                this._currentStructure |= NumberStructure.FractionValid;

                            break;

                        // exponent sign can appear only after exponent marker
                        case JsonTokens.NumberSign:
                        case JsonTokens.ExponentSignPositive:
                            if (this._lastPart != NumberPart.ExponentMarker)
                                return _cleanup(this, ValueParseResult.Failure);

                            this._currentStructure |= NumberStructure.SignedExponent;
                            this._lastPart = NumberPart.ExponentSign;
                            break;

                        // this is a situation where a non number-character is encountered
                        // this is invalid if immediately after number sign, decimal point, exponent marker, or
                        // exponent sign, otherwise consider it a completed number
                        default:
                            switch (this._lastPart)
                            {
                                case NumberPart.NumberSign:
                                case NumberPart.FractionDot:
                                case NumberPart.ExponentMarker:
                                case NumberPart.ExponentSign:
                                    return _cleanup(this, ValueParseResult.Failure);
                            }

                            offByOne = true;
                            completedParsing = true;
                            break;
                    }
                }

                // due to postincrement, at the end of the parsing process we are off by one
                if (offByOne)
                    --consumedLength;
            }
            // we got an empty buffer so we can assume this means EOF on underlying data source as well
            // in practice this will only happen on JSON that consists of a number value only at the root
            else
            {
                // if last part is not a legal end to a number, fail
                switch (this._lastPart)
                {
                    case NumberPart.NumberSign:
                    case NumberPart.FractionDot:
                    case NumberPart.ExponentMarker:
                    case NumberPart.ExponentSign:
                        return _cleanup(this, ValueParseResult.Failure);
                }

                completedParsing = true;
            }

            // did we reach the end of input before running out of it
            var input = readerSpan;
            if (completedParsing)
            {
                bool parseResult;
                int expectedLength, buffConsumed;

                // do we need a special buffer?
                if (this.Buffer != null)
                {
                    Span<byte> buff = stackalloc byte[(int)this.Buffer.Length + consumedLength];

                    this.Buffer.Read(buff, 0, out var written);
                    input.Slice(0, consumedLength).CopyTo(buff[written..]);

                    parseResult = Utf8Parser.TryParse(buff, out result, out buffConsumed);
                    expectedLength = buff.Length;
                }
                // no, parse as-is
                else
                {
                    input = input.Slice(0, consumedLength);
                    parseResult = Utf8Parser.TryParse(input, out result, out buffConsumed);
                    expectedLength = input.Length;
                }

                return _cleanup(this, parseResult && buffConsumed == expectedLength ? ValueParseResult.Success : ValueParseResult.Failure);
            }
            // no, store state and yield back
            else
            {
                if (this.Buffer == null)
                    this.Buffer = new MemoryBuffer<byte>(segmentSize: 128, initialSegmentCount: 1);

                this.Buffer.Write(input.Slice(0, consumedLength));
                return ValueParseResult.EOF;
            }

            static ValueParseResult _cleanup(IDisposable rdr, ValueParseResult result)
            {
                rdr.Dispose();
                return result;
            }
        }

        public void Dispose()
        {
            if (this.Buffer != null)
            {
                this.Buffer.Dispose();
                this.Buffer = null;
            }

            this._currentStructure = NumberStructure.None;
            this._lastPart = NumberPart.None;
        }

        [Flags]
        private enum NumberStructure : int
        {
            None = 0,
            HasSign = 1,
            LeadingZero = 2,
            LeadingNonzero = 4,
            Fraction = 8,
            FractionValid = 16,
            Exponent = 32,
            SignedExponent = 64,
            ExponentPositive = 128,
            ExponentNegative = 256,
            ExponentValid = 512
        }

        private enum NumberPart : int
        {
            None = 0,
            NumberSign = 1,
            FirstDigit = 2,
            Digit = 3,
            FractionDot = 4,
            FractionDigit = 5,
            ExponentMarker = 6,
            ExponentSign = 7,
            ExponentDigit = 8
        }
    }
}
