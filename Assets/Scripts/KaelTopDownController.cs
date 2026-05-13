using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class KaelTopDownController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("Tilemap Collision Rules")]
    [Tooltip("Tilemap containing blocked object tiles: trees, ruins, fences, rocks, cottage wreckage, barrels, walls, etc.")]
    [SerializeField] private Tilemap blockedObjectTilemap;

    [Tooltip("Optional tilemap containing end/transition tiles.")]
    [SerializeField] private Tilemap endPointTilemap;

    [Tooltip("Optional ground tilemap used only to prevent Kael from walking outside the painted level.")]
    [SerializeField] private Tilemap groundTilemap;

    [Tooltip("If true, Kael cannot leave the area covered by the Ground Tilemap.")]
    [SerializeField] private bool preventLeavingMapBounds = true;

    [Tooltip("How far from Kael's center to check for blocked cells. Increase if he clips into objects; decrease if he gets blocked too early.")]
    [SerializeField] private float collisionCheckRadius = 0.18f;

    [Header("Level Transition")]
    [SerializeField] private string nextSceneName;
    [SerializeField] private bool enemyMustBeDefeatedBeforeExit = true;
    private bool enemyDefeated = false;

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
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        ReadInput();
        AnimateSprite();
        CheckEndPoint();
    }

    private void FixedUpdate()
    {
        MoveWithTileCollision();
    }

    private void ReadInput()
    {
        input = Vector2.zero;
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1;

        if (input.sqrMagnitude > 1f) input.Normalize();

        if (input.sqrMagnitude > 0.01f)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                lastDirection = input.x > 0 ? Vector2.right : Vector2.left;
            else
                lastDirection = input.y > 0 ? Vector2.up : Vector2.down;
        }
    }

    private void MoveWithTileCollision()
    {
        if (input.sqrMagnitude <= 0.01f) return;

        Vector2 currentPosition = rb.position;
        Vector2 movement = input.normalized * moveSpeed * Time.fixedDeltaTime;
        Vector2 nextPosition = currentPosition + movement;

        if (CanMoveTo(nextPosition))
        {
            rb.MovePosition(nextPosition);
            return;
        }

        // Sliding lets Kael move along walls/trees instead of getting stuck on corners.
        Vector2 xOnly = currentPosition + new Vector2(movement.x, 0f);
        if (CanMoveTo(xOnly))
        {
            rb.MovePosition(xOnly);
            return;
        }

        Vector2 yOnly = currentPosition + new Vector2(0f, movement.y);
        if (CanMoveTo(yOnly))
        {
            rb.MovePosition(yOnly);
        }
    }

    private bool CanMoveTo(Vector2 worldPosition)
    {
        Vector2[] points =
        {
            worldPosition,
            worldPosition + Vector2.up * collisionCheckRadius,
            worldPosition + Vector2.down * collisionCheckRadius,
            worldPosition + Vector2.left * collisionCheckRadius,
            worldPosition + Vector2.right * collisionCheckRadius
        };

        foreach (Vector2 point in points)
        {
            if (IsBlocked(point)) return false;
            if (preventLeavingMapBounds && IsOutsideGroundMap(point)) return false;
        }

        return true;
    }

    private bool IsBlocked(Vector2 worldPosition)
    {
        if (blockedObjectTilemap == null) return false;
        Vector3Int cell = blockedObjectTilemap.WorldToCell(worldPosition);
        return blockedObjectTilemap.HasTile(cell);
    }

    private bool IsOutsideGroundMap(Vector2 worldPosition)
    {
        if (groundTilemap == null) return false;
        Vector3Int cell = groundTilemap.WorldToCell(worldPosition);
        return !groundTilemap.HasTile(cell);
    }

    private void CheckEndPoint()
    {
        if (endPointTilemap == null) return;
        Vector3Int cell = endPointTilemap.WorldToCell(transform.position);
        if (!endPointTilemap.HasTile(cell)) return;

        if (enemyMustBeDefeatedBeforeExit && !enemyDefeated)
        {
            Debug.Log("The path continues, but enemies remain.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            Debug.LogWarning("Next Scene Name is empty. Add a scene name in the Inspector.");
    }

    public void MarkEnemyDefeated()
    {
        enemyDefeated = true;
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
        if (lastDirection == Vector2.up) return backIdle;
        if (lastDirection == Vector2.left) return leftIdle;
        if (lastDirection == Vector2.right) return rightIdle;
        return frontIdle;
    }

    private Sprite[] GetWalkSprites()
    {
        if (lastDirection == Vector2.up) return backWalk;
        if (lastDirection == Vector2.left) return leftWalk;
        if (lastDirection == Vector2.right) return rightWalk;
        return frontWalk;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("ENTERED!");
            EnemyOverworld enemy = other.GetComponent<EnemyOverworld>();
            if (enemy != null)
                enemy.startFight();
        }
    }
}
