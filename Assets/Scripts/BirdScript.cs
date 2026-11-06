using System.Collections;
using UnityEngine;

/// <summary>
/// Điều khiển nhân vật chính (Red Fighter Plane)
/// Hỗ trợ trạng thái Lơ lửng Toàn cảnh (Pre-Start Hover), vật lý bay Flappy,
/// hệ thống 3 Máu (HP), chớp nháy bất tử khi trúng đòn và bắn đạn tự động.
/// </summary>
public class BirdScript : MonoBehaviour
{
    [Header("Physics & Movement")]
    public Rigidbody2D myRigidbody;
    public float flapStrength = 8.5f;
    public float topBound = 6.2f;
    public float bottomBound = -4.8f;
    public bool birdIsAlive = true;

    [Header("Health & Defense")]
    public int maxHealth = 3;
    public int currentHealth = 3;
    public float invulnerableDuration = 1.2f;
    public bool isInvulnerable = false;
    private SpriteRenderer mySpriteRenderer;
    private Coroutine invulnerableCoroutine;

    [Header("Combat & Shooting")]
    public Transform firePoint;
    public string bulletPoolTag = "PlayerBullet";
    public float fireRate = 0.30f;
    public int bulletDamage = 2;
    public int weaponLevel = 1;
    public bool autoFire = true;
    private float fireTimer = 0f;

    public enum WeaponType
    {
        NormalLaser,
        Spread3Way,
        RapidDual,
        PlasmaPiercing
    }

    [Header("Weapon Upgrades")]
    public WeaponType currentWeapon = WeaponType.NormalLaser;
    public float weaponBuffTimer = 0f;

    [Header("Visual Juice")]
    public float tiltSmooth = 3.5f;
    public ParticleSystem flapParticles;

    [Header("References")]
    public LogicScript logic;

    private Vector3 initialHoverPosition;

    void Awake()
    {
        if (myRigidbody == null)
        {
            myRigidbody = GetComponent<Rigidbody2D>();
        }
        mySpriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        initialHoverPosition = transform.position;
        currentHealth = maxHealth;

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

        // Khóa trọng lực khi đang ở màn hình Bao quát trước khi bắt đầu
        if (myRigidbody != null && (logic == null || !logic.IsGameStarted))
        {
            myRigidbody.simulated = false;
        }
    }

    void Update()
    {
        if (!birdIsAlive) return;

        // Trạng thái 0: Lơ lửng trên màn hình Toàn cảnh trước khi nhấn Space/Click
        if (logic != null && !logic.IsGameStarted)
        {
            float hoverY = initialHoverPosition.y + Mathf.Sin(Time.time * 4.0f) * 0.22f;
            transform.position = new Vector3(initialHoverPosition.x, hoverY, initialHoverPosition.z);
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 4.0f) * 4.0f);

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                logic.StartGame();
                if (myRigidbody != null)
                {
                    myRigidbody.simulated = true;
                }
                FlapNow();
            }
            return;
        }

        // Trạng thái nghỉ giữa các đợt khi vừa chiến thắng Boss
        if (logic != null && logic.isWaveVictory)
        {
            if (myRigidbody != null && myRigidbody.simulated)
            {
                myRigidbody.linearVelocity = Vector2.zero;
                myRigidbody.simulated = false;
            }
            float hoverY = initialHoverPosition.y + Mathf.Sin(Time.time * 3.5f) * 0.22f;
            transform.position = Vector3.Lerp(transform.position, new Vector3(initialHoverPosition.x, hoverY, initialHoverPosition.z), Time.deltaTime * 5f);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, Time.deltaTime * 6f);
            return;
        }

        // 1. Nhận lệnh bay (Nhấn Space hoặc Chuột trái)
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            FlapNow();
        }

        // 2. Cập nhật góc nghiêng theo vận tốc rơi (Pitch Tilt)
        float currentYVel = GetVerticalVelocity();
        float targetAngle = Mathf.Clamp(currentYVel * tiltSmooth, -45f, 25f);
        transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);

        // 3. Kiểm tra ranh giới sống chết (Out-of-Bounds Check)
        if (transform.position.y > topBound)
        {
            transform.position = new Vector3(transform.position.x, topBound - 0.25f, transform.position.z);
            SetVerticalVelocity(-3.5f);
            TakeDamage(1);
        }
        else if (transform.position.y < bottomBound)
        {
            transform.position = new Vector3(transform.position.x, bottomBound + 0.25f, transform.position.z);
            SetVerticalVelocity(flapStrength * 0.75f);
            TakeDamage(1);
        }

        // 4. Đếm thời gian hiệu lực của buff vũ khí tạm thời (nếu có)
        if (weaponBuffTimer > 0f)
        {
            weaponBuffTimer -= Time.deltaTime;
            if (weaponBuffTimer <= 0f)
            {
                currentWeapon = WeaponType.NormalLaser;
            }
        }

        // 5. Cơ chế bắn đạn tự động / thủ công
        fireTimer += Time.deltaTime;
        if ((autoFire || Input.GetKey(KeyCode.F) || Input.GetMouseButton(1)) && fireTimer >= fireRate)
        {
            Shoot();
            fireTimer = 0f;
        }
    }

    public void ResumeFlight()
    {
        if (myRigidbody != null)
        {
            myRigidbody.simulated = true;
            SetVerticalVelocity(flapStrength * 0.6f);
        }
    }

    public void UpgradeMaxHealth(int amount = 1)
    {
        maxHealth += amount;
        currentHealth = maxHealth; // Hồi đầy máu như phần thưởng nâng cấp
    }

    public void UpgradeDamage(int amount = 1)
    {
        bulletDamage += amount;
        weaponLevel++;
    }

    public void UpgradeFireRate(float reduction = 0.04f)
    {
        fireRate = Mathf.Max(0.08f, fireRate - reduction);
    }

    public void SetWeapon(WeaponType newWeapon, float duration = 15f)
    {
        currentWeapon = newWeapon;
        weaponBuffTimer = duration;
    }

    public string GetWeaponDisplayName()
    {
        switch (currentWeapon)
        {
            case WeaponType.Spread3Way:
                return "Đạn Chùm 3 Tia";
            case WeaponType.RapidDual:
                return "Song Tia Siêu Tốc";
            case WeaponType.PlasmaPiercing:
                return "Plasma Xuyên Phá";
            default:
                return "Đạn Laser Vàng";
        }
    }

    private void FlapNow()
    {
        SetVerticalVelocity(flapStrength);

        if (flapParticles != null)
        {
            flapParticles.Play();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Flap");
        }
    }

    void Shoot()
    {
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.right * 0.9f;

        if (ObjectPooler.Instance == null) return;

        switch (currentWeapon)
        {
            case WeaponType.Spread3Way:
                // Bắn 3 tia tỏa hình quạt: 0 độ, +14 độ, -14 độ
                SpawnConfiguredBullet(spawnPos, Vector2.right, bulletDamage, false, new Color(1f, 0.65f, 0.1f));
                SpawnConfiguredBullet(spawnPos, Quaternion.Euler(0, 0, 14f) * Vector2.right, bulletDamage, false, new Color(1f, 0.65f, 0.1f));
                SpawnConfiguredBullet(spawnPos, Quaternion.Euler(0, 0, -14f) * Vector2.right, bulletDamage, false, new Color(1f, 0.65f, 0.1f));
                break;

            case WeaponType.RapidDual:
                // Bắn 2 luồng song song trên và dưới
                SpawnConfiguredBullet(spawnPos + Vector3.up * 0.16f, Vector2.right, bulletDamage, false, new Color(1f, 0.95f, 0.4f));
                SpawnConfiguredBullet(spawnPos + Vector3.down * 0.16f, Vector2.right, bulletDamage, false, new Color(1f, 0.95f, 0.4f));
                break;

            case WeaponType.PlasmaPiercing:
                // Bắn quả cầu năng lượng lớn xuyên qua nhiều tầng mục tiêu
                SpawnConfiguredBullet(spawnPos, Vector2.right, bulletDamage + 2, true, new Color(0.2f, 0.9f, 1f));
                break;

            default:
                // Đạn Laser chuẩn màu vàng
                SpawnConfiguredBullet(spawnPos, Vector2.right, bulletDamage, false, Color.yellow);
                break;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Shoot");
        }
    }

    private void SpawnConfiguredBullet(Vector3 pos, Vector2 dir, int dmg, bool piercing, Color color)
    {
        GameObject bulletObj = ObjectPooler.Instance.SpawnFromPool(bulletPoolTag, pos, Quaternion.identity);
        if (bulletObj != null)
        {
            BulletScript bullet = bulletObj.GetComponent<BulletScript>();
            if (bullet != null)
            {
                bullet.Setup(dmg, dir, piercing, color);
            }
        }
    }

    public void TakeDamage(int damage = 1)
    {
        if (!birdIsAlive || isInvulnerable) return;

        currentHealth -= damage;

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.22f, 0.28f);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Explosion");
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            KillBird();
        }
        else
        {
            if (invulnerableCoroutine != null)
            {
                StopCoroutine(invulnerableCoroutine);
            }
            invulnerableCoroutine = StartCoroutine(InvulnerabilityRoutine());
        }
    }

    public void Heal(int amount = 1)
    {
        if (!birdIsAlive) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Score");
        }

        StartCoroutine(HealFlashRoutine());
    }

    private IEnumerator HealFlashRoutine()
    {
        if (mySpriteRenderer != null)
        {
            Color orig = mySpriteRenderer.color;
            mySpriteRenderer.color = new Color(0.4f, 1f, 0.4f, 1f);
            yield return new WaitForSeconds(0.18f);
            mySpriteRenderer.color = isInvulnerable ? new Color(1f, 0.4f, 0.4f, 0.6f) : Color.white;
        }
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;
        float elapsed = 0f;
        float flashInterval = 0.1f;

        while (elapsed < invulnerableDuration)
        {
            if (mySpriteRenderer != null)
            {
                mySpriteRenderer.color = new Color(1f, 0.35f, 0.35f, 0.45f);
            }
            yield return new WaitForSeconds(flashInterval);

            if (mySpriteRenderer != null)
            {
                mySpriteRenderer.color = Color.white;
            }
            yield return new WaitForSeconds(flashInterval);

            elapsed += flashInterval * 2f;
        }

        if (mySpriteRenderer != null)
        {
            mySpriteRenderer.color = Color.white;
        }
        isInvulnerable = false;
        invulnerableCoroutine = null;
    }

    public void KillBird()
    {
        if (!birdIsAlive) return;

        birdIsAlive = false;
        currentHealth = 0;

        if (invulnerableCoroutine != null)
        {
            StopCoroutine(invulnerableCoroutine);
            invulnerableCoroutine = null;
        }

        if (mySpriteRenderer != null)
        {
            mySpriteRenderer.color = Color.white;
        }

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.35f, 0.45f);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Explosion");
        }

        if (logic != null)
        {
            logic.gameOver();
        }
    }

    private void SetVerticalVelocity(float yStrength)
    {
        if (myRigidbody == null) return;
#if UNITY_6000_0_OR_NEWER
        myRigidbody.linearVelocity = Vector2.up * yStrength;
#else
        myRigidbody.velocity = Vector2.up * yStrength;
#endif
    }

    private float GetVerticalVelocity()
    {
        if (myRigidbody == null) return 0f;
#if UNITY_6000_0_OR_NEWER
        return myRigidbody.linearVelocity.y;
#else
        return myRigidbody.velocity.y;
#endif
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (logic != null && !logic.IsGameStarted) return;
        SetVerticalVelocity(flapStrength * 0.75f);
        TakeDamage(1);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (logic != null && !logic.IsGameStarted) return;

        // Va chạm Trigger với Thân Boss (Layer 8) hoặc Đạn Boss (Layer 9)
        if (other.gameObject.layer == 8 || other.gameObject.layer == 9 || other.CompareTag("Boss"))
        {
            TakeDamage(1);
        }
    }
}
