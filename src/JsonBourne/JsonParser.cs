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
using JsonBourne.DocumentReader;

namespace JsonBourne
{
    /// <summary>
    /// Parses a JSON document and creates parsed JSON document trees. This class accepts UTF-8 inputs only. This class is not thread-safe.
    /// </summary>
    public sealed class JsonParser : IDisposable
    {
        private JsonStreamReader StreamReader { get; set; }
        private JsonMemoryReader MemoryReader { get; set; }

        /// <summary>
        /// Parses a JSON document from supplied memory region.
        /// </summary>
        /// <param name="input">Memory region to parse.</param>
        /// <returns>Parsed JSON document.</returns>
        public JsonValue Parse(ReadOnlyMemory<byte> input)
            => this.Parse(input.Span);

        /// <summary>
        /// Parses a JSON document from supplied memory region.
        /// </summary>
        /// <param name="input">Memory region to parse.</param>
        /// <returns>Parsed JSON document.</returns>
        public JsonValue Parse(ReadOnlySpan<byte> input)
        {
            var jsonReader = this.MemoryReader ??= new JsonMemoryReader();
            return jsonReader.ParseJson(input);
        }

        /// <summary>
        /// Parses a JSON document from supplied stream.
        /// </summary>
        /// <param name="inputStream">Stream to parse.</param>
        /// <param name="cancellationToken">Token to cancel asynchronous operations.</param>
        /// <param name="memoryPool">Memory pool to use for buffering.</param>
        /// <returns>Parsed JSON document.</returns>
        public async Task<JsonValue> ParseAsync(Stream inputStream, MemoryPool<byte> memoryPool = default, CancellationToken cancellationToken = default)
        {
            var jsonReader = this.StreamReader ??= new JsonStreamReader();
            return await jsonReader.ParseJsonAsync(inputStream, memoryPool, cancellationToken);
        }

        /// <summary>
        /// Releases all resources used by this parser.
        /// </summary>
        public void Dispose()
        {
            this.MemoryReader?.Dispose();
            this.StreamReader?.Dispose();
        }
    }
}
