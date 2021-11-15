using System.IO;
using System.Linq;
using System.Text;
using NumSharp.Generic;
using Xunit;
using Xunit.Abstractions;

namespace L1PathFinder.Tests {
    public class SimpleeTest {
        private readonly ITestOutputHelper output;

        private static readonly int[] mazeArray = new int[] {
            0,
            1,
            0,
            0,
            0,
            0,
            0,
            0,
            1,
            0,
            1,
            0,
            0,
            0,
            0,
            1,
            0,
            1,
            1,
            1,
            0,
            0,
            1,
            0,
            1,
            0,
            0,
            0,
            0,
            1,
            0,
            1,
            0,
            0,
            0,
            0,
            1,
            0,
            1,
            0,
            0,
            0,
            0,
            1,
            0,
            1,
            0,
            1,
            1,
            0,
            0,
            0,
            1,
            0,
            0,
            0,
        };
        private readonly NDArray<int> maze = new (mazeArray, new int[] { 8, 7 });

        private readonly Point[] Result = new [] {
            new Point (0, 0), new Point (7, 0), new Point (7, 2), new Point (0, 2),
            new Point (0, 4), new Point (1, 4), new Point (1, 6), new Point (3, 6),
            new Point (5, 6), new Point (5, 4), new Point (7, 4), new Point (7, 6)
        };

        public SimpleeTest (ITestOutputHelper outputHelper) {
            this.output = outputHelper;
        }

        [Fact]
        public void TestNdArray () {
            Assert.Equal (8, maze.shape[0]);
            Assert.Equal (7, maze.shape[1]);
            Assert.NotNull (maze.transpose ());
            output.WriteLine ("");
            output.WriteLine (GridPath.ToString (maze));
            output.WriteLine (GridPath.ToString (maze.transpose ()));
        }

        [Fact]
        public void Test1 () {
            //Create path planner
            var planner = Planner.Create (maze);

            var dist = planner.Search (0, 0, 7, 6, out var path);
            Assert.Equal (31, dist);
            Assert.NotStrictEqual (Result, path.ToArray ());

            //Log output
            output.WriteLine ($"path length { dist}");

            var gridPath = new GridPath (maze);
            output.WriteLine ("\n" + gridPath.Draw (path));
        }

        [Fact]
        public void TestMapFile () {
            var mapFile = "cfd.map";
            var filePath = Path.GetFullPath (Path.Combine ("../../../../", mapFile));
            Assert.True (File.Exists (filePath), filePath);
            var map = MapExtensions.LoadMap (filePath);
            Assert.NotNull (map);

            var grid = new GridPath (map);
            Assert.Equal (271, grid.Height);
            Assert.Equal (294, grid.Width);
            // output.WriteLine (grid.ToString());

            var planner = Planner.Create (map);

            var dist = planner.Search (144, 5, 114, 93, out var path);
            Assert.Equal (138, dist);
            var gridPath = new GridPath (map);
            output.WriteLine (gridPath.Draw (path));
        }

        [Fact]
        public void TestGridPath () {
            var gridPath = new GridPath (maze);
            output.WriteLine ("\n" + gridPath.ToString ());
            var mapFile = "cfd.map";
            var filePath = Path.GetFullPath (Path.Combine ("../../../../", mapFile));
            Assert.True (File.Exists (filePath), filePath);
            var map = MapExtensions.LoadMap (filePath);

            gridPath = new GridPath (map);
            output.WriteLine ("\n" + gridPath.ToString ());
        }

        [Fact]
        public void TestMapParser () {
            var mapStr = @"
.@.....
.@.@...
.@.@@@.
.@.@...
.@.@...
.@.@...
.@.@.@@
...@...";
            var mapArray = MapExtensions.ParserMap (mapStr).ToArray ();
            Assert.Equal (56, mapArray.Length);
            Assert.NotStrictEqual (mazeArray, mapArray);
            NDArray<int> array = new (mapArray, new int[] { 8, 7 });

            Assert.NotStrictEqual (maze.shape, array.shape);

            var grid = new GridPath (maze);
            output.WriteLine ("maze:\n" + grid.ToString ());

            output.WriteLine ("array:\n", new GridPath (array).ToString ());

            var planner = Planner.Create (array);
            var dist = planner.Search (0, 0, 7, 6, out var path);
            Assert.Equal (31, dist);

            // Assert.NotStrictEqual (maze, array);

            grid = new GridPath (array);
            output.WriteLine ("\n" + grid.Draw (path));
        }
    }
}