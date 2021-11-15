using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NumSharp;
using NumSharp.Generic;

namespace L1PathFinder.Tests {
    internal static class MapExtensions {
        public static NDArray<int> LoadMap (string mapFile) {
            using var reader = File.OpenText (mapFile);
            var line = reader.ReadLine ();
            return ReadOctile (reader);
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
                    val = 0;
                    return true;
                case '@':
                    val = 1;
                    return true;
            }
            val = -1;
            return false;
        }

        internal static IEnumerable<int> ParserMap (string mapStr) {
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
                new int[] { height, width }
            );
        }
    }

    public class GridPath {
        private readonly int _width;
        private readonly int _height;
        private readonly NDArray<char> grid;

        public int Width => _width;

        public int Height => _height;

        public GridPath (int width, int height) {
            this._width = width;
            this._height = height;
            this.grid = BuildGrid (width, height);
        }

        public GridPath (NDArray<int> array) {
            this._height = array.shape[0];
            this._width = array.shape[1];
            this.grid = BuildGrid (array);
        }

        public char this [int x, int y] {
            get {
                return this.grid[y, x];
            }
            set {
                this.grid[y, x] = value;
            }
        }

        public string Draw (IEnumerable<Point> path) {
            var tmpGrid = this.grid.Clone ();
            foreach (var p in path) {
                tmpGrid.SetValue ('#', new int[] { p.x, p.y });
            }
            return ToString (tmpGrid);
        }

        private static NDArray<char> BuildGrid (NDArray<int> array) {
            return new (array.ToArray<int> ().Select (i => i == 0 ? '.' : '@').ToArray (), array.shape);
        }

        private static NDArray<char> BuildGrid (int width, int height) {
            var array = new char[width * height];
            Array.Fill (array, '.');
            return new NDArray<char> (array, new [] { height, width });
        }

        public override string ToString () {
            return ToString (this.grid);
        }

        public static string ToString (NDArray<char> grid) {
            var size = grid.GetSize ();
            var builder = new StringBuilder ();
            for (var y = 0; y < size.Height; y++) {
                for (int x = 0; x < size.Width; x++) {
                    builder.Append (grid[y, x]);
                }
                builder.AppendLine ();
            }
            return builder.ToString ();
        }

        public static string ToString (NDArray grid) {
            var size = grid.GetSize ();
            var builder = new StringBuilder ();
            for (var y = 0; y < size.Height; y++) {
                builder.AppendLine (grid.GetString (y));
                // for (int x = 0; x < size.Width; x++) {
                //     grid.GetString(y)
                //     builder.Append (grid[y, x].ToString ());
                // }
                // builder.AppendLine ();
            }
            return builder.ToString ();
        }
    }

    public static class NDArrayExtensions {
        public static Size GetSize (this NDArray array) {
            return new Size (array.shape[1], array.shape[0]);
        }

        public static Size GetSize<T> (this NDArray<T> array) where T : unmanaged {
            return new Size (array.shape[1], array.shape[0]);
        }
    }

    public struct Size {
        public readonly int Width;

        public readonly int Height;

        public Size (int width, int height) {
            Width = width;
            Height = height;
        }
    }
}