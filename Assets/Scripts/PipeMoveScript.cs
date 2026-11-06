using UnityEngine;

/// <summary>
/// Di chuyển chướng ngại vật sang trái khi ván chơi đã bắt đầu (IsGameStarted == true)
/// Giữ nguyên vị trí khi ở màn hình Bao quát trước khi bắt đầu.
/// </summary>
public class PipeMoveScript : MonoBehaviour
{
    public float moveSpeed = 5.5f;
    public float deadZone = -16.0f;
    private LogicScript logic;

    void Start()
    {
        logic = FindObjectOfType<LogicScript>();
    }

    void Update()
    {
        if (logic != null && (!logic.IsGameStarted || logic.IsGameEnded))
        {
            return;
        }

        transform.position += Vector3.left * moveSpeed * Time.deltaTime;

        if (transform.position.x < deadZone)
        {
            Destroy(gameObject);
        }
    }
}
