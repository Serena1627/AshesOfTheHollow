using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TopDownDynamicSorting : MonoBehaviour
{
    [Header("Sorting")]
    [Tooltip("Large base value so positive Y positions do not collapse below 1.")]
    [SerializeField] private int baseSortingOrder = 5000;

    [Tooltip("Small manual adjustment. Higher draws in front.")]
    [SerializeField] private int sortingOffset = 0;

    [SerializeField] private int sortingPrecision = 100;

    [Header("Sorting Limits")]
    [SerializeField] private int minimumSortingOrder = 1;
    [SerializeField] private int maximumSortingOrder = 10000;

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

        int calculatedSortingOrder =
            baseSortingOrder
            + sortingOffset
            - Mathf.RoundToInt(yPosition * sortingPrecision);

        spriteRenderer.sortingOrder = Mathf.Clamp(
            calculatedSortingOrder,
            minimumSortingOrder,
            maximumSortingOrder
        );
    }
}