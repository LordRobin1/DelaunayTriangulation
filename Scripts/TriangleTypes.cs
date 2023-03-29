using UnityEngine;

namespace Delaunay {

    public class TriangleTypes : MonoBehaviour {

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

            public bool IsCounterClockwise() {
                float signedSum = 0;

                signedSum += (b.x - a.x) * (b.y + a.y);
                signedSum += (c.x - b.x) * (c.y + b.y);
                signedSum += (a.x - c.x) * (a.y + c.y);

                return signedSum < 0;
            }

            // not truly equal, orientation etc. could be different
            public override bool Equals(object obj) => this.Equals(obj as Triangle);

            public override int GetHashCode() => this.GetHashCode(); //compiler complains without

            public bool Equals(Triangle other_tri) {
                return  (a == other_tri.a || a == other_tri.b || a == other_tri.c) &&
                        (b == other_tri.a || b == other_tri.b || b == other_tri.c) &&
                        (c == other_tri.a || c == other_tri.b || c == other_tri.c);
            }
            
            public static bool operator == (Triangle tri_1, Triangle tri_2) {
                return tri_1.Equals(tri_2);
            }
            public static bool operator != (Triangle tri_1, Triangle tri_2) {
                return !tri_1.Equals(tri_2);
            }

            #region just for visualization
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
            #endregion
        }

        public class CircumCircle {
            public Vector3 center;
            public float radius;

            public CircumCircle(Vector3 _center, float _radius) {
                center = _center;
                radius = _radius;
            }
        }

        public class Line {
            public Vector3 start;
            public Vector3 direction;

            public Line(Vector3 _start, Vector3 _direction) {
                start = _start;
                direction = _direction;
            }
        }

        public class Edge {
            public Vector3 a;
            public Vector3 b;

            public Edge(Vector3 _a, Vector3 _b) {
                a = _a;
                b = _b;
            }

            public override bool Equals(object obj) => this.Equals(obj as Edge);

            public override int GetHashCode() => this.GetHashCode(); //compiler complains without

            public bool Equals(Edge other) {
                return (a == other.a || a == other.b) &&
                       (b == other.a || b == other.b);
            }

            public static bool operator == (Edge e_1, Edge e_2) {
                return e_1.Equals(e_2);
            }
            public static bool operator != (Edge e_1, Edge e_2) {
                return !e_1.Equals(e_2);
            }
        }
    }
}
