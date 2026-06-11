using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemySpriteWalkAnimator : MonoBehaviour
{
    [Header("Direction Sprites")]
    [SerializeField] private Sprite[] frontWalk;
    [SerializeField] private Sprite[] backWalk;
    [SerializeField] private Sprite[] leftWalk;
    [SerializeField] private Sprite[] rightWalk;

    [Header("Animation Settings")]
    [SerializeField] private float animationFPS = 8f;
    [SerializeField] private float movementThreshold = 0.001f;

    private SpriteRenderer spriteRenderer;

    private Vector3 lastPosition;
    private Vector2 lastDirection = Vector2.down;

    private float animationTimer;
    private int frameIndex;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastPosition = transform.position;
    }

    private void LateUpdate()
    {
        Vector3 positionDelta = transform.position - lastPosition;
        Vector2 movement = new Vector2(positionDelta.x, positionDelta.y);

        bool isMoving = movement.sqrMagnitude > movementThreshold;

        if (isMoving)
        {
            UpdateDirection(movement);
            AnimateWalk();
        }
        else
        {
            ShowIdleFrame();
        }

        lastPosition = transform.position;
    }

    private void UpdateDirection(Vector2 movement)
    {
        if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
        {
            lastDirection = movement.x > 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            lastDirection = movement.y > 0 ? Vector2.up : Vector2.down;
        }
    }

    private void AnimateWalk()
    {
        Sprite[] currentSprites = GetCurrentWalkSprites();

        if (currentSprites == null || currentSprites.Length == 0)
            return;

        animationTimer += Time.deltaTime;

        if (animationTimer >= 1f / animationFPS)
        {
            animationTimer = 0f;
            frameIndex = (frameIndex + 1) % currentSprites.Length;
        }

        spriteRenderer.sprite = currentSprites[frameIndex];
    }

    private void ShowIdleFrame()
    {
        Sprite[] currentSprites = GetCurrentWalkSprites();

        if (currentSprites == null || currentSprites.Length == 0)
            return;

        frameIndex = 0;
        animationTimer = 0f;
        spriteRenderer.sprite = currentSprites[0];
    }

    private Sprite[] GetCurrentWalkSprites()
    {
        if (lastDirection == Vector2.up)
            return backWalk;

        if (lastDirection == Vector2.left)
            return leftWalk;

        if (lastDirection == Vector2.right)
            return rightWalk;

        return frontWalk;
    }
}