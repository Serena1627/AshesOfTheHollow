using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class KaelTopDownController : MonoBehaviour
{
    [System.Serializable]
    public class SpriteSet
    {
        [Header("Idle Sprites")]
        public Sprite frontIdle;
        public Sprite backIdle;
        public Sprite leftIdle;
        public Sprite rightIdle;

        [Header("Walk Sprites")]
        public Sprite[] frontWalk;
        public Sprite[] backWalk;
        public Sprite[] leftWalk;
        public Sprite[] rightWalk;
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("Tilemap Logic")]
    [SerializeField] private Tilemap blockedObjectTilemap;
    [SerializeField] private Tilemap endPointTilemap;

    [Header("Collision Check")]
    [SerializeField] private float collisionCheckRadius = 0.18f;

    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName;
    [SerializeField] private bool enemyMustBeDefeatedBeforeExit = true;

    [Header("Sprite Sets")]
    [SerializeField] private bool useNoGearSpritesOnStart = false;
    [SerializeField] private SpriteSet noGearSprites;
    [SerializeField] private SpriteSet armoredSprites;

    [Header("Current Idle Sprites")]
    [SerializeField] private Sprite frontIdle;
    [SerializeField] private Sprite backIdle;
    [SerializeField] private Sprite leftIdle;
    [SerializeField] private Sprite rightIdle;

    [Header("Current Walk Sprites")]
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

    private bool enemyDefeated = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (useNoGearSpritesOnStart)
            SwitchToNoGearSprites();
    }

    private void Update()
    {
        ReadInput();
        AnimateSprite();
        CheckEndPoint();
    }

    private void FixedUpdate()
    {
        MoveWithBlockedTileCheck();
    }

    private void ReadInput()
    {
        input = Vector2.zero;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

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

    private void MoveWithBlockedTileCheck()
    {
        if (input.sqrMagnitude <= 0.01f)
            return;

        Vector2 currentPosition = rb.position;
        Vector2 movement = input.normalized * moveSpeed * Time.fixedDeltaTime;
        Vector2 nextPosition = currentPosition + movement;

        if (CanMoveTo(nextPosition))
        {
            rb.MovePosition(nextPosition);
            return;
        }

        Vector2 xOnlyPosition = currentPosition + new Vector2(movement.x, 0f);
        if (CanMoveTo(xOnlyPosition))
        {
            rb.MovePosition(xOnlyPosition);
            return;
        }

        Vector2 yOnlyPosition = currentPosition + new Vector2(0f, movement.y);
        if (CanMoveTo(yOnlyPosition))
        {
            rb.MovePosition(yOnlyPosition);
        }
    }

    private bool CanMoveTo(Vector2 worldPosition)
    {
        Vector2[] checkPoints =
        {
            worldPosition,
            worldPosition + Vector2.up * collisionCheckRadius,
            worldPosition + Vector2.down * collisionCheckRadius,
            worldPosition + Vector2.left * collisionCheckRadius,
            worldPosition + Vector2.right * collisionCheckRadius
        };

        foreach (Vector2 point in checkPoints)
        {
            if (IsBlocked(point))
                return false;
        }

        return true;
    }

    private bool IsBlocked(Vector2 worldPosition)
    {
        if (blockedObjectTilemap == null)
            return false;

        Vector3Int cellPosition = blockedObjectTilemap.WorldToCell(worldPosition);
        return blockedObjectTilemap.HasTile(cellPosition);
    }

    private void CheckEndPoint()
    {
        if (endPointTilemap == null)
            return;

        Vector3Int cellPosition = endPointTilemap.WorldToCell(transform.position);

        if (!endPointTilemap.HasTile(cellPosition))
            return;

        if (enemyMustBeDefeatedBeforeExit && !enemyDefeated)
        {
            Debug.Log("The path continues, but enemies remain.");
            return;
        }

        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogWarning("Next Scene Name is empty. Add a scene name in the Inspector.");
            return;
        }

        SceneManager.LoadScene(nextSceneName);
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

    public void ApplySpriteSet(SpriteSet spriteSet)
    {
        if (spriteSet == null)
        {
            Debug.LogWarning("Tried to apply a null SpriteSet.");
            return;
        }

        frontIdle = spriteSet.frontIdle;
        backIdle = spriteSet.backIdle;
        leftIdle = spriteSet.leftIdle;
        rightIdle = spriteSet.rightIdle;

        frontWalk = spriteSet.frontWalk;
        backWalk = spriteSet.backWalk;
        leftWalk = spriteSet.leftWalk;
        rightWalk = spriteSet.rightWalk;

        frameIndex = 0;
        animationTimer = 0f;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.sprite = GetIdleSprite();
    }

    public void SwitchToNoGearSprites()
    {
        ApplySpriteSet(noGearSprites);
    }

    public void SwitchToArmoredSprites()
    {
        ApplySpriteSet(armoredSprites);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("ENTERED!");

            EnemyOverworld enemy = other.GetComponent<EnemyOverworld>();

            if (enemy != null)
            {
                enemy.startFight();
            }
            else
            {
                Debug.LogWarning("Enemy tag found, but EnemyOverworld script is missing.");
            }
        }
    }
}