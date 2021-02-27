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
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text.Unicode;
using Emzi0767.Types;
using JsonBourne.DocumentModel;

namespace JsonBourne.DocumentReader
{
    // BEHOLD, EVEN MIGHTIER STATE MACHINE COROUTINE

    internal sealed class JsonStringReader : IJsonValueReader<string>
    {
        // Multibyte marker
        private const byte MultibyteMask = 0b1000_0000;

        // Multibyte 4 byte marker
        private const byte MultibyteCount4 = 0b1111_0000;
        private const byte MultibyteCount4Mask = 0b1111_1000;

        // Multibyte 3 byte marker
        private const byte MultibyteCount3 = 0b1110_0000;
        private const byte MultibyteCount3Mask = 0b1111_0000;

        // Multibyte 2 byte marker
        private const byte MultibyteCount2 = 0b1100_0000;
        private const byte MultibyteCount2Mask = 0b1110_0000;

        // there is a couple possible scenarios here
        // the most optimistic one: we parse the entire string in one buffer
        // then there's various scenarios for things that will not work quite as well
        // for example (multibuffer scenarios):
        // - border byte is not part of a multibyte sequence or escape sequence
        // - border byte is part of an escape sequence - 6 byte buffer needed to hold it (can be one-char like \n or
        //   5-char like \u0007)
        // - border byte is part of multibyte unicode sequence - 4 byte buffer needed to hold it (just reuse above)
        //
        // furthermore, we need to parse, convert, decode, and unescape as we go, so:
        // - for '\', flag escape
        // - for x & 0x80 != 0, flag multibyte ()
        // - for x < 0x80 && char.IsControl(x), flag invalid (not legal to appear unencoded in the middle of a string)
        // - for '"', finalize string
        //
        // basic state machine
        // 1. determine if [0] is '"'
        //   - if not, fail
        // 2. for each char
        //   - if any above special case
        //     copy and decode what we have so far
        //     decode it
        //   - else
        //     advance position
        //
        // if a multibyte appears but the buffer doesn't end in the middle of it, just ignore it and continue as
        // usual, otherwise, parse the first byte (11110___ -> 4b, 1110____ -> 3b, 110_____ -> 2b), store whatever
        // part of the sequence there is to the buffer, then copy and decode anything prior to it. afterwards, swap
        // buffer and continue deocding the sequence.
        //
        // in case of escape, first copy and decode everything prior, then decode escape, in a similar manner to the
        // one outlined for multibytes, i.e. copy to buffer if boundary is crossed, then decode the buffer or slice

        private Memory<byte> Buffer { get; }
        private MemoryBuffer<char> DecodedBuffer { get; set; }

        private int _buffPos;
        private ContentType _buffContent;

        public JsonStringReader()
        {
            this.Buffer = new byte[6]; // up to 4 multibyte, up to 6 escape
            this._buffPos = 0;
            this._buffContent = ContentType.None;
        }

        public ValueParseResult TryParse(ReadOnlyMemory<byte> buffer, out string result, out int consumedLength)
            => this.TryParse(buffer.Span, out result, out consumedLength);

        [SkipLocalsInit]
        public ValueParseResult TryParse(ReadOnlySpan<byte> readerSpan, out string result, out int consumedLength)
        { 
            result = null;
            consumedLength = 0;

            // is input empty
            if (readerSpan.Length <= 0)
            {
                // did any prior processing occur
                return this.DecodedBuffer != null
                    ? _cleanup(this, ValueParseResult.Failure)
                    : ValueParseResult.EOF;
            }

            // if we are not continuing, ensure it's a string that's being parsed
            var startPos = 0;
            if (this.DecodedBuffer == null)
            {
                if (readerSpan[consumedLength++] != JsonTokens.QuoteMark)
                    return _cleanup(this, ValueParseResult.Failure);

                this.DecodedBuffer = new MemoryBuffer<char>(segmentSize: 2048, initialSegmentCount: 1);
                startPos = consumedLength;
            }

            // if continuing, check if anything is pending in the buffer
            var blen = (int)this._buffContent;
            Span<char> decoded = stackalloc char[512];
            switch (this._buffContent)
            {
                // short escape: \" \\ \/ \b \f \n \r \t
                // long escape: \uXXXX
                case ContentType.EscapeSequence:
                case ContentType.ExtendedEscapeSequence:
                    if (this._buffContent != ContentType.ExtendedEscapeSequence && readerSpan[0] == JsonTokens.UnicodePrefix)
                    {
                        this._buffContent = ContentType.ExtendedEscapeSequence;
                        blen = (int)this._buffContent;
                    }

                    if (readerSpan.Length + this._buffPos < blen + 1)
                    {
                        readerSpan.CopyTo(this.Buffer[this._buffPos..].Span);
                        this._buffPos += readerSpan.Length;
                        consumedLength += readerSpan.Length;
                        return ValueParseResult.EOF;
                    }

                    readerSpan.Slice(0, blen + 1 - this._buffPos).CopyTo(this.Buffer[this._buffPos..].Span);

                    if (!JsonTokens.TryUnescape(this.Buffer.Span, out decoded[0], out var consumed))
                        return _cleanup(this, ValueParseResult.Failure);

                    consumedLength += consumed - this._buffPos;
                    startPos = consumedLength;
                    this.DecodedBuffer.Write(decoded.Slice(0, 1));
                    this._buffContent = ContentType.None;
                    this._buffPos = 0;

                    break;

                // utf-8 multibyte
                case ContentType.Multibyte2Sequence:
                case ContentType.Multibyte3Sequence:
                case ContentType.Multibyte4Sequence:
                    if (readerSpan.Length + this._buffPos < blen)
                    {
                        readerSpan.CopyTo(this.Buffer[this._buffPos..].Span);
                        this._buffPos += readerSpan.Length;
                        consumedLength += readerSpan.Length;
                        return ValueParseResult.EOF;
                    }

                    readerSpan.Slice(0, blen - this._buffPos).CopyTo(this.Buffer[this._buffPos..].Span);

                    if (Utf8.ToUtf16(this.Buffer.Slice(0, blen).Span, decoded, out var c, out var e) != OperationStatus.Done || c != blen)
                        return _cleanup(this, ValueParseResult.Failure);

                    consumedLength += c - this._buffPos;
                    startPos = consumedLength;
                    this.DecodedBuffer.Write(decoded.Slice(0, e));
                    this._buffContent = ContentType.None;
                    this._buffPos = 0;

                    break;
            }

            // read and decode the string
            var completedParsing = false;
            while (consumedLength < readerSpan.Length)
            {
                var b = readerSpan[consumedLength++];

                // is end quote
                if (b == JsonTokens.QuoteMark)
                {
                    completedParsing = true;

                    blen = consumedLength - startPos - 1;
                    if (blen > 0)
                    {
                        if (!_decode(readerSpan.Slice(startPos, blen), decoded, this.DecodedBuffer))
                            return _cleanup(this, ValueParseResult.Failure);
                    }

                    break;
                }

                // is escape sequence
                if (b == JsonTokens.ReverseSolidus)
                {
                    // more than one character since last copy - this is because if we have a buffer like
                    // "|h|e|\|n|l|o|"
                    // 0 1 2 3 4 5 6 7
                    //       ^
                    //     ^
                    // the upper carret points to solidus, bottom points to last copy, the slice contains only the
                    // solidus, and thus copying would yield incorrect results
                    blen = consumedLength - startPos - 1;
                    if (blen > 0)
                    {
                        if (!_decode(readerSpan.Slice(startPos, blen), decoded, this.DecodedBuffer))
                            return _cleanup(this, ValueParseResult.Failure);
                    }

                    // short escape: \" \\ \/ \b \f \n \r \t
                    // OR unidentified long escape: \uXXXX
                    if (readerSpan.Length - consumedLength < 1)
                    {
                        // store state
                        this.Buffer.Span[0] = b;
                        this._buffContent = ContentType.EscapeSequence;
                        this._buffPos = 1;
                        return ValueParseResult.EOF;
                    }

                    // long escape: \uXXXX
                    if (readerSpan[consumedLength++] == JsonTokens.UnicodePrefix && readerSpan.Length - consumedLength < 4)
                    {
                        readerSpan[(consumedLength - 2)..].CopyTo(this.Buffer.Span);
                        this._buffContent = ContentType.ExtendedEscapeSequence;
                        this._buffPos = readerSpan.Length - consumedLength + 2;
                        consumedLength += this._buffPos - 2;
                        return ValueParseResult.EOF;
                    }

                    // unescape
                    if (!JsonTokens.TryUnescape(readerSpan[(consumedLength - 2)..], out decoded[0], out var consumed))
                        return _cleanup(this, ValueParseResult.Failure);

                    // write to decoded
                    consumedLength += consumed - 2;
                    startPos = consumedLength;
                    this.DecodedBuffer.Write(decoded.Slice(0, 1));
                    continue;
                }

                // is singlebyte
                if (b < MultibyteMask)
                {
                    // is multibyte? if so, not legal to appear
                    if (char.IsControl((char)b))
                        return _cleanup(this, ValueParseResult.Failure);

                    // legal, carry on
                    continue;
                }

                // is multibyte
                if ((b & MultibyteMask) != 0)
                {
                    // determine how many bytes
                    int seqLen;
                    if ((b & MultibyteCount2Mask) == MultibyteCount2)
                        seqLen = 2;
                    else if ((b & MultibyteCount3Mask) == MultibyteCount3)
                        seqLen = 3;
                    else if ((b & MultibyteCount4Mask) == MultibyteCount4)
                        seqLen = 4;
                    else
                        return _cleanup(this, ValueParseResult.Failure);

                    // if not enough input, signal EOF
                    if (readerSpan.Length - consumedLength < seqLen - 1)
                    {
                        // decode and store up to now
                        blen = consumedLength - startPos - 1;
                        if (blen > 0)
                        {
                            if (!_decode(readerSpan.Slice(startPos, blen), decoded, this.DecodedBuffer))
                                return _cleanup(this, ValueParseResult.Failure);
                        }

                        // copy sequence
                        readerSpan[(consumedLength - 1)..].CopyTo(this.Buffer.Span);
                        this._buffPos = readerSpan.Length - consumedLength + 1;
                        this._buffContent = seqLen switch
                        {
                            2 => ContentType.Multibyte2Sequence,
                            3 => ContentType.Multibyte3Sequence,
                            4 => ContentType.Multibyte4Sequence
                        };

                        consumedLength += this._buffPos - 1;
                        return ValueParseResult.EOF;
                    }

                    // jump ahead
                    consumedLength += seqLen - 1;
                }
            }

            // did we reach the end of input before running out of it
            var input = readerSpan;
            if (completedParsing)
            {
                result = string.Create((int)this.DecodedBuffer.Count, this.DecodedBuffer, static (@out, @in) => @in.Read(@out, 0, out var written));
                return _cleanup(this, ValueParseResult.Success);
            }
            // no, store state and yield back
            else
            {
                blen = consumedLength - startPos;
                if (blen > 0)
                {
                    if (!_decode(readerSpan[startPos..], decoded, this.DecodedBuffer))
                        return _cleanup(this, ValueParseResult.Failure);
                }

                return ValueParseResult.EOF;
            }

            static bool _decode(ReadOnlySpan<byte> input, Span<char> output, MemoryBuffer<char> destination)
            {
                // in UTF-16, a rune can consist of one or two chars
                // you have surrogate pairs that way (HiLo)
                // 
                // in UTF-8, the most bytes per rune is 4, and that can result in 1 or 2 UTF-16 characters
                // in terms of single-byte characters, these all form one-char rune each
                // two-char sequences can only result from 2-4 UTF-8 bytes, meaning we have 1 output per 2-4 inputs
                // so highest output:input ratio is 1:1

                // short buffer, do on the stack
                if (input.Length < output.Length)
                {
                    if (Utf8.ToUtf16(input, output, out var c, out var e) != OperationStatus.Done || c != input.Length)
                        return false;

                    destination.Write(output.Slice(0, e));
                }
                // long buffer, go through heap
                else
                {
                    using var mem = MemoryPool<char>.Shared.Rent(JsonUtilities.UTF8.GetMaxCharCount(input.Length));
                    if (Utf8.ToUtf16(input, mem.Memory.Span, out var c, out var e) != OperationStatus.Done || c != input.Length)
                        return false;

                    destination.Write(mem.Memory.Slice(0, e).Span);
                }

                return true;
            }

            static ValueParseResult _cleanup(IDisposable rdr, ValueParseResult result)
            {
                rdr.Dispose();
                return result;
            }
        }

        public void Dispose()
        {
            if (this.DecodedBuffer != null)
            {
                this.DecodedBuffer.Dispose();
                this.DecodedBuffer = null;
            }

            this._buffPos = 0;
            this._buffContent = ContentType.None;
        }

        private enum ContentType
        {
            None = 0,
            Multibyte2Sequence = 2,
            Multibyte3Sequence = 3,
            Multibyte4Sequence = 4,
            EscapeSequence = 1,
            ExtendedEscapeSequence = 5 // \uXXXX
        }
    }
}
