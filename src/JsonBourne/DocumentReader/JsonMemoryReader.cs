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
using JsonBourne.DocumentModel;

namespace JsonBourne.DocumentReader
{
    internal sealed class JsonMemoryReader : IDisposable
    {
        private JsonDocumentReader Reader { get; } = new JsonDocumentReader();

        public JsonMemoryReader()
        { }

        public JsonValue ParseJson(ReadOnlyMemory<byte> inputBuffer)
            => this.ParseJson(inputBuffer.Span);

        public JsonValue ParseJson(ReadOnlySpan<byte> inputBuffer)
        {
            ValueParseResult parseResult;
            JsonValue jsonValue;
            try
            {
                // strip BOM if necessary
                if (inputBuffer.Length > 3 && inputBuffer.Slice(0, 3).SequenceEqual(JsonTokens.BOM))
                    inputBuffer = inputBuffer[3..];

                using var jsonReader = this.Reader;

                parseResult = jsonReader.TryParse(inputBuffer, out jsonValue, out _, out _, out _);
                switch (parseResult.Type)
                {
                    case ValueParseResultType.Success:
                        return jsonValue;

                    case ValueParseResultType.Failure:
                        throw new JsonParseException(parseResult.StreamPosition, parseResult.Line, parseResult.Column, parseResult.FailingRune, parseResult.Reason);
                }

                parseResult = jsonReader.TryParse(ReadOnlySpan<byte>.Empty, out jsonValue, out _, out _, out _);
            }
            catch (Exception ex)
            {
                throw new JsonParseException(ex);
            }

            return parseResult.Type switch
            {
                ValueParseResultType.Success => jsonValue,
                _ => throw new JsonParseException(parseResult.StreamPosition, parseResult.Line, parseResult.Column, parseResult.FailingRune, parseResult.Reason),
            };
        }

        public void Dispose()
            => this.Reader.Dispose();
    }
}
