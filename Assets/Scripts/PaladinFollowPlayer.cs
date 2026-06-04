using System.Collections.Generic;
using UnityEngine;

public class PaladinFollowPlayer : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Follow Settings")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float followDelay = 0.45f;
    [SerializeField] private float minimumDistanceFromPlayer = 1.0f;
    [SerializeField] private float recordSpacing = 0.05f;

    private readonly List<PathPoint> playerPath = new List<PathPoint>();
    private Vector3 lastRecordedPlayerPosition;

    public Vector2 MovementDirection { get; private set; }
    public bool IsMoving { get; private set; }

    private struct PathPoint
    {
        public Vector3 position;
        public float time;

        public PathPoint(Vector3 position, float time)
        {
            this.position = position;
            this.time = time;
        }
    }

    private void Start()
    {
        FindPlayerIfNeeded();

        if (player == null)
        {
            Debug.LogWarning("Paladin follower could not find an object tagged Player.");
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

        RecordPlayerPath();
        FollowRecordedPath();
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
        playerPath.Clear();
        lastRecordedPlayerPosition = player.position;
        playerPath.Add(new PathPoint(player.position, Time.time));

        StopMoving();
    }

    private void RecordPlayerPath()
    {
        if (Vector3.Distance(player.position, lastRecordedPlayerPosition) < recordSpacing)
        {
            return;
        }

        playerPath.Add(new PathPoint(player.position, Time.time));
        lastRecordedPlayerPosition = player.position;
    }

    private void FollowRecordedPath()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Paladin has reached his allowed distance from Kael.
        // He must not continue into Kael's exact position.
        if (distanceToPlayer <= minimumDistanceFromPlayer)
        {
            StopMoving();
            return;
        }

        float delayedTime = Time.time - followDelay;
        Vector3 targetPosition = transform.position;
        bool targetFound = false;

        for (int i = playerPath.Count - 1; i >= 0; i--)
        {
            if (playerPath[i].time <= delayedTime)
            {
                targetPosition = playerPath[i].position;
                targetFound = true;
                break;
            }
        }

        if (!targetFound)
        {
            StopMoving();
            return;
        }

        Vector3 previousPosition = transform.position;

        Vector3 nextPosition = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        // Prevent the movement step from passing inside the protected space around Kael.
        float nextDistanceToPlayer = Vector2.Distance(nextPosition, player.position);

        if (nextDistanceToPlayer < minimumDistanceFromPlayer)
        {
            Vector2 awayFromPlayer =
                (Vector2)(transform.position - player.position);

            if (awayFromPlayer.sqrMagnitude <= 0.001f)
            {
                awayFromPlayer = Vector2.down;
            }

            awayFromPlayer.Normalize();

            nextPosition = player.position +
                (Vector3)(awayFromPlayer * minimumDistanceFromPlayer);
        }

        transform.position = nextPosition;

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

    private void RemoveOldPathPoints()
    {
        float oldestNeededTime = Time.time - followDelay - 1f;

        while (playerPath.Count > 2 && playerPath[1].time < oldestNeededTime)
        {
            playerPath.RemoveAt(0);
        }
    }

    private void StopMoving()
    {
        MovementDirection = Vector2.zero;
        IsMoving = false;
    }
}