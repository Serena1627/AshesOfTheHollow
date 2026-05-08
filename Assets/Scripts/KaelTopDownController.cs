using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class KaelTopDownController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

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

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private Vector2 input;
    private Vector2 lastDirection = Vector2.down;

    private float animationTimer;
    private int frameIndex;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        ReadInput();
        AnimateSprite();
    }

    private void FixedUpdate()
    {
        Vector2 movement = input.normalized * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);
    }

    private void ReadInput()
    {
        input = Vector2.zero;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            input.x -= 1;

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            input.x += 1;

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            input.y += 1;

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            input.y -= 1;

        if (input.sqrMagnitude > 1f)
            input.Normalize();

        if (input.sqrMagnitude > 0.01f)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                lastDirection = input.x > 0 ? Vector2.right : Vector2.left;
            else
                lastDirection = input.y > 0 ? Vector2.up : Vector2.down;
        }
    }

    private void AnimateSprite()
    {
        bool isMoving = input.sqrMagnitude > 0.01f;

        if (!isMoving)
        {
            frameIndex = 0;
            animationTimer = 0f;
            spriteRenderer.sprite = GetIdleSprite();
            return;
        }

        Sprite[] currentWalkSprites = GetWalkSprites();

        if (currentWalkSprites == null || currentWalkSprites.Length == 0)
        {
            spriteRenderer.sprite = GetIdleSprite();
            return;
        }

        animationTimer += Time.deltaTime;

        if (animationTimer >= 1f / animationFPS)
        {
            animationTimer = 0f;
            frameIndex = (frameIndex + 1) % currentWalkSprites.Length;
        }

        spriteRenderer.sprite = currentWalkSprites[frameIndex];
    }

    private Sprite GetIdleSprite()
    {
        if (lastDirection == Vector2.up)
            return backIdle;

        if (lastDirection == Vector2.left)
            return leftIdle;

        if (lastDirection == Vector2.right)
            return rightIdle;

        return frontIdle;
    }

    private Sprite[] GetWalkSprites()
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