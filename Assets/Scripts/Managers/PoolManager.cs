using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    Dictionary<GameObject, List<GameObject>> stageObjectDict = new Dictionary<GameObject, List<GameObject>>();

    private void Awake()
    {
        
    }

    public void ClearAllPools() 
    {
        foreach (var pair in stageObjectDict)
        {
            List<GameObject> list = pair.Value;

            foreach (GameObject obj in list)
            {
                Destroy(obj);
            }

            list.Clear();
        }

        stageObjectDict.Clear();
    }

    public GameObject GetObject(GameObject _prefab, float time = 10f)
    {
        if (stageObjectDict.ContainsKey(_prefab))
        {
            List<GameObject> listOfPrefabs = stageObjectDict[_prefab];

            foreach (GameObject obj in listOfPrefabs)
            {
                if (obj.activeSelf)
                {
                    continue;
                }
                else
                {
                    obj.SetActive(true);
                    return obj;
                }
            }

            int lastIndex = listOfPrefabs.Count - 1;

            for (int i = 0; i < 10; i++) 
            {
                GameObject newObj = Instantiate(_prefab, transform);
                newObj.SetActive(false);
                listOfPrefabs.Add(newObj);
            }

            return listOfPrefabs[lastIndex + 1];
        }
        else 
        {
            CreateNewList(_prefab);
            return GetObject(_prefab);
        }
    }

    public void ReturnObject(GameObject _obj)
    {
        _obj.SetActive(false);
    }

    private void CreateNewList(GameObject _prefab, int _count = 10) 
    {
        List<GameObject> newList = new List<GameObject>();

        for (int i = 0; i < _count; i++)
        {
            GameObject newObj = Instantiate(_prefab, transform);
            newObj.SetActive(false);
            newList.Add(newObj);
        }

        stageObjectDict.Add(_prefab, newList);
    }
}
    