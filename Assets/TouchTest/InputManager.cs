using UnityEngine;

[DefaultExecutionOrder(-1)]
public class InputManager : Singleton<InputManager>
{

    public delegate void StartTouch(Vector2 position, float time);
    public event StartTouch OnStartTouch;
    public delegate void EndTouch(Vector2 position, float time);
    public event EndTouch OnEndTouch;

    [SerializeField] public Camera mainCamera ;


    private void Awake()
    {
        mainCamera = mainCamera != null ? mainCamera : Camera.main; //gameObject.GetComponent<Camera>();
    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchPosition = touch.position;
            Vector3 worldPosition = ScreenToWorld(mainCamera, touchPosition);

            if (touch.phase == TouchPhase.Began)
            {
                Debug.Log($"Start: touchpos={touchPosition}, worldPos={worldPosition}");
                StartTouchPrimary(touchPosition, touch.deltaTime);
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                Debug.Log($"End: touchpos={touchPosition}, worldPos={worldPosition}");
                EndTouchPrimary(touchPosition, touch.deltaTime);
            }
        }
    }

    private void StartTouchPrimary(Vector2 position, float time)
    {
        OnStartTouch?.Invoke(position, time);
    }

    private void EndTouchPrimary(Vector2 position, float time)
    {
        OnEndTouch?.Invoke(position, time);
    }

    public static Vector3 ScreenToWorld(Camera camera, Vector3 screenPosition, float zPosition = -1.5f)
    {
        Ray ray = camera.ScreenPointToRay(screenPosition);
        Plane plane = new Plane(Vector3.forward, 1.5f);

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 worldPosition = ray.GetPoint(enter);
            worldPosition.z = zPosition; // Adjust the Z position
            return worldPosition;
        }

        return Vector3.zero;
    }

    public Vector3 PrimaryPosition()
    {
        if (Input.touchCount > 0)
        {
            return ScreenToWorld(mainCamera, Input.GetTouch(0).position);
        }
        return Vector3.zero;
    }
}
