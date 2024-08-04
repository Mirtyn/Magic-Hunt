using UnityEngine;

[CreateAssetMenu()]
public class EnemySO : ScriptableObject
{
    public Sprite Visual;
    //public Sc EnemyScript;
    public EnemyType EnemyType;
}
public enum EnemyType
{
    Zombie
}