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
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using JsonBourne.DocumentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JsonBourne.Tests
{
    [TestClass]
    public sealed class ParserTests
    {
        [DataTestMethod]
        [DataRow(null, "null")]
        [DataRow(true, "true")]
        [DataRow(false, "false")]
        [DataRow(3.14, "3.14")]
        [DataRow("😒", "\"😒\"")]
        [DataRow(new object[] { null, true, false, 3.14, "😒" }, "[null,true,false,3.14,\"😒\"]")]
        [DataRow(new object[] { "null", null, "true", true, "false", false, "number", 3.14, "string", "😒" }, "{\"null\":null,\"true\":true,\"false\":false,\"number\":3.14,\"string\":\"😒\"}")]
        [DataRow(new object[] { "null", null, "true", true, "false", false, "number", 3.14, "string", "😒", "arr", new object[] { new object[] { "a", 42.0 }, new object[] { "a", 69.0 } }, "obj", new object[] { } },
            "{\"null\":null,\"true\":true,\"false\":false,\"number\":3.14,\"string\":\"😒\",\"arr\":[{\"a\":42},{\"a\":69}],\"obj\":{}}")]
        public void TestBufferParse(object expected, string buffer)
        {
            var buff = JsonUtilities.UTF8.GetBytes(buffer);
            var parser = new JsonParser();
            var jsonValue = parser.Parse(buff.AsSpan());
            this.Validate(expected, jsonValue);
        }

        [DataTestMethod]
        [DataRow("dumpraw.json")]
        [DataRow("dump.json")]
        public async Task TestStreamParseAsync(string file)
        {
            JsonDocument reference;
            using (var fs = File.OpenRead(file))
            {
                reference = await JsonDocument.ParseAsync(fs);
            }

            var jsonParser = new JsonParser();
            JsonValue jsonValue;
            using (var fs = File.OpenRead(file))
                jsonValue = await jsonParser.ParseAsync(fs);

            this.ValidateRecursive(reference.RootElement, jsonValue);
        }

        private void ValidateRecursive(JsonElement reference, JsonValue actual, string path = "")
        {
            if (actual is JsonObjectValue actualObject)
            {
                if (reference.ValueKind != JsonValueKind.Object)
                    Assert.Fail($"Expected not object, got object '{path}'.");

                foreach (var prop in reference.EnumerateObject())
                {
                    var (k, v) = (prop.Name, prop.Value);

                    if (!actualObject.ContainsKey(k))
                        Assert.Fail($"Missing key '{path}.{k}'.");

                    this.ValidateRecursive(v, actualObject[k], path + "." + k);
                }
            }
            else if (actual is JsonArrayValue actualArray)
            {
                if (reference.ValueKind != JsonValueKind.Array)
                    Assert.Fail($"Expected not array, got array '{path}'.");

                if (actualArray.Count != reference.GetArrayLength())
                    Assert.Fail($"Array length mismatch '{path}'.");

                for (var i = 0; i < reference.GetArrayLength(); i++)
                    this.ValidateRecursive(reference[i], actualArray[i], path + ".[" + i + "]");
            }
            else if (actual is JsonNullValue)
            {
                if (reference.ValueKind != JsonValueKind.Null)
                    Assert.Fail($"Expected not null, got null '{path}'.");

                return;
            }
            else if (actual is JsonBooleanValue actualBoolean)
            {
                if (reference.ValueKind != JsonValueKind.True && reference.ValueKind != JsonValueKind.False)
                    Assert.Fail($"Expected not bool, got bool '{path}'.");

                if (!actualBoolean.Equals(reference.GetBoolean()))
                    Assert.Fail($"Value mismatch '{path}'.");
            }
            else if (actual is JsonNumberValue actualNumber)
            {
                if (reference.ValueKind != JsonValueKind.Number)
                    Assert.Fail($"Expected not number, got number '{path}'.");

                if (!actualNumber.Equals(reference.GetDouble()))
                    Assert.Fail($"Value mismatch '{path}'.");
            }
            else if (actual is JsonStringValue actualString)
            {
                if (reference.ValueKind != JsonValueKind.String)
                    Assert.Fail($"Expected not string, got string '{path}'.");

                if (!actualString.Equals(reference.GetString()))
                    Assert.Fail($"Value mismatch '{path}'.");
            }
            else
            {
                Assert.Fail($"??? at '{path}'");
            }
        }

        private void Validate(object expected, JsonValue actual)
        {
            switch (actual)
            {
                case JsonArrayValue actualArray:
                    if (expected is object[] expectedArray)
                    {
                        Assert.IsTrue(this.ValidateArray(expectedArray, actualArray));
                        return;
                    }

                    Assert.Fail("Value is array, expected something else.");
                    break;

                case JsonObjectValue actualObject:
                    if (expected is object[] expectedObject)
                    {
                        Assert.IsTrue(this.ValidateObject(expectedObject, actualObject));
                        return;
                    }

                    Assert.Fail("Value is object, expected something else.");
                    break;
            }

            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.Equals(expected));
        }

        private bool ValidateArray(object[] expected, JsonArrayValue actual)
        {
            if (expected.Length != actual.Count)
                return false;

            for (var i = 0; i < expected.Length; i++)
            {
                var e = expected[i];
                var a = actual[i];

                switch (a)
                {
                    case JsonArrayValue actualArray:
                        if (e is not object[] expectedArray)
                            return false;

                        if (!this.ValidateArray(expectedArray, actualArray))
                            return false;

                        continue;

                    case JsonObjectValue actualObject:
                        if (e is not object[] expectedObject)
                            return false;

                        if (!this.ValidateObject(expectedObject, actualObject))
                            return false;

                        continue;
                }

                if (!a.Equals(e))
                    return false;
            }

            return true;
        }

        private bool ValidateObject(object[] expected, JsonObjectValue actual)
        {
            if (expected.Length / 2 != actual.Count)
                return false;

            for (var i = 0; i < expected.Length / 2; i++)
            {
                var k = expected[i * 2] as string;
                var v = expected[i * 2 + 1];

                if (!actual.ContainsKey(k))
                    return false;

                var a = actual[k];
                switch (a)
                {
                    case JsonArrayValue actualArray:
                        if (v is not object[] expectedArray)
                            return false;

                        if (!this.ValidateArray(expectedArray, actualArray))
                            return false;

                        continue;

                    case JsonObjectValue actualObject:
                        if (v is not object[] expectedObject)
                            return false;

                        if (!this.ValidateObject(expectedObject, actualObject))
                            return false;

                        continue;
                }

                if (!a.Equals(v))
                    return false;
            }

            return true;
        }
    }
}
