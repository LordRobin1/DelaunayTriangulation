using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelaunayTriangulation : MonoBehaviour 
{
    class Triangle {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public Triangle (Vector3 _a, Vector3 _b, Vector3 _c) {
            a = _a;
            b = _b;
            c = _c;
        }
    }

    public int amount = 10;
    Vector3[] points;
    List<Triangle> tris = new List<Triangle>();

    Triangle bigtri = new Triangle (
        new Vector3(-50, -50, 0),
        new Vector3(50, -50, 0),
        new Vector3(0, 50, 0)
    );
    
    void Start() {
        points = new Vector3[amount];
        tris.Add(bigtri);
        GeneratePoints();
    }

    void OnDrawGizmos () {
        // draw points 
        foreach (Vector3 point in points) {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(point, .25f);
        }
        // draw bigtri
            // foreach (Vector3 vert in bigtri) {}
            // foreach does not work yet on 'Triangle', IEnumerable and IEnumerator need to be implemented
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(bigtri.a, 1f);
        Gizmos.DrawSphere(bigtri.b, 1f);
        Gizmos.DrawSphere(bigtri.c, 1f);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(bigtri.a, bigtri.b);
        Gizmos.DrawLine(bigtri.b, bigtri.c);
        Gizmos.DrawLine(bigtri.c, bigtri.a);
    }

    public void GeneratePoints() {
        for (int i = 0; i < amount; i++) {
            points[i] = new Vector3(Random.value * 10, Random.value * 10, 0.0f);
        }
        Triangulation();
    }

    void Triangulation () {
        int i = 1;
        foreach (Vector3 point in points) {
            Triangle currentTri = checkTris(point);
            Debug.Log(currentTri);
            // new tris
            Triangle a = new Triangle (
                currentTri.a,
                currentTri.b,
                point
            );
            Triangle b = new Triangle (
                currentTri.b,
                currentTri.c,
                point
            );
            Triangle c = new Triangle (
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

    Triangle checkTris(Vector3 p) {
        // check with barycentric coordinates
        for (int i = 0; i < tris.Count; i++) {
            Vector3 a = tris[i].a;
            Vector3 b = tris[i].b;
            Vector3 c = tris[i].c;

            double alpha = ((c.x - a.x) * (p.y - a.y) - (c.y - a.y) * (p.x - a.x)) 
                           / ((b.y - a.y) * (c.x - a.x) - (b.x - a.x) * (c.y - a.y));

            double beta = (p.y - a.y - alpha * (b.y - a.y)) / (c.y - a.y);
            if (alpha >= 0 && beta >= 0 && (alpha + beta) <= 1) {
                Debug.Log("Passed ✔");
                return tris[i];
            }
        }
        Debug.Log($"Point {p} did not return a triangle");
        return null;
    }
}
