using System.Collections.Generic;
using r.e.p.o_cheat.Localization;
using Xunit;

namespace r.e.p.o_cheat.Tests
{
	public class TranslationValidatorTests
	{
		[Fact]
		public void SmartTokenParityPasses()
		{
			Assert.True(TranslationValidator.ValidatePair("Press {0} to open", "按 {0} 打开", out string error), error);
		}

		[Fact]
		public void SmartTokenMissingFails()
		{
			Assert.False(TranslationValidator.ValidatePair("Press {0} to open", "按打开", out _));
		}

		[Fact]
		public void NestedSmartTokenExtractedWhole()
		{
			List<string> tokens = TranslationValidator.ExtractSmartTokens("{players:list:{}|,   |   and   }");
			Assert.Single(tokens);
			Assert.Equal("{players:list:{}|,   |   and   }", tokens[0]);
		}

		[Fact]
		public void KeybindParityPassesAndFails()
		{
			Assert.True(TranslationValidator.ValidatePair("Use [move] to move.", "使用 [move] 移动。", out _));
			Assert.False(TranslationValidator.ValidatePair("Use [move] to move.", "使用 [jump] 移动。", out _));
			Assert.False(TranslationValidator.ValidatePair("Use [move] to move.", "使用移动。", out _));
		}

		[Fact]
		public void NewlineCountMustMatch()
		{
			Assert.True(TranslationValidator.ValidatePair("A\n\nB", "甲\n\n乙", out _));
			Assert.False(TranslationValidator.ValidatePair("A\n\nB", "甲乙", out _));
		}

		[Fact]
		public void TmpTagParityChecks()
		{
			Assert.True(TranslationValidator.ValidatePair("<b><u>IMPORTANT</u></b>", "<b><u>重要</u></b>", out _));
			Assert.False(TranslationValidator.ValidatePair("<b>X</b>", "<b>X", out _));
			// attribute values may differ (e.g. color); only tag structure must match
			Assert.True(TranslationValidator.ValidatePair("<color=red>x</color>", "<color=blue>x</color>", out _));
			Assert.False(TranslationValidator.ValidatePair("plain", "<color=red>x</color>", out _));
		}

		[Fact]
		public void PercentTokensChecked()
		{
			Assert.True(TranslationValidator.ValidatePair("value %d", "数值 %d", out _));
			Assert.False(TranslationValidator.ValidatePair("value %d", "数值", out _));
		}

		[Fact]
		public void UnicodeSurvives()
		{
			Assert.True(TranslationValidator.ValidatePair("Extraction", "撤离", out _));
			Assert.True(TranslationValidator.ValidatePair("撤离点已激活", "Extraction point activated", out _));
		}
	}

	public class RichTextPreserverTests
	{
		[Fact]
		public void CountsOpenAndCloseTags()
		{
			Dictionary<string, RichTextPreserver.Pair> counts = RichTextPreserver.CountTags("<sprite name=truck> <color=#fff>hi</color>");
			Assert.Equal(1, counts["sprite"].Open);
			Assert.Equal(0, counts["sprite"].Close);
			Assert.Equal(1, counts["color"].Open);
			Assert.Equal(1, counts["color"].Close);
		}

		[Fact]
		public void UnbalancedNonVoidTagFails()
		{
			Assert.False(RichTextPreserver.ValidateTagParity("<b>x</b>", "<b>x</b><i>", out _));
			Assert.True(RichTextPreserver.ValidateTagParity("<b>x</b>", "<b>y</b>", out _));
		}
	}
}
