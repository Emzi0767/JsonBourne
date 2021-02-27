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
using System.ComponentModel;
using System.Diagnostics;

namespace JsonBourne.DocumentModel
{
    /// <summary>
    /// Represents a base JSON value.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class JsonValue : IEquatable<bool>, IEquatable<string>, IEquatable<double>, IEquatable<long>, IEquatable<ulong>, IEquatable<int>, IEquatable<uint>, IEquatable<JsonValue>
    {
        /// <summary>
        /// Gets whether this value represents a null.
        /// </summary>
        public bool IsNull
            => this is JsonNullValue;

        /// <summary>
        /// Gets the debugger display of this JSON value.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public abstract string DebuggerDisplay { get; }

        internal JsonValue()
        { }

        /// <summary>
        /// Checks whether the value of this JSON value is a boolean and equal to another boolean.
        /// </summary>
        /// <param name="other">Value to compare against.</param>
        /// <returns>Whether the values are equal.</returns>
        public bool Equals(bool other)
            => this is JsonBooleanValue @bool && @bool.Value == other;

        /// <summary>
        /// Checks whether the value of this JSON value is a string and equal to another string.
        /// </summary>
        /// <param name="other">Value to compare against.</param>
        /// <returns>Whether the values are equal.</returns>
        public bool Equals(string other)
            => this is JsonStringValue @string && @string.Value == other;

        /// <summary>
        /// Checks whether the value of this JSON value is a number and equal to another double.
        /// </summary>
        /// <param name="other">Value to compare against.</param>
        /// <returns>Whether the values are equal.</returns>
        public bool Equals(double other)
            => this is JsonNumberValue number && number.Value == other;

        /// <summary>
        /// Checks whether the value of this JSON value is a number and equal to another long.
        /// </summary>
        /// <param name="other">Value to compare against.</param>
        /// <returns>Whether the values are equal.</returns>
        public bool Equals(long other)
            => this is JsonNumberValue number && number.Value == other;

        /// <summary>
        /// Checks whether the value of this JSON value is a number and equal to another ulong.
        /// </summary>
        /// <param name="other">Value to compare against.</param>
        /// <returns>Whether the values are equal.</returns>
        public bool Equals(ulong other)
            => this is JsonNumberValue number && number.Value == other;

        /// <summary>
        /// Checks whether the value of this JSON value is a number and equal to another int.
        /// </summary>
        /// <param name="other">Value to compare against.</param>
        /// <returns>Whether the values are equal.</returns>
        public bool Equals(int other)
            => this is JsonNumberValue number && number.Value == other;

        /// <summary>
        /// Checks whether the value of this JSON value is a number and equal to another uint.
        /// </summary>
        /// <param name="other">Value to compare against.</param>
        /// <returns>Whether the values are equal.</returns>
        public bool Equals(uint other)
            => this is JsonNumberValue number && number.Value == other;

        /// <summary>
        /// Checks whether the value of this JSON value is equal to that of another.
        /// </summary>
        /// <param name="other">JSON value to compare against.</param>
        /// <returns>Whether the values are equal.</returns>
        public bool Equals(JsonValue other)
        {
            if (other is null)
                return false;

            return other switch
            {
                JsonNullValue when this is JsonNullValue => true,
                JsonBooleanValue right when this is JsonBooleanValue left => left.Value == right.Value,
                JsonNumberValue right when this is JsonNumberValue left => left.Value == right.Value,
                JsonStringValue right when this is JsonStringValue left => left.Value == right.Value,
                JsonArrayValue right when this is JsonArrayValue left => left.Value.Equals(right.Value),
                JsonObjectValue right when this is JsonObjectValue left => left.Value.Equals(right.Value),
                _ => false,
            };
        }

        /// <summary>
        /// Checks whether the value of this JSON value is equal to value of another object.
        /// </summary>
        /// <param name="obj">Object to compare against.</param>
        /// <returns>Whether the values are equal.</returns>
        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case null:
                    return this.IsNull;

                case JsonValue other:
                    return this.Equals(other);

                case bool b:
                    return this.Equals(b);

                case string s:
                    return this.Equals(s);

                case double f64:
                    return this.Equals(f64);

                case long i64:
                    return this.Equals(i64);

                case ulong u64:
                    return this.Equals(u64);

                case int i32:
                    return this.Equals(i32);

                case uint u32:
                    return this.Equals(u32);
            }

            return false;
        }

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

        /// <summary>
        /// Converts a JSON value to 64-bit integer value, if possible.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static explicit operator long(JsonValue value)
            => value is JsonValue<double> jsonDouble
            ? (long)jsonDouble.Value
            : throw new InvalidCastException("This value is not an floating-point value.");

        /// <summary>
        /// Converts a JSON value to 64-bit unsigned integer value, if possible.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static explicit operator ulong(JsonValue value)
            => value is JsonValue<double> jsonDouble
            ? (ulong)jsonDouble.Value
            : throw new InvalidCastException("This value is not an floating-point value.");

        /// <summary>
        /// Converts a JSON value to 32-bit integer value, if possible.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static explicit operator int(JsonValue value)
            => value is JsonValue<double> jsonDouble
            ? (int)jsonDouble.Value
            : throw new InvalidCastException("This value is not an floating-point value.");

        /// <summary>
        /// Converts a JSON value to 32-bit unsigned integer value, if possible.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        public static explicit operator uint(JsonValue value)
            => value is JsonValue<double> jsonDouble
            ? (uint)jsonDouble.Value
            : throw new InvalidCastException("This value is not an floating-point value.");
    }

    /// <summary>
    /// Represents a typed JSON value.
    /// </summary>
    /// <typeparam name="T">Type of the JSON value.</typeparam>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class JsonValue<T> : JsonValue
    {
        /// <summary>
        /// Gets the value associated with this JSON value.
        /// </summary>
        public virtual T Value { get; }

        /// <summary>
        /// Gets the debugger display of this JSON value.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public override string DebuggerDisplay
            => $"JSON Value: {this.Value}";

        internal JsonValue(T value)
        {
            this.Value = value;
        }
    }
}
