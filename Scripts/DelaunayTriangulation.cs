using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using static Delaunay.TriangleTypes;

public class DelaunayTriangulation : MonoBehaviour {

    public Renderer render;

    public int amount = 3;
    Vector3[] points;
    int index = 0;
    float time = 2;
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
        Debug.Log($"bigtri counterclockwise: {bigtri.IsCounterClockwise()}");
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
        // Triangulation();
    }

    void Initialize() {
        AddSingleTri(bigtri, points[0]);
        points = points.Skip(1).ToArray();
    }

    void Update() { // previously Triangulation
        Timer();

        if (time <= 0 && Input.GetKey(KeyCode.Space) && index < points.Length) {
            AddVertex(points[index]);
            index++;
            time = 2;
            return;
        }
        // CleanUp();
        //foreach (Vector3 point in points) {
        //    AddVertex(point);
        //}
    }

    void Timer() {
        if (time > 0) {
            time -= Time.deltaTime;
        }
    }

    void CleanUp() {
        List<Triangle> badtriangles = new List<Triangle>();

        foreach (Triangle tri in tris) {

            (bool a_bad, bool b_bad, bool c_bad) =
                CommonPoints(tri, bigtri);

            if (a_bad || b_bad || c_bad) {
                badtriangles.Add(tri);
            }
        }

        foreach (Triangle tri in badtriangles) {
            tris.Remove(tri);
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
            Debug.Log("1 bad triangle");
            AddSingleTri(solo, p);
            return;
        }

        List<Edge> polygon = new();

        for (int i = 0; i < badTriangles.Count; i++) {
            Triangle tri = badTriangles[i];
            bool ab_bad = false;
            bool bc_bad = false;
            bool ca_bad = false;

            for (int j = 0; j < badTriangles.Count; j++) {
                if (i == j) continue;
                Triangle tri_2 = badTriangles[j];
                (ab_bad, bc_bad, ca_bad) = CommonEdges(tri, tri_2);
            }
            // don't keep multiples, edges can be reversed but they're still considered the same!
            if (!ab_bad && !polygon.Contains(tri.edge_ab)) polygon.Add(tri.edge_ab);
            if (!bc_bad && !polygon.Contains(tri.edge_bc)) polygon.Add(tri.edge_bc);
            if (!ca_bad && !polygon.Contains(tri.edge_ca)) polygon.Add(tri.edge_ca);
        }

        foreach (Triangle tri in badTriangles) {
            // Triangles can be same, even though their points are differently oriented
            Debug.LogWarning(tris.Remove(tri));
        }

        foreach (Edge edge in polygon) {
            Triangle newTri = new(edge.a, edge.b, p);

            if (!newTri.IsCounterClockwise()) {
                newTri = new(edge.a, p, edge.b);
            }
            tris.Add(newTri);
        }
        return;
    }

    (bool, bool, bool) CommonEdges(Triangle fst, Triangle snd) {
        // does not cover edge case of edges being the same apart from being reversed
        if (fst == snd) {
            return (true, true, true);
        }
        Edge ab = fst.edge_ab;
        Edge bc = fst.edge_bc;
        Edge ca = fst.edge_ca;

        bool ab_bad = false;
        bool bc_bad = false;
        bool ca_bad = false;

        Edge ab_2 = snd.edge_ab;
        Edge bc_2 = snd.edge_bc;
        Edge ca_2 = snd.edge_ca;

        if (ab.Equals(ab_2) || ab.Equals(bc_2) || ab.Equals(ca_2)) {
            ab_bad = true;
        }
        if (bc.Equals(ab_2) || bc.Equals(bc_2) || bc.Equals(ca_2)) {
            bc_bad = true;
        }
        if (ca.Equals(ab_2) || ca.Equals(bc_2) || ca.Equals(ca_2)) {
            ca_bad = true;
        }
        return (ab_bad, bc_bad, ca_bad);
    }

    void AddSingleTri(Triangle currentTri, Vector3 point) {
        Triangle a = new Triangle(
            currentTri.a,
            currentTri.b,
            point
        );
        if (!a.IsCounterClockwise()) {
            a = new(a.a, point, a.b);
        }

        Triangle b = new Triangle(
            currentTri.b,
            currentTri.c,
            point
        );
        if (!b.IsCounterClockwise()) {
            b = new(b.a, point, b.b);
        }

        Triangle c = new Triangle(
            currentTri.c,
            currentTri.a,
            point
        );
        if (!c.IsCounterClockwise()) {
            c = new(c.a, point, c.b);
        }

        tris.Add(a);
        tris.Add(b);
        tris.Add(c);

        tris.Remove(currentTri);
    }

    (bool, bool, bool) CommonPoints(Triangle fst, Triangle snd) {
        bool a_same = false;
        bool b_same = false;
        bool c_same = false;

        if (fst.a == snd.a || fst.a == snd.b || fst.a == snd.c) {
            a_same = true;
        }
        if (fst.b == snd.a || fst.b == snd.b || fst.b == snd.c) {
            b_same = true;
        }
        if (fst.c == snd.a || fst.c == snd.b || fst.c == snd.c) {
            c_same = true;
        }
        return (a_same, b_same, c_same);
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