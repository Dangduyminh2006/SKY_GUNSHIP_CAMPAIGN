using UnityEngine;

/// <summary>
/// Hoạt cảnh quay cánh quạt mượt mà cho phi cơ (Player & Boss)
/// Luân chuyển giữa các frame sprite của Kenney Tappy Plane.
/// </summary>
public class PlaneAnimator : MonoBehaviour
{
    [Header("Animation Frames")]
    public Sprite[] frames;
    public float fps = 15f;

    private SpriteRenderer spriteRenderer;
    private float timer;
    private int currentFrameIndex;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (frames == null || frames.Length == 0 || spriteRenderer == null) return;

        timer += Time.deltaTime;
        if (timer >= 1f / fps)
        {
            timer = 0f;
            currentFrameIndex = (currentFrameIndex + 1) % frames.Length;
            spriteRenderer.sprite = frames[currentFrameIndex];
        }
    }
}

