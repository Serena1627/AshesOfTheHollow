using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyChasePlayer2D : MonoBehaviour
{
    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";

    [Header("Chase Settings")]
    [SerializeField] private float detectionRadius = 4f;
    [SerializeField] private float chaseSpeed = 2.5f;
    [SerializeField] private float stopDistance = 0.35f;

    [Header("Tilemap Collision")]
    [SerializeField] private Tilemap blockedObjectTilemap;
    [SerializeField] private float collisionCheckRadius = 0.18f;

    [Header("Behavior")]
    [SerializeField] private bool returnToStartWhenPlayerLeaves = false;
    [SerializeField] private float returnSpeed = 1.5f;
    [SerializeField] private float giveUpDistance = 7f;

    private Rigidbody2D rb;
    private Transform playerTransform;
    private Vector2 startPosition;
    private bool chasing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        startPosition = rb.position;
    }

    private void Start()
    {
        FindPlayerByTag();
    }

    private void FixedUpdate()
    {
        if (playerTransform == null)
        {
            FindPlayerByTag();

            if (playerTransform == null)
                return;
        }

        float distanceToPlayer = Vector2.Distance(rb.position, playerTransform.position);

        if (!chasing && distanceToPlayer <= detectionRadius)
        {
            chasing = true;
        }

        if (chasing)
        {
            if (distanceToPlayer > giveUpDistance)
            {
                chasing = false;
                return;
            }

            ChasePlayer(distanceToPlayer);
        }
        else if (returnToStartWhenPlayerLeaves)
        {
            ReturnToStart();
        }
    }

    private void FindPlayerByTag()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogWarning(
                "EnemyChasePlayer2D: No GameObject found with tag '" + playerTag + "'."
            );
        }
    }

    private void ChasePlayer(float distanceToPlayer)
    {
        if (distanceToPlayer <= stopDistance)
            return;

        Vector2 direction = ((Vector2)playerTransform.position - rb.position).normalized;
        Vector2 movement = direction * chaseSpeed * Time.fixedDeltaTime;
        Vector2 nextPosition = rb.position + movement;

        if (CanMoveTo(nextPosition))
        {
            rb.MovePosition(nextPosition);
            return;
        }

        Vector2 xOnlyPosition = rb.position + new Vector2(movement.x, 0f);
        if (CanMoveTo(xOnlyPosition))
        {
            rb.MovePosition(xOnlyPosition);
            return;
        }

        Vector2 yOnlyPosition = rb.position + new Vector2(0f, movement.y);
        if (CanMoveTo(yOnlyPosition))
        {
            rb.MovePosition(yOnlyPosition);
        }
    }

    private void ReturnToStart()
    {
        float distanceToStart = Vector2.Distance(rb.position, startPosition);

        if (distanceToStart <= 0.05f)
            return;

        Vector2 direction = (startPosition - rb.position).normalized;
        Vector2 movement = direction * returnSpeed * Time.fixedDeltaTime;
        Vector2 nextPosition = rb.position + movement;

        if (CanMoveTo(nextPosition))
            rb.MovePosition(nextPosition);
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

    public void SetBlockedObjectTilemap(Tilemap newBlockedObjectTilemap)
    {
        blockedObjectTilemap = newBlockedObjectTilemap;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, giveUpDistance);
    }
}