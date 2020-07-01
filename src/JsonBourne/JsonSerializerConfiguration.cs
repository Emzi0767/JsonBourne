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
using Emzi0767;

namespace JsonBourne
{
    /// <summary>
    /// Specifies configuration options for emitting and consuming JSON data by the <see cref="JsonSerializer"/>.
    /// </summary>
    public sealed class JsonSerializerConfiguration
    {
        /// <summary>
        /// <para>Gets or sets the indentation mode.</para>
        /// <para>See <see cref="JsonIndentation"/> enum for more details and available options.</para>
        /// <para>Defaults to <see cref="JsonIndentation.FourSpaces"/>.</para>
        /// </summary>
        public JsonIndentation IndentationMode { get; set; } = JsonIndentation.FourSpaces;

        /// <summary>
        /// <para>Gets or sets whether <see langword="null"/> values will be emitted into the resulting JSON.</para>
        /// <para>This controls the emitting behaviour for <see cref="Nullable{T}"/> types. <see cref="Optional{T}"/> types are not affected by this.</para>
        /// <para>Defaults to <see langword="true"/>.</para>
        /// </summary>
        public bool EmitNullValues { get; set; } = true;

        /// <summary>
        /// <para>Gets or sets whether the <see cref="JsonSerializer"/> should emit and parse JSON comments.</para>
        /// <para>JSON comments are a non-standard feature of the JSON specification, and are generally not supported by most software.</para>
        /// <para>Settings this to true will make the parser consume comments, and the serializer will emit them. A setting of false will prevent comments from being emitted, and the parser will throw exceptions whenever any are encountered.</para>
        /// <para>Defautls to <see langword="false"/>.</para>
        /// </summary>
        public bool EnableComments { get; set; } = false;

        /// <summary>
        /// <para>Gets or sets how <see cref="JsonSerializer"/> handles encoding of objects representing time.</para>
        /// <para>See <see cref="JsonTimeEncoding"/> enum for more details and available options.</para>
        /// <para>Defaults to <see cref="JsonTimeEncoding.Default"/>.</para>
        /// </summary>
        public JsonTimeEncoding TimeEncoding { get; set; } = JsonTimeEncoding.Default;

        /// <summary>
        /// <para>Gets or sets how <see cref="JsonSerializer"/> handles encoding of special floating-point values.</para>
        /// <para>See <see cref="JsonFloatSpecialValueEncoding"/> enum for more details and available options.</para>
        /// <para>Defaults to <see cref="JsonFloatSpecialValueEncoding.String"/>.</para>
        /// </summary>
        public JsonFloatSpecialValueEncoding FloatSpecialValueEncoding { get; set; } = JsonFloatSpecialValueEncoding.String;
    }
}
