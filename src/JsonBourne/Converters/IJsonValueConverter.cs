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

namespace JsonBourne.Converters
{
    /// <summary>
    /// Describes a converter, which converts a <see cref="JsonValue"/> to a .NET type.
    /// </summary>
    public interface IJsonValueConverter
    {
        /// <summary>
        /// Indicates whether this converter is capable of converting this value.
        /// </summary>
        /// <param name="propertyType">Type of value to check.</param>
        /// <returns>Whether this converter is capable of handling the indicated type.</returns>
        bool CanConvert(Type propertyType);

        /// <summary>
        /// Attempts to convert a given JSON value to a .NET value of supplied type.
        /// </summary>
        /// <param name="jsonValue">JSON value to convert.</param>
        /// <param name="propertyType">Target type of property to convert.</param>
        /// <param name="configuration">Configuration for the JSON parser.</param>
        /// <param name="result">Resulting value, if applicable.</param>
        /// <returns>Whether the conversion succeeded.</returns>
        bool TryConvertFromJson(JsonValue jsonValue, Type propertyType, JsonSerializerConfiguration configuration, out object result);

        /// <summary>
        /// Attempts to convert a given .NET value of supplied type to a JSON value.
        /// </summary>
        /// <param name="value">.NET value to convert.</param>
        /// <param name="propertyType">Source type of property to convert.</param>
        /// <param name="configuration">Configuration for the JSON parser.</param>
        /// <param name="result">Resulting value, if applicable.</param>
        /// <returns>Whether the conversion succeeded.</returns>
        bool TryConvertToJson(object value, Type propertyType, JsonSerializerConfiguration configuration, out JsonValue result);
    }
}
