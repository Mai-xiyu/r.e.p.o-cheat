using r.e.p.o_cheat.Localization;
using Xunit;

namespace r.e.p.o_cheat.Tests
{
	public class TranslationDatabaseTests
	{
		[Fact]
		public void LoadsTablesAndDirectSections()
		{
			var db = TranslationDatabase.FromJson(
				"{\"Game\":{\"EXTRACTION.LOCKED\":\"已锁定\"},\"direct\":{\"Good job!\":\"干得漂亮！\"}}");
			Assert.Equal(1, db.TableEntryCount);
			Assert.Equal(1, db.DirectCount);
			Assert.Equal(0, db.RejectedCount);
		}

		[Fact]
		public void TableLookupHitAndMiss()
		{
			var db = TranslationDatabase.FromJson("{\"Game\":{\"EXTRACTION.LOCKED\":\"已锁定\"}}");
			Assert.True(db.TryGetTable("Game", "EXTRACTION.LOCKED", out string zh));
			Assert.Equal("已锁定", zh);
			Assert.False(db.TryGetTable("Game", "MISSING.KEY", out _));
			Assert.False(db.TryGetTable("HUD", "EXTRACTION.LOCKED", out _));
		}

		[Fact]
		public void MissingTranslationFallsBackToSource()
		{
			// contract: missing -> original English (never blank)
			var db = TranslationDatabase.FromJson("{\"Game\":{}}");
			Assert.False(db.TryGetTable("Game", "EXTRACTION.READY", out string zh));
			Assert.Null(zh);
			Assert.False(db.TryGetDirect("Use [move] to move.", out string direct));
			Assert.Null(direct);
		}

		[Fact]
		public void DirectExactAndTemplateMatch()
		{
			var db = TranslationDatabase.FromJson(
				"{\"direct\":{\"Press {0} to interact\":\"按 {0} 互动\",\"Good job!\":\"干得漂亮！\",\"{0} items\":\"{0} 个物品\"}}");
			Assert.True(db.TryGetDirect("Good job!", out string exact));
			Assert.Equal("干得漂亮！", exact);
			// filled-in argument gets substituted back into the translation
			Assert.True(db.TryGetDirect("Press F to interact", out string templated));
			Assert.Equal("按 F 互动", templated);
			Assert.True(db.TryGetDirect("Press [E] to interact", out string bracketTemplated));
			Assert.Equal("按 [E] 互动", bracketTemplated);
			// key starting with a placeholder
			Assert.True(db.TryGetDirect("5 items", out string leading));
			Assert.Equal("5 个物品", leading);
			Assert.False(db.TryGetDirect("Press  to interact", out _));
		}

		[Fact]
		public void InvalidDirectEntryRejected()
		{
			var db = TranslationDatabase.FromJson("{\"direct\":{\"Press {0} now\":\"马上按\"}}");
			Assert.Equal(0, db.DirectCount);
			Assert.Equal(1, db.RejectedCount);
			Assert.False(db.TryGetDirect("Press F now", out _));
		}

		[Fact]
		public void MergeLaterWinsPerKey()
		{
			var db = TranslationDatabase.FromJson("{\"Game\":{\"EXTRACTION.LOCKED\":\"已锁定\"}}");
			db.MergeJson("{\"Game\":{\"EXTRACTION.LOCKED\":\"锁住了\",\"EXTRACTION.READY\":\"就绪\"}}");
			Assert.Equal(2, db.TableEntryCount);
			Assert.True(db.TryGetTable("Game", "EXTRACTION.LOCKED", out string zh));
			Assert.Equal("锁住了", zh);
		}

		[Fact]
		public void InvalidJsonThrowsNothing()
		{
			// malformed JSON must not take the module down
			var db = TranslationDatabase.FromJson("{not json");
			Assert.Equal(0, db.TableEntryCount);
			Assert.Equal(0, db.RejectedCount);
		}

		[Fact]
		public void EmbeddedTranslationCorpusIsComplete()
		{
			// pins the shipped corpus: HUD 192 + Menu 397 + Game 14 (+1 direct entry)
			var db = TranslationDatabase.Load();
			Assert.NotNull(db);
			Assert.Equal(603, db.TableEntryCount);
			Assert.True(db.TryGetTable("HUD", "TUTORIAL.MOVEMENT.TEXT", out string tutorial));
			Assert.Equal("使用 [move] 移动。", tutorial);
			Assert.True(db.TryGetTable("Menu", "LOBBY.PING", out string ping));
			Assert.Equal("{ping} 毫秒", ping);
		}
	}

	public class GameVersionInfoTests
	{
		[Fact]
		public void KnownShaIs64HexChars()
		{
			Assert.Equal(64, Compatibility.GameVersionInfo.KnownSha256.Length);
			Assert.Matches("^[0-9A-F]+$", Compatibility.GameVersionInfo.KnownSha256);
		}

		[Fact]
		public void UnknownGameTypeNotPresent()
		{
			Assert.False(Compatibility.GameVersionInfo.HasGameType("TypeThatDoesNotExist, Assembly-CSharp"));
		}

		[Fact]
		public void StateIsSaneOutsideTheGameHost()
		{
			// no game loaded here -> Unknown. If the test host happens to load a copy of the
			// game DLL (transitive copy-local), the fingerprint must be Exact - never
			// Partial/Compatible for an exact copy.
			Assert.Contains(
				Compatibility.GameVersionInfo.State,
				new[] { Compatibility.CompatState.Unknown, Compatibility.CompatState.Exact });
		}
	}
}
