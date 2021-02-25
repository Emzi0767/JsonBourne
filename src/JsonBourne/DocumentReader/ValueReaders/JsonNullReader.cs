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
using System.Buffers.Binary;
using JsonBourne.DocumentModel;

namespace JsonBourne.DocumentReader
{
    internal sealed class JsonNullReader : IJsonValueReader<object>
    {
        private Memory<byte> Buffer { get; }

        private int _buffPos;

        public JsonNullReader()
        {
            this.Buffer = new byte[4];
            this._buffPos = 0;
        }

        public ValueParseResult TryParse(ReadOnlyMemory<byte> buffer, out object result, out int consumedLength)
        {
            var readerSpan = buffer.Span;

            result = null;
            consumedLength = 0;

            // if span is empty, signal EOF immediately
            if (readerSpan.Length <= 0)
                return ValueParseResult.EOF;

            // determine what we're reading
            var src = this._buffPos > 0 ? this.Buffer.Span : readerSpan;
            if (src[0] != JsonTokens.NullFirst)
            {
                this._buffPos = 0;
                return ValueParseResult.Failure;
            }

            // if reader buffer is too small, copy its contents then signal EOF
            var tooSmall = readerSpan.Length < 4 - this._buffPos;
            if (tooSmall || this._buffPos > 0)
            {
                readerSpan.CopyTo(this.Buffer.Span[this._buffPos..]);
                this._buffPos += readerSpan.Length;
                consumedLength = readerSpan.Length;

                if (tooSmall)
                    return ValueParseResult.EOF;
            }
            else
            {
                consumedLength = 4;
            }

            // try to parse
            this._buffPos = 0;
            var src32 = BinaryPrimitives.ReadInt32LittleEndian(src);
            return src32 switch
            {
                JsonTokens.Null32 => ValueParseResult.Success,

                // tokens didn't match
                _ => ValueParseResult.Failure,
            };
        }
    }
}
