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
using System.Buffers.Binary;
using System.Text;
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

        public ValueParseResult TryParse(ReadOnlyMemory<byte> buffer, out object result, out int consumedLength, out int lineSpan, out int colSpan)
            => this.TryParse(buffer.Span, out result, out consumedLength, out lineSpan, out colSpan);

        public ValueParseResult TryParse(ReadOnlySpan<byte> readerSpan, out object result, out int consumedLength, out int lineSpan, out int colSpan)
        {
            result = null;
            consumedLength = 0;
            lineSpan = 1;
            colSpan = 0;

            // is input empty
            if (readerSpan.Length <= 0)
            {
                // did any prior processing occur
                return this._buffPos > 0
                    ? _cleanup(this, ValueParseResult.FailureEOF)
                    : ValueParseResult.EOF;
            }

            // determine what we're reading
            var src = this._buffPos > 0 ? this.Buffer.Span : readerSpan;
            if (src[0] != JsonTokens.NullFirst)
            {
                this._buffPos = 0;
                if (Rune.DecodeFromUtf8(readerSpan, out var rune, out _) != OperationStatus.Done)
                    rune = default;

                return ValueParseResult.Failure("Unexpected token, expected null.", rune);
            }

            // if reader buffer is too small, copy its contents then signal EOF
            var tooSmall = readerSpan.Length < 4 - this._buffPos;
            if (tooSmall || this._buffPos > 0)
            {
                var tlen = Math.Min(4 - this._buffPos, readerSpan.Length);

                readerSpan.Slice(0, tlen).CopyTo(this.Buffer.Span[this._buffPos..]);
                this._buffPos += tlen;
                consumedLength = tlen;

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
            colSpan = 4;
            return src32 switch
            {
                JsonTokens.Null32 => ValueParseResult.Success,

                // tokens didn't match
                _ => ValueParseResult.Failure("Unexpected token, expected null.", default),
            };

            static ValueParseResult _cleanup(IDisposable rdr, ValueParseResult result)
            {
                rdr.Dispose();
                return result;
            }
        }

        public void Dispose()
            => this._buffPos = 0;
    }
}
