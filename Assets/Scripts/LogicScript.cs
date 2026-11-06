using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý vòng lặp trò chơi (Mediator) theo 4 Stage:
/// Stage 1 (Panoramic Pre-Start) -> Stage 2 & 3 (Flight & Obstacle Corridor) -> Stage 4 (Boss Fight)
/// Tích hợp [ExecuteAlways] tự động nạp Sprite trực tiếp cho cả Scene View (Edit Mode) & Game View (Play Mode).
/// </summary>
[ExecuteAlways]
public class LogicScript : MonoBehaviour
{
    [Header("Score Settings")]
    public int playerScore = 0;
    public int scoreToSpawnBoss = 10;
    public Text scoreText;

    [Header("UI Panels & Pre-Start Objects")]
    public GameObject getReadyBanner;
    public GameObject tapIcon;
    public GameObject gameOverScreen;
    public GameObject victoryScreen;
    public GameObject bossWarningBanner;
    public GameObject bossUIContainer;

    [Header("World References")]
    public PipeSpawnScript pipeSpawner;
    public BossScript boss;

    [Header("Wave Progression & Boss Scaling")]
    public int currentWave = 1;
    public int bossKills = 0;
    public bool isWaveVictory = false;
    public int baseBossHP = 60;
    public int bossHPIncreasePerWave = 100;
    public const int MaxBossHP = 1000;

    public bool IsGameStarted { get; private set; } = false;
    public bool IsGameEnded { get; private set; } = false;
    public bool isVictory = false;
    private bool bossPhaseActive = false;
    private bool bossWarningBannerActive = false;
    private bool healthCrateSpawned = false;
    private bool weaponCrateSpawned = false;

    [Header("Post-Boss Upgrade State")]
    public bool upgradeChosenThisWave = false;
    public string upgradeNotification = "";

    private static Dictionary<string, Sprite> runtimeSpriteCache = new Dictionary<string, Sprite>();

    public static Sprite GetOrLoadSprite(string assetRelativePath, float ppu = 100f)
    {
        if (string.IsNullOrEmpty(assetRelativePath)) return null;
        if (runtimeSpriteCache.TryGetValue(assetRelativePath, out Sprite cached) && cached != null)
        {
            return cached;
        }

        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetRelativePath);
        if (!File.Exists(fullPath)) return null;

        byte[] bytes = File.ReadAllBytes(fullPath);
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        if (ImageConversion.LoadImage(tex, bytes))
        {
            tex.name = Path.GetFileNameWithoutExtension(assetRelativePath);
            Sprite sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), ppu);
            sp.name = tex.name;
            runtimeSpriteCache[assetRelativePath] = sp;
            return sp;
        }
        return null;
    }

    public static void HydrateSceneSprites()
    {
        Sprite sky = GetOrLoadSprite("Assets/Sprites/Environment/background.png");
        Sprite ground = GetOrLoadSprite("Assets/Sprites/Environment/groundGrass.png");
        Sprite rockDown = GetOrLoadSprite("Assets/Sprites/Environment/rockDown.png");
        Sprite rockUp = GetOrLoadSprite("Assets/Sprites/Environment/rock.png");
        Sprite starGold = GetOrLoadSprite("Assets/Sprites/Environment/starGold.png");
        Sprite puffSmall = GetOrLoadSprite("Assets/Sprites/Environment/puffSmall.png");
        Sprite planeRed1 = GetOrLoadSprite("Assets/Sprites/Player/planeRed1.png");
        Sprite planeRed2 = GetOrLoadSprite("Assets/Sprites/Player/planeRed2.png");
        Sprite planeRed3 = GetOrLoadSprite("Assets/Sprites/Player/planeRed3.png");
        Sprite planeGreen1 = GetOrLoadSprite("Assets/Sprites/Boss/planeGreen1.png");
        Sprite planeGreen2 = GetOrLoadSprite("Assets/Sprites/Boss/planeGreen2.png");
        Sprite planeGreen3 = GetOrLoadSprite("Assets/Sprites/Boss/planeGreen3.png");
        Sprite textGetReady = GetOrLoadSprite("Assets/Sprites/UI/textGetReady.png");
        Sprite tap = GetOrLoadSprite("Assets/Sprites/UI/tap.png");
        Sprite healthCrate = GetOrLoadSprite("Assets/Sprites/UI/healthCrate.png");

        SpriteRenderer[] allRenderers = FindObjectsOfType<SpriteRenderer>(true);
        foreach (SpriteRenderer sr in allRenderers)
        {
            if (sr == null) continue;
            string n = sr.gameObject.name;

            if (n.StartsWith("SkyBackground")) sr.sprite = sky;
            else if (n.StartsWith("Ground")) sr.sprite = ground;
            else if (n == "TwinTowerFly") sr.sprite = planeRed1;
            else if (n == "EnginePuff") sr.sprite = puffSmall;
            else if (n == "TopPipe") sr.sprite = rockDown;
            else if (n == "BottomPipe") sr.sprite = rockUp;
            else if (n == "GoldStar") sr.sprite = starGold;
            else if (n == "Boss") sr.sprite = planeGreen1;
            else if (n == "UI_GetReadyBanner") sr.sprite = textGetReady;
            else if (n == "UI_TapIcon") sr.sprite = tap;
            else if (n == "HealthCrate") sr.sprite = healthCrate;
        }

        PlaneAnimator[] animators = FindObjectsOfType<PlaneAnimator>(true);
        foreach (PlaneAnimator anim in animators)
        {
            if (anim == null) continue;
            if (anim.gameObject.name == "TwinTowerFly")
            {
                anim.frames = new Sprite[] { planeRed1, planeRed2, planeRed3 };
            }
            else if (anim.gameObject.name == "Boss")
            {
                anim.frames = new Sprite[] { planeGreen1, planeGreen2, planeGreen3 };
            }
        }
    }

    void OnEnable()
    {
        HydrateSceneSprites();
    }

    void Start()
    {
        HydrateSceneSprites();

        if (!Application.isPlaying) return;

        if (pipeSpawner == null)
        {
            pipeSpawner = FindObjectOfType<PipeSpawnScript>();
        }
        if (boss == null)
        {
            boss = FindObjectOfType<BossScript>();
        }
        if (boss != null)
        {
            boss.maxHP = CalculateBossHP(currentWave);
            boss.currentHP = boss.maxHP;
        }
        if (getReadyBanner == null)
        {
            getReadyBanner = GameObject.Find("UI_GetReadyBanner");
        }
        if (tapIcon == null)
        {
            tapIcon = GameObject.Find("UI_TapIcon");
        }
    }

    void Update()
    {
        if (!Application.isPlaying) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToMainMenu();
        }

        if (isWaveVictory)
        {
            // Phím tắt chọn nâng cấp sau khi hạ Boss
            if (!upgradeChosenThisWave)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
                {
                    ChooseUpgrade(1);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                {
                    ChooseUpgrade(2);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
                {
                    ChooseUpgrade(3);
                }
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                // Chỉ cho phép nhấn Enter tiếp tục sau khi người chơi đã chủ động chọn nâng cấp (bỏ hoàn toàn phím Space)
                ProceedToNextWave();
            }
        }
        else if (IsGameEnded)
        {
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Space))
            {
                restartGame();
            }
        }
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    public int CalculateBossHP(int wave)
    {
        return Mathf.Clamp(baseBossHP + (wave - 1) * bossHPIncreasePerWave, baseBossHP, MaxBossHP);
    }

    public void ChooseUpgrade(int option)
    {
        if (upgradeChosenThisWave) return;

        BirdScript player = FindObjectOfType<BirdScript>();
        if (player == null) return;

        switch (option)
        {
            case 1:
                player.UpgradeMaxHealth(1);
                upgradeNotification = $"ĐÃ NÂNG CẤP: +1 MÁU TỐI ĐA & HỒI ĐẦY 100% MÁU (MÁU MỚI: {player.maxHealth})!";
                break;
            case 2:
                player.UpgradeDamage(1);
                upgradeNotification = $"ĐÃ NÂNG CẤP: +1 SÁT THƯƠNG ĐẠN (SÁT THƯƠNG MỚI: {player.bulletDamage})!";
                break;
            case 3:
                player.UpgradeFireRate(0.04f);
                upgradeNotification = $"ĐÃ NÂNG CẤP: TĂNG TỐC ĐỘ BẮN (TỐC ĐỘ MỚI: {(1f / player.fireRate):F1} PHÁT/GIÂY)!";
                break;
        }

        upgradeChosenThisWave = true;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Score");
        }
    }

    public void ProceedToNextWave()
    {
        if (!isWaveVictory) return;
        isWaveVictory = false;
        upgradeChosenThisWave = false;
        currentWave++;

        scoreToSpawnBoss = playerScore + 10;
        healthCrateSpawned = false;
        weaponCrateSpawned = false;
        bossPhaseActive = false;
        bossWarningBannerActive = false;

        if (bossUIContainer != null)
        {
            bossUIContainer.SetActive(false);
        }

        int nextBossHP = CalculateBossHP(currentWave);
        if (boss != null)
        {
            boss.ResetForWave(nextBossHP);
        }

        if (pipeSpawner != null)
        {
            pipeSpawner.ResetForWave(10); // Đợt mới sinh chính xác 10 cột, không thừa sao
        }

        BirdScript player = FindObjectOfType<BirdScript>();
        if (player != null)
        {
            player.ResumeFlight();
        }
    }

    public void StartGame()
    {
        if (IsGameStarted || IsGameEnded) return;
        IsGameStarted = true;

        if (getReadyBanner != null) getReadyBanner.SetActive(false);
        if (tapIcon != null) tapIcon.SetActive(false);

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.TransitionToCombatView();
        }
    }

    public void addScore(int scoreToAdd)
    {
        if (IsGameEnded || !IsGameStarted) return;

        playerScore += scoreToAdd;
        if (scoreText != null)
        {
            scoreText.text = playerScore.ToString();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Score");
        }

        // Cung cấp Hộp tiếp tế Vũ khí ngẫu nhiên ở giữa hành lang (khoảng score 4-5)
        if (playerScore >= scoreToSpawnBoss - 5 && !weaponCrateSpawned && !bossPhaseActive)
        {
            weaponCrateSpawned = true;
            SpawnRandomWeaponPickup(new Vector3(14.0f, Random.Range(-0.4f, 1.0f), 0f));
        }

        // Cung cấp Hộp tiếp tế máu khi chuẩn bị đánh Boss (khoảng score 8-9)
        if (playerScore >= scoreToSpawnBoss - 2 && !healthCrateSpawned && !bossPhaseActive)
        {
            healthCrateSpawned = true;
            SpawnHealthCrate(new Vector3(14.0f, Random.Range(-0.4f, 1.0f), 0f));
        }

        // Kiểm tra mốc chuyển sang Stage 4: Đấu Boss
        if (playerScore >= scoreToSpawnBoss && !bossPhaseActive)
        {
            StartCoroutine(TriggerBossEncounterRoutine());
        }
    }

    public void SpawnRandomWeaponPickup(Vector3 position)
    {
        GameObject crateObj = new GameObject("WeaponCrate");
        crateObj.transform.position = position;
        crateObj.layer = 0;

        SpriteRenderer sr = crateObj.AddComponent<SpriteRenderer>();
        sr.sprite = GetOrLoadSprite("Assets/Sprites/UI/healthCrate.png");
        sr.sortingOrder = 6;

        BoxCollider2D col = crateObj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.55f, 0.55f);

        WeaponPickup wp = crateObj.AddComponent<WeaponPickup>();
        float r = Random.value;
        if (r < 0.45f)
        {
            wp.weaponType = BirdScript.WeaponType.Spread3Way;
            sr.color = new Color(1f, 0.6f, 0.2f);
        }
        else if (r < 0.75f)
        {
            wp.weaponType = BirdScript.WeaponType.RapidDual;
            sr.color = new Color(1f, 0.95f, 0.2f);
        }
        else
        {
            wp.weaponType = BirdScript.WeaponType.PlasmaPiercing;
            sr.color = new Color(0.2f, 0.85f, 1f);
        }
    }

    public void SpawnHealthCrate(Vector3 position)
    {
        GameObject crateObj = new GameObject("HealthCrate");
        crateObj.transform.position = position;
        crateObj.layer = 0;

        SpriteRenderer sr = crateObj.AddComponent<SpriteRenderer>();
        sr.sprite = GetOrLoadSprite("Assets/Sprites/UI/healthCrate.png");
        sr.sortingOrder = 6;

        BoxCollider2D col = crateObj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.55f, 0.55f);

        crateObj.AddComponent<HealthPickup>();
    }

    private IEnumerator TriggerBossEncounterRoutine()
    {
        bossPhaseActive = true;
        bossWarningBannerActive = true;

        if (pipeSpawner != null)
        {
            pipeSpawner.SetSpawning(false);
            pipeSpawner.ClearExcessPipes(); // Xóa sạch các cột dư thừa phía trước để triệt tiêu hoàn toàn ngôi sao thứ 11
        }

        if (bossWarningBanner != null)
        {
            bossWarningBanner.SetActive(true);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Warning");
            AudioManager.Instance.PlayBGM("BossBGM");
        }

        yield return new WaitForSeconds(2.5f);

        bossWarningBannerActive = false;
        if (bossWarningBanner != null)
        {
            bossWarningBanner.SetActive(false);
        }

        if (bossUIContainer != null)
        {
            bossUIContainer.SetActive(true);
        }

        if (boss != null)
        {
            boss.ActivateBoss();
        }
    }

    public void gameOver()
    {
        if (IsGameEnded) return;
        IsGameEnded = true;
        isVictory = false;

        MatchHistoryManager.SaveMatch(playerScore, currentWave, bossKills, false);

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
        }
    }

    public void OnBossDefeated()
    {
        if (IsGameEnded) return;

        bossKills++;
        isWaveVictory = true;
        upgradeChosenThisWave = false;
        upgradeNotification = "";
        bossPhaseActive = false;
        bossWarningBannerActive = false;

        if (bossUIContainer != null)
        {
            bossUIContainer.SetActive(false);
        }

        if (ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.DeactivateAllByTag("BossBullet");
            ObjectPooler.Instance.DeactivateAllByTag("PlayerBullet");
        }

        MatchHistoryManager.SaveMatch(playerScore, currentWave, bossKills, true);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Victory");
            AudioManager.Instance.PlayBGM("StageBGM");
        }
    }

    public void gameWon()
    {
        OnBossDefeated();
    }

    public void restartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Vẽ khung nền tối độ tương phản cao kèm viền vàng kim giúp chữ luôn nổi bật rõ nét trên nền bầu trời sáng.
    /// </summary>
    public static void DrawContrastBox(Rect rect, float alpha = 0.88f)
    {
        Color prev = GUI.color;
        // Viền ngoài màu vàng hổ phách
        GUI.color = new Color(0.95f, 0.76f, 0.20f, Mathf.Min(1f, alpha + 0.08f));
        GUI.DrawTexture(new Rect(rect.x - 2f, rect.y - 2f, rect.width + 4f, rect.height + 4f), Texture2D.whiteTexture);

        // Nền chính màu đen than đậm (tương phản tuyệt đối với chữ vàng/trắng/cam)
        GUI.color = new Color(0.07f, 0.09f, 0.14f, alpha);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = prev;
    }

    /// <summary>
    /// Vẽ nhãn chữ kèm bóng đổ đen sắc nét để không bao giờ bị chìm vào phong cảnh nền.
    /// </summary>
    public static void DrawShadowLabel(Rect rect, string text, GUIStyle style, Color textColor)
    {
        Color prevColor = style.normal.textColor;
        style.normal.textColor = new Color(0f, 0f, 0f, 0.95f);
        GUI.Label(new Rect(rect.x + 1.5f, rect.y + 1.5f, rect.width, rect.height), text, style);
        style.normal.textColor = textColor;
        GUI.Label(rect, text, style);
        style.normal.textColor = prevColor;
    }

    void OnGUI()
    {
        Color goldColor = new Color(1.0f, 0.88f, 0.15f);
        Color warmAmber = new Color(1.0f, 0.68f, 0.18f);
        Color emeraldGreen = new Color(0.25f, 0.98f, 0.45f);
        Color creamWhite = new Color(1.0f, 0.97f, 0.88f);

        // 0. Màn hình Bao quát Trò chơi Trước khi Bắt đầu (Pre-Start Panoramic Overview HUD)
        if (!IsGameStarted && !IsGameEnded)
        {
            float topW = Mathf.Min(Screen.width - 40f, 860f);
            float topX = (Screen.width - topW) * 0.5f;
            DrawContrastBox(new Rect(topX, 14f, topW, 66f), 0.90f);

            GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 20;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            DrawShadowLabel(new Rect(topX, 18f, topW, 28f), "SKY GUNSHIP CAMPAIGN - MÀN HÌNH BAO QUÁT CHIẾN DỊCH (4 GIAI ĐOẠN)", headerStyle, goldColor);

            GUIStyle subStyle = new GUIStyle(GUI.skin.label);
            subStyle.fontSize = 14;
            subStyle.fontStyle = FontStyle.Bold;
            subStyle.alignment = TextAnchor.MiddleCenter;
            DrawShadowLabel(new Rect(topX, 46f, topW, 24f), "[Giai đoạn 1: Bầu trời & Đất] -> [Giai đoạn 2: Phi cơ đỏ] -> [Giai đoạn 3: 10 Sao vàng] -> [Giai đoạn 4: Trùm Không Hạm]", subStyle, creamWhite);

            float botW = 590f, botH = 46f;
            float botX = (Screen.width - botW) * 0.5f;
            float botY = Screen.height - 68f;
            DrawContrastBox(new Rect(botX, botY, botW, botH), 0.92f);

            GUIStyle promptStyle = new GUIStyle(GUI.skin.label);
            promptStyle.fontSize = 17;
            promptStyle.fontStyle = FontStyle.Bold;
            promptStyle.alignment = TextAnchor.MiddleCenter;
            Color pulseColor = (Mathf.PingPong(Time.time * 3f, 1f) > 0.4f) ? goldColor : Color.white;
            DrawShadowLabel(new Rect(botX, botY, botW, botH), "NHẤN [SPACE] HOẶC [CHUỘT TRÁI] ĐỂ BẮT ĐẦU BAY VÀ BẮN", promptStyle, pulseColor);

            // Nút điều hướng về Menu chính
            GUIStyle navBtnStyle = new GUIStyle(GUI.skin.button);
            navBtnStyle.fontSize = 14;
            navBtnStyle.fontStyle = FontStyle.Bold;
            navBtnStyle.normal.textColor = goldColor;
            if (GUI.Button(new Rect(20, 14, 145, 36), "VỀ MENU (Esc)", navBtnStyle))
            {
                ReturnToMainMenu();
            }
            return;
        }

        // 1. Khung HUD nền tối góc trên bên trái giúp chữ hoàn toàn tách biệt với bầu trời sáng
        DrawContrastBox(new Rect(14f, 12f, 575f, 92f), 0.86f);

        GUIStyle scoreStyle = new GUIStyle(GUI.skin.label);
        scoreStyle.fontSize = 19;
        scoreStyle.fontStyle = FontStyle.Bold;

        string scoreLabel = bossPhaseActive
            ? $"ĐỢT {currentWave}: ĐẠI CHIẾN TRÙM  |  ĐIỂM: {playerScore}  |  TRÙM ĐÃ HẠ: {bossKills}"
            : $"ĐỢT {currentWave}  |  ĐIỂM: {playerScore} / {scoreToSpawnBoss}  |  TRÙM ĐÃ HẠ: {bossKills}";

        DrawShadowLabel(new Rect(24, 16, 555, 30), scoreLabel, scoreStyle, goldColor);

        // HUD thanh máu người chơi (sử dụng Texture màu, không dùng emoji)
        BirdScript player = FindObjectOfType<BirdScript>();
        if (player != null && player.birdIsAlive && !IsGameEnded)
        {
            float hpBarW = 130f, hpBarH = 14f;
            GUI.color = new Color(0.2f, 0.2f, 0.25f, 1f);
            GUI.DrawTexture(new Rect(24, 50, hpBarW + 4, hpBarH + 4), Texture2D.whiteTexture);
            float hpRatio = Mathf.Clamp01((float)player.currentHealth / player.maxHealth);
            GUI.color = (player.currentHealth == 1) ? new Color(1f, 0.25f, 0.25f) : emeraldGreen;
            GUI.DrawTexture(new Rect(26, 52, hpBarW * hpRatio, hpBarH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUIStyle hpTextStyle = new GUIStyle(GUI.skin.label);
            hpTextStyle.fontSize = 14;
            hpTextStyle.fontStyle = FontStyle.Bold;
            DrawShadowLabel(new Rect(166, 47, 160, 24), $"MÁU: {player.currentHealth} / {player.maxHealth}", hpTextStyle, Color.white);

            // Hiển thị chỉ số Hỏa lực & Chủng loại vũ khí bằng màu Vàng Cam ấm áp dễ đọc (thay thế hoàn toàn màu xanh dương nhạt)
            GUIStyle combatStyle = new GUIStyle(GUI.skin.label);
            combatStyle.fontSize = 13;
            combatStyle.fontStyle = FontStyle.Bold;

            string weaponInfo = (player.weaponBuffTimer > 0)
                ? $"{player.GetWeaponDisplayName()} ({player.weaponBuffTimer:F1}s)"
                : player.GetWeaponDisplayName();

            DrawShadowLabel(new Rect(24, 74, 555, 22), 
                $"HỎA LỰC: CẤP {player.weaponLevel} (SÁT THƯƠNG {player.bulletDamage} | {(1f / player.fireRate):F1} PHÁT/S)  |  VŨ KHÍ: {weaponInfo}", 
                combatStyle, warmAmber);
        }

        // 2. Banner cảnh báo Boss xuất hiện
        if (bossWarningBannerActive && !IsGameEnded)
        {
            float w = 640f, h = 84f;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.35f;
            DrawContrastBox(new Rect(x, y, w, h), 0.94f);

            GUIStyle warningStyle = new GUIStyle(GUI.skin.label);
            warningStyle.fontSize = 21;
            warningStyle.fontStyle = FontStyle.Bold;
            warningStyle.alignment = TextAnchor.MiddleCenter;
            Color warnColor = (Mathf.PingPong(Time.time * 6f, 1f) > 0.5f) ? new Color(1f, 0.3f, 0.3f) : goldColor;

            string bossHpInfo = (boss != null) ? $"{boss.maxHP} HP" : "MÁU KHỦNG";
            DrawShadowLabel(new Rect(x, y + 10f, w, h - 20f), $"CẢNH BÁO: CHIẾN HẠM TRÙM ĐỢT {currentWave} ĐANG TIẾN VÀO!\n(SỨC CHỊU ĐỰNG: {bossHpInfo})", warningStyle, warnColor);
        }

        // 3. Thanh máu Boss
        if (bossPhaseActive && boss != null && boss.gameObject.activeInHierarchy && !IsGameEnded)
        {
            float barW = 520f, barH = 28f;
            float barX = (Screen.width - barW) * 0.5f;
            float barY = 20f;

            DrawContrastBox(new Rect(barX - 4, barY - 4, barW + 8, barH + 8), 0.90f);
            float hpPercent = Mathf.Clamp01((float)boss.currentHP / boss.maxHP);
            GUI.color = (boss.isEnraged) ? new Color(0.95f, 0.15f, 0.15f) : new Color(0.90f, 0.22f, 0.30f);
            GUI.DrawTexture(new Rect(barX, barY, barW * hpPercent, barH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUIStyle bossTitleStyle = new GUIStyle(GUI.skin.label);
            bossTitleStyle.fontSize = 15;
            bossTitleStyle.fontStyle = FontStyle.Bold;
            bossTitleStyle.alignment = TextAnchor.MiddleCenter;
            DrawShadowLabel(new Rect(barX, barY, barW, barH), $"CHIẾN HẠM TRÙM ĐỢT {currentWave}: {boss.currentHP} / {boss.maxHP} HP", bossTitleStyle, Color.white);
        }

        // 4. Màn hình Chiến thắng Đợt đấu & Hệ thống Chọn Nâng Cấp Chiến Cơ
        if (isWaveVictory && !IsGameEnded)
        {
            float vicW = 660f, vicH = 430f;
            float vicX = (Screen.width - vicW) * 0.5f;
            float vicY = (Screen.height - vicH) * 0.5f;

            DrawContrastBox(new Rect(vicX, vicY, vicW, vicH), 0.94f);

            GUIStyle vicTitleStyle = new GUIStyle(GUI.skin.label);
            vicTitleStyle.fontSize = 26;
            vicTitleStyle.fontStyle = FontStyle.Bold;
            vicTitleStyle.alignment = TextAnchor.MiddleCenter;
            DrawShadowLabel(new Rect(vicX, vicY + 14, vicW, 34), $"CHIẾN THẮNG ĐỢT {currentWave}!", vicTitleStyle, emeraldGreen);

            GUIStyle vicSubStyle = new GUIStyle(GUI.skin.label);
            vicSubStyle.fontSize = 14;
            vicSubStyle.fontStyle = FontStyle.Bold;
            vicSubStyle.alignment = TextAnchor.MiddleCenter;
            DrawShadowLabel(new Rect(vicX, vicY + 48, vicW, 22), "CHIẾN HẠM TRÙM ĐÃ BỊ HẠ GỤC! HÃY CHỌN 1 NÂNG CẤP CHIẾN CƠ:", vicSubStyle, goldColor);

            // 3 THẺ NÂNG CẤP CHIẾN CƠ (MÁU, SÁT THƯƠNG, TỐC BẮN)
            float cardW = 190f, cardH = 175f;
            float cardY = vicY + 76f;
            float spacing = 16f;
            float totalCardsW = cardW * 3 + spacing * 2;
            float startCardX = vicX + (vicW - totalCardsW) * 0.5f;

            BirdScript bird = FindObjectOfType<BirdScript>();

            // THẺ 1: GIA CỐ MÁU
            float c1X = startCardX;
            DrawContrastBox(new Rect(c1X, cardY, cardW, cardH), 0.92f);
            GUIStyle cardTitleStyle1 = new GUIStyle(GUI.skin.label);
            cardTitleStyle1.fontSize = 15;
            cardTitleStyle1.fontStyle = FontStyle.Bold;
            cardTitleStyle1.alignment = TextAnchor.MiddleCenter;
            DrawShadowLabel(new Rect(c1X, cardY + 8, cardW, 24), "[1] GIA CỐ MÁU", cardTitleStyle1, emeraldGreen);

            GUIStyle cardDescStyle = new GUIStyle(GUI.skin.label);
            cardDescStyle.fontSize = 12;
            cardDescStyle.alignment = TextAnchor.UpperCenter;
            string c1Desc = $"+1 Máu Tối Đa\n& Hồi đầy 100% máu\n\nHiện tại: {bird?.maxHealth} HP\n(Đang có: {bird?.currentHealth} HP)";
            DrawShadowLabel(new Rect(c1X + 8, cardY + 36, cardW - 16, 75), c1Desc, cardDescStyle, creamWhite);

            GUIStyle cardBtnStyle1 = new GUIStyle(GUI.skin.button);
            cardBtnStyle1.fontSize = 13;
            cardBtnStyle1.fontStyle = FontStyle.Bold;
            cardBtnStyle1.normal.textColor = emeraldGreen;
            if (GUI.Button(new Rect(c1X + 15, cardY + cardH - 42, cardW - 30, 32), "CHỌN (Phím 1)", cardBtnStyle1))
            {
                ChooseUpgrade(1);
            }

            // THẺ 2: TĂNG SÁT THƯƠNG
            float c2X = startCardX + cardW + spacing;
            DrawContrastBox(new Rect(c2X, cardY, cardW, cardH), 0.92f);
            DrawShadowLabel(new Rect(c2X, cardY + 8, cardW, 24), "[2] SÁT THƯƠNG", cardTitleStyle1, goldColor);

            string c2Desc = $"+1 Sát Thương Đạn\n(Tăng DPS diệt Boss)\n\nHiện tại: {bird?.bulletDamage} Dmg\nCấp vũ khí: Cấp {bird?.weaponLevel}";
            DrawShadowLabel(new Rect(c2X + 8, cardY + 36, cardW - 16, 75), c2Desc, cardDescStyle, creamWhite);

            GUIStyle cardBtnStyle2 = new GUIStyle(cardBtnStyle1);
            cardBtnStyle2.normal.textColor = goldColor;
            if (GUI.Button(new Rect(c2X + 15, cardY + cardH - 42, cardW - 30, 32), "CHỌN (Phím 2)", cardBtnStyle2))
            {
                ChooseUpgrade(2);
            }

            // THẺ 3: TĂNG TỐC BẮN (Đổi từ màu xanh dương sang màu Cam Hổ Phách sáng rõ)
            float c3X = startCardX + (cardW + spacing) * 2;
            DrawContrastBox(new Rect(c3X, cardY, cardW, cardH), 0.92f);
            DrawShadowLabel(new Rect(c3X, cardY + 8, cardW, 24), "[3] TĂNG TỐC BẮN", cardTitleStyle1, warmAmber);

            float ratePerSec = (bird != null && bird.fireRate > 0) ? (1f / bird.fireRate) : 3.3f;
            string c3Desc = $"Bắn nhanh hơn +25%\n(Giảm thời gian trễ)\n\nTốc độ: {ratePerSec:F1} phát/s\nGiãn cách: {bird?.fireRate:F2}s";
            DrawShadowLabel(new Rect(c3X + 8, cardY + 36, cardW - 16, 75), c3Desc, cardDescStyle, creamWhite);

            GUIStyle cardBtnStyle3 = new GUIStyle(cardBtnStyle1);
            cardBtnStyle3.normal.textColor = warmAmber;
            if (GUI.Button(new Rect(c3X + 15, cardY + cardH - 42, cardW - 30, 32), "CHỌN (Phím 3)", cardBtnStyle3))
            {
                ChooseUpgrade(3);
            }

            // THÔNG BÁO VÀ CHỈ SỐ SAU KHI NÂNG CẤP
            float infoY = cardY + cardH + 12f;
            GUIStyle notifStyle = new GUIStyle(GUI.skin.label);
            notifStyle.fontSize = 14;
            notifStyle.fontStyle = FontStyle.Bold;
            notifStyle.alignment = TextAnchor.MiddleCenter;

            if (!string.IsNullOrEmpty(upgradeNotification))
            {
                DrawShadowLabel(new Rect(vicX, infoY, vicW, 22), upgradeNotification, notifStyle, emeraldGreen);
            }
            else
            {
                DrawShadowLabel(new Rect(vicX, infoY, vicW, 22), "Nhấn phím [1], [2], [3] hoặc click nút để nhận ngay nâng cấp!", notifStyle, goldColor);
            }

            int nextWaveHP = CalculateBossHP(currentWave + 1);
            GUIStyle subInfoStyle = new GUIStyle(GUI.skin.label);
            subInfoStyle.fontSize = 13;
            subInfoStyle.fontStyle = FontStyle.Bold;
            subInfoStyle.alignment = TextAnchor.MiddleCenter;
            DrawShadowLabel(new Rect(vicX, infoY + 24, vicW, 20), 
                $"Trùm Đợt {currentWave + 1} sẽ sở hữu {nextWaveHP} HP (Tối đa {MaxBossHP} HP)  |  Kỷ lục: {MatchHistoryManager.GetHighScore()} điểm", 
                subInfoStyle, creamWhite);

            // NÚT BẤM TIẾP TỤC ĐỢT SAU (Đã gỡ phím Space để tránh vô tình bỏ qua nâng cấp khi đang bay)
            GUIStyle proceedBtnStyle = new GUIStyle(GUI.skin.button);
            proceedBtnStyle.fontSize = 16;
            proceedBtnStyle.fontStyle = FontStyle.Bold;
            proceedBtnStyle.normal.textColor = upgradeChosenThisWave ? emeraldGreen : goldColor;

            string proceedBtnText = upgradeChosenThisWave
                ? $"TIẾP TỤC ĐỢT {currentWave + 1} (Click hoặc Phím Enter)"
                : $"BỎ QUA & TIẾP TỤC ĐỢT {currentWave + 1} (Tự động +1 Máu)";

            if (GUI.Button(new Rect(vicX + 60, vicY + vicH - 62, vicW - 120, 44), proceedBtnText, proceedBtnStyle))
            {
                if (!upgradeChosenThisWave)
                {
                    ChooseUpgrade(1);
                }
                ProceedToNextWave();
            }
        }

        // 5. Màn hình Game Over
        if (IsGameEnded)
        {
            float panelW = 490f, panelH = 300f;
            float panelX = (Screen.width - panelW) * 0.5f;
            float panelY = (Screen.height - panelH) * 0.5f;

            DrawContrastBox(new Rect(panelX, panelY, panelW, panelH), 0.94f);

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 28;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            DrawShadowLabel(new Rect(panelX, panelY + 16, panelW, 38), "TRÒ CHƠI KẾT THÚC", titleStyle, new Color(1f, 0.32f, 0.32f));

            GUIStyle statsStyle = new GUIStyle(GUI.skin.label);
            statsStyle.fontSize = 15;
            statsStyle.fontStyle = FontStyle.Bold;
            statsStyle.alignment = TextAnchor.MiddleCenter;
            DrawShadowLabel(new Rect(panelX, panelY + 58, panelW, 26), $"ĐIỂM SỐ ĐẠT ĐƯỢC: {playerScore}  |  DỪNG CHÂN: ĐỢT {currentWave}", statsStyle, creamWhite);
            DrawShadowLabel(new Rect(panelX, panelY + 86, panelW, 26), $"SỐ CHIẾN HẠM ĐÃ HẠ: {bossKills}  |  KỶ LỤC ĐIỂM: {MatchHistoryManager.GetHighScore()}", statsStyle, goldColor);

            GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
            btnStyle.fontSize = 16;
            btnStyle.fontStyle = FontStyle.Bold;
            btnStyle.normal.textColor = goldColor;

            if (GUI.Button(new Rect(panelX + 40, panelY + 130, panelW - 80, 48), "CHƠI LẠI (Phím Space / R)", btnStyle))
            {
                restartGame();
            }

            GUIStyle menuBtnStyle = new GUIStyle(GUI.skin.button);
            menuBtnStyle.fontSize = 15;
            menuBtnStyle.fontStyle = FontStyle.Bold;
            menuBtnStyle.normal.textColor = Color.white;

            if (GUI.Button(new Rect(panelX + 40, panelY + 190, panelW - 80, 44), "VỀ MENU CHÍNH (Phím Esc)", menuBtnStyle))
            {
                ReturnToMainMenu();
            }
        }
    }
}
