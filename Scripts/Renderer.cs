using System.Collections.Generic;
using UnityEngine;
using static Delaunay.DelaunayTriangulation;

public class Renderer : MonoBehaviour {

    public int circleSteps = 10;
    public LineRenderer lineRenderer;

    List<Triangle> triangles = new();
    Vector3[] points;

    public void SetTriangles(List<Triangle> tris, Vector3[] points) {
        triangles = tris;
        this.points = points;
    }

    void DrawCircle(Triangle tri) {
        lineRenderer.positionCount = circleSteps;
        CircumCircle circ = tri.GetCircumCircle();

        float radius = circ.radius;
        Vector3 center = circ.center;

        for (int i = 0; i < circleSteps; i++) {
            float circumferenceProgress = (float)i / circleSteps;
            float currentRadiant = circumferenceProgress * 2 * Mathf.PI;
            
            float xScaled = Mathf.Cos(currentRadiant);
            float yScaled = Mathf.Sin(currentRadiant);

            float x = xScaled * radius;
            float y = yScaled * radius;

            Vector3 currentPosition = new Vector3(x, y, 0);

            lineRenderer.SetPosition(i, currentPosition);
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        foreach(Triangle triangle in triangles) {
            Gizmos.DrawSphere(triangle.a, 1f);
            Gizmos.DrawSphere(triangle.b, 1f);
            Gizmos.DrawSphere(triangle.c, 1f);

            Gizmos.DrawLine(triangle.a, triangle.b);
            Gizmos.DrawLine(triangle.b, triangle.c);
            Gizmos.DrawLine(triangle.c, triangle.a);

            DrawCircle(triangle);
        }
    }
}
