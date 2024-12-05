using UnityEngine;
using UnityEngine.Splines;

public class SplineAligner : MonoBehaviour
{
    public SplineContainer splineContainer;  // Reference to your SplineContainer 
    public SplineInstantiate splineInstantiate;
    public Vector3 upVector = Vector3.up;    // Reference up vector, typically Vector3.up

    void Start()
    {
        if (splineContainer == null || splineInstantiate == null) return;

        Spline spline = splineContainer.Spline; // Get the spline from the container

        int objectCount = splineInstantiate._instances.Count;
        // Iterate and instantiate objects along the spline
        for (int i = 0; i < objectCount; i++)
        {
            // Calculate normalized position along spline (from 0 to 1)
            float t = (float)i / (objectCount - 1);

            // Get position and tangent at t
            Vector3 tangent = spline.EvaluateTangent(t);

            // Calculate normal using cross product to ensure consistent orientation
            Vector3 binormal = Vector3.Cross(upVector, tangent).normalized;
            Vector3 normal = Vector3.Cross(tangent, binormal).normalized;


            // Align the object's rotation with the spline's direction
            Quaternion rotation = Quaternion.LookRotation(tangent, normal);

            splineInstantiate._instances[i].transform.rotation = Quaternion.Euler( Mathf.Rad2Deg * rotation.x, Mathf.Rad2Deg * rotation.y, Mathf.Rad2Deg * rotation.z * 2) ;


        }
    }
}
