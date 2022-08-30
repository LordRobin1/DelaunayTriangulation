using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelaunayTriangulation : MonoBehaviour
{
    public int amount = 10;
    Vector3[] points;
    Vector3[][] tris; 
    // amount of triangels by given points n and vertices in the convex-hull b: 2n - 2 - b
    // b will be the vertices of bigtri for now, meaning b = 3
    // n is currently 10
    // => 2 * 10 - 2 - 3 = 15 triangels

    Vector3[] bigtri = new Vector3[] {
        new Vector3(-50, -50, 0),
        new Vector3(50, -50, 0),
        new Vector3(0, 50, 0)
    };
    
    void Start() {
        points = new Vector3[amount];
        tris = new Vector3[15][];
        tris[0] = bigtri;
        GeneratePoints();
    }

    void OnDrawGizmos () {
        foreach (Vector3 point in points) {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(point, .25f);
        }
        foreach (Vector3 vert in bigtri) {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(vert, 1f);
        }
    }

    public void GeneratePoints() {
        for (int i = 0; i < amount; i++) {
            points[i] = new Vector3(Random.value * 10, Random.value * 10, 0.0f);
            Debug.Log($"vert: {points[i]}");
        }
        Debug.Log($"tris[0]: {tris[0][0]}");
        Triangulation();
    }
    void Triangulation () {
        foreach (Vector3 point in points) {
            Vector3[] currentTri = checkTris(point);
            Debug.Log($"currentTri: {currentTri[0]}, {currentTri[1]}, {currentTri[2]}");
        }
    }

    Vector3[] checkTris(Vector3 p) {
        // check with barycentric coordinates
        for (int i = 0; i < tris.Length; i++) {
            Vector3 a = tris[i][0];
            Vector3 b = tris[i][1];
            Vector3 c = tris[i][2];

            double alpha = ((c.x - a.x) * (p.y - a.y) - (c.y - a.y) * (p.x - a.x)) 
                           / ((b.y - a.y) * (c.x - a.x) - (b.x - a.x) * (c.y - a.y));

            double beta = (p.y - a.y - alpha * (b.y - a.y)) / (c.y - a.y);
            if (alpha >= 0 && beta >= 0 && (alpha + beta) <= 1) {
                Vector3[] hello = tris[i];
                Debug.Log("Passed ✔");
                return tris[i];
            }
        }
        Vector3[] empty = new Vector3[3];
        return empty;
    }
    
}
