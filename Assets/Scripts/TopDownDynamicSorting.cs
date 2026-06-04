using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TopDownDynamicSorting : MonoBehaviour
{
    [Header("Sorting")]
    [SerializeField] private int sortingOffset = 0;
    [SerializeField] private int sortingPrecision = 100;

    [Header("Optional Foot Position")]
    [Tooltip("Use an empty child placed at the character's feet. Leave empty to use this object's position.")]
    [SerializeField] private Transform feetPoint;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        float yPosition = feetPoint != null
            ? feetPoint.position.y
            : transform.position.y;

        spriteRenderer.sortingOrder =
            sortingOffset - Mathf.RoundToInt(yPosition * sortingPrecision);
    }
}