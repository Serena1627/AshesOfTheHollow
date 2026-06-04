using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PaladinFollowerAnimator : MonoBehaviour
{
    [Header("Movement Reference")]
    [SerializeField] private PaladinFollowPlayer followMovement;

    [Header("Idle Sprites")]
    [SerializeField] private Sprite frontIdle;
    [SerializeField] private Sprite backIdle;
    [SerializeField] private Sprite leftIdle;
    [SerializeField] private Sprite rightIdle;

    [Header("Walk Sprites")]
    [SerializeField] private Sprite[] frontWalk;
    [SerializeField] private Sprite[] backWalk;
    [SerializeField] private Sprite[] leftWalk;
    [SerializeField] private Sprite[] rightWalk;

    [Header("Animation")]
    [SerializeField] private float animationFPS = 8f;

    private SpriteRenderer spriteRenderer;
    private Vector2 lastDirection = Vector2.down;
    private float animationTimer;
    private int frameIndex;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (followMovement == null)
        {
            followMovement = GetComponent<PaladinFollowPlayer>();
        }
    }

    private void Update()
    {
        if (followMovement == null)
        {
            spriteRenderer.sprite = GetIdleSprite();
            return;
        }

        if (followMovement.IsMoving)
        {
            UpdateFacingDirection(followMovement.MovementDirection);
            AnimateWalking();
        }
        else
        {
            ShowIdleSprite();
        }
    }

    private void UpdateFacingDirection(Vector2 movementDirection)
    {
        if (movementDirection.sqrMagnitude <= 0.001f)
        {
            return;
        }

        if (Mathf.Abs(movementDirection.x) > Mathf.Abs(movementDirection.y))
        {
            lastDirection = movementDirection.x > 0f
                ? Vector2.right
                : Vector2.left;
        }
        else
        {
            lastDirection = movementDirection.y > 0f
                ? Vector2.up
                : Vector2.down;
        }
    }

    private void AnimateWalking()
    {
        Sprite[] walkSprites = GetWalkSprites();

        if (walkSprites == null || walkSprites.Length == 0)
        {
            spriteRenderer.sprite = GetIdleSprite();
            return;
        }

        animationTimer += Time.deltaTime;

        float safeFPS = Mathf.Max(1f, animationFPS);
        float secondsPerFrame = 1f / safeFPS;

        if (animationTimer >= secondsPerFrame)
        {
            animationTimer = 0f;
            frameIndex = (frameIndex + 1) % walkSprites.Length;
        }

        if (frameIndex >= walkSprites.Length)
        {
            frameIndex = 0;
        }

        spriteRenderer.sprite = walkSprites[frameIndex];
    }

    private void ShowIdleSprite()
    {
        frameIndex = 0;
        animationTimer = 0f;
        spriteRenderer.sprite = GetIdleSprite();
    }

    private Sprite GetIdleSprite()
    {
        if (lastDirection == Vector2.up)
        {
            return backIdle;
        }

        if (lastDirection == Vector2.left)
        {
            return leftIdle;
        }

        if (lastDirection == Vector2.right)
        {
            return rightIdle;
        }

        return frontIdle;
    }

    private Sprite[] GetWalkSprites()
    {
        if (lastDirection == Vector2.up)
        {
            return backWalk;
        }

        if (lastDirection == Vector2.left)
        {
            return leftWalk;
        }

        if (lastDirection == Vector2.right)
        {
            return rightWalk;
        }

        return frontWalk;
    }
}