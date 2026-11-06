using UnityEngine;

/// <summary>
/// Hộp tiếp tế Vũ khí (Weapon Power-up Pod):
/// Bay lơ lửng từ phải sang trái trong hành lang bay.
/// Khi phi cơ chạm vào sẽ nâng cấp tạm thời sang chủng loại vũ khí tương ứng trong 15 giây:
/// - Spread3Way: Đạn chùm 3 tia hình quạt
/// - RapidDual: Song tia siêu tốc
/// - PlasmaPiercing: Tia plasma xuyên thấu
/// </summary>
public class WeaponPickup : MonoBehaviour
{
    public BirdScript.WeaponType weaponType = BirdScript.WeaponType.Spread3Way;
    public float moveSpeed = 4.0f;
    public float buffDuration = 15.0f;
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
            // Đổi màu để nhận diện từng loại vũ khí
            switch (weaponType)
            {
                case BirdScript.WeaponType.Spread3Way:
                    sr.color = new Color(1f, 0.6f, 0.2f); // Cam (Đạn chùm)
                    break;
                case BirdScript.WeaponType.RapidDual:
                    sr.color = new Color(1f, 0.95f, 0.2f); // Vàng sáng (Gatling)
                    break;
                case BirdScript.WeaponType.PlasmaPiercing:
                    sr.color = new Color(0.2f, 0.85f, 1f); // Cyan (Plasma)
                    break;
            }
        }
    }

    void Update()
    {
        // Bay sang trái
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;

        // Bập bềnh sóng sin
        float newY = startY + Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

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
                bird.SetWeapon(weaponType, buffDuration);

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

