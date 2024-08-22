using UnityEngine;

[CreateAssetMenu(fileName = "TilesHolderSO", menuName = "ScriptableObjects/TilesHolderSO")]
public class TilesHolderSO : ScriptableObject
{
    public Sprite[] Tiles;
    public GameObject Prefab;
}