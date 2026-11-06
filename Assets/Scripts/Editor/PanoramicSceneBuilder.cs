#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Công cụ tự động xây dựng Prefabs & Scene Toàn cảnh (4 Stages Overview) cho Sky Gunship Campaign.
/// Hỗ trợ chạy trực tiếp từ Menu hoặc qua CLI Batchmode (-executeMethod PanoramicSceneBuilder.BuildAll).
/// </summary>
[InitializeOnLoad]
public static class PanoramicSceneBuilder
{
    private const string PendingFlagPath = "ProjectSettings/PanoramicRebuildPending.flag";

    static PanoramicSceneBuilder()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (File.Exists(PendingFlagPath))
            {
                File.Delete(PendingFlagPath);
                BuildAll();
                if (SceneView.lastActiveSceneView != null)
                {
                    SceneView.lastActiveSceneView.in2DMode = true;
                    SceneView.lastActiveSceneView.LookAt(new Vector3(4.2f, 0.2f, 0f), Quaternion.identity, 14f, true, false);
                    SceneView.lastActiveSceneView.Repaint();
                }
            }
        };
    }

    [MenuItem("Sky Gunship Campaign/Rebuild Panoramic Scene (Stage 1-4)")]
    public static void BuildAll()
    {
        Debug.Log("[PanoramicSceneBuilder] Starting full rebuild of Prefabs and Panoramic Scene...");

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        // 1. Load & Configure Sprites from Assets/Sprites (Kenney CC0)
        Sprite skySprite = LoadOrConfigureSprite("Assets/Sprites/Environment/background.png", 100f);
        Sprite groundSprite = LoadOrConfigureSprite("Assets/Sprites/Environment/groundGrass.png", 100f);
        Sprite rockDownSprite = LoadOrConfigureSprite("Assets/Sprites/Environment/rockDown.png", 100f);
        Sprite rockUpSprite = LoadOrConfigureSprite("Assets/Sprites/Environment/rock.png", 100f);
        Sprite starGoldSprite = LoadOrConfigureSprite("Assets/Sprites/Environment/starGold.png", 100f);
        Sprite puffSmallSprite = LoadOrConfigureSprite("Assets/Sprites/Environment/puffSmall.png", 100f);

        Sprite planeRed1 = LoadOrConfigureSprite("Assets/Sprites/Player/planeRed1.png", 100f);
        Sprite planeRed2 = LoadOrConfigureSprite("Assets/Sprites/Player/planeRed2.png", 100f);
        Sprite planeRed3 = LoadOrConfigureSprite("Assets/Sprites/Player/planeRed3.png", 100f);

        Sprite planeGreen1 = LoadOrConfigureSprite("Assets/Sprites/Boss/planeGreen1.png", 100f);
        Sprite planeGreen2 = LoadOrConfigureSprite("Assets/Sprites/Boss/planeGreen2.png", 100f);
        Sprite planeGreen3 = LoadOrConfigureSprite("Assets/Sprites/Boss/planeGreen3.png", 100f);

        Sprite laserGreen = LoadOrConfigureSprite("Assets/Sprites/Weapons/laserGreen.png", 100f);
        Sprite laserRed = LoadOrConfigureSprite("Assets/Sprites/Weapons/laserRed.png", 100f);

        Sprite textGetReady = LoadOrConfigureSprite("Assets/Sprites/UI/textGetReady.png", 100f);
        Sprite tapIconSprite = LoadOrConfigureSprite("Assets/Sprites/UI/tap.png", 100f);

        // 2. Load Audio Clips
        AudioClip sfxFlap = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/flap.ogg");
        AudioClip sfxShoot = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/shoot.ogg");
        AudioClip sfxBossShoot = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/boss_shoot.ogg");
        AudioClip sfxScore = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/score.ogg");
        AudioClip sfxWarning = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/warning.ogg");
        AudioClip sfxHit = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/hit.ogg");
        AudioClip sfxExplosion = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/explosion.ogg");
        AudioClip sfxVictory = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/victory.ogg");

        if (!Directory.Exists("Assets/Prefabs"))
        {
            Directory.CreateDirectory("Assets/Prefabs");
        }
        if (!Directory.Exists("Assets/Scenes"))
        {
            Directory.CreateDirectory("Assets/Scenes");
        }

        // 3. Build Prefabs
        GameObject playerBulletPrefab = BuildPlayerBulletPrefab(laserGreen);
        GameObject bossBulletPrefab = BuildBossBulletPrefab(laserRed);
        GameObject pipePrefab = BuildPipePrefab(rockDownSprite, rockUpSprite, starGoldSprite);

        // 4. Create New Panoramic Main Scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // --- CAMERA (Panoramic Overview -> Combat Zoom) ---
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        camObj.transform.position = new Vector3(4.2f, 0.2f, -10f);
        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6.5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.45f, 0.76f, 0.95f, 1f);
        camObj.AddComponent<AudioListener>();
        CameraShake camShake = camObj.AddComponent<CameraShake>();
        camShake.panoramicPosition = new Vector3(4.2f, 0.2f, -10f);
        camShake.panoramicOrthoSize = 6.5f;
        camShake.combatPosition = new Vector3(0f, 0f, -10f);
        camShake.combatOrthoSize = 5.0f;

        // --- STAGE 1: PANORAMIC SKY & SCROLLING GRASS GROUND ---
        CreateSkyPanel("SkyBackground_Left", skySprite, new Vector3(-7.5f, 0.5f, 0f));
        CreateSkyPanel("SkyBackground_Center", skySprite, new Vector3(4.5f, 0.5f, 0f));
        CreateSkyPanel("SkyBackground_Right", skySprite, new Vector3(16.5f, 0.5f, 0f));

        CreateGroundSegment("Ground_1", groundSprite, new Vector3(-6.0f, -4.7f, 0f));
        CreateGroundSegment("Ground_2", groundSprite, new Vector3(6.5f, -4.7f, 0f));
        CreateGroundSegment("Ground_3", groundSprite, new Vector3(19.0f, -4.7f, 0f));

        // --- STAGE 2: PLAYER RED FIGHTER PLANE ---
        GameObject playerObj = new GameObject("TwinTowerFly");
        playerObj.tag = "Player";
        playerObj.layer = 6;
        playerObj.transform.position = new Vector3(-4.2f, 0.5f, 0f);
        playerObj.transform.localScale = new Vector3(1.15f, 1.15f, 1f);

        SpriteRenderer playerSR = playerObj.AddComponent<SpriteRenderer>();
        playerSR.sprite = planeRed1;
        playerSR.sortingOrder = 10;

        PlaneAnimator playerAnim = playerObj.AddComponent<PlaneAnimator>();
        playerAnim.frames = new Sprite[] { planeRed1, planeRed2, planeRed3 };
        playerAnim.fps = 20f;

        Rigidbody2D playerRB = playerObj.AddComponent<Rigidbody2D>();
        playerRB.gravityScale = 2.4f;
        playerRB.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CircleCollider2D playerCol = playerObj.AddComponent<CircleCollider2D>();
        playerCol.radius = 0.34f;

        GameObject firePointObj = new GameObject("FirePoint");
        firePointObj.transform.SetParent(playerObj.transform, false);
        firePointObj.transform.localPosition = new Vector3(0.6f, -0.05f, 0f);

        GameObject enginePuffObj = new GameObject("EnginePuff");
        enginePuffObj.transform.SetParent(playerObj.transform, false);
        enginePuffObj.transform.localPosition = new Vector3(-0.65f, 0f, 0f);
        enginePuffObj.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
        SpriteRenderer puffSR = enginePuffObj.AddComponent<SpriteRenderer>();
        puffSR.sprite = puffSmallSprite;
        puffSR.sortingOrder = 9;

        BirdScript birdScript = playerObj.AddComponent<BirdScript>();
        birdScript.myRigidbody = playerRB;
        birdScript.flapStrength = 8.5f;
        birdScript.topBound = 6.2f;
        birdScript.bottomBound = -4.8f;
        birdScript.firePoint = firePointObj.transform;
        birdScript.bulletPoolTag = "PlayerBullet";
        birdScript.fireRate = 0.30f;
        birdScript.autoFire = true;

        // --- STAGE 3: PRE-PLACED OBSTACLE CORRIDOR & PIPE SPAWNER ---
        InstantiateObstacleStage(pipePrefab, "Obstacle_Stage_1", new Vector3(0.5f, -0.4f, 0f));
        InstantiateObstacleStage(pipePrefab, "Obstacle_Stage_2", new Vector3(5.0f, 0.2f, 0f));
        InstantiateObstacleStage(pipePrefab, "Obstacle_Stage_3", new Vector3(9.5f, -0.2f, 0f));

        GameObject spawnerObj = new GameObject("Pipe Spawner");
        spawnerObj.transform.position = new Vector3(14.0f, 0f, 0f);
        PipeSpawnScript spawnerScript = spawnerObj.AddComponent<PipeSpawnScript>();
        spawnerScript.pipePrefab = pipePrefab;
        spawnerScript.pipe = pipePrefab;
        spawnerScript.spawnRate = 2.2f;
        spawnerScript.heightOffset = 1.4f;
        spawnerScript.isSpawning = true;

        // --- STAGE 4: BOSS COMBAT GUNSHIP (ACTIVE IN STANDBY PANORAMA) ---
        GameObject bossObj = new GameObject("Boss");
        bossObj.tag = "Boss";
        bossObj.layer = 8;
        bossObj.transform.position = new Vector3(13.8f, 0.8f, 0f);
        bossObj.transform.localScale = new Vector3(-2.5f, 2.5f, 1f); // Quay mặt sang trái

        SpriteRenderer bossSR = bossObj.AddComponent<SpriteRenderer>();
        bossSR.sprite = planeGreen1;
        bossSR.sortingOrder = 9;

        PlaneAnimator bossAnim = bossObj.AddComponent<PlaneAnimator>();
        bossAnim.frames = new Sprite[] { planeGreen1, planeGreen2, planeGreen3 };
        bossAnim.fps = 18f;

        BoxCollider2D bossCol = bossObj.AddComponent<BoxCollider2D>();
        bossCol.isTrigger = true;
        bossCol.size = new Vector2(0.82f, 0.68f);

        GameObject bossFirePointObj = new GameObject("BossFirePoint");
        bossFirePointObj.transform.SetParent(bossObj.transform, false);
        bossFirePointObj.transform.localPosition = new Vector3(0.45f, -0.05f, 0f);

        BossScript bossScript = bossObj.AddComponent<BossScript>();
        bossScript.maxHP = 50;
        bossScript.currentHP = 50;
        bossScript.currentState = BossScript.BossState.Standby;
        bossScript.panoramicStandbyPos = new Vector3(13.8f, 0.8f, 0f);
        bossScript.offscreenWaitX = 24.0f;
        bossScript.anchorX = 5.5f;
        bossScript.enterSpeed = 7.5f;
        bossScript.sinAmplitude = 2.4f;
        bossScript.sinFrequency = 1.8f;
        bossScript.actionCooldown = 2.2f;
        bossScript.bossFirePoint = bossFirePointObj.transform;
        bossScript.bossBulletTag = "BossBullet";
        bossScript.playerTarget = playerObj.transform;
        bossScript.dashTargetX = -4.2f;
        bossScript.dashSpeed = 18.0f;
        bossScript.retreatSpeed = 10.0f;
        bossScript.spriteRenderer = bossSR;

        // --- PRE-START WORLD UI BANNERS ---
        GameObject getReadyObj = new GameObject("UI_GetReadyBanner");
        getReadyObj.transform.position = new Vector3(4.2f, 3.2f, 0f);
        getReadyObj.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
        SpriteRenderer getReadySR = getReadyObj.AddComponent<SpriteRenderer>();
        getReadySR.sprite = textGetReady;
        getReadySR.sortingOrder = 20;

        GameObject tapObj = new GameObject("UI_TapIcon");
        tapObj.transform.position = new Vector3(-2.6f, -0.6f, 0f);
        tapObj.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
        SpriteRenderer tapSR = tapObj.AddComponent<SpriteRenderer>();
        tapSR.sprite = tapIconSprite;
        tapSR.sortingOrder = 20;

        // --- CORE MANAGERS ---
        GameObject logicObj = new GameObject("Logic Manager");
        logicObj.tag = "Logic";
        LogicScript logicScript = logicObj.AddComponent<LogicScript>();
        logicScript.scoreToSpawnBoss = 10;
        logicScript.pipeSpawner = spawnerScript;
        logicScript.boss = bossScript;
        logicScript.getReadyBanner = getReadyObj;
        logicScript.tapIcon = tapObj;

        birdScript.logic = logicScript;
        bossScript.logic = logicScript;

        GameObject poolerObj = new GameObject("Object Pooler");
        ObjectPooler pooler = poolerObj.AddComponent<ObjectPooler>();
        pooler.pools = new List<ObjectPooler.Pool>
        {
            new ObjectPooler.Pool { tag = "PlayerBullet", prefab = playerBulletPrefab, size = 30 },
            new ObjectPooler.Pool { tag = "BossBullet", prefab = bossBulletPrefab, size = 50 }
        };

        GameObject audioObj = new GameObject("Audio Manager");
        AudioSource bgmSource = audioObj.AddComponent<AudioSource>();
        AudioSource sfxSource = audioObj.AddComponent<AudioSource>();
        AudioManager audioMgr = audioObj.AddComponent<AudioManager>();
        audioMgr.bgmSource = bgmSource;
        audioMgr.sfxSource = sfxSource;
        audioMgr.sfxList = new List<AudioManager.SoundEffect>
        {
            new AudioManager.SoundEffect { name = "Flap", clip = sfxFlap, volume = 0.85f },
            new AudioManager.SoundEffect { name = "Shoot", clip = sfxShoot, volume = 0.65f },
            new AudioManager.SoundEffect { name = "BossShoot", clip = sfxBossShoot, volume = 0.85f },
            new AudioManager.SoundEffect { name = "Score", clip = sfxScore, volume = 0.90f },
            new AudioManager.SoundEffect { name = "Warning", clip = sfxWarning, volume = 1.00f },
            new AudioManager.SoundEffect { name = "Hit", clip = sfxHit, volume = 0.80f },
            new AudioManager.SoundEffect { name = "Explosion", clip = sfxExplosion, volume = 1.00f },
            new AudioManager.SoundEffect { name = "Victory", clip = sfxVictory, volume = 1.00f }
        };

        // Save Scene & Ensure Build Settings
        string scenePath = "Assets/Scenes/FlappyBeatUp_MainScene.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenuScene.unity", true),
            new EditorBuildSettingsScene(scenePath, true)
        };

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[PanoramicSceneBuilder] Successfully built and saved FlappyBeatUp_MainScene.unity and all Prefabs!");
    }

    private static Sprite LoadOrConfigureSprite(string assetPath, float ppu)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            bool changed = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }
            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }
            if (Mathf.Abs(importer.spritePixelsPerUnit - ppu) > 0.1f)
            {
                importer.spritePixelsPerUnit = ppu;
                changed = true;
            }
            if (changed)
            {
                importer.SaveAndReimport();
            }
        }
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (s == null)
        {
            Debug.LogError("[PanoramicSceneBuilder] Failed to load sprite at: " + assetPath);
        }
        return s;
    }

    private static GameObject BuildPlayerBulletPrefab(Sprite laserGreen)
    {
        GameObject go = new GameObject("PlayerBullet");
        go.layer = 7;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = laserGreen;
        sr.sortingOrder = 8;

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.55f, 0.16f);

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        BulletScript bs = go.AddComponent<BulletScript>();
        bs.speed = 22.0f;
        bs.damage = 2;
        bs.maxX = 22.0f;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/PlayerBullet.prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static GameObject BuildBossBulletPrefab(Sprite laserRed)
    {
        GameObject go = new GameObject("BossBullet");
        go.layer = 9;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = laserRed;
        sr.sortingOrder = 8;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.18f;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        BossBulletScript bbs = go.AddComponent<BossBulletScript>();
        bbs.speed = 11.5f;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/BossBullet.prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static GameObject BuildPipePrefab(Sprite rockDown, Sprite rockUp, Sprite starGold)
    {
        GameObject root = new GameObject("Pipe");
        PipeMoveScript move = root.AddComponent<PipeMoveScript>();
        move.moveSpeed = 5.5f;
        move.deadZone = -16.0f;

        // Top Rock Pillar
        GameObject top = new GameObject("TopPipe");
        top.transform.SetParent(root.transform, false);
        top.transform.localPosition = new Vector3(0f, 5.4f, 0f);
        top.transform.localScale = new Vector3(1.25f, 2.8f, 1f);
        SpriteRenderer topSR = top.AddComponent<SpriteRenderer>();
        topSR.sprite = rockDown;
        topSR.sortingOrder = 2;
        top.AddComponent<PolygonCollider2D>();

        // Bottom Rock Pillar
        GameObject bot = new GameObject("BottomPipe");
        bot.transform.SetParent(root.transform, false);
        bot.transform.localPosition = new Vector3(0f, -5.4f, 0f);
        bot.transform.localScale = new Vector3(1.25f, 2.8f, 1f);
        SpriteRenderer botSR = bot.AddComponent<SpriteRenderer>();
        botSR.sprite = rockUp;
        botSR.sortingOrder = 2;
        bot.AddComponent<PolygonCollider2D>();

        // Middle Trigger + Gold Star Visual
        GameObject mid = new GameObject("MiddleTrigger");
        mid.layer = 3;
        mid.transform.SetParent(root.transform, false);
        mid.transform.localPosition = Vector3.zero;
        BoxCollider2D midCol = mid.AddComponent<BoxCollider2D>();
        midCol.isTrigger = true;
        midCol.size = new Vector2(0.8f, 3.8f);
        mid.AddComponent<PipeMiddleScript>();

        GameObject star = new GameObject("GoldStar");
        star.transform.SetParent(mid.transform, false);
        star.transform.localPosition = Vector3.zero;
        star.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
        SpriteRenderer starSR = star.AddComponent<SpriteRenderer>();
        starSR.sprite = starGold;
        starSR.sortingOrder = 4;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/Pipe.prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static void CreateSkyPanel(string name, Sprite skySprite, Vector3 pos)
    {
        GameObject sky = new GameObject(name);
        sky.transform.position = pos;
        sky.transform.localScale = new Vector3(1.8f, 3.2f, 1f);
        SpriteRenderer sr = sky.AddComponent<SpriteRenderer>();
        sr.sprite = skySprite;
        sr.sortingOrder = -10;
    }

    private static void CreateGroundSegment(string name, Sprite groundSprite, Vector3 pos)
    {
        GameObject ground = new GameObject(name);
        ground.transform.position = pos;
        ground.transform.localScale = new Vector3(1.6f, 1.5f, 1f);
        SpriteRenderer sr = ground.AddComponent<SpriteRenderer>();
        sr.sprite = groundSprite;
        sr.sortingOrder = 5;

        BoxCollider2D col = ground.AddComponent<BoxCollider2D>();
        col.offset = new Vector2(0f, -0.15f);
        col.size = new Vector2(8.08f, 0.5f);

        ScrollingGround scroll = ground.AddComponent<ScrollingGround>();
        scroll.scrollSpeed = 5.5f;
        scroll.thresholdX = -18.5f;
        scroll.resetOffset = 37.5f;
    }

    private static void InstantiateObstacleStage(GameObject pipePrefab, string name, Vector3 pos)
    {
        GameObject inst = PrefabUtility.InstantiatePrefab(pipePrefab) as GameObject;
        if (inst != null)
        {
            inst.name = name;
            inst.transform.position = pos;
        }
    }
}
#endif
