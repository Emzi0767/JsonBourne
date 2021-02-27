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
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace JsonBourne.DocumentModel
{
    /// <summary>
    /// Represents an object in a JSON document.
    /// </summary>
    public sealed class JsonObjectValue : JsonValue<IReadOnlyDictionary<string, JsonValue>>, IReadOnlyDictionary<string, JsonValue>
    {
        /// <summary>
        /// Gets a JSON value under specified key.
        /// </summary>
        /// <param name="key">Key to retrieve a value for.</param>
        /// <returns>Retrieved value.</returns>
        public JsonValue this[string key]
            => this.Value[key];

        /// <summary>
        /// Gets the collection of keys for this JSON object.
        /// </summary>
        public IEnumerable<string> Keys
            => this.Value.Keys;

        /// <summary>
        /// Gets the collection of values for this JSON object.
        /// </summary>
        public IEnumerable<JsonValue> Values
            => this.Value.Values;

        /// <summary>
        /// Gets the total number of key-value pairs in this object.
        /// </summary>
        public int Count
            => this.Value.Count;

        /// <summary>
        /// Gets the debugger display of this JSON value.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public override string DebuggerDisplay
            => $"JSON Value (object): {this.Count} children";

        internal JsonObjectValue(IReadOnlyDictionary<string, JsonValue> values)
            : base(values)
        { }

        /// <summary>
        /// Checks whether this object contains a specified key.
        /// </summary>
        /// <param name="key">Key to check for.</param>
        /// <returns>Whether the key is present in the object.</returns>
        public bool ContainsKey(string key)
            => this.Value.ContainsKey(key);

        /// <summary>
        /// Attempts to retrieve a value from this object by its key.
        /// </summary>
        /// <param name="key">Key to attempt to retrieve the value for.</param>
        /// <param name="value">Retrieved value.</param>
        /// <returns>Whether the retrieval was successful.</returns>
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out JsonValue value)
            => this.Value.TryGetValue(key, out value);

        /// <summary>
        /// Gets an enumerator over key-value pairs contained in this object.
        /// </summary>
        /// <returns>Enumerator over key-value pairs in this object.</returns>
        public IEnumerator<KeyValuePair<string, JsonValue>> GetEnumerator()
            => this.Value.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();
    }
}
