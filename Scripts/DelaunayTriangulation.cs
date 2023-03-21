using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Delaunay {
    public class DelaunayTriangulation : MonoBehaviour {

        public Renderer render;

        public struct CircumCircle {
            public Vector3 center;
            public float radius;

            public CircumCircle(Vector3 _center, float _radius) {
                center = _center;
                radius = _radius;
            }
        }

        public struct Line {
            public Vector3 start;
            public Vector3 direction;

            public Line(Vector3 _start, Vector3 _direction) {
                start = _start;
                direction = _direction;
            }
        }

        public struct Edge {
            public Vector3 a;
            public Vector3 b;

            public Edge(Vector3 _a, Vector3 _b) {
                a = _a;
                b = _b;
            }
        }

        public class Triangle {
            #region Props
            public Vector3 a;
            public Vector3 b;
            public Vector3 c;

            public Vector3 ab;
            public Vector3 bc;
            public Vector3 ca;

            public Edge edge_ab;
            public Edge edge_bc;
            public Edge edge_ca;

            public readonly Vector3 mid_ab;
            public readonly Vector3 mid_bc;
            public readonly Vector3 mid_ac;
            #endregion

            public static Triangle zero {
                get {
                    return new Triangle(
                        Vector3.zero,
                        Vector3.zero,
                        Vector3.zero
                    ); ;
                }
            }

            public Triangle(Vector3 _a, Vector3 _b, Vector3 _c) {
                a = _a;
                b = _b;
                c = _c;

                ab = _b - _a;
                bc = _c - _b;
                ca = _a - _c;

                edge_ab = new Edge(_a, _b);
                edge_bc = new Edge(_b, _c);
                edge_ca = new Edge(_c, _a);

                mid_ab = (_a + _b) / 2;
                mid_bc = (_b + _c) / 2;
                mid_ac = (_c + _a) / 2;
            }

            public bool InCircumCircle(Vector3 point) {
                // using determinant to check if point lies inside triangle's circumcircle
                
                Matrix4x4 matrix = new Matrix4x4();

                matrix.SetRow(0, new Vector4(a.x, a.y, Mathf.Pow(a.x, 2) + Mathf.Pow(a.y, 2), 1));
                matrix.SetRow(1, new Vector4(b.x, b.y, Mathf.Pow(b.x, 2) + Mathf.Pow(b.y, 2), 1));
                matrix.SetRow(2, new Vector4(c.x, c.y, Mathf.Pow(c.x, 2) + Mathf.Pow(c.y, 2), 1));
                matrix.SetRow(3, new Vector4(point.x, point.y, Mathf.Pow(point.x, 2) + Mathf.Pow(point.y, 2), 1));

                bool inside = matrix.determinant > 0;

                return inside;
            }

            // just for visualization
            public CircumCircle GetCircumCircle() {
                Vector3 cross = Vector3.Cross(ab, bc);
                Vector3 direction_1 = Vector3.Cross(ab, cross);
                Line line_1 = new Line(mid_ab, direction_1.normalized);

                Vector3 direction_2 = Vector3.Cross(bc, Vector3.Cross(bc, ca));
                Line line_2 = new Line(mid_bc, direction_2.normalized);

                Vector3 intersection = LineIntersection(line_1, line_2);
                float radius = Vector3.Distance(intersection, a);

                return new CircumCircle(intersection, radius);
            }
            Vector3 LineIntersection(Line line_1, Line line_2) {
                // linear equation system
                Vector3 factors = line_2.start - line_1.start;
                Vector3 direction_1 = line_1.direction;
                Vector3 direction_2 = line_2.direction;

                // formula for lambda derived with the general form of the equation system
                float lambda = (factors.y * direction_2.x - factors.x * direction_2.y)
                               / (direction_1.y * direction_2.x - direction_1.x * direction_2.y);

                Vector3 intersection = line_1.start + (lambda * direction_1);

                return intersection;
            }
        }

        public int amount = 3;
        Vector3[] points;
        List<Triangle> tris = new List<Triangle>();

        Triangle bigtri = new Triangle(
            new Vector3(-50, -50, 0),
            new Vector3(50, -50, 0),
            new Vector3(0, 50, 0)
        );

        void Start() {
            points = new Vector3[amount];
            RandomizePoints();

            // testing stuff
            CircumCircle circumCircle = bigtri.GetCircumCircle();
            Debug.Log($"Circumcircle: {circumCircle.center}, {circumCircle.radius}");
        }

        void OnDrawGizmos() {
            // draw points 
            foreach (Vector3 point in points) {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(point, .25f);
            }
            render.SetTriangles(tris, points);
        }

        public void RandomizePoints() {
            points = new Vector3[amount];

            for (int i = 0; i < points.Length; i++) {
                points[i] = new Vector3(Random.Range(-20, 20), Random.Range(-20, 20), 0.0f);
            }
            tris.Clear();

            Initialize();

            Triangulation();
        }

        void Initialize() {
            AddSingleTri(bigtri, points[0]);
            points = points.Skip(1).ToArray();
        }

        void Triangulation() {
            foreach (Vector3 point in points) {
                AddVertex(point);
            }
        }

        void AddVertex(Vector3 p) {
            List<Triangle> badTriangles = new();

            Triangle solo = Triangle.zero; // this should never remain Triangle.zero

            foreach (Triangle tri in tris) {
                if (tri.InCircumCircle(p)) {
                    solo = tri;
                    badTriangles.Add(tri);
                }
            }

            if (badTriangles.Count == 0) {
                if (bigtri.InCircumCircle(p)) {
                    AddSingleTri(bigtri, p);
                    return;
                }
                Debug.LogError($"Point {p} not inside of any circumcircle");
                return;
            }
            if (badTriangles.Count == 1) {
                AddSingleTri(solo, p);
                return;
            }

            List<Edge> polygon = new();

            for (int i = 0; i < badTriangles.Count; i++) {
                Triangle tri = badTriangles[i];
                Edge ab = tri.edge_ab;
                Edge bc = tri.edge_bc;
                Edge ca = tri.edge_ca;

                for (int j = 0; j < badTriangles.Count; j++) {
                    if (i == j) continue;

                    Triangle tri_2 = badTriangles[j];
                    Edge ab_2 = tri_2.edge_ab;
                    Edge bc_2 = tri_2.edge_bc;
                    Edge ca_2 = tri_2.edge_ca;

                    if (!ab.Equals(ab_2) && !ab.Equals(bc_2) && !ab.Equals(ca_2)) {
                        polygon.Add(ab);
                    }
                    if (!bc.Equals(ab_2) && !bc.Equals(bc_2) && !bc.Equals(ca_2)) {
                        polygon.Add(bc);
                    }
                    if (!ca.Equals(ab_2) && !ca.Equals(bc_2) && !ca.Equals(ca_2)) {
                        polygon.Add(ca);
                    }
                }
            }

            foreach (Triangle tri in badTriangles) {
                tris.Remove(tri);
                //if(badTriangles.Contains(tri)) {
                //}
            }

            foreach (Edge edge in polygon) {
                // don't know if counterclockwise order of points is preserved
                // which is important for the matrix's determinant's value
                // this is most likely a big problem for future Roachl
                Triangle newTri = new(edge.a, edge.b, p);
                tris.Add(newTri);
            }

            return;
        }

        void AddSingleTri(Triangle currentTri, Vector3 point) {
            Triangle a = new Triangle(
                currentTri.a,
                currentTri.b,
                point
            );
            Triangle b = new Triangle(
                currentTri.b,
                currentTri.c,
                point
            );
            Triangle c = new Triangle(
                currentTri.c,
                currentTri.a,
                point
            );

            tris.Add(a);
            tris.Add(b);
            tris.Add(c);

            tris.Remove(currentTri);
        }

        // (weight 1, weight 2, inside triangle) # unused, except the determinant thing turns out to not be working
        (double, double, bool) RelativePosition(Triangle tri, Vector3 p) {
            // check with barycentric coordinates
            Vector3 a = tri.a;
            Vector3 b = tri.b;
            Vector3 c = tri.c;

            double weight_1 = ((c.x - a.x) * (p.y - a.y) - (c.y - a.y) * (p.x - a.x))
                            / ((b.y - a.y) * (c.x - a.x) - (b.x - a.x) * (c.y - a.y));

            double weight_2 = (p.y - a.y - weight_1 * (b.y - a.y)) / (c.y - a.y);

            if (weight_1 >= 0 && weight_2 >= 0 && (weight_1 + weight_2) <= 1) {
                return (weight_1, weight_2, true);
            }

            return (weight_1, weight_2, false);
        }
    }
}
