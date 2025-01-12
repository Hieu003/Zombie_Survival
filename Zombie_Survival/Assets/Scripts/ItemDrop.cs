using UnityEngine;

[CreateAssetMenu(fileName = "ItemDropRates", menuName = "ScriptableObjects/ItemDropRates", order = 1)]
public class ItemDropRates : ScriptableObject
{
    [System.Serializable]
    public class DropRate
    {
        public GameObject itemPrefab;
        public float dropWeight;
    }

    public DropRate[] dropRates;
}