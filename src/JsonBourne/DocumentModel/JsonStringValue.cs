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

namespace JsonBourne.DocumentModel
{
    /// <summary>
    /// Represents a string value in a JSON document.
    /// </summary>
    public sealed class JsonStringValue : JsonValue<string>
    {
        internal JsonStringValue(string value)
            : base(value)
        { }

        /// <summary>
        /// Converts this JSON string token to a .NET string.
        /// </summary>
        /// <param name="jsonString">JSON string value to convert.</param>
        public static implicit operator string(JsonStringValue jsonString)
            => jsonString.Value;
    }
}
