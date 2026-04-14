using System.Collections.Generic;
using UnityEngine;

namespace Match3.Core
{
    public sealed class ObjectPool : MonoBehaviour
    {
        public static ObjectPool Instance { get; private set; }

        private readonly Dictionary<GameObject, List<GameObject>> objectPools =
            new Dictionary<GameObject, List<GameObject>>();

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        public void CreatePool(GameObject prefab, int count)
        {
            if (prefab == null) return;
            if (!objectPools.ContainsKey(prefab))
                objectPools[prefab] = new List<GameObject>();

            var pool = objectPools[prefab];
            for (int i = 0; i < count; i++)
            {
                var obj = Instantiate(prefab, transform);
                obj.SetActive(false);
                pool.Add(obj);
            }
        }

        public GameObject GetObject(GameObject prefab, Transform parent)
        {
            if (prefab == null) return null;
            if (!objectPools.ContainsKey(prefab))
                objectPools[prefab] = new List<GameObject>();

            var pool = objectPools[prefab];
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i] != null && !pool[i].activeSelf)
                {
                    pool[i].transform.SetParent(parent, false);
                    pool[i].SetActive(true);
                    return pool[i];
                }
            }

            var newObj = Instantiate(prefab, parent);
            newObj.SetActive(true);
            pool.Add(newObj);
            return newObj;
        }

        public T GetObject<T>(GameObject prefab, Transform parent) where T : Component
        {
            var obj = GetObject(prefab, parent);
            return obj != null ? obj.GetComponent<T>() : null;
        }

        public void Restore(GameObject obj)
        {
            if (obj == null) return;
            obj.SetActive(false);
            obj.transform.SetParent(transform, false);
        }

        public void Restore_Obj(GameObject prefab)
        {
            if (prefab == null || !objectPools.ContainsKey(prefab)) return;
            var pool = objectPools[prefab];
            for (int i = 0; i < pool.Count; i++)
                if (pool[i] != null && pool[i].activeSelf)
                {
                    pool[i].SetActive(false);
                    pool[i].transform.SetParent(transform, false);
                }
        }

        public void Restore_All()
        {
            foreach (var kvp in objectPools)
                for (int i = 0; i < kvp.Value.Count; i++)
                    if (kvp.Value[i] != null && kvp.Value[i].activeSelf)
                        if (kvp.Value[i].GetComponent<Board>() == null)
                        {
                            kvp.Value[i].SetActive(false);
                            kvp.Value[i].transform.SetParent(transform, false);
                        }
        }
    }
}
