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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JsonBourne.DocumentModel;

namespace JsonBourne.DocumentReader
{
    internal sealed class JsonStreamReader
    {
        private JsonDocumentReader Reader { get; } = new JsonDocumentReader();

        public JsonStreamReader()
        { }

        public async Task<JsonValue> ParseJsonAsync(Stream inputStream, MemoryPool<byte> memoryPool = default, CancellationToken cancellationToken = default)
        {
            memoryPool ??= MemoryPool<byte>.Shared;

            ValueParseResult parseResult;
            JsonValue jsonValue;
            try
            {
                var br = 0;
                var first = true;
                using var jsonReader = this.Reader;
                using var memOwner = memoryPool.Rent(4096);
                var mem = memOwner.Memory;

                while ((br = await inputStream.ReadAsync(mem, cancellationToken)) != 0)
                {
                    if (first)
                    {
                        // strip BOM if necessary
                        if (br > 3 && mem.Slice(0, 3).Span.SequenceEqual(JsonTokens.BOM))
                            mem = mem[3..];
                    }

                    parseResult = jsonReader.TryParse(mem.Slice(0, br), out jsonValue, out _, out _, out _);
                    switch (parseResult.Type)
                    {
                        case ValueParseResultType.Success:
                            return jsonValue;

                        case ValueParseResultType.Failure:
                        case ValueParseResultType.Intederminate:
                            throw new JsonParseException(parseResult.StreamPosition, parseResult.Line, parseResult.Column, parseResult.FailingRune, parseResult.Reason);
                    }

                    if (first)
                    {
                        mem = memOwner.Memory;
                        first = false;
                    }
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
    }
}
