namespace L1PathFinder {
    public readonly struct Point {
        public Point (int x, int y) {
            this.x = x;
            this.y = y;
        }

        public readonly int x;
        public readonly int y;

        public override bool Equals (object o) {
            if (o is IPoint c) {
                // return (int) (c.x * 100) == (int) (this.x * 100) &&
                //     (int) (c.y * 100) == (int) (this.y * 100);
                return x == c.x && y == c.y;
            }
            return false;
        }

        public override int GetHashCode () {
            return (((int) x & 0xFFFF) << 16) + ((int) y & 0xFFFF);
        }

        public override string ToString () {
            return $"({x},{y})";
        }
    }
}