using System;
using System.Collections.Generic;
using System.Linq;
using NumSharp.Generic;

namespace L1PathFinder {
  class Constants {
    public const int LEAF_CUTOFF = 64;
    public const int BUCKET_SIZE = 32;
  }

  internal class BucketInfo {
    public List<IPoint> left;
    public List<IPoint> right;
    public List<IPoint> on;
    public IPoint steiner0;
    public IPoint steiner1;
    public double y0;
    public double y1;
  }

  internal class Bucket {
    public double y0;
    public double y1;
    public Vertex top;
    public Vertex bottom;
    public List<Vertex> left;
    public List<Vertex> right;
    public List<Vertex> on;

    public Bucket (double y0, double y1, Vertex top, Vertex bottom, List<Vertex> left, List<Vertex> right, List<Vertex> on) {
      this.y0 = y0;
      this.y1 = y1;
      this.top = top;
      this.bottom = bottom;
      this.left = left;
      this.right = right;
      this.on = on;
    }
  }

  internal class Partition {
    public int x;
    public List<IPoint> left;
    public List<IPoint> right;
    public List<IPoint> on;
    public List<IPoint> vis;
  }

  public class Planner {
    private Geometry geometry;
    private Graph graph;
    private INode root;
    internal Planner (Geometry geometry, Graph graph, INode root) {
      this.geometry = geometry;
      this.graph = graph;
      this.root = root;
    }

    private static int CompareBucket (Bucket bucket, double y) {
      return Math.Sign (bucket.y0 - y);
    }

    private static void ConnectList (List<Vertex> nodes, Geometry geom, Graph graph, bool target, int x, double y) {
      for (var i = 0; i < nodes.Count; ++i) {
        var v = nodes[i];
        if (!geom.StabBox (v.x, v.y, x, y)) {
          if (target) {
            graph.AddT (v);
          } else {
            graph.AddS (v);
          }
        }
      }
    }

    private static void ConnectNodes (Geometry geom, Graph graph, INode node, bool target, int x, double y) {
      //Mark target nodes
      while (node != null) {
        //Check leaf case
        if (node.leaf) {
          var vv = node.verts;
          var nn = vv.Count;
          for (var i = 0; i < nn; ++i) {
            var v = vv[i];
            if (!geom.StabBox (v.x, v.y, x, y)) {
              if (target) {
                graph.AddT (v);
              } else {
                graph.AddS (v);
              }
            }
          }
          break;
        }

        //Otherwise, glue into buckets
        var buckets = node.buckets;
        var idx = BSearch.Search (buckets, y, CompareBucket);
        if (idx >= 0) {
          var bb = buckets[idx];
          if (y < bb.y1) {
            //Common case:
            if (node.x >= x) {
              //Connect right
              ConnectList (bb.right, geom, graph, target, x, y);
            }
            if (node.x <= x) {
              //Connect left
              ConnectList (bb.left, geom, graph, target, x, y);
            }
            //Connect on
            ConnectList (bb.on, geom, graph, target, x, y);
          } else {
            //Connect to bottom of bucket above
            var v = buckets[idx].bottom;
            if (v != null && !geom.StabBox (v.x, v.y, x, y)) {
              if (target) {
                graph.AddT (v);
              } else {
                graph.AddS (v);
              }
            }
            //Connect to top of bucket below
            if (idx + 1 < buckets.Count) {
              var v2 = buckets[idx + 1].top;
              if (v2 != null && !geom.StabBox (v.x, v.y, x, y)) {
                if (target) {
                  graph.AddT (v2);
                } else {
                  graph.AddS (v2);
                }
              }
            }
          }
        } else {
          //Connect to top of box
          var v = buckets[0].top;
          if (v != null && !geom.StabBox (v.x, v.y, x, y)) {
            if (target) {
              graph.AddT (v);
            } else {
              graph.AddS (v);
            }
          }
        }
        if (node.x > x) {
          node = node.left;
        } else if (node.x < x) {
          node = node.right;
        } else {
          break;
        }
      }
    }

    public double Search (int tx, int ty, int sx, int sy, out List<Point> outo) {
      outo = new List<Point> ();
      var geom = this.geometry;

      //Degenerate case:  s and t are equal
      if (tx == sx && ty == sy) {
        if (!geom.StabBox (tx, ty, sx, sy)) {
          outo.Add (new Point (sx, sy));
          return 0;
        }
        return double.PositiveInfinity;
      }

      //Check easy case - s and t directly connected
      if (!geom.StabBox (tx, ty, sx, sy)) {
        if (outo != null) {
          if (sx != tx && sy != ty) {
            outo.Add (new Point (tx, ty));
            outo.Add (new Point (sx, ty));
            outo.Add (new Point (sx, sy));
          } else {
            outo.Add (new Point (tx, ty));
            outo.Add (new Point (sx, sy));
          }
        }
        return Math.Abs (tx - sx) + Math.Abs (ty - sy);
      }

      //Prepare graph
      var graph = this.graph;
      graph.SetSourceAndTarget (sx, sy, tx, ty);

      //Mark target
      ConnectNodes (geom, graph, this.root, true, tx, ty);

      //Mark source
      ConnectNodes (geom, graph, this.root, false, sx, sy);

      //Run A*
      var dist = graph.Search ();

      //Recover path
      if (outo != null && dist < double.PositiveInfinity) {
        graph.GetPath (outo);
      }

      return dist;
    }

    private static int ComparePoint (IPoint a, IPoint b) {
      var d = a.y - b.y;
      if (d != 0) {
        return Math.Sign (d);
      }
      return Math.Sign (a.x - b.x);
    }

    private static Partition MakePartition (int x, List<IPoint> corners, Geometry geom, object edges) {
      var left = new List<IPoint> ();
      var right = new List<IPoint> ();
      var on = new List<IPoint> ();

      //Intersect rays along x horizontal line
      for (var i = 0; i < corners.Count; ++i) {
        var c = corners[i];
        if (!geom.StabRay (c.x, c.y, x)) {
          on.Add (c);
        }
        if (c.x < x) {
          left.Add (c);
        } else if (c.x > x) {
          right.Add (c);
        }
      }

      //Sort on events by y then x
      on.Sort (ComparePoint);

      //Construct vertices and horizontal edges
      var vis = new List<IPoint> ();
      var rem = new List<IPoint> ();
      for (var i = 0; i < on.Count;) {
        var l = x;
        var r = x;
        var v = on[i];
        var y = v.y;
        while (i < on.Count && on[i].y == y && on[i].x < x) {
          l = on[i++].x;
        }
        if (l < x) {
          vis.Add (new Vertex (l, y));
        }
        while (i < on.Count && on[i].y == y && on[i].x == x) {
          rem.Add (on[i]);
          vis.Add (on[i]);
          ++i;
        }
        if (i < on.Count && on[i].y == y) {
          r = on[i++].x;
          while (i < on.Count && on[i].y == y) {
            ++i;
          }
        }
        if (r > x) {
          vis.Add (new Vertex (r, y));
        }
      }

      return new Partition () {
        x = x,
          left = left,
          right = right,
          on = rem,
          vis = vis
      };
    }

    public static Planner Create (NDArray<int> grid) {
      Geometry geom;
      Graph graph = new Graph ();
      Dictionary<IPoint, Vertex> verts = new Dictionary<IPoint, Vertex> ();
      List<IPoint[]> edges = new List<IPoint[]> ();

      Vertex MakeVertex (IPoint point) {
        if (point == null) {
          return null;
        }
        if (verts.ContainsKey (point)) {
          return verts[point];
        }
        return verts[point] = graph.AddVertex (point.x, point.y);
      }

      Leaf MakeLeaf (List<IPoint> corners, int x0, int x1) {
        var localVerts = new List<Vertex> ();
        for (var i = 0; i < corners.Count; ++i) {
          var u = corners[i];
          var ux = graph.AddVertex (u.x, u.y);
          localVerts.Add (ux);
          verts[u] = ux;
          for (var j = 0; j < i; ++j) {
            var v = corners[j];
            if (!geom.StabBox (u.x, u.y, v.x, v.y)) {
              edges.Add (new [] { u, v });
            }
          }
        }
        return new Leaf (localVerts);
      }

      BucketInfo MakeBucket (List<IPoint> corners, int x) {
        //Split visible corners into 3 cases
        var left = new List<IPoint> ();
        var right = new List<IPoint> ();
        var on = new List<IPoint> ();
        for (var i = 0; i < corners.Count; ++i) {
          if (corners[i].x < x) {
            left.Add (corners[i]);
          } else if (corners[i].x > x) {
            right.Add (corners[i]);
          } else {
            on.Add (corners[i]);
          }
        }

        //Add Steiner vertices if needed
        IPoint addSteiner (int y, bool first) {
          if (!geom.StabTile (x, y)) {
            for (var i = 0; i < on.Count; ++i) {
              if (on[i].x == x && on[i].y == y) {
                return on[i];
              }
            }
            var pair = new Vertex (x, y);
            if (first) {
              on.Insert (0, pair);
            } else {
              on.Add (pair);
            }
            if (verts.ContainsKey (pair)) {
              verts[pair] = graph.AddVertex (x, y);
            }
            return pair;
          }
          return null;
        }

        var y0 = corners[0].y;
        var y1 = corners[corners.Count - 1].y;
        var loSteiner = addSteiner (y0, true);
        var hiSteiner = addSteiner (y1, false);

        void bipartite (List<IPoint> a, List<IPoint> b) {
          for (var i = 0; i < a.Count; ++i) {
            var u = a[i];
            for (var j = 0; j < b.Count; ++j) {
              var v = b[j];

              if (!geom.StabBox (u.x, u.y, v.x, v.y)) {
                edges.Add (new [] { u, v });
              }
            }
          }
        }

        bipartite (left, right);
        bipartite (on, left);
        bipartite (on, right);

        //Connect vertical edges
        for (var i = 1; i < on.Count; ++i) {
          var u = on[i - 1];
          var v = on[i];
          if (!geom.StabBox (u.x, u.y, v.x, v.y)) {
            edges.Add (new [] { u, v });
          }
        }

        return new BucketInfo () {
          left = left,
            right = right,
            on = on,
            steiner0 = loSteiner,
            steiner1 = hiSteiner,
            y0 = y0,
            y1 = y1
        };
      }

      //Make tree
      geom = Geometry.CreateGeometry (grid);
      INode MakeTree (List<IPoint> corners, int x0, int x1) {
        if (corners.Count == 0) {
          return null;
        }

        if (corners.Count < Constants.LEAF_CUTOFF) {
          return MakeLeaf (corners, x0, x1);
        }

        var x = corners[(int) ((uint) corners.Count >> 1)].x;
        var partition = MakePartition (x, corners, geom, edges);
        var left = MakeTree (partition.left, x0, x);
        var right = MakeTree (partition.right, x, x1);

        //Construct vertices
        for (var i = 0; i < partition.on.Count; ++i) {
          verts[partition.on[i]] = graph.AddVertex (partition.on[i].x, partition.on[i].y);
        }

        //Build buckets
        var vis = partition.vis;
        var buckets = new List<Bucket> ();
        IPoint lastSteiner = null;
        for (var i = 0; i < vis.Count;) {
          var v0 = i;
          var v1 = Math.Min (i + Constants.BUCKET_SIZE - 1, vis.Count - 1);
          while (++v1 < vis.Count && vis[v1 - 1].y == vis[v1].y) { }
          i = v1;
          var bb = MakeBucket (vis.Skip (v0).Take (v1 - v0).ToList (), x);
          if (lastSteiner != null && bb.steiner0 != null &&
            !geom.StabBox (lastSteiner.x, lastSteiner.y, bb.steiner0.x, bb.steiner0.y)) {
            edges.Add (new [] { lastSteiner, bb.steiner0 });
          }
          lastSteiner = bb.steiner1;
          buckets.Add (new Bucket (
            bb.y0,
            bb.y1,
            MakeVertex (bb.steiner0),
            MakeVertex (bb.steiner1),
            bb.left.Select (MakeVertex).ToList (),
            bb.right.Select (MakeVertex).ToList (),
            bb.on.Select (MakeVertex).ToList ()

          ));
        }
        return new Node (x, buckets, left, right);
      }
      var root = MakeTree (geom.corners, int.MinValue, int.MaxValue);

      //Link edges
      for (var i = 0; i < edges.Count; ++i) {
        graph.Link (verts[edges[i][0]], verts[edges[i][1]]);
      }

      //Initialized graph
      graph.Init ();

      //Return resulting tree
      return new Planner (geom, graph, root);

    }
  }
}