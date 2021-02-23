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

using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace JsonBourne.DocumentModel
{
    /// <summary>
    /// Represents an array in a JSON document.
    /// </summary>
    public sealed class JsonArrayValue : JsonValue<IReadOnlyList<JsonValue>>, IReadOnlyList<JsonValue>
    {
        /// <summary>
        /// Gets a JSON value at specified index.
        /// </summary>
        /// <param name="index">Index to get the value for.</param>
        /// <returns>Retrieved value.</returns>
        public JsonValue this[int index]
            => this.Value[index];

        /// <summary>
        /// Gets the total number of items in this list.
        /// </summary>
        public int Count
            => this.Value.Count;

        internal JsonArrayValue(ImmutableArray<JsonValue> values)
            : base(values)
        { }

        /// <summary>
        /// Creates an enumerator over this JSON array.
        /// </summary>
        /// <returns>Enumerator over the JSON array.</returns>
        public IEnumerator<JsonValue> GetEnumerator()
            => this.Value.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();
    }
}
