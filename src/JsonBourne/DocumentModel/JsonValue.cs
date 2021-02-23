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

namespace JsonBourne.DocumentModel
{
    /// <summary>
    /// Represents a base JSON value.
    /// </summary>
    public abstract class JsonValue
    {
        internal JsonValue()
        { }

        /// <summary>
        /// Converts a JSON value to boolean, if possible.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static explicit operator bool(JsonValue value)
            => value is JsonValue<bool> jsonBoolean
            ? jsonBoolean.Value
            : throw new InvalidCastException("This value is not a boolean value.");

        /// <summary>
        /// Converts a JSON value to string, if possible.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static explicit operator string(JsonValue value)
            => value is JsonValue<string> jsonString
            ? jsonString.Value
            : throw new InvalidCastException("This value is not a string value.");

        /// <summary>
        /// Converts a JSON value to 64-bit floating-point value, if possible.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static explicit operator double(JsonValue value)
            => value is JsonValue<double> jsonDouble
            ? jsonDouble.Value
            : throw new InvalidCastException("This value is not an floating-point value.");
    }

    /// <summary>
    /// Represents a typed JSON value.
    /// </summary>
    /// <typeparam name="T">Type of the JSON value.</typeparam>
    public abstract class JsonValue<T> : JsonValue
    {
        /// <summary>
        /// Gets the value associated with this JSON value.
        /// </summary>
        public virtual T Value { get; }

        internal JsonValue(T value)
        {
            this.Value = value;
        }
    }
}
