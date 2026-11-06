using UnityEngine;

/// <summary>
/// Hộp tiếp tế máu (Health Supply Crate):
/// Xuất hiện lơ lửng trên hành lang bay (đặc biệt khi sắp gặp Boss), di chuyển sang trái.
/// Khi phi cơ người chơi chạm vào sẽ hồi 1 HP (tối đa 3 HP) và tự thu hồi/hủy.
/// </summary>
public class HealthPickup : MonoBehaviour
{
    public float moveSpeed = 4.2f;
    public int healAmount = 1;
    public float deadZone = -16.0f;
    public float bobbingSpeed = 4.0f;
    public float bobbingAmount = 0.25f;

    private float startY;

    void Start()
    {
        startY = transform.position.y;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite == null)
        {
            sr.sprite = LogicScript.GetOrLoadSprite("Assets/Sprites/UI/healthCrate.png");
        }
    }

    void Update()
    {
        // Di chuyển sang trái
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;

        // Hiệu ứng bập bềnh lượn sóng (Floating Sine Bob)
        float newY = startY + Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Tự tiêu hủy khi bay khỏi màn hình
        if (transform.position.x < deadZone)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.gameObject.layer == 3 || other.gameObject.layer == 6)
        {
            BirdScript bird = other.GetComponent<BirdScript>();
            if (bird != null && bird.birdIsAlive)
            {
                bird.Heal(healAmount);

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX("Score");
                }

                if (ObjectPooler.Instance != null)
                {
                    ObjectPooler.Instance.SpawnFromPool("HitEffect", transform.position, Quaternion.identity);
                }

                Destroy(gameObject);
            }
        }
    }
}

