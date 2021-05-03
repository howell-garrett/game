using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurveLineRenderer : MonoBehaviour
{
    [ExecuteInEditMode]
    public Transform point1;
    public Transform point2;
    public Transform point3;
    public LineRenderer lineRenderer;
    public int vertexCount = 12;


    public static List<Vector3> GetCurvePoints(Vector3 start, Vector3 end, float height)
    {

        List<Vector3> pointList = new List<Vector3>();
        Vector3 midpoint = Vector3.Lerp(start, end, .5f);

        midpoint += new Vector3(0, height, 0);
        for (float ratio = 0f / 12; ratio < 1; ratio += 1.0f / 12)
        {
            var tangentLineVertex1 = Vector3.Lerp(start, midpoint, ratio);
            var tangentLineVertex2 = Vector3.Lerp(midpoint, end, ratio);
            var bezierPoint = Vector3.Lerp(tangentLineVertex1, tangentLineVertex2, ratio);
            pointList.Add(bezierPoint);
        }
        return pointList;
    }
   
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(point1.position, point2.position);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(point2.position, point3.position);
        
        for (float ratio = 0.5f / vertexCount; ratio < 1; ratio += 1.0f/ vertexCount)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                Vector3.Lerp(point1.position, point2.position, ratio),
                Vector3.Lerp(point2.position, point3.position, ratio));
        }
    }
}
