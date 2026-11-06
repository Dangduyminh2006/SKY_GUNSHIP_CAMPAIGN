using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hệ thống tối ưu hóa bộ nhớ: Tái sử dụng đạn và hiệu ứng hạt thông qua Queue
/// Tránh Garbage Collection Spikes gây tụt FPS
/// </summary>
public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public static ObjectPooler Instance;
    public List<Pool> pools = new List<Pool>();
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        InitializePools();
    }

    private void InitializePools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        if (pools == null) return;

        foreach (Pool pool in pools)
        {
            if (pool == null || string.IsNullOrEmpty(pool.tag)) continue;

            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                if (pool.prefab != null)
                {
                    GameObject obj = Instantiate(pool.prefab, transform);
                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                }
            }

            poolDictionary[pool.tag] = objectPool;
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (poolDictionary == null || !poolDictionary.ContainsKey(tag))
        {
            return null;
        }

        Queue<GameObject> objectPool = poolDictionary[tag];
        if (objectPool.Count == 0) return null;

        GameObject objectToSpawn = objectPool.Dequeue();

        if (objectToSpawn == null)
        {
            Pool poolDef = pools?.Find(p => p.tag == tag);
            if (poolDef != null && poolDef.prefab != null)
            {
                objectToSpawn = Instantiate(poolDef.prefab, transform);
            }
            else
            {
                return null;
            }
        }

        // Vô hiệu hóa tạm thời trước khi dịch chuyển vị trí để ngăn TrailRenderer vẽ vệt nối chéo màn hình
        if (objectToSpawn.activeSelf)
        {
            objectToSpawn.SetActive(false);
        }

        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        TrailRenderer trail = objectToSpawn.GetComponent<TrailRenderer>();
        if (trail != null)
        {
            trail.emitting = false;
            trail.Clear();
        }

        objectToSpawn.SetActive(true);

        if (trail != null)
        {
            trail.Clear();
            trail.emitting = true;
        }

        objectPool.Enqueue(objectToSpawn);

        return objectToSpawn;
    }

    public void DeactivateAllByTag(string tag)
    {
        if (poolDictionary == null || !poolDictionary.ContainsKey(tag)) return;

        foreach (GameObject obj in poolDictionary[tag])
        {
            if (obj != null && obj.activeSelf)
            {
                obj.SetActive(false);
            }
        }
    }
}
