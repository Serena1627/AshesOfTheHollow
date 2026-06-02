using System.Collections.Generic;
using UnityEngine;

public class PaladinFollowPlayer : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Follow Settings")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float followDelay = 0.45f;
    [SerializeField] private float stopDistance = 0.15f;
    [SerializeField] private float recordSpacing = 0.05f;

    private readonly List<RecordedPosition> playerPath = new List<RecordedPosition>();
    private Vector3 lastRecordedPlayerPosition;

    private struct RecordedPosition
    {
        public Vector3 position;
        public float time;

        public RecordedPosition(Vector3 position, float time)
        {
            this.position = position;
            this.time = time;
        }
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (player == null)
        {
            Debug.LogWarning("PaladinFollowPlayer could not find an object tagged Player.");
            enabled = false;
            return;
        }

        lastRecordedPlayerPosition = player.position;
        playerPath.Add(new RecordedPosition(player.position, Time.time));
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            return;
        }

        RecordPlayerPath();
        FollowRecordedPath();
        RemoveOldPositions();
    }

    private void RecordPlayerPath()
    {
        if (Vector3.Distance(player.position, lastRecordedPlayerPosition) < recordSpacing)
        {
            return;
        }

        playerPath.Add(new RecordedPosition(player.position, Time.time));
        lastRecordedPlayerPosition = player.position;
    }

    private void FollowRecordedPath()
    {
        float targetTime = Time.time - followDelay;
        Vector3 targetPosition = transform.position;
        bool foundPosition = false;

        for (int i = playerPath.Count - 1; i >= 0; i--)
        {
            if (playerPath[i].time <= targetTime)
            {
                targetPosition = playerPath[i].position;
                foundPosition = true;
                break;
            }
        }

        if (!foundPosition)
        {
            return;
        }

        if (Vector3.Distance(transform.position, targetPosition) <= stopDistance)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }

    private void RemoveOldPositions()
    {
        float oldestNeededTime = Time.time - followDelay - 1f;

        while (playerPath.Count > 2 && playerPath[1].time < oldestNeededTime)
        {
            playerPath.RemoveAt(0);
        }
    }

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;

        if (player != null)
        {
            lastRecordedPlayerPosition = player.position;
            playerPath.Clear();
            playerPath.Add(new RecordedPosition(player.position, Time.time));
        }
    }
}