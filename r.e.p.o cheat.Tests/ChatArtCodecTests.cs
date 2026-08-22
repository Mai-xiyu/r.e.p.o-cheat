using r.e.p.o_cheat;
using Xunit;

namespace r.e.p.o_cheat.Tests
{
    public class ChatArtCodecTests
    {
        [Fact]
        public void MultilineArtStaysInOnePayload()
        {
            string[] payloads = ChatArtCodec.BuildPayloads("A A\r\n<B>");

            Assert.Single(payloads);
            Assert.Equal("Ａ　Ａ\n＜Ｂ＞", payloads[0]);
        }

        [Fact]
        public void PlainSingleLineCommandIsNotFullwidthConverted()
        {
            string[] payloads = ChatArtCodec.BuildPayloads("/help");

            Assert.Single(payloads);
            Assert.Equal("/help", payloads[0]);
        }

        [Fact]
        public void ArtPayloadContainsNoAsciiSpacesOrAsciiAngleBrackets()
        {
            string payload = Assert.Single(ChatArtCodec.BuildPayloads("x  y\n<tag>"));

            Assert.DoesNotContain(' ', payload);
            Assert.DoesNotContain('<', payload);
            Assert.DoesNotContain('>', payload);
            Assert.Contains('\n', payload);
        }

        [Fact]
        public void OversizedPayloadPrefersNewlineBoundary()
        {
            string lineA = new string('A', 40);
            string lineB = new string('B', 40);
            string lineC = new string('C', 40);

            string[] payloads = ChatArtCodec.BuildPayloads(
                lineA + "\n" + lineB + "\n" + lineC,
                64);

            Assert.Equal(3, payloads.Length);
            Assert.Equal(new string('Ａ', 40), payloads[0]);
            Assert.Equal(new string('Ｂ', 40), payloads[1]);
            Assert.Equal(new string('Ｃ', 40), payloads[2]);
        }

        [Fact]
        public void OversizedPayloadDoesNotSplitSurrogatePair()
        {
            string input = new string('中', 63) + "😀" + new string('文', 80);
            string[] payloads = ChatArtCodec.BuildPayloads(input, 64);

            Assert.True(payloads.Length >= 2);
            for (int i = 0; i < payloads.Length; i++)
            {
                string chunk = payloads[i];
                Assert.False(chunk.Length > 0 && char.IsHighSurrogate(chunk[chunk.Length - 1]));
                Assert.False(chunk.Length > 0 && char.IsLowSurrogate(chunk[0]));
            }
        }
    }
}
