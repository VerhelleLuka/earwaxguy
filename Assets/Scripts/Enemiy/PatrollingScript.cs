using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrollingScript : MonoBehaviour
{
    public List<Vector3> patrollingPoints = new List<Vector3>();
    public int currentPointIndex = 0;
    public int nextPointIndex = 1;
    private float DistanceMargin = 0.5f;

    public float movementSpeed = 3f;

    private void Start()
    {
        transform.position = patrollingPoints[0];
    }

    private void Update()
    {
        Debug.Log(nextPointIndex + currentPointIndex);
        transform.position += (patrollingPoints[nextPointIndex] - patrollingPoints[currentPointIndex]).normalized * Time.deltaTime * movementSpeed;

        if (Vector3.Distance(transform.position, patrollingPoints[nextPointIndex]) < DistanceMargin)
            SetNewTarget();

    }

    private void SetNewTarget()
    {
        ++currentPointIndex;

        if (currentPointIndex >= patrollingPoints.Count)
            currentPointIndex = 0;

        nextPointIndex = currentPointIndex + 1;

        if (nextPointIndex >= patrollingPoints.Count)
            nextPointIndex = 0;

    }
}
