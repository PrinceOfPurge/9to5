using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class FoodPooler : MonoBehaviour
{
    public static FoodPooler Instance;
    [SerializeField] private PooledFood[] foodPrefabs;
    private Dictionary<int, IObjectPool<PooledFood>> _pools = new Dictionary<int, IObjectPool<PooledFood>>();

    void Awake()
    {
        Instance = this;
        for (int i = 0; i < foodPrefabs.Length; i++)
        {
            int index = i; 
            _pools.Add(index, new ObjectPool<PooledFood>(
                createFunc: () => Instantiate(foodPrefabs[index]),
                actionOnGet: (obj) => obj.gameObject.SetActive(true),
                actionOnRelease: (obj) => obj.gameObject.SetActive(false),
                defaultCapacity: 10, maxSize: 30
            ));
        }
    }

    public PooledFood GetFood(int typeIndex)
    {
        var food = _pools[typeIndex].Get();
        food.SetPool(_pools[typeIndex]);
        return food;
    }
}