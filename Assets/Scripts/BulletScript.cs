using UnityEngine;

/// <summary>
/// Đạn người chơi bắn ra: Hỗ trợ đa dạng các loại vũ khí (Đạn chuẩn, Đạn chùm 3 hướng, Song tia, Plasma xuyên phá),
/// kèm vệt đuôi đạn phát sáng (TrailRenderer) và cơ chế sát thương tùy biến theo cấp nâng cấp.
/// </summary>
public class BulletScript : MonoBehaviour
{
    public float speed = 22.0f;
    public int damage = 2;
    public float maxX = 22.0f;

    [Header("Visual Effects")]
    public Color bulletColor = new Color(1f, 0.95f, 0.2f, 1f);
    public Color trailStartColor = new Color(1f, 0.96f, 0.25f, 0.9f);
    public Color trailEndColor = new Color(1f, 0.65f, 0.05f, 0f);

    [Header("Advanced Weapon Mechanics")]
    public Vector2 moveDirection = Vector2.right;
    public bool isPiercing = false;
    public int maxPierces = 3;
    private int currentPierces = 0;

    private TrailRenderer trail;

    void Awake()
    {
        SetupTrailRenderer();
    }

    void OnEnable()
    {
        currentPierces = 0;
        moveDirection = Vector2.right;
        transform.localScale = Vector3.one;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (sr.sprite == null || sr.sprite.name != "laserYellow")
            {
                Sprite yellowSprite = LogicScript.GetOrLoadSprite("Assets/Sprites/Weapons/laserYellow.png");
                if (yellowSprite != null)
                {
                    sr.sprite = yellowSprite;
                }
            }
            sr.color = bulletColor;
        }

        if (trail != null)
        {
            trail.Clear();
            trail.emitting = true;
        }
    }

    void OnDisable()
    {
        if (trail != null)
        {
            trail.emitting = false;
            trail.Clear();
        }
    }

    private void SetupTrailRenderer()
    {
        trail = GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
        }

        trail.time = 0.12f;
        trail.startWidth = 0.20f;
        trail.endWidth = 0.02f;
        trail.autodestruct = false;
        trail.sortingOrder = 7;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.96f, 0.25f), 0.0f),
                new GradientColorKey(new Color(1f, 0.60f, 0.05f), 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.85f, 0.0f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        trail.colorGradient = gradient;

        if (trail.material == null || trail.material.name.Contains("Default"))
        {
            Shader spriteShader = Shader.Find("Sprites/Default");
            if (spriteShader != null)
            {
                trail.material = new Material(spriteShader);
            }
        }
    }

    /// <summary>
    /// Thiết lập cấu hình đạn động theo chủng loại vũ khí và chỉ số sát thương
    /// </summary>
    public void Setup(int dmg, Vector2 dir, bool piercing = false, Color? overrideColor = null)
    {
        damage = dmg;
        moveDirection = dir.normalized;
        isPiercing = piercing;
        currentPierces = 0;

        // Xoay đầu đạn theo hướng bay
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = overrideColor.HasValue ? overrideColor.Value : bulletColor;

            if (piercing)
            {
                transform.localScale = new Vector3(1.5f, 1.5f, 1f);
            }
            else
            {
                transform.localScale = Vector3.one;
            }
        }

        if (trail != null)
        {
            trail.Clear();
            trail.emitting = true;
        }
    }

    void Update()
    {
        transform.position += (Vector3)moveDirection * speed * Time.deltaTime;

        if (transform.position.x > maxX || Mathf.Abs(transform.position.y) > 8.5f)
        {
            gameObject.SetActive(false); // Trả về Object Pool
        }
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.CompareTag("Boss") || hitInfo.gameObject.layer == 8)
        {
            BossScript target = hitInfo.GetComponent<BossScript>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }

            if (ObjectPooler.Instance != null)
            {
                ObjectPooler.Instance.SpawnFromPool("HitEffect", transform.position, Quaternion.identity);
            }

            if (isPiercing)
            {
                currentPierces++;
                if (currentPierces >= maxPierces)
                {
                    gameObject.SetActive(false);
                }
            }
            else
            {
                gameObject.SetActive(false); // Trả đạn về Pool
            }
        }
    }
}
