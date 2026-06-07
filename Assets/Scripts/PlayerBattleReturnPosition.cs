using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerBattleReturnPosition : MonoBehaviour
{
    private void Start()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (SceneEntryData.HasPendingEntryForScene(currentSceneName))
        {
            Debug.Log(
                "PlayerBattleReturnPosition skipped because SceneEntryData has an arrival point for " +
                currentSceneName
            );

            return;
        }

        if (!BattleData.TryGetReturnPosition(out Vector3 returnPosition))
        {
            return;
        }

        transform.position = returnPosition;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.position = returnPosition;
            rb.linearVelocity = Vector2.zero;
        }

        BattleData.ClearReturnPosition();

        Debug.Log(
            "Player returned to overworld battle position: " +
            returnPosition
        );
    }
}