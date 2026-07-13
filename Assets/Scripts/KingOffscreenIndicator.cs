using UnityEngine;

[DefaultExecutionOrder(1000)]
public class KingOffscreenIndicator : MonoBehaviour
{
    [SerializeField] private Transform king;
    [SerializeField] private SpriteRenderer kingRenderer;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private UIHandler uiHandler;

    private void Awake()
    {
        if (uiHandler == null)
        {
            uiHandler = GetComponent<UIHandler>();
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (kingRenderer == null && king != null)
        {
            kingRenderer = king.GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void OnDisable()
    {
        uiHandler?.HideKingIndicator();
    }

    private void LateUpdate()
    {
        if (king == null || targetCamera == null || uiHandler == null)
        {
            uiHandler?.HideKingIndicator();
            return;
        }

        Bounds bounds = kingRenderer != null
            ? kingRenderer.bounds
            : new Bounds(king.position, Vector3.zero);

        Vector3 centerViewportPosition = targetCamera.WorldToViewportPoint(bounds.center);
        Vector3 topViewportPosition = targetCamera.WorldToViewportPoint(
            new Vector3(bounds.center.x, bounds.max.y, bounds.center.z));
        Vector3 bottomViewportPosition = targetCamera.WorldToViewportPoint(
            new Vector3(bounds.center.x, bounds.min.y, bounds.center.z));

        if (centerViewportPosition.z <= 0f)
        {
            uiHandler.HideKingIndicator();
            return;
        }

        if (bottomViewportPosition.y > 1f)
        {
            uiHandler.ShowKingIndicator(KingIndicatorDirection.Above, centerViewportPosition.x);
        }
        else if (topViewportPosition.y < 0f)
        {
            uiHandler.ShowKingIndicator(KingIndicatorDirection.Below, centerViewportPosition.x);
        }
        else
        {
            uiHandler.HideKingIndicator();
        }
    }
}
