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

using System.ComponentModel;
using System.Diagnostics;

namespace JsonBourne.DocumentModel
{
    /// <summary>
    /// Represents a true or false token in a JSON document.
    /// </summary>
    public sealed class JsonBooleanValue : JsonValue<bool>
    {
        /// <summary>
        /// Gets the true JSON value.
        /// </summary>
        public static JsonBooleanValue True { get; } = new JsonBooleanValue(true);

        /// <summary>
        /// Gets the false JSON value.
        /// </summary>
        public static JsonBooleanValue False { get; } = new JsonBooleanValue(false);

        /// <summary>
        /// Gets the debugger display of this JSON value.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public override string DebuggerDisplay
            => $"JSON Value (boolean): {this.Value}";

        private JsonBooleanValue(bool val)
            : base(val)
        { }

        /// <summary>
        /// Implicitly converts this JSON boolean token to a .NET boolean.
        /// </summary>
        /// <param name="jsonBoolean">JSON boolean value to convert.</param>
        public static implicit operator bool(JsonBooleanValue jsonBoolean)
            => jsonBoolean.Value;
    }
}
