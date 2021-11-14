using System.Collections.Generic;
using System.IO;
using System.Linq;
using NumSharp;
using NumSharp.Generic;
using Xunit;
using Xunit.Abstractions;

namespace L1PathFinder.Tests {
    public class SimpleeTest {
        private readonly ITestOutputHelper output;

        private readonly NDArray<int> maze = new (new int[] {
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
            0
        }, new int[] { 8, 7 });

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
            output.WriteJson (path);
        }

        [Fact]
        public void TestMapFile () {
            var mapFile = "cfd.map";
            var filePath = Path.GetFullPath (Path.Combine ("../../../../", mapFile));
            Assert.True (File.Exists (filePath), filePath);
            var map = MapExtensions.LoadMap (filePath);
            Assert.NotNull (map);

            var planner = Planner.Create (map);

            var dist = planner.Search (5, 144, 94, 123, out var path);
            Assert.Equal (150, dist);
            // Assert.NotStrictEqual (Result, path.ToArray ());
            output.WriteJson (path);
        }
    }

    internal static class MapExtensions {
        public static NDArray<int> LoadMap (string mapFile) {
            using var reader = File.OpenText (mapFile);
            var line = reader.ReadLine ();
            // if (line.Contains ("type octile")) {
            return ReadOctile (reader);
            // } else {

            // }
        }

        private static int[] ParseLines (IEnumerable<string> lines) {
            return lines.SelectMany (l => ParseLine (l)).ToArray ();
        }

        private static IEnumerable<int> ParseLine (string line) {
            foreach (var c in line) {
                if (c == '\n' || c == '\r') {
                    continue;
                }
                yield return (c == '.' || c == 'G') ? 1 : 0;
            }
        }

        private static bool ParserMap (char c, out int val) {
            switch (c) {
            case '.':
                val = 1;
                return true;
            case '@':
                val = 0;
                return true;
            }
            val = -1;
            return false;
        }

        private static IEnumerable<int> ParserMap (string mapStr) {
            foreach (var c in mapStr) {
                if (ParserMap (c, out var val)) {
                    yield return val;
                }
            }
        }

        /// <summary>
        /// 读取 octile 文件
        /// </summary>
        /// <remarks>
        /// height 271
        /// width 294
        /// map
        /// </remarks>
        private static NDArray<int> ReadOctile (TextReader reader) {
            var height = int.Parse (reader.ReadLine ().Split (" ") [1]);
            var width = int.Parse (reader.ReadLine ().Split (" ") [1]);
            reader.ReadLine ();
            var map = reader.ReadToEnd ();
            return new NDArray<int> (
                ParserMap (map).ToArray (),
                new int[] { width, height }
            );
        }
    }
}