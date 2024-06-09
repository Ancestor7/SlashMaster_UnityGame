using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEditor.Progress;
using Random = UnityEngine.Random;

public class Player : Singleton<Player>
{
    // public static Player Instance { get; private set; }

    [SerializeField] private GameObject trailPrefab;
    [SerializeField] private GameObject linePrefab;

    public GameObject parent;
    public Camera mainCamera;
    public Light playerLight;

    private GameObject trail;
    private GameObject PlayerHud;
    private InputManager inputManager;
    private Vector2 startPosition, endPosition;
    private List<Vector2> touchPoints = new List<Vector2>();
    private Coroutine trailCoroutine, damageLineSpawner;
    private RectTransform lineAreaRectTransform;
    private float lineAreaWidth, lineAreaHeight;
    private List<DamageLine> damageLines;
    private List<GameObject> damageLineObjects;

    void Awake()
    {
        /*
        #if UNITY_EDITOR
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        #endif
        */
    }

    void Start()
    {
        inputManager = InputManager.Instance;
        DontDestroyOnLoad(gameObject);
        mainCamera = parent.transform.GetChild(0).gameObject.GetComponent<Camera>();
        inputManager.mainCamera = mainCamera;
        playerLight = parent.transform.GetChild(1).gameObject.GetComponent<Light>();
    }

    private void InitializeFightRect()
    {
        PlayerHud = DungeonController.Instance.PlayerHud;
        lineAreaRectTransform = PlayerHud.transform.GetChild(7).GetComponent<RectTransform>();
        lineAreaWidth = lineAreaRectTransform.rect.width;
        lineAreaHeight = lineAreaRectTransform.rect.height;
    }

    public void OnFightStart()
    {
        InitializeFightRect();

        inputManager.OnStartTouch += SwipeStart;
        inputManager.OnEndTouch += SwipeEnd;
        
        if (damageLineSpawner != null )
        {
            StopCoroutine(damageLineSpawner);
            damageLineSpawner = null;
            damageLines.Clear();
            for (int i = 0; i < damageLineObjects.Count; i++)
            {
                Destroy(damageLineObjects[i]);
            }
            damageLineObjects.Clear();
        }
        damageLines = new List<DamageLine>();
        damageLineObjects = new List<GameObject>();
        damageLineSpawner = StartCoroutine(SpawnDamageLines());
    }

    public void OnFightEnd()
    {
        if (damageLineSpawner != null)
        {
            StopCoroutine(damageLineSpawner);
            damageLineSpawner = null;
        }
        damageLines.Clear();
        for (int i = 0; i < damageLineObjects.Count; i++)
        {
            Destroy(damageLineObjects[i]);
        }
        damageLineObjects.Clear();

        inputManager.OnStartTouch -= SwipeStart;
        inputManager.OnEndTouch -= SwipeEnd;
    }

    private void SwipeStart(Vector2 position, float time)
    {
        if (trailCoroutine != null)
        {
            StopCoroutine(trailCoroutine);
            Destroy(trail);
        }

        startPosition = position;
        touchPoints.Clear();
        touchPoints.Add(position);

        trail = Instantiate(trailPrefab);
        trail.SetActive(true);
        trail.transform.position = position;

        trailCoroutine = StartCoroutine(Trail());
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
        if (trailCoroutine != null)
        {
            StopCoroutine(trailCoroutine);
            trailCoroutine = null;
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

    #region Line Detection Logic

    private void DetectSwipe()
    {
        float averageDistance = 0f, variance = 0f, varianceThreshold = 50000f;

        foreach (Vector2 point in touchPoints)
        {
            averageDistance += PointToLineDistance(point, damageLines[0].startPos, damageLines[0].endPos);
        }
        averageDistance /= touchPoints.Count;

        foreach (Vector2 point in touchPoints)
        {
            float distance = PointToLineDistance(point, damageLines[0].startPos, damageLines[0].endPos);
            variance += Mathf.Pow(distance - averageDistance, 2);
        }
        variance /= touchPoints.Count;

        Debug.Log("Average Distance: " + averageDistance);
        Debug.Log("Variance: " + variance);

        if (variance < varianceThreshold)
        {
            Debug.Log("Success");
            DestroyLine();
        }
        else
        {
            Debug.Log("Fail");
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

    #endregion

    #region Line Spawn

    private Vector2 MapRectToScreen(Vector2 vector2)
    {
        Vector2 mappingVector = new Vector2(Screen.width * lineAreaRectTransform.anchorMin.x + lineAreaWidth / 2f, Screen.height * lineAreaRectTransform.anchorMin.y + lineAreaHeight / 2f);

        Vector2 newVector = new Vector2(vector2.x + mappingVector.x, vector2.y + mappingVector.y);

        return newVector;
    }

    private IEnumerator SpawnDamageLines()
    {
        while (!Enemy.Instance.isDead)
        {
            if (damageLines.Count == 0)
            {
                var lineProbabilities = new List<(LineType lineType, int probability)>
            {
                (LineType.Line, 45),
                (LineType.V, 25),
                (LineType.Z, 20),
                (LineType.W, 10)
            };

                int randomValue = Random.Range(0, 100);
                int cumulativeProbability = 0;
                LineType chosenLineType = LineType.Line;
                foreach (var (lineType, probability) in lineProbabilities)
                {
                    cumulativeProbability += probability;
                    if (randomValue < cumulativeProbability)
                    {
                        chosenLineType = lineType;
                    }
                }

                SpawnLine(chosenLineType);
            }
            yield return null;
        }
    }

    private void SpawnLine(LineType lineType, float nextLineAngle = 0)
    {
        lineType = LineType.Line;
        if (lineType == LineType.Line)
        {
            GameObject damageLineObject = Instantiate(linePrefab, Vector2.zero, Quaternion.identity);
            damageLineObject.transform.SetParent(lineAreaRectTransform, false);

            float radius = Random.Range(0.9f, 1f);
            Vector2 startPoint = Random.insideUnitCircle.normalized * radius;
            startPoint = new Vector2(startPoint.x * lineAreaWidth / 2f, startPoint.y * lineAreaHeight / 2f);
            Vector2 endPoint = new Vector2(0 - startPoint.x, 0 - startPoint.y);
            damageLineObject.transform.position = startPoint;

            RectTransform lineRectTransform = damageLineObject.GetComponent<RectTransform>();
            lineRectTransform.anchoredPosition = startPoint;

            Vector2 direction = startPoint - endPoint;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            lineRectTransform.localRotation = Quaternion.Euler(0, 0, angle);

            float lineLength = Vector2.Distance(startPoint, endPoint);
            lineRectTransform.sizeDelta = new Vector2(lineLength, lineRectTransform.sizeDelta.y);

            DamageLine damageLine = new DamageLine(damageLineObject, MapRectToScreen(startPoint), MapRectToScreen(endPoint));

            damageLineObjects.Add(damageLineObject);
            damageLines.Add(damageLine);
        }
        if (lineType == LineType.V)
        {
            // 2 line

        }
        if (lineType == LineType.Z)
        {
            // 3 line
        }
        if (lineType == LineType.W)
        {
            // 4 line
        }

        /*

        Debug.Log("SpawnLineShape");

        GameObject line = Instantiate(linePrefab, Vector2.zero, Quaternion.identity);
        line.transform.SetParent(lineAreaRectTransform, false);

        float radius = Random.Range(0.9f, 1f);
        Vector2 startPoint = Random.insideUnitCircle.normalized * radius;
        startPoint = new Vector2(startPoint.x * lineAreaWidth / 2f, startPoint.y * lineAreaHeight / 2f);
        Vector2 endPoint = new Vector2(0 - startPoint.x, 0 - startPoint.y);

        line.transform.position = startPoint;

        Debug.Log($"Start point {startPoint}, End Point {endPoint}");
        Debug.Log($"Relative: Start point {MapRectToScreen(startPoint)}, End Point {MapRectToScreen(endPoint)}");

        RectTransform lineRectTransform = line.GetComponent<RectTransform>();
        lineRectTransform.anchoredPosition = startPoint;

        Vector2 direction = startPoint - endPoint;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        lineRectTransform.localRotation = Quaternion.Euler(0, 0, angle);

        float lineLength = Vector2.Distance(startPoint, endPoint);
        Debug.Log($"Line Length {lineLength}");

        lineRectTransform.sizeDelta = new Vector2(lineLength, lineRectTransform.sizeDelta.y);

        Debug.Log($"Line Start: {startPoint}, Line End: {endPoint}");
        Debug.DrawLine(lineAreaRectTransform.TransformPoint(startPoint), lineAreaRectTransform.TransformPoint(endPoint), Color.red);
        
        */
    }

    private void DestroyLine()
    {
        DungeonController.Instance.DamageEnemy();
        damageLines.Clear();
        for (int i = 0; i < damageLineObjects.Count; i++)
        {
            Destroy(damageLineObjects[i]);
        }
        damageLineObjects.Clear();
    }

    #endregion

}

public enum LineType
{
    Line,
    V,
    W,
    Z
}

public class DamageLine
{
    public GameObject lineObject;
    public Vector2 startPos;
    public Vector2 endPos;

    public DamageLine(GameObject lineObject, Vector2 startPos, Vector2 endPos)
    {
        this.lineObject = lineObject;
        this.startPos = startPos;
        this.endPos = endPos;
    }
}