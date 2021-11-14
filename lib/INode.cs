using System.Collections.Generic;

namespace L1PathFinder {
    internal interface INode {
        List<Vertex> verts { get; }
        List<Bucket> buckets { get; }
        bool leaf { get; }
        INode left { get; }
        INode right { get; }
        double x { get; }
    }

    internal class Node : INode {
        public double x { get; private set; }
        public bool leaf { get { return false; } }
        public List<Bucket> buckets { get; private set; }
        public List<Vertex> verts { get { return null; } }
        public INode left { get; private set; }
        public INode right { get; private set; }

        public Node (double x, List<Bucket> buckets, INode left, INode right) {
            this.x = x;
            this.buckets = buckets;
            this.left = left;
            this.right = right;
        }
    }

    internal class Leaf : INode {
        public double x { get { return 0; } }
        public List<Bucket> buckets { get { return null; } }
        public List<Vertex> verts { get; private set; }
        public bool leaf { get { return true; } }
        public INode left { get { return null; } }
        public INode right { get { return null; } }
        public Leaf (List<Vertex> verts) {
            this.verts = verts;
        }
    }

}