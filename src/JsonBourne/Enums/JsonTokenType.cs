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

namespace JsonBourne.Enums
{
    /// <summary>
    /// Specifies the type of a JSON token encountered by the parser.
    /// </summary>
    [Flags]
    public enum JsonTokenType : ulong
    {
        /// <summary>
        /// Unknown token type.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Line feed character (\n).
        /// </summary>
        LineFeed = 1 << 1,

        /// <summary>
        /// Carriage return character (\r).
        /// </summary>
        CarriageReturn = 1 << 2,

        /// <summary>
        /// Horizontal tabulation character (\t).
        /// </summary>
        HorizontalTab = 1 << 3,

        /// <summary>
        /// Regular space character (\x20).
        /// </summary>
        Space = 1 << 4,

        /// <summary>
        /// Any whitespace character.
        /// </summary>
        Whitespace = LineFeed | CarriageReturn | HorizontalTab | Space,

        /// <summary>
        /// Denotes the start of an object ({, opening curly bracket).
        /// </summary>
        ObjectStart = 1 << 5,

        /// <summary>
        /// Denotes the end of an object (}, closing curly bracket).
        /// </summary>
        ObjectEnd = 1 << 6,

        /// <summary>
        /// Denotes the start of a list ([, opening square bracket).
        /// </summary>
        ListStart = 1 << 7,

        /// <summary>
        /// Denotes the end of a list (], closing square bracket).
        /// </summary>
        ListEnd = 1 << 8,

        /// <summary>
        /// Denotes a value consituting a string ("...", any valid character enclosed by quotation marks).
        /// </summary>
        String = 1 << 9,

        /// <summary>
        /// Denotes the optional negative sign of a number (-, minus).
        /// </summary>
        NumberSign = 1 << 10,

        /// <summary>
        /// Denotes a digit 0-9.
        /// </summary>
        NumberDigit = 1 << 11,

        /// <summary>
        /// Denotes the separator of a fractional part (., dot).
        /// </summary>
        NumberFractionSeparator = 1 << 12,

        /// <summary>
        /// Denotes the start of a number's exponent (e or E, letter E, capital or otherwise).
        /// </summary>
        NumberExponentSeparator = 1 << 13,

        /// <summary>
        /// Denotes the sign of a number's exponent (+ or -, plus or minus).
        /// </summary>
        NumberExponentSign = 1 << 14,

        /// <summary>
        /// Denotes a value constituting a number (valid IEEE754 64-bit floating-point number). Composite type.
        /// </summary>
        Number = NumberSign | NumberDigit | NumberFractionSeparator | NumberExponentSeparator | NumberExponentSign,

        /// <summary>
        /// Denotes a true boolean value (literal: true).
        /// </summary>
        BooleanTrue = 1 << 15,

        /// <summary>
        /// Denotes a false boolean value (literal: false).
        /// </summary>
        BooleanFalse = 1 << 16,

        /// <summary>
        /// Denotes any valid boolean value (true or false). Composite type.
        /// </summary>
        Boolean = BooleanTrue | BooleanFalse,

        /// <summary>
        /// Denotes the lack of value (literal: null).
        /// </summary>
        Null = 1 << 17,

        /// <summary>
        /// Denotes a token separating two members in an object or two values in a list (,, comma).
        /// </summary>
        MemberSeparator = 1 << 18,

        /// <summary>
        /// Denotes a token separating an object member's value from its name.
        /// </summary>
        MemberNameSeparator = 1 << 19,

        /// <summary>
        /// Denotes a value or a start thereof (a number, a string, a boolean, an object start, a list start, or a null value).
        /// </summary>
        Item = Number | String | Boolean | ObjectStart | ListStart | Null,
    }
}
