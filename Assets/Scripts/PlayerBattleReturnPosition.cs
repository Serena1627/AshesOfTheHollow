using UnityEngine;

public class PlayerBattleReturnPosition : MonoBehaviour
{
    private void Start()
    {
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

        Debug.Log("Player returned to overworld battle position: " + returnPosition);
    }
}