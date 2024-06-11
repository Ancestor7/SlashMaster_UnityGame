using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineSpawner : MonoBehaviour
{
    public GameObject PlayerHud;
    public GameObject linePrefab;

    private RectTransform lineAreaRectTransform;
    private float lineAreaWidth, lineAreaHeight;

    private void Start()
    {
        lineAreaRectTransform = PlayerHud.transform.GetChild(0).GetComponent<RectTransform>();
        lineAreaWidth = lineAreaRectTransform.rect.width;
        lineAreaHeight = lineAreaRectTransform.rect.height;
        SpawnLineShape();
    }

    private Vector2 MapRectToScreen(Vector2 vector2)
    {
        Vector2 mappingVector = new Vector2(Screen.width * lineAreaRectTransform.anchorMin.x + lineAreaWidth / 2f, Screen.height * lineAreaRectTransform.anchorMin.y + lineAreaHeight / 2f);

        Vector2 newVector = new Vector2(vector2.x + mappingVector.x, vector2.y + mappingVector.y);

        return newVector;
    }

    private void SpawnLineShape()
    {
        GameObject line = Instantiate(linePrefab, Vector2.zero, Quaternion.identity);
        line.transform.SetParent(lineAreaRectTransform, false);

        float radius = Random.Range(0.9f, 1f);
        Vector2 startPoint = Random.insideUnitCircle.normalized * radius;
        startPoint = new Vector2(startPoint.x * lineAreaWidth / 2f, startPoint.y * lineAreaHeight / 2f);
        Vector2 endPoint = new Vector2(0 - startPoint.x, 0 - startPoint.y);

        line.transform.position = startPoint;

        Debug.Log($"Start point {MapRectToScreen(startPoint)}, End Point {MapRectToScreen(endPoint)}");

        RectTransform lineRectTransform = line.GetComponent<RectTransform>();
        lineRectTransform.anchoredPosition = startPoint;

        Vector2 direction = startPoint - endPoint;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        lineRectTransform.localRotation = Quaternion.Euler(0, 0, angle);

        float lineLength = Vector2.Distance(startPoint, endPoint);

        lineRectTransform.sizeDelta = new Vector2(lineLength, lineRectTransform.sizeDelta.y);

        //Debug.DrawLine(lineAreaRectTransform.TransformPoint(startPoint), lineAreaRectTransform.TransformPoint(endPoint), Color.red);
    }

    private void SpawnVshape()
    {

    }

    private void SpawnZshape()
    {

    }

    private void SpawnWshape()
    {

    }
}
