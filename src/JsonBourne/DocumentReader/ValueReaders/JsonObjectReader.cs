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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsonBourne.DocumentModel;

namespace JsonBourne.DocumentReader.ValueReaders
{
    internal sealed class JsonObjectReader : IJsonValueReader<IReadOnlyDictionary<string, JsonValue>>
    {
        private int _lineSpan, _colSpan, _streamPos;
        private IDictionary<string, JsonValue> _obj;

        public JsonObjectReader()
        {
            this._obj = null;
            this._lineSpan = 1;
            this._colSpan = 0;
            this._streamPos = 0;
        }

        public ValueParseResult TryParse(ReadOnlyMemory<byte> buffer, out IReadOnlyDictionary<string, JsonValue> result, out int consumedLength, out int lineSpan, out int colSpan)
            => this.TryParse(buffer.Span, out result, out consumedLength, out lineSpan, out colSpan);

        public ValueParseResult TryParse(ReadOnlySpan<byte> readerSpan, out IReadOnlyDictionary<string, JsonValue> result, out int consumedLength, out int lineSpan, out int colSpan)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            this._obj = null;
            this._streamPos = 0;
            this._lineSpan = 1;
            this._colSpan = 0;
        }
    }
}
