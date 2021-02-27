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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JsonBourne.DocumentModel;
using JsonBourne.DocumentReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JsonBourne.Tests
{
    [TestClass]
    public sealed class ValueReaderTests
    {
        private static UTF8Encoding UTF8 { get; } = new(false);

        [DataTestMethod]
        [DataRow(true, 4, "true")]
        [DataRow(false, 5, "false")]
        [DataRow(true, 4, "tr", "ue")]
        [DataRow(false, 5, "fa", "lse")]
        [DataRow(true, 4, "t", "r", "u", "e")]
        [DataRow(false, 5, "f", "a", "l", "s", "e")]
        [DataRow(null, 0, "fals ")]
        [DataRow(null, 0, "tru ")]
        [DataRow(null, 0, "null")]
        public void TestBooleanParser(bool? expected, int expectedLength, params string[] buffers)
        {
            var reader = new JsonBooleanReader();

            var totalConsumed = 0;
            foreach (var buffer in buffers)
            {
                var b = UTF8.GetBytes(buffer);

                var result = reader.TryParse(b.AsMemory(), out var actual, out var consumed, out _, out _);
                totalConsumed += consumed;
                switch (result.Type)
                {
                    case ValueParseResultType.Failure when expected != null:
                        Assert.Fail("Failed to parse when failure was not expected.");
                        return;

                    case ValueParseResultType.Failure when expected == null:
                        return;

                    case ValueParseResultType.Success:
                        Assert.AreEqual(expected, actual);
                        Assert.AreEqual(expectedLength, totalConsumed);
                        return;

                    case ValueParseResultType.EOF:
                        Assert.AreEqual(b.Length, consumed);
                        break;
                }
            }

            Assert.Inconclusive();
        }

        [DataTestMethod]
        [DataRow(true, "null")]
        [DataRow(true, "nu", "ll")]
        [DataRow(true, "n", "u", "l", "l")]
        [DataRow(false, "nul ")]
        [DataRow(false, "true")]
        [DataRow(false, "false")]
        public void TestNullParser(bool successExpected, params string[] buffers)
        {
            var reader = new JsonNullReader();

            var totalConsumed = 0;
            foreach (var buffer in buffers)
            {
                var b = UTF8.GetBytes(buffer);

                var result = reader.TryParse(b.AsMemory(), out var actual, out var consumed, out _, out _);
                totalConsumed += consumed;
                switch (result.Type)
                {
                    case ValueParseResultType.Failure when successExpected:
                        Assert.Fail("Failed to parse when failure was not expected.");
                        return;

                    case ValueParseResultType.Failure when !successExpected:
                        return;

                    case ValueParseResultType.Success:
                        Assert.IsNull(actual);
                        Assert.AreEqual(4, totalConsumed);
                        return;

                    case ValueParseResultType.EOF:
                        Assert.AreEqual(b.Length, consumed);
                        break;
                }
            }

            Assert.Inconclusive();
        }

        [DataTestMethod]
        [DataRow(1.0, "1\n")]
        [DataRow(1.0, "1.0\n")]
        [DataRow(1e3, "1e3\n")]
        [DataRow(1.5e3, "1.5e3\n")]
        [DataRow(1.5e3, "1.50e3\n")]
        [DataRow(-3e-1, "-3e-1\n")]
        [DataRow(-3e+1, "-3e+1\n")]
        [DataRow(-0.6, "-0.6\n")]
        [DataRow(11.6, "11.6\n")]
        [DataRow(132.0, "132\n")]
        [DataRow(132.0e7, "132E+07\n")]
        [DataRow(1.0, "1")]
        [DataRow(1.0, "1.0")]
        [DataRow(1e3, "1e3")]
        [DataRow(1.5e3, "1.5e3")]
        [DataRow(1.5e3, "1.50e3")]
        [DataRow(-3e-1, "-3e-1")]
        [DataRow(-3e+1, "-3e+1")]
        [DataRow(-0.6, "-0.6")]
        [DataRow(11.6, "11.6")]
        [DataRow(132.0, "132")]
        [DataRow(132.0e7, "132E+07")]
        [DataRow(null, "-e1\n")]
        [DataRow(null, "-.5\n")]
        [DataRow(null, "1.e2\n")]
        [DataRow(132.0e7, "132", "E+07\n")]
        public void TestNumberParser(double? expected, params string[] buffers)
        {
            var reader = new JsonNumberReader();

            double actual; int consumed; ValueParseResult result;
            foreach (var buffer in buffers)
            {
                var b = UTF8.GetBytes(buffer);

                result = reader.TryParse(b.AsMemory(), out actual, out consumed, out _, out _);
                switch (result.Type)
                {
                    case ValueParseResultType.Failure when expected != null:
                        Assert.Fail("Failed to parse when failure was not expected.");
                        return;

                    case ValueParseResultType.Failure when expected == null:
                        return;

                    case ValueParseResultType.Success:
                        Assert.AreEqual(expected, actual);
                        return;

                    case ValueParseResultType.EOF:
                        Assert.AreEqual(b.Length, consumed);
                        break;
                }
            }

            result = reader.TryParse(default(ReadOnlyMemory<byte>), out actual, out consumed, out _, out _);
            if ((result.Type == ValueParseResultType.Failure && expected != null) || (result == ValueParseResult.Success && expected == null))
                Assert.Fail("Unexpected result");

            if (result == ValueParseResult.Success && expected != null)
            {
                Assert.AreEqual(expected, actual);
                Assert.AreEqual(0, consumed);
                return;
            }
            else if (result.Type == ValueParseResultType.Failure && expected == null)
            {
                Assert.AreEqual(0, consumed);
                return;
            }

            Assert.Inconclusive();
        }

        [DataTestMethod]
        [DataRow("abc", @"""abc""")]
        [DataRow("ab\nc", @"""ab\nc""")]
        [DataRow("abc", @"""ab\u0063""")]
        [DataRow("abc", @"""ab", @"\u0063""")]
        [DataRow("abc😒", @"""ab", @"\u0063😒""")]
        [DataRow("abc", @"""ab\", @"u0063""")]
        [DataRow("ab\n", @"""ab\", @"n""")]
        [DataRow("ab\n", @"""ab\", @"n", @"""")]
        [DataRow("abc", @"""ab\", @"u", @"0063""")]
        [DataRow("abcd", @"""ab\", @"u", @"0063d""")]
        [DataRow("abcd", @"""ab\", @"u", @"0063", @"d""")]
        [DataRow("abcd", @"""ab\u0", @"063", @"d""")]
        [DataRow(null, @"ab", @"\u0063""")]
        public void TestStringParser(string expected, params string[] buffers)
        {
            var reader = new JsonStringReader();

            var totalConsumed = 0;
            foreach (var buffer in buffers)
            {
                var b = UTF8.GetBytes(buffer);

                var result = reader.TryParse(b.AsMemory(), out var actual, out var consumed, out _, out _);
                totalConsumed += consumed;
                switch (result.Type)
                {
                    case ValueParseResultType.Failure when expected != null:
                        Assert.Fail("Failed to parse when failure was not expected.");
                        return;

                    case ValueParseResultType.Failure when expected == null:
                        return;

                    case ValueParseResultType.Success:
                        Assert.AreEqual(expected, actual);
                        return;

                    case ValueParseResultType.EOF:
                        Assert.AreEqual(b.Length, consumed);
                        break;
                }
            }

            Assert.Inconclusive();
        }

        [DataTestMethod]
        [DataRow("abc", @"""ab\u0063""", 3)]
        [DataRow("abc😒", @"""ab\u0063😒""", 3)]
        [DataRow("abc😒", @"""ab\u0063😒""", 3, 9)]
        [DataRow("abc😒", @"""ab\u0063😒""", 3, 10)]
        [DataRow("abc😒", @"""ab\u0063😒""", 3, 9, 10)]
        [DataRow("abc😒", @"""ab\u0063😒""", 3, 11)]
        [DataRow("abc", @"""ab\u0063""", 4)]
        [DataRow("ab\n", @"""ab\n""", 4)]
        [DataRow("ab\n", @"""ab\n""", 4, 5)]
        [DataRow("abc", @"""ab\u0063""", 4, 5)]
        [DataRow("abcd", @"""ab\u0063d""", 4, 5)]
        [DataRow("abcd", @"""ab\u0063d""", 4, 5, 9)]
        [DataRow("abcd", @"""ab\u0063d""", 6, 9)]
        public void TestUtf8StringParser(string expected, string input, params int[] slicePoints)
        {
            var allBuffer = UTF8.GetBytes(input);
            var allBuffers = allBuffer.AsSpan();

            var reader = new JsonStringReader();

            var totalConsumed = 0;
            var lpos = 0;
            foreach (var slicePoint in slicePoints.Concat(new[] { allBuffer.Length }))
            {
                var b = allBuffers[lpos..slicePoint];
                lpos = slicePoint;

                var result = reader.TryParse(b, out var actual, out var consumed, out _, out _);
                totalConsumed += consumed;
                switch (result.Type)
                {
                    case ValueParseResultType.Failure when expected != null:
                        Assert.Fail("Failed to parse when failure was not expected.");
                        return;

                    case ValueParseResultType.Failure when expected == null:
                        return;

                    case ValueParseResultType.Success:
                        Assert.AreEqual(expected, actual);
                        return;

                    case ValueParseResultType.EOF:
                        Assert.AreEqual(b.Length, consumed);
                        break;
                }
            }

            Assert.Inconclusive();
        }

        [DataTestMethod]
        [DataRow(new object[] { null, false, 1.5, true, -1.0, "😒" }, @"[null, false, 1.5, true, -1, ""😒""]")]
        [DataRow(new object[] { null, false, 1.5, true, -1.0, "😒" }, @"[nu", @"ll, false, 1.5, true, -1, ""😒""]")]
        [DataRow(new object[] { null, false, 1.5, true, -1.0, "😒" }, @"[nu", @"ll, fa", @"lse, 1.5, tr", @"ue, -1, """, @"😒""]")]
        [DataRow(new object[] { null, false, 1.5, true, -1.0, "😒" }, @"[null, false, 1.5", @", true, -1, """, @"😒""]")]
        [DataRow(new object[] { null, false, 1.5, true, -1.0, "😒" }, @"[null, false, 1.5, true, -1, ", @"""", @"😒""]")]
        public void TestSimpleArrayParser(object[] expected, params string[] buffers)
        {
            var reader = new JsonArrayReader(new ValueReaderCollection());

            var totalConsumed = 0;
            foreach (var buffer in buffers)
            {
                var b = UTF8.GetBytes(buffer);

                var result = reader.TryParse(b.AsMemory(), out var actual, out var consumed, out _, out _);
                totalConsumed += consumed;
                switch (result.Type)
                {
                    case ValueParseResultType.Failure when expected != null:
                        Assert.Fail("Failed to parse when failure was not expected.");
                        return;

                    case ValueParseResultType.Failure when expected == null:
                        return;

                    case ValueParseResultType.Success:
                        Assert.AreEqual(expected.Length, actual.Length);
                        Assert.IsTrue(actual.Select((x, i) => new { v = x, i }).All(x => x.v.Equals(expected[x.i])));
                        return;

                    case ValueParseResultType.EOF:
                        Assert.AreEqual(b.Length, consumed);
                        break;
                }
            }

            Assert.Inconclusive();
        }

        [DataTestMethod]
        [DataRow(new object[] { null, false, 1.5, true, -1.0, "😒", new object[] { 2.0, "2.0", 4.0 }, 1.0, new object[] { true, true, false } },
            "[\n\tnull,\n\tfalse,\n\t1.5,\n\ttrue,\n\t-1,\n\t\"😒\",\n\t[\n\t\t2.0,\n\t\t\"2.0\",\n\t\t4.0\n\t],\n\t1.0,\n\t[\n\t\ttrue,\n\t\ttrue,\n\t\tfalse\n\t]\n]")]
        public void TestRecursiveArrayParser(object[] expected, params string[] buffers)
        {
            var reader = new JsonArrayReader(new ValueReaderCollection());

            var totalConsumed = 0;
            foreach (var buffer in buffers)
            {
                var b = UTF8.GetBytes(buffer);

                var result = reader.TryParse(b.AsMemory(), out var actual, out var consumed, out _, out _);
                totalConsumed += consumed;
                switch (result.Type)
                {
                    case ValueParseResultType.Failure when expected != null:
                        Assert.Fail("Failed to parse when failure was not expected.");
                        return;

                    case ValueParseResultType.Failure when expected == null:
                        return;

                    case ValueParseResultType.Success:
                        Assert.AreEqual(expected.Length, actual.Length);
                        Assert.IsTrue(_validate(expected, actual));
                        return;

                    case ValueParseResultType.EOF:
                        Assert.AreEqual(b.Length, consumed);
                        break;
                }
            }

            Assert.Inconclusive();

            static bool _validate(object[] expected, IReadOnlyList<JsonValue> actual)
            {
                if (expected.Length != actual.Count)
                    return false;

                for (var i = 0; i < expected.Length; i++)
                {
                    if (expected[i] is object[] innerExpected)
                    {
                        if (actual[i] is not JsonArrayValue innerActual)
                            return false;

                        if (!_validate(innerExpected, innerActual.Value))
                            return false;

                        continue;
                    }

                    if (!actual[i].Equals(expected[i]))
                        return false;
                }

                return true;
            }
        }
    }
}
