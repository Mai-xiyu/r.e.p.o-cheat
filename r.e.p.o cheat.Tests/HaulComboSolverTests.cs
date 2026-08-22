using r.e.p.o_cheat;
using Xunit;

namespace r.e.p.o_cheat.Tests
{
    public class HaulComboSolverTests
    {
        [Fact]
        public void PicksMinimumOvershootSubset()
        {
            int[] values = { 3100, 2900, 1500, 800 };
            Assert.True(HaulComboSolver.TryPick(values, 7400, out int[] indices, out int sum));
            Assert.Equal(7500, sum);
            Assert.Equal(3, indices.Length);
        }

        [Fact]
        public void ReturnsFalseWhenEmpty()
        {
            Assert.False(HaulComboSolver.TryPick(new int[0], 100, out _, out _));
        }

        [Fact]
        public void GreedyStillReturnsItemsWhenTargetUnreachable()
        {
            int[] values = { 100, 200, 50 };
            Assert.True(HaulComboSolver.TryPick(values, 10000, out int[] indices, out int sum));
            Assert.Equal(350, sum);
            Assert.Equal(3, indices.Length);
        }
    }
}
