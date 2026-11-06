using UnityEngine;

/// <summary>
/// Đạn do Boss bắn ra: Bay theo hướng truyền vào và kích hoạt Game Over khi trúng người chơi
/// </summary>
public class BossBulletScript : MonoBehaviour
{
    public float speed = 12.0f;
    private Vector2 moveDirection = Vector2.left;

    void OnEnable()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite == null)
        {
            sr.sprite = LogicScript.GetOrLoadSprite("Assets/Sprites/Weapons/laserRed.png");
        }
    }

    public void SetDirection(Vector2 dir)
    {
        moveDirection = dir.normalized;
    }

    void Update()
    {
        transform.position += (Vector3)moveDirection * speed * Time.deltaTime;

        if (transform.position.x < -24.0f || Mathf.Abs(transform.position.y) > 14.0f)
        {
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.CompareTag("Player") || hitInfo.gameObject.layer == 3 || hitInfo.gameObject.layer == 6)
        {
            BirdScript bird = hitInfo.GetComponent<BirdScript>();
            if (bird != null)
            {
                bird.TakeDamage(1);
            }
            gameObject.SetActive(false);
        }
    }
}
