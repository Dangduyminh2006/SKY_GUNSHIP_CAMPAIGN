using System.Collections;
using UnityEngine;

/// <summary>
/// Quản lý góc nhìn Toàn cảnh (Pre-Start Panoramic Overview) -> Chiến đấu (Combat View)
/// Kết hợp hiệu ứng rung chấn màn hình (Camera Shake) gắn trực tiếp lên Main Camera.
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    [Header("Panoramic Pre-Start View")]
    public Vector3 panoramicPosition = new Vector3(4.2f, 0.2f, -10f);
    public float panoramicOrthoSize = 6.5f;

    [Header("Active Combat View")]
    public Vector3 combatPosition = new Vector3(0f, 0f, -10f);
    public float combatOrthoSize = 5.0f;
    public float transitionSpeed = 3.2f;

    private Camera cam;
    private bool inCombatView = false;
    private Vector3 targetBasePos;
    private float targetOrthoSize;
    private Vector3 shakeOffset = Vector3.zero;
    private Coroutine shakeCoroutine;

    void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
        targetBasePos = panoramicPosition;
        targetOrthoSize = panoramicOrthoSize;

        transform.localPosition = panoramicPosition;
        if (cam != null && cam.orthographic)
        {
            cam.orthographicSize = panoramicOrthoSize;
        }
    }

    public void TransitionToCombatView()
    {
        inCombatView = true;
        targetBasePos = combatPosition;
        targetOrthoSize = combatOrthoSize;
    }

    void LateUpdate()
    {
        if (inCombatView)
        {
            Vector3 currentBase = transform.localPosition - shakeOffset;
            Vector3 nextBase = Vector3.Lerp(currentBase, targetBasePos, Time.deltaTime * transitionSpeed);
            transform.localPosition = nextBase + shakeOffset;

            if (cam != null && cam.orthographic)
            {
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetOrthoSize, Time.deltaTime * transitionSpeed);
            }
        }
        else
        {
            transform.localPosition = panoramicPosition + shakeOffset;
        }
    }

    public void Shake(float duration, float magnitude)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            shakeOffset = new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero;
        shakeCoroutine = null;
    }
}
