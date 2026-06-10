using System.Collections.Generic;
using UnityEngine;

public class PaladinFollowPlayer : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Follow Settings")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float followDelay = 0.35f;
    [SerializeField] private float stopDistance = 0.06f;
    [SerializeField] private float recordSpacing = 0.05f;

    [Header("Moving Formation")]
    [SerializeField] private float movingVerticalFollowDistance = 1.05f;

    [Tooltip("Distance behind the target while moving left/right.")]
    [SerializeField] private float movingSideHorizontalOffset = 1.15f;

    [Tooltip("Vertical offset while moving left/right. Negative means slightly below.")]
    [SerializeField] private float movingSideVerticalOffset = -0.25f;

    [Header("Idle Formation")]
    [SerializeField] private float idleVerticalFollowDistance = 0.85f;

    [Tooltip("Distance behind the target while idle and facing left/right.")]
    [SerializeField] private float idleSideHorizontalOffset = 0.95f;

    [Tooltip("Vertical offset while idle and facing left/right. Negative means slightly below.")]
    [SerializeField] private float idleSideVerticalOffset = -0.15f;

    [Header("Safety")]
    [SerializeField] private float minimumDistanceFromTarget = 0.55f;

    private readonly List<PathPoint> targetPath = new List<PathPoint>();

    private Vector3 lastRecordedTargetPosition;
    private Vector2 targetFacingDirection = Vector2.down;
    private float lastTargetMovedTime;

    public Vector2 MovementDirection { get; private set; }
    public bool IsMoving { get; private set; }

    private struct PathPoint
    {
        public Vector3 position;
        public Vector2 facingDirection;
        public float time;

        public PathPoint(Vector3 position, Vector2 facingDirection, float time)
        {
            this.position = position;
            this.facingDirection = facingDirection;
            this.time = time;
        }
    }

    private void Start()
    {
        FindPlayerIfNeeded();

        if (player == null)
        {
            Debug.LogWarning(name + " could not find an object tagged Player.");
            enabled = false;
            return;
        }

        ResetPath();
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            FindPlayerIfNeeded();

            if (player == null)
            {
                StopMoving();
                return;
            }

            ResetPath();
        }

        UpdateTargetFacingDirection();
        RecordTargetPath();
        FollowFormation();
        RemoveOldPathPoints();
    }

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;

        if (player != null)
        {
            ResetPath();
        }
    }

    private void FindPlayerIfNeeded()
    {
        if (player != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void ResetPath()
    {
        targetPath.Clear();

        lastRecordedTargetPosition = player.position;
        targetFacingDirection = Vector2.down;
        lastTargetMovedTime = Time.time;

        targetPath.Add(
            new PathPoint(
                player.position,
                targetFacingDirection,
                Time.time
            )
        );

        StopMoving();
    }

    private void UpdateTargetFacingDirection()
    {
        Vector2 delta = player.position - lastRecordedTargetPosition;

        if (delta.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        lastTargetMovedTime = Time.time;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            targetFacingDirection = delta.x > 0f
                ? Vector2.right
                : Vector2.left;
        }
        else
        {
            targetFacingDirection = delta.y > 0f
                ? Vector2.up
                : Vector2.down;
        }
    }

    private void RecordTargetPath()
    {
        if (Vector3.Distance(player.position, lastRecordedTargetPosition) < recordSpacing)
        {
            return;
        }

        targetPath.Add(
            new PathPoint(
                player.position,
                targetFacingDirection,
                Time.time
            )
        );

        lastRecordedTargetPosition = player.position;
    }

    private void FollowFormation()
    {
        bool targetRecentlyMoved = Time.time - lastTargetMovedTime < 0.15f;

        Vector3 baseTargetPosition;
        Vector2 baseFacingDirection;

        if (targetRecentlyMoved)
        {
            GetDelayedTargetPoint(out baseTargetPosition, out baseFacingDirection);
        }
        else
        {
            baseTargetPosition = player.position;
            baseFacingDirection = targetFacingDirection;
        }

        Vector3 desiredPosition = GetFormationPosition(
            baseTargetPosition,
            baseFacingDirection,
            targetRecentlyMoved
        );

        desiredPosition = KeepAwayFromCurrentTargetPosition(desiredPosition);

        float distanceToDesiredPosition =
            Vector3.Distance(transform.position, desiredPosition);

        if (distanceToDesiredPosition <= stopDistance)
        {
            StopMoving();
            return;
        }

        Vector3 previousPosition = transform.position;

        transform.position = Vector3.MoveTowards(
            transform.position,
            desiredPosition,
            moveSpeed * Time.deltaTime
        );

        Vector2 movement = transform.position - previousPosition;

        if (movement.sqrMagnitude > 0.0001f)
        {
            MovementDirection = movement.normalized;
            IsMoving = true;
        }
        else
        {
            StopMoving();
        }
    }

    private void GetDelayedTargetPoint(
        out Vector3 delayedPosition,
        out Vector2 delayedFacingDirection
    )
    {
        float delayedTime = Time.time - followDelay;

        delayedPosition = player.position;
        delayedFacingDirection = targetFacingDirection;

        for (int i = targetPath.Count - 1; i >= 0; i--)
        {
            if (targetPath[i].time <= delayedTime)
            {
                delayedPosition = targetPath[i].position;
                delayedFacingDirection = targetPath[i].facingDirection;
                return;
            }
        }
    }

    private Vector3 GetFormationPosition(
        Vector3 baseTargetPosition,
        Vector2 facingDirection,
        bool useMovingSpacing
    )
    {
        float verticalDistance = useMovingSpacing
            ? movingVerticalFollowDistance
            : idleVerticalFollowDistance;

        float sideHorizontalOffset = useMovingSpacing
            ? movingSideHorizontalOffset
            : idleSideHorizontalOffset;

        float sideVerticalOffset = useMovingSpacing
            ? movingSideVerticalOffset
            : idleSideVerticalOffset;

        if (facingDirection == Vector2.right)
        {
            return baseTargetPosition + new Vector3(
                -sideHorizontalOffset,
                sideVerticalOffset,
                0f
            );
        }

        if (facingDirection == Vector2.left)
        {
            return baseTargetPosition + new Vector3(
                sideHorizontalOffset,
                sideVerticalOffset,
                0f
            );
        }

        if (facingDirection == Vector2.up)
        {
            return baseTargetPosition + Vector3.down * verticalDistance;
        }

        return baseTargetPosition + Vector3.up * verticalDistance;
    }

    private Vector3 KeepAwayFromCurrentTargetPosition(Vector3 desiredPosition)
    {
        Vector2 fromTarget = desiredPosition - player.position;

        if (fromTarget.sqrMagnitude < minimumDistanceFromTarget * minimumDistanceFromTarget)
        {
            if (fromTarget.sqrMagnitude <= 0.0001f)
            {
                fromTarget = Vector2.down;
            }

            fromTarget.Normalize();

            desiredPosition = player.position +
                (Vector3)(fromTarget * minimumDistanceFromTarget);
        }

        return desiredPosition;
    }

    private void RemoveOldPathPoints()
    {
        float oldestNeededTime = Time.time - followDelay - 1f;

        while (targetPath.Count > 2 && targetPath[1].time < oldestNeededTime)
        {
            targetPath.RemoveAt(0);
        }
    }

    private void StopMoving()
    {
        MovementDirection = Vector2.zero;
        IsMoving = false;
    }
}