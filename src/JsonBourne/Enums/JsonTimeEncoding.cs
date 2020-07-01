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

namespace JsonBourne
{
    /// <summary>
    /// Defines how <see cref="JsonSerializer"/> handles <see cref="DateTime"/>, <see cref="DateTimeOffset"/>, and <see cref="TimeSpan"/> objects.
    /// </summary>
    [Flags]
    public enum JsonTimeEncoding : int
    {
        // DateTime / DateTimeOffset

        /// <summary>
        /// <para>Specifies that <see cref="DateTime"/> and <see cref="DateTimeOffset"/> objects will be encoded as UNIX timestamps, using seconds as unit of time.</para>
        /// <para>This format does not preserve timezone information, and encodes all dates as UTC. Dates that are not already UTC are converted beforehand.</para>
        /// <para>Result is encoded as a 64-bit signed integer representing number of seconds since midnight UTC, 1970-01-01.</para>
        /// <para><see cref="DateTime"/> objects with kind set to <see cref="DateTimeKind.Unspecified"/> are assumed to be UTC.</para>
        /// </summary>
        TimestampUnixSeconds =           1 << 0,

        /// <summary>
        /// <para>Specifies that <see cref="DateTime"/> and <see cref="DateTimeOffset"/> objects will be encoded as UNIX timestamps, using milliseconds as unit of time.</para>
        /// <para>This format does not preserve timezone information, and encodes all dates as UTC. Dates that are not already UTC are converted beforehand.</para>
        /// <para>Result is encoded as a 64-bit signed integer representing number of milliseconds since midnight UTC, 1970-01-01.</para>
        /// <para><see cref="DateTime"/> objects with kind set to <see cref="DateTimeKind.Unspecified"/> are assumed to be UTC.</para>
        /// </summary>
        TimestampUnixMilliseconds =      1 << 1,

        /// <summary>
        /// <para>Specifies that <see cref="DateTime"/> and <see cref="DateTimeOffset"/> objects will be encoded as ISO-8601 date strings, preserving timezone information.</para>
        /// <para>This format preserves timezone information.</para>
        /// <para>Result is encoded as a string, using format string of yyyy-MM-ddTHH:mm:ss.ffffffzzz.</para>
        /// <para><see cref="DateTime"/> objects with kind set to <see cref="DateTimeKind.Unspecified"/> are assumed to be UTC.</para>
        /// </summary>
        TimestampIsoStringTz =           1 << 2,

        /// <summary>
        /// <para>Specifies that <see cref="DateTime"/> and <see cref="DateTimeOffset"/> objects will be encoded as ISO-8601 date strings, without preserving timezone information.</para>
        /// <para>This format does not preserve timezone information, and converts all dates to UTC before converstion.</para>
        /// /// <para>Result is encoded as a string, using format string of yyyy-MM-ddTHH:mm:ss.ffffff.</para>
        /// <para><see cref="DateTime"/> objects with kind set to <see cref="DateTimeKind.Unspecified"/> are assumed to be UTC.</para>
        /// </summary>
        TimestampIsoStringNoTz =         1 << 3,

        /// <summary>
        /// <para>Specifies that <see cref="DateTime"/> and <see cref="DateTimeOffset"/> objects will be encoded as Windows NT timestamps.</para>
        /// <para>This format does not preserve timezone information, and encodes all dates as UTC. Dates that are not already UTC are converted beforehand.</para>
        /// <para>Result is encoded as a 64-bit unsigned integer representing number of 100 nanosecond intervals elapsed since midnight UTC, 1601-01-01.</para>
        /// <para><see cref="DateTime"/> objects with kind set to <see cref="DateTimeKind.Unspecified"/> are assumed to be UTC.</para>
        /// </summary>
        TimestampWindowsNtTime =         1 << 4,

        // TimeSpan

        /// <summary>
        /// <para>Specifies that <see cref="TimeSpan"/> objects will be encoded as integral number of milliseconds they encompass, rounded to nearest integral value.</para>
        /// <para>This is a medium resolution format, usage of which might result in partial precision loss.</para>
        /// <para>Result is encoded as a 64-bit signed integer.</para>
        /// </summary>
        TimespanIntegralMilliseconds =   1 << 16,

        /// <summary>
        /// <para>Specifies that <see cref="TimeSpan"/> objects will be encoded as number of milliseconds they encompass, including fractional part.</para>
        /// <para>This is a medium resolution format, usage of which might result in partial precision loss.</para>
        /// <para>Result is encoded as a 64-bit IEEE floating-point value.</para>
        /// </summary>
        TimespanFractionalMilliseconds = 1 << 17,

        /// <summary>
        /// <para>Specifies that <see cref="TimeSpan"/> objects will be encoded as number of seconds they encompass, rounded to nearest integral value.</para>
        /// <para>This is a medium resolution format, usage of which might result in partial precision loss.</para>
        /// <para>Result is encoded as a 64-bit signed integer.</para>
        /// </summary>
        TimespanIntegralSeconds =        1 << 18,

        /// <summary>
        /// <para>Specifies that <see cref="TimeSpan"/> objects will be encoded as number of seconds they encompass, including fractional part.</para>
        /// <para>This is a medium resolution format, usage of which might result in partial precision loss.</para>
        /// <para>Result is encoded as a 64-bit IEEE floating-point value.</para>
        /// </summary>
        TimespanFractionalSeconds =      1 << 19,

        /// <summary>
        /// <para>Specifies that <see cref="TimeSpan"/> objects will be encoded as number of 100 nanosecond intervals they encompass.</para>
        /// <para>This is a high resolution format.</para>
        /// <para>Result is encoded as a 64-bit signed integer.</para>
        /// </summary>
        TimespanTicks =                  1 << 20,

        /// <summary>
        /// <para>Specifies that <see cref="TimeSpan"/> objects will be encoded as short timespan strings, using g format ([-][d':']h':'mm':'ss[.FFFFFFF]).</para>
        /// <para>Result is encoded as a string.</para>
        /// </summary>
        TimespanShortString =            1 << 21,

        /// <summary>
        /// <para>Specifies that <see cref="TimeSpan"/> objects will be encoded as long timespan strings, using G format ([-]d':'hh':'mm':'ss.fffffff).</para>
        /// <para>Result is encoded as a string.</para>
        /// </summary>
        TimespanLongString =             1 << 22,

        // Composite styles

        /// <summary>
        /// <para>Composite style, which is a combination of <see cref="TimestampUnixMilliseconds"/> and <see cref="TimespanTicks"/>.</para>
        /// <para>These settings combine portability with good precision.</para>
        /// </summary>
        Default =                        TimestampUnixMilliseconds | TimespanTicks,

        /// <summary>
        /// <para>Composite style, which is a combination of <see cref="TimestampIsoStringTz"/> and <see cref="TimespanLongString"/>.</para>
        /// <para>These settings provide portable string results, with timezone information where applicable.</para>
        /// </summary>
        Strings =                        TimestampIsoStringTz | TimespanLongString,

        /// <summary>
        /// <para>Composite style, which is a combination of <see cref="TimestampUnixSeconds"/> and <see cref="TimespanIntegralSeconds"/>.</para>
        /// <para>These settings are meant to provide rough timestamp encoding, and are meant for situations where values are large, and approximations are acceptable.</para>
        /// </summary>
        LowResolution =                  TimestampUnixSeconds | TimespanIntegralSeconds,

        /// <summary>
        /// <para>Composite style, which is a combination of <see cref="TimestampWindowsNtTime"/> and <see cref="TimespanTicks"/>.</para>
        /// <para>These settings are meant to provide good timestamp accuracy, and are meant for situations where accurate values are required.</para>
        /// </summary>
        HighResolution =                 TimestampWindowsNtTime | TimespanTicks
    }
}
