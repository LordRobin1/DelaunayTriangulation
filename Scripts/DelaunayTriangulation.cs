using System.Collections.Generic;
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

        public class Triangle {
            public Vector3 a;
            public Vector3 b;
            public Vector3 c;

            public Vector3 ab;
            public Vector3 bc;
            public Vector3 ca;

            public readonly Vector3 mid_ab;
            public readonly Vector3 mid_bc;
            public readonly Vector3 mid_ac;

            public Triangle(Vector3 _a, Vector3 _b, Vector3 _c) {
                a = _a;
                b = _b;
                c = _c;

                ab = _b - _a;
                bc = _c - _b;
                ca = _a - _c;

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

            // just for fun
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
            public Vector3 LineIntersection(Line line_1, Line line_2) {
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
            tris.Add(bigtri);
            GeneratePoints();
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

        public void GeneratePoints() {
            for (int i = 0; i < amount; i++) {
                points[i] = new Vector3(Random.Range(-20, 20), Random.Range(-20, 20), 0.0f);
            }
            tris.RemoveRange(1, tris.Count - 1);
            Debug.Log(tris.Count);
            Triangulation();
        }

        void Triangulation() {
            int i = 1;
            foreach (Vector3 point in points) {
                Triangle currentTri = CheckTris(point);
                Debug.Log($"{currentTri.a}, {currentTri.b}, {currentTri.c}\n" +
                          $"{currentTri.mid_ab}, {currentTri.mid_bc}, {currentTri.mid_ac}");
                // new tris
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
                // Untested code
                tris.Add(a);
                tris.Add(b);
                tris.Add(c);
                i += 3;
            }
        }

        Triangle CheckTris(Vector3 p) {
            // check with barycentric coordinates
            for (int i = 1; i < tris.Count; i++) {
                Vector3 a = tris[i].a;
                Vector3 b = tris[i].b;
                Vector3 c = tris[i].c;

                double weight_1 = ((c.x - a.x) * (p.y - a.y) - (c.y - a.y) * (p.x - a.x))
                               / ((b.y - a.y) * (c.x - a.x) - (b.x - a.x) * (c.y - a.y));

                double weight_2 = (p.y - a.y - weight_1 * (b.y - a.y)) / (c.y - a.y);

                if (weight_1 >= 0 && weight_2 >= 0 && (weight_1 + weight_2) <= 1) {
                    Debug.Log("in some Triangle ✔");
                    return tris[i];
                }
            }
            Debug.Log($"Point {p} is not in bigTri");
            return tris[0];
        }
    }
}
