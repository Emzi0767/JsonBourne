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
    internal sealed class JsonNumberReader : IJsonValueReader<double>
    {
        private MemoryBuffer Buffer { get; set; }

        private NumberStructure _currentStructure = NumberStructure.None;
        private NumberPart _lastPart = NumberPart.None;

        public JsonNumberReader()
        {
            this.Buffer = null;
        }

        public ValueParseResult TryParse(ReadOnlyMemory<byte> buffer, out double result, out int consumedLength)
        {
            var readerSpan = buffer.Span;

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
                    case JsonTokens.NumberSign:
                        this._currentStructure = NumberStructure.HasSign;
                        this._lastPart = NumberPart.NumberSign;
                        break;

                    case JsonTokens.Digit0:
                        this._currentStructure = NumberStructure.LeadingZero;
                        this._lastPart = NumberPart.FirstDigit;
                        break;

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
                while (consumedLength < buffer.Length)
                {
                    switch (readerSpan[consumedLength++])
                    {
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

                        case JsonTokens.DecimalSeparator:
                            if (this._lastPart != NumberPart.Digit && this._lastPart != NumberPart.FirstDigit)
                                return _cleanup(this, ValueParseResult.Failure);

                            this._currentStructure |= NumberStructure.Fraction;
                            this._lastPart = NumberPart.FractionDot;
                            break;

                        case JsonTokens.ExponentSmall:
                        case JsonTokens.ExponentCapital:
                            if (this._lastPart != NumberPart.FirstDigit && this._lastPart != NumberPart.Digit && this._lastPart != NumberPart.FractionDigit)
                                return _cleanup(this, ValueParseResult.Failure);

                            this._currentStructure |= NumberStructure.Exponent;
                            this._lastPart = NumberPart.ExponentMarker;

                            if (this._currentStructure.HasFlag(NumberStructure.Fraction))
                                this._currentStructure |= NumberStructure.FractionValid;

                            break;

                        case JsonTokens.NumberSign:
                        case JsonTokens.ExponentSignPositive:
                            if (this._lastPart != NumberPart.ExponentMarker)
                                return _cleanup(this, ValueParseResult.Failure);

                            this._currentStructure |= NumberStructure.SignedExponent;
                            this._lastPart = NumberPart.ExponentSign;
                            break;

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

                if (offByOne)
                    --consumedLength;
            }
            else
            {
                completedParsing = true;
            }

            // did we reach the end of input before running out of it
            var input = readerSpan;
            if (completedParsing)
            {
                // do we need a special buffer?
                if (this.Buffer != null)
                {
                    Span<byte> buff = stackalloc byte[(int)this.Buffer.Length + consumedLength];

                    this.Buffer.Read(buff, 0, out var written);
                    input.Slice(0, consumedLength).CopyTo(buff[written..]);

                    var parseResult = Utf8Parser.TryParse(buff, out result, out var buffConsumed);
                    return _cleanup(this, parseResult && buffConsumed == buff.Length ? ValueParseResult.Success : ValueParseResult.Failure);
                }
                else
                {
                    input = input.Slice(0, consumedLength);
                    return Utf8Parser.TryParse(input, out result, out var buffConsumed) && buffConsumed == input.Length ? ValueParseResult.Success : ValueParseResult.Failure;
                }
            }
            else
            {
                if (this.Buffer == null)
                    this.Buffer = new MemoryBuffer(segmentSize: 128, initialSegmentCount: 1);

                this.Buffer.Write(input.Slice(0, consumedLength));
                return ValueParseResult.EOF;
            }

            static ValueParseResult _cleanup(JsonNumberReader rdr, ValueParseResult result)
            {
                if (rdr.Buffer != null)
                {
                    rdr.Buffer.Dispose();
                    rdr.Buffer = null;
                }

                rdr._currentStructure = NumberStructure.None;
                rdr._lastPart = NumberPart.None;

                return result;
            }
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
