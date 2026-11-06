using UnityEngine;

/// <summary>
/// Sinh ngẫu nhiên các cặp chướng ngại vật đá + ngôi sao vàng theo chu kỳ sau khi người chơi nhấn Space.
/// Kiểm soát chặt chẽ số lượng cột sinh ra trong mỗi đợt để đảm bảo đúng 10 sao vàng,
/// tuyệt đối không bị dư thừa ngôi sao thứ 11 trước khi gặp Boss.
/// </summary>
public class PipeSpawnScript : MonoBehaviour
{
    [UnityEngine.Serialization.FormerlySerializedAs("pipe")]
    public GameObject pipePrefab;
    public GameObject pipe;
    public float spawnRate = 2.2f;
    public float heightOffset = 1.6f;
    public bool isSpawning = true;
    public int pipesRemaining = 7; // Đợt 1 có 3 cột tạo sẵn, cần sinh đúng 7 cột để đạt tổng 10 sao
    private float timer = 0f;
    private LogicScript logic;

    void Start()
    {
        logic = FindObjectOfType<LogicScript>();

        // Tự động đếm số lượng chướng ngại vật đã đặt sẵn trong Scene
        PipeMiddleScript[] prePlaced = FindObjectsOfType<PipeMiddleScript>();
        int prePlacedCount = (prePlaced != null) ? prePlaced.Length : 3;
        pipesRemaining = Mathf.Max(0, 10 - prePlacedCount);
    }

    void Update()
    {
        if (!isSpawning) return;
        if (logic != null && (!logic.IsGameStarted || logic.IsGameEnded)) return;

        if (pipesRemaining <= 0)
        {
            isSpawning = false;
            return;
        }

        if (timer < spawnRate)
        {
            timer += Time.deltaTime;
        }
        else
        {
            SpawnPipe();
            timer = 0f;
        }
    }

    void SpawnPipe()
    {
        if (pipesRemaining <= 0)
        {
            isSpawning = false;
            return;
        }

        GameObject targetPrefab = pipePrefab != null ? pipePrefab : pipe;
        if (targetPrefab == null) return;

        float lowestPoint = transform.position.y - heightOffset;
        float highestPoint = transform.position.y + heightOffset;

        Instantiate(
            targetPrefab,
            new Vector3(transform.position.x, Random.Range(lowestPoint, highestPoint), 0f),
            Quaternion.identity
        );
        LogicScript.HydrateSceneSprites();

        pipesRemaining--;
        if (pipesRemaining <= 0)
        {
            isSpawning = false; // Ngừng sinh ngay khi đã đủ 10 chướng ngại vật
        }
    }

    public void SetSpawning(bool active)
    {
        isSpawning = active;
    }

    /// <summary>
    /// Tái lập số lượng cột cần sinh cho đợt tiếp theo (mỗi đợt đúng 10 cột = 10 sao vàng).
    /// </summary>
    public void ResetForWave(int count = 10)
    {
        pipesRemaining = count;
        isSpawning = true;
        timer = 0f;
    }

    /// <summary>
    /// Dọn sạch các chướng ngại vật còn sót lại ngoài tầm nhìn phía trước khi bắt đầu đấu Boss
    /// </summary>
    public void ClearExcessPipes()
    {
        isSpawning = false;
        PipeMoveScript[] allPipes = FindObjectsOfType<PipeMoveScript>();
        foreach (PipeMoveScript p in allPipes)
        {
            if (p != null && p.transform.position.x > 8.0f)
            {
                Destroy(p.gameObject);
            }
        }
    }
}
