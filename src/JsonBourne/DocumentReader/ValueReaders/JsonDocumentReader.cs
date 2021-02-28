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
using System.Text;
using JsonBourne.DocumentModel;

namespace JsonBourne.DocumentReader
{
    internal sealed class JsonDocumentReader : IJsonValueReader<JsonValue>
    {
        private int _lineSpan, _colSpan, _streamPos;
        private IJsonValueReader _innerReader;

        public JsonDocumentReader()
        {
            this.Reset();
        }

        public ValueParseResult TryParse(ReadOnlyMemory<byte> buffer, out JsonValue result, out int consumedLength, out int lineSpan, out int colSpan)
            => this.TryParse(buffer.Span, out result, out consumedLength, out lineSpan, out colSpan);

        public ValueParseResult TryParse(ReadOnlySpan<byte> readerSpan, out JsonValue result, out int consumedLength, out int lineSpan, out int colSpan)
        {
            result = null;
            consumedLength = 0;
            lineSpan = 1;
            colSpan = 0;

            // is input empty
            if (readerSpan.Length <= 0 && this._innerReader == null)
            {
                // did any prior processing occur
                return this._innerReader != null
                    ? _cleanup(this, ValueParseResult.FailureEOF)
                    : ValueParseResult.EOF;
            }

            // continue parsing?
            if (this._innerReader != null)
            {
                var innerResult = this.ParseInner(readerSpan, out result, ref consumedLength);
                switch (innerResult.Type)
                {
                    case ValueParseResultType.Success:
                        this._innerReader.Dispose();
                        this._innerReader = null;
                        return _cleanup(this, innerResult);

                    case ValueParseResultType.EOF:
                        return innerResult;

                    case ValueParseResultType.Failure:
                        return _cleanup(this, innerResult);
                }
            }

            // not continuing, find any token
            while (consumedLength < readerSpan.Length)
            {
                switch (readerSpan[consumedLength++])
                {
                    case JsonTokens.WhitespaceSpace:
                        ++this._colSpan;
                        ++this._streamPos;
                        break;

                    case JsonTokens.WhitespaceHorizontalTab:
                        this._colSpan += 4; // fite me
                        ++this._streamPos;
                        break;

                    case JsonTokens.WhitespaceCarriageReturn:
                        // usually as part of CRLF, really no other reason for it to exist
                        // old macs don't exist
                        break;

                    case JsonTokens.WhitespaceNewline:
                        ++this._lineSpan;
                        this._colSpan = 0;
                        ++this._streamPos;
                        break;

                    case JsonTokens.NullFirst:
                        this._innerReader = new JsonNullReader();
                        break;

                    case JsonTokens.TrueFirst:
                    case JsonTokens.FalseFirst:
                        this._innerReader = new JsonBooleanReader();
                        break;

                    case JsonTokens.NumberSign:
                    case JsonTokens.Digit0:
                    case JsonTokens.Digit1:
                    case JsonTokens.Digit2:
                    case JsonTokens.Digit3:
                    case JsonTokens.Digit4:
                    case JsonTokens.Digit5:
                    case JsonTokens.Digit6:
                    case JsonTokens.Digit7:
                    case JsonTokens.Digit8:
                    case JsonTokens.Digit9:
                        this._innerReader = new JsonNumberReader();
                        break;

                    case JsonTokens.QuoteMark:
                        this._innerReader = new JsonStringReader();
                        break;

                    case JsonTokens.OpeningBracket:
                        this._innerReader = new JsonArrayReader(new ValueReaderCollection());
                        break;

                    case JsonTokens.OpeningBrace:
                        this._innerReader = new JsonObjectReader(new ValueReaderCollection());
                        break;

                    default:
                        if (Rune.DecodeFromUtf8(readerSpan[(consumedLength - 1)..], out var rune, out _) != OperationStatus.Done)
                            rune = default;

                        return _cleanup(this, ValueParseResult.Failure("Unexpected token while parsing JSON.", rune));
                }

                // parsing done?
                if (this._innerReader != null)
                {
                    var innerResult = this.ParseInner(readerSpan[(consumedLength - 1)..], out result, ref consumedLength);
                    switch (innerResult.Type)
                    {
                        case ValueParseResultType.Success:
                            this._innerReader.Dispose();
                            this._innerReader = null;
                            return _cleanup(this, innerResult);

                        case ValueParseResultType.EOF:
                            return innerResult;

                        case ValueParseResultType.Failure:
                            return _cleanup(this, innerResult);
                    }
                }
            }

            return _cleanup(this, ValueParseResult.FailureEOF);

            static ValueParseResult _cleanup(IJsonValueReader rdr, ValueParseResult result)
            {
                rdr.Reset();
                return result;
            }
        }

        public void Dispose()
            => this.Reset();

        public void Reset()
        {
            this._innerReader?.Dispose();
            this._innerReader = null;
            this._lineSpan = 1;
            this._colSpan = 0;
            this._streamPos = 0;
        }

        private ValueParseResult ParseInner(ReadOnlySpan<byte> input, out JsonValue resultValue, ref int consumedBytes)
        {
            ValueParseResult innerResult;
            int innerLineSpan, innerColSpan, innerConsumed;
            resultValue = null;
            switch (this._innerReader)
            {
                case JsonNullReader nullReader:
                    {
                        innerResult = nullReader.TryParse(input, out _, out innerConsumed, out innerLineSpan, out innerColSpan);
                        if (innerResult.Type == ValueParseResultType.Success)
                            resultValue = JsonNullValue.Null;
                    }
                    break;

                case JsonBooleanReader boolReader:
                    {
                        innerResult = boolReader.TryParse(input, out var innerResultValue, out innerConsumed, out innerLineSpan, out innerColSpan);
                        if (innerResult.Type == ValueParseResultType.Success)
                            resultValue = innerResultValue ? JsonBooleanValue.True : JsonBooleanValue.False;
                    }
                    break;

                case JsonNumberReader numberReader:
                    {
                        innerResult = numberReader.TryParse(input, out var innerResultValue, out innerConsumed, out innerLineSpan, out innerColSpan);
                        if (innerResult.Type == ValueParseResultType.Success)
                            resultValue = new JsonNumberValue(innerResultValue);
                    }
                    break;

                case JsonStringReader stringReader:
                    {
                        innerResult = stringReader.TryParse(input, out var innerResultValue, out innerConsumed, out innerLineSpan, out innerColSpan);
                        if (innerResult.Type == ValueParseResultType.Success)
                            resultValue = new JsonStringValue(innerResultValue);
                    }
                    break;

                case JsonArrayReader arrayReader:
                    {
                        innerResult = arrayReader.TryParse(input, out var innerResultValue, out innerConsumed, out innerLineSpan, out innerColSpan);
                        if (innerResult.Type == ValueParseResultType.Success)
                            resultValue = new JsonArrayValue(innerResultValue);
                    }
                    break;

                case JsonObjectReader objectReader:
                    {
                        innerResult = objectReader.TryParse(input, out var innerResultValue, out innerConsumed, out innerLineSpan, out innerColSpan);
                        if (innerResult.Type == ValueParseResultType.Success)
                            resultValue = new JsonObjectValue(innerResultValue);
                    }
                    break;

                default:
                    return ValueParseResult.Failure("Unexpected inner reader type.", default);
            }

            this._streamPos += innerConsumed;
            consumedBytes += innerConsumed - 1;

            switch (innerResult.Type)
            {
                // inner parser concluded its operation
                // append its result, advance state
                case ValueParseResultType.Success:
                    this._lineSpan += innerLineSpan - 1;
                    this._colSpan = innerLineSpan > 1
                        ? innerColSpan
                        : this._colSpan + innerColSpan;

                    return ValueParseResult.Success;

                // inner parser ran out of input
                case ValueParseResultType.EOF:
                    return ValueParseResult.EOF;

                // inner parser failed
                case ValueParseResultType.Failure:
                default:
                    this._lineSpan += innerLineSpan - 1;
                    this._colSpan = innerLineSpan > 1
                        ? innerColSpan
                        : this._colSpan + innerColSpan;
                    return innerResult.Enrich(this._streamPos, this._lineSpan - 1, this._colSpan);
            }
        }
    }
}
