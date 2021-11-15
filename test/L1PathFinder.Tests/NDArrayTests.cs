using NumSharp;
using NumSharp.Generic;
using Xunit;
using Xunit.Abstractions;

namespace L1PathFinder.Tests {
    public class NDArrayTests {
        private readonly ITestOutputHelper output;

        public NDArrayTests (ITestOutputHelper outputHelper) {
            this.output = outputHelper;
        }

        [Fact]
        public void TestCreate () {
            NDArray array = np.array<int> (new [] { new [] { 1, 2, 3 }, new [] { 4, 5, 6 } });
            Assert.Equal (new [] { 2, 3 }, array.shape);

            output.WriteLine (GridPath.ToString (array.AsGeneric<int> ()));

            var grid = new GridPath (array.AsGeneric<int> ());
            Assert.Equal (3, grid.Width);
            Assert.Equal (2, grid.Height);
        }

        [Fact]
        public void TestNdArray1 () {
            var array = new NDArray<int> (new int[] { 1, 2, 3, 4, 5, 6 }, new int[] { 2, 3 });

            var grid = new GridPath (array);
            output.WriteLine (GridPath.ToString (array));

            Assert.Equal (3, grid.Width);
            Assert.Equal (2, grid.Height);

            output.WriteLine (grid.ToString ());
        }

        [Fact]
        public void TestGetString () {
            var array = new NDArray<int> (new int[] { 1, 2, 3, 4, 5, 6 }, new int[] { 2, 3 });
            var size = array.GetSize ();
            for (var y = 0; y < size.Height; y++) {
                for (var x = 0; x < size.Width; x++) {
                    output.WriteLine (array.GetInt32(y,x).ToString());
                }
            }
        }
    }
}