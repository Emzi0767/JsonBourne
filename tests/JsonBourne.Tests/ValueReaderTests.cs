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

using System.Text;
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

                var result = reader.TryParse(b, out var actual, out var consumed);
                totalConsumed += consumed;
                switch (result)
                {
                    case ValueParseResult.Failure when expected != null:
                        Assert.Fail("Failed to parse when failure was not expected.");
                        return;

                    case ValueParseResult.Failure when expected == null:
                        return;

                    case ValueParseResult.Success:
                        Assert.AreEqual(expected, actual);
                        Assert.AreEqual(expectedLength, totalConsumed);
                        return;

                    case ValueParseResult.EOF:
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

                var result = reader.TryParse(b, out var actual, out var consumed);
                totalConsumed += consumed;
                switch (result)
                {
                    case ValueParseResult.Failure when successExpected:
                        Assert.Fail("Failed to parse when failure was not expected.");
                        return;

                    case ValueParseResult.Failure when !successExpected:
                        return;

                    case ValueParseResult.Success:
                        Assert.IsNull(actual);
                        Assert.AreEqual(4, totalConsumed);
                        return;

                    case ValueParseResult.EOF:
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
        public void TestNumberParser(double? expected, params string[] buffers)
        {
            var reader = new JsonNumberReader();

            double actual; int consumed; ValueParseResult result;
            foreach (var buffer in buffers)
            {
                var b = UTF8.GetBytes(buffer);

                result = reader.TryParse(b, out actual, out consumed);
                switch (result)
                {
                    case ValueParseResult.Failure when expected != null:
                        Assert.Fail("Failed to parse when failure was not expected.");
                        return;

                    case ValueParseResult.Failure when expected == null:
                        return;

                    case ValueParseResult.Success:
                        Assert.AreEqual(expected, actual);
                        return;

                    case ValueParseResult.EOF:
                        Assert.AreEqual(b.Length, consumed);
                        break;
                }
            }

            result = reader.TryParse(default, out actual, out consumed);
            if ((result == ValueParseResult.Failure && expected != null) || (result == ValueParseResult.Success && expected == null))
                Assert.Fail("Unexpected result");

            if (result == ValueParseResult.Success && expected != null)
            {
                Assert.AreEqual(expected, actual);
                Assert.AreEqual(0, consumed);
                return;
            }
            else if (result == ValueParseResult.Failure && expected == null)
            {
                Assert.AreEqual(0, consumed);
                return;
            }

            Assert.Inconclusive();
        }
    }
}
