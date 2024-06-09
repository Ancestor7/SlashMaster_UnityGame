using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeDetection : Singleton<SwipeDetection>
{
    [SerializeField] private float minimumDistance = .2f;
    [SerializeField] private float maximumTime = 1f;
    [SerializeField, Range(0f, 1f)] private float directionThreshold = .9f;
    [SerializeField] private GameObject trailPrefab;

    private GameObject trail;
    private InputManager inputManager;
    private Vector2 startPosition, endPosition;

    private List<Vector2> touchPoints = new List<Vector2>();
    private Coroutine coroutine;

    private void Awake()
    {
        inputManager = InputManager.Instance;
    }

    private void OnEnable()
    {
        inputManager.OnStartTouch += SwipeStart;
        inputManager.OnEndTouch += SwipeEnd;
    }

    private void OnDisable()
    {
        inputManager.OnStartTouch -= SwipeStart;
        inputManager.OnEndTouch -= SwipeEnd;
    }

    private void SwipeStart(Vector2 position, float time)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            Destroy(trail);
        }

        startPosition = position;
        touchPoints.Clear();
        touchPoints.Add(position);

        trail = Instantiate(trailPrefab);
        trail.SetActive(true);
        trail.transform.position = position; 

        coroutine = StartCoroutine(Trail());
    }

    private IEnumerator Trail()
    {
        while (true)
        {
            Vector3 currentPosition = inputManager.PrimaryPosition();
            trail.transform.position = currentPosition;
            touchPoints.Add(currentPosition);
            yield return null;
        }
    }

    private void SwipeEnd(Vector2 position, float time)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }

        if (trail != null)
        {
            trail.SetActive(false);
            Destroy(trail);
        }

        endPosition = position;
        touchPoints.Add(position);

        DetectSwipe();
    }

    private void DetectSwipe()
    {
        if (Vector3.Distance(startPosition,endPosition) >= minimumDistance)
        {
            //Debug.Log("Swipe Detected");
            Debug.DrawLine(startPosition, endPosition,Color.red,5f);
            Vector2 direction2D = endPosition - startPosition;
            Vector2 direction2Dnormal = direction2D.normalized;
            
            //SwipeDirection(direction2Dnormal);

            //DetectShape(touchPoints);
        }
    }

    private void SwipeDirection(Vector2 direction)
    {
        if (Vector2.Dot(Vector2.up, direction) > directionThreshold)
        {
            Debug.Log("Swipe Up");
        }
        if (Vector2.Dot(Vector2.down, direction) > directionThreshold)
        {
            Debug.Log("Swipe Down");
        }
        if (Vector2.Dot(Vector2.left, direction) > directionThreshold)
        {
            Debug.Log("Swipe Left");
        }
        if (Vector2.Dot(Vector2.right, direction) > directionThreshold)
        {
            Debug.Log("Swipe Right");
        }
    }

    private void DetectShape(List<Vector2> points)
    {
        if (IsLine(points))
        {
            Debug.Log("Is Line");
        }
    }

    private static float PointToLineDistance(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        Vector2 lineToPoint = point - lineStart;
        Vector2 line = lineEnd - lineStart;
        float t = Vector2.Dot(lineToPoint, line) / line.sqrMagnitude;
        t = Mathf.Clamp01(t);
        Vector2 projection = lineStart + t * line;
        return Vector2.Distance(point, projection);
    }

    private bool IsLine(List<Vector2> points)
    {
        float averageDistance = 0f, variance = 0f, varianceThreshold = 0.01f;

        foreach (Vector2 point in points)
        {
            averageDistance += PointToLineDistance(point,startPosition,endPosition);
        }
        averageDistance /= points.Count;

        foreach (Vector2 point in points)
        {
            float distance = PointToLineDistance(point, startPosition, endPosition);
            variance += Mathf.Pow(distance - averageDistance,2);
        }
        variance /= points.Count;

        Debug.Log("Average Distance: " + averageDistance);
        Debug.Log("Variance: " + variance);

        return variance < varianceThreshold;
    }

    private bool IsCircle(List<Vector2> points)
    {

        return false;
    }
}
