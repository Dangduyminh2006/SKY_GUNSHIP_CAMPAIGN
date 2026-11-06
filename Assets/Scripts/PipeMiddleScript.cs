using UnityEngine;

/// <summary>
/// Phát hiện người chơi bay qua khe giữa 2 mỏm đá để ăn Ngôi sao vàng và cộng điểm
/// </summary>
public class PipeMiddleScript : MonoBehaviour
{
    private LogicScript logic;
    private bool scored = false;

    void Start()
    {
        if (logic == null)
        {
            GameObject logicObj = GameObject.FindGameObjectWithTag("Logic");
            if (logicObj != null)
            {
                logic = logicObj.GetComponent<LogicScript>();
            }
            if (logic == null)
            {
                logic = FindObjectOfType<LogicScript>();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (scored) return;

        if (collision.gameObject.layer == 3 || collision.gameObject.layer == 6 || collision.CompareTag("Player"))
        {
            BirdScript bird = collision.GetComponent<BirdScript>();
            if (bird == null)
            {
                bird = collision.GetComponentInParent<BirdScript>();
            }

            if (bird != null && bird.birdIsAlive && logic != null && logic.IsGameStarted && !logic.IsGameEnded)
            {
                scored = true;
                logic.addScore(1);

                // Ẩn biểu tượng Ngôi sao vàng khi đã thu thập
                Transform star = transform.Find("GoldStar");
                if (star != null)
                {
                    star.gameObject.SetActive(false);
                }
            }
        }
    }
}
