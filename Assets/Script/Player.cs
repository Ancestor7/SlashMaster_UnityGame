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

    private float minimumDistance = 100f, maxAngle = 22.5f;
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

    public void ResetPlayer()
    {
        trail = null;
        PlayerHud = null;
        touchPoints.Clear();
        trailCoroutine = null;
        damageLineSpawner = null;
        lineAreaRectTransform = null;
        lineAreaWidth = 0;
        lineAreaHeight = 0;
        damageLines.Clear();
        damageLineObjects.Clear();
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

        if (trailCoroutine != null)
        {
            StopCoroutine(trailCoroutine);
        }

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
        Vector2 lineDirection = new Vector2(damageLines[0].endPos.x - damageLines[0].startPos.x, damageLines[0].endPos.y - damageLines[0].startPos.y).normalized;
        Vector2 swipeDirection = new Vector2(endPosition.x - startPosition.x, endPosition.y - startPosition.y).normalized;
        //Debug.Log("Line direction: " + lineDirection + ", Swipe direction: " + swipeDirection);
        float angle = Vector2.Angle(swipeDirection, lineDirection);
        float lineLength = Vector3.Distance(startPosition, endPosition);
        //Debug.Log("Length: " + lineLength);
        if (angle <= maxAngle && lineLength >= minimumDistance)
        {
            float averageDistance = 0f, averageThreshold = 1500f;
            foreach (Vector2 point in touchPoints)
            {
                averageDistance += PointToLineDistance(point, damageLines[0].startPos, damageLines[0].endPos);
            }
            averageDistance /= touchPoints.Count;

            Debug.Log("Average Distance: " + averageDistance);

            if (averageDistance < averageThreshold)
            {
                Debug.Log("Success");
                AudioSource attacksound = transform.GetComponent<AudioSource>();
                attacksound.volume = PlayerPrefs.GetFloat("soundVolume")*0.5f;
                attacksound.Play();
                DestroyLine();
            }
            else
            {
                Debug.Log("Fail");
            }
        }
    }


    private static float PointToLineDistance(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        // line equation => 0 = -1 * y slope * x + constant
        float slope = (lineEnd.y - lineStart.y)/(lineEnd.x - lineStart.x);
        float constant = lineStart.y - slope * lineStart.x;

        // distance from point to line => d = Mathf.abs
        float distance = Mathf.Abs(slope * point.x + -1 * point.y + constant) / Mathf.Sqrt(slope * slope + 1);

        return distance;
        /*
        Vector2 pointVector = point - lineStart;
        Vector2 lineVector = lineEnd - lineStart;
        float t = Vector2.Dot(pointVector, lineVector) / lineVector.magnitude;
        //t = Mathf.Clamp01(t);
        Vector2 projection = lineStart + t * lineVector;
        return Vector2.Distance(point, projection);
        */
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
            Debug.Log("Line: Start:" + MapRectToScreen(startPoint) + " End:" + MapRectToScreen(endPoint));
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