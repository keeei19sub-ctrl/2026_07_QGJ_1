using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ShopColliderVisualizer : MonoBehaviour
{
    [SerializeField] private Color colliderColor = new Color(0f, 1f, 0f, 1f);

    private void OnDrawGizmos()
    {
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();

        if (boxCollider == null || !boxCollider.enabled)
        {
            return;
        }

        Gizmos.color = colliderColor;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(boxCollider.offset, boxCollider.size);
    }
}
