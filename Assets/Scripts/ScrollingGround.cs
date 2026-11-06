using UnityEngine;

/// <summary>
/// Hiệu ứng mặt đất cỏ xanh cuộn vô tận tạo cảm giác bay lượn khi ván chơi bắt đầu
/// </summary>
public class ScrollingGround : MonoBehaviour
{
    public float scrollSpeed = 5.5f;
    public float thresholdX = -16.0f;
    public float resetOffset = 32.0f;
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

        transform.position += Vector3.left * scrollSpeed * Time.deltaTime;
        if (transform.position.x <= thresholdX)
        {
            transform.position += new Vector3(resetOffset, 0f, 0f);
        }
    }
}
