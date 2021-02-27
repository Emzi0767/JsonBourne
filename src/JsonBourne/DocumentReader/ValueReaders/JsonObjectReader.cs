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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsonBourne.DocumentModel;

namespace JsonBourne.DocumentReader
{
    internal sealed class JsonObjectReader : IJsonValueReader<IReadOnlyDictionary<string, JsonValue>>
    {
        private int _lineSpan, _colSpan, _streamPos;
        private IDictionary<string, JsonValue> _obj;
        private IJsonValueReader _innerReader;
        private readonly ValueReaderCollection _innerReaders;

        public JsonObjectReader(ValueReaderCollection valueReaders)
        {
            this._innerReaders = valueReaders;
            this.Dispose();
        }

        public ValueParseResult TryParse(ReadOnlyMemory<byte> buffer, out IReadOnlyDictionary<string, JsonValue> result, out int consumedLength, out int lineSpan, out int colSpan)
            => this.TryParse(buffer.Span, out result, out consumedLength, out lineSpan, out colSpan);

        public ValueParseResult TryParse(ReadOnlySpan<byte> readerSpan, out IReadOnlyDictionary<string, JsonValue> result, out int consumedLength, out int lineSpan, out int colSpan)
        {
            result = null;
            consumedLength = 0;
            lineSpan = 1;
            colSpan = 0;

            // is input empty
            if (readerSpan.Length <= 0)
            {
                // did any prior processing occur
                return this._obj != null
                    ? _cleanup(this, ValueParseResult.FailureEOF)
                    : ValueParseResult.EOF;
            }

            // if we are not continuing, ensure it's an object that's being parsed
            if (this._obj == null)
            {
                if (readerSpan[consumedLength++] != JsonTokens.OpeningBrace)
                {
                    if (Rune.DecodeFromUtf8(readerSpan, out var rune, out _) != OperationStatus.Done)
                        rune = default;

                    return _cleanup(this, ValueParseResult.Failure("Unexpected token, expected {.", rune));
                }

                this._obj = new Dictionary<string, JsonValue>();
                ++colSpan;
            }

            // if continuing, check if a token is being parsed
            throw new NotImplementedException();

            static ValueParseResult _cleanup(IDisposable rdr, ValueParseResult result)
            {
                rdr.Dispose();
                return result;
            }
        }

        public void Dispose()
        {
            this._innerReader?.Dispose();
            this._innerReader = null;
            this._obj = null;
            this._streamPos = 0;
            this._lineSpan = 1;
            this._colSpan = 0;
        }

        private enum ExpectedToken
        {
            ItemSeparator,

        }
    }
}
