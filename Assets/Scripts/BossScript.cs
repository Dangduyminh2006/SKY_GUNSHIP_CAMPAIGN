using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Máy trạng thái hữu hạn FSM cho Chiến hạm Boss (Combat Gunship):
/// Standby (Hiển thị trên màn hình Toàn cảnh trước khi bắt đầu) -> Entering -> Idle -> RangedAttack / DashAttack -> Enraged -> Dead
/// </summary>
public class BossScript : MonoBehaviour
{
    public enum BossState
    {
        Standby,
        Entering,
        Idle,
        RangedAttack,
        DashAttack,
        Enraged,
        Dead
    }

    [Header("Health Settings")]
    public int maxHP = 50;
    public int currentHP;
    public Slider healthSlider;

    [Header("FSM State")]
    public BossState currentState = BossState.Standby;
    public bool isEnraged = false;

    [Header("Positions & Anchors")]
    public Vector3 panoramicStandbyPos = new Vector3(13.8f, 0.8f, 0f);
    public float offscreenWaitX = 24.0f;
    public float anchorX = 5.5f;
    public float enterSpeed = 7.5f;

    [Header("Movement (Idle Sine Wave)")]
    public float sinAmplitude = 2.4f;
    public float sinFrequency = 1.8f;
    public float actionCooldown = 2.2f;
    private float actionTimer = 0f;

    [Header("Ranged Attack")]
    public Transform bossFirePoint;
    public string bossBulletTag = "BossBullet";
    public Transform playerTarget;

    [Header("Dash Attack")]
    public float dashTargetX = -4.2f;
    public float dashSpeed = 18.0f;
    public float retreatSpeed = 10.0f;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color normalColor = Color.white;
    public Color enragedColor = new Color(1f, 0.45f, 0.45f);
    public GameObject explosionPrefab;

    [Header("References")]
    public LogicScript logic;

    private Collider2D bossCollider;

    void Awake()
    {
        currentHP = maxHP;
        bossCollider = GetComponent<Collider2D>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    void Start()
    {
        if (logic == null)
        {
            logic = FindObjectOfType<LogicScript>();
        }

        FindPlayer();
        // Giữ Boss hiển thị trên màn hình Toàn cảnh (Standby) trước khi bắt đầu!
        currentState = BossState.Standby;
    }

    private void FindPlayer()
    {
        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTarget = playerObj.transform;
            }
            else
            {
                BirdScript bird = FindObjectOfType<BirdScript>();
                if (bird != null)
                {
                    playerTarget = bird.transform;
                }
            }
        }
    }

    public void ActivateBoss()
    {
        gameObject.SetActive(true);
        transform.position = new Vector3(13.0f, 0.5f, 0f);
        currentState = BossState.Entering;
        currentHP = maxHP;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHP;
            healthSlider.value = currentHP;
        }

        FindPlayer();
    }

    void Update()
    {
        switch (currentState)
        {
            case BossState.Standby:
                HandleStandby();
                break;

            case BossState.Entering:
                HandleEntering();
                break;

            case BossState.Idle:
                HandleIdle();
                break;
        }
    }

    private void HandleStandby()
    {
        // Khi chưa bắt đầu game: Lơ lửng tại vị trí Toàn cảnh để người chơi quan sát toàn bộ 4 Stage
        if (logic == null || !logic.IsGameStarted)
        {
            float hoverY = panoramicStandbyPos.y + Mathf.Sin(Time.time * 2.5f) * 0.35f;
            transform.position = new Vector3(panoramicStandbyPos.x, hoverY, 0f);
        }
        else
        {
            // Khi người chơi bắt đầu bay ở Stage 2-3: Tạm lùi ra ngoài biên phải chờ đủ 10 điểm
            transform.position = Vector3.MoveTowards(
                transform.position,
                new Vector3(offscreenWaitX, panoramicStandbyPos.y, 0f),
                enterSpeed * 1.5f * Time.deltaTime
            );
        }
    }

    private void HandleEntering()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            new Vector3(anchorX, 0f, 0f),
            enterSpeed * Time.deltaTime
        );

        if (Mathf.Abs(transform.position.x - anchorX) < 0.05f)
        {
            currentState = BossState.Idle;
            actionTimer = 0f;
        }
    }

    private void HandleIdle()
    {
        float newY = Mathf.Sin(Time.time * sinFrequency) * sinAmplitude;
        transform.position = new Vector3(anchorX, newY, 0f);

        actionTimer += Time.deltaTime;
        if (actionTimer >= actionCooldown)
        {
            actionTimer = 0f;
            if (Random.value < 0.6f)
            {
                StartCoroutine(RangedAttackRoutine());
            }
            else
            {
                StartCoroutine(DashAttackRoutine());
            }
        }
    }

    private IEnumerator RangedAttackRoutine()
    {
        currentState = BossState.RangedAttack;

        yield return new WaitForSeconds(0.3f);

        float[] spreadAngles = isEnraged
            ? new float[] { -28f, -14f, 0f, 14f, 28f }
            : new float[] { -18f, 0f, 18f };

        Vector2 baseDirection = Vector2.left;
        if (playerTarget != null)
        {
            baseDirection = (playerTarget.position - transform.position).normalized;
        }

        Vector3 spawnPos = bossFirePoint != null ? bossFirePoint.position : transform.position + Vector3.left * 1.5f;

        foreach (float angle in spreadAngles)
        {
            if (ObjectPooler.Instance != null)
            {
                GameObject bullet = ObjectPooler.Instance.SpawnFromPool(bossBulletTag, spawnPos, Quaternion.identity);
                if (bullet != null)
                {
                    BossBulletScript bulletScript = bullet.GetComponent<BossBulletScript>();
                    if (bulletScript != null)
                    {
                        Vector2 spreadDir = Quaternion.Euler(0, 0, angle) * baseDirection;
                        bulletScript.SetDirection(spreadDir);
                    }
                }
            }
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("BossShoot");
        }

        yield return new WaitForSeconds(0.4f);

        if (currentState != BossState.Dead)
        {
            currentState = BossState.Idle;
        }
    }

    private IEnumerator DashAttackRoutine()
    {
        currentState = BossState.DashAttack;

        Color originalColor = isEnraged ? enragedColor : normalColor;
        for (int i = 0; i < 3; i++)
        {
            if (spriteRenderer != null) spriteRenderer.color = Color.yellow;
            yield return new WaitForSeconds(0.08f);
            if (spriteRenderer != null) spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.08f);
        }

        float lockedY = transform.position.y;

        while (transform.position.x > dashTargetX)
        {
            transform.position += Vector3.left * dashSpeed * Time.deltaTime;
            yield return null;
        }

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.15f, 0.25f);
        }

        yield return new WaitForSeconds(0.2f);

        while (transform.position.x < anchorX)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                new Vector3(anchorX, lockedY, 0f),
                retreatSpeed * Time.deltaTime
            );
            yield return null;
        }

        if (currentState != BossState.Dead)
        {
            currentState = BossState.Idle;
        }
    }

    public void TakeDamage(int damage)
    {
        // Không nhận sát thương khi đang ở chế độ chờ Toàn cảnh (Standby) hoặc đã nổ
        if (currentState == BossState.Dead || currentState == BossState.Standby) return;

        currentHP -= damage;
        if (healthSlider != null)
        {
            healthSlider.value = currentHP;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Hit");
        }

        StartCoroutine(DamageFlashRoutine());

        if (!isEnraged && currentHP <= maxHP * 0.4f)
        {
            TriggerEnraged();
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void TriggerEnraged()
    {
        isEnraged = true;
        sinFrequency = 2.8f;
        sinAmplitude = 2.8f;
        actionCooldown = 1.3f;
        dashSpeed = 24.0f;
        retreatSpeed = 14.0f;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = enragedColor;
        }

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.3f, 0.35f);
        }
    }

    private IEnumerator DamageFlashRoutine()
    {
        if (spriteRenderer != null)
        {
            Color baseColor = isEnraged ? enragedColor : normalColor;
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.06f);
            spriteRenderer.color = baseColor;
        }
    }

    private void Die()
    {
        currentState = BossState.Dead;
        StopAllCoroutines();

        if (bossCollider != null)
        {
            bossCollider.enabled = false;
        }

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.6f, 0.45f);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Explosion");
        }

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // Đánh bại Boss: Hồi 1 máu cho người chơi
        BirdScript player = FindObjectOfType<BirdScript>();
        if (player != null && player.birdIsAlive)
        {
            player.Heal(1);
        }

        if (logic != null)
        {
            logic.OnBossDefeated();
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Đặt lại trạng thái của Boss cho đợt tiếp theo với lượng máu tăng dần (tối đa 1000 HP).
    /// </summary>
    public void ResetForWave(int newMaxHP)
    {
        maxHP = Mathf.Clamp(newMaxHP, 10, LogicScript.MaxBossHP);
        currentHP = maxHP;
        isEnraged = false;
        currentState = BossState.Standby;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }

        if (bossCollider != null)
        {
            bossCollider.enabled = true;
        }

        sinFrequency = 1.8f;
        sinAmplitude = 2.4f;
        actionCooldown = 2.2f;
        dashSpeed = 18.0f;
        retreatSpeed = 10.0f;

        transform.position = new Vector3(offscreenWaitX, panoramicStandbyPos.y, 0f);
        gameObject.SetActive(true);

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHP;
            healthSlider.value = currentHP;
        }
    }
}
