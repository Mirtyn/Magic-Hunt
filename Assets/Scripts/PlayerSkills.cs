using UnityEngine;

public class PlayerSkills : ProjectBehaviour
{
    public static PlayerSkills Instance { get; private set; }
    public Transform ThisTransform { get; private set; }
    public float SpawnProjectilesDistance { get; private set; } = 0.8f;

    private void Awake()
    {
        Instance = this;
        ThisTransform = transform;
    }

    private void Start()
    {
        PlayerInput.Instance.AlphaKeyPressed += AlphaKeyPressed;
    }

    private void AlphaKeyPressed(object sender, PlayerInput.AlphaKeyPressedEventArgs e)
    {
        if (InventoryOpen) return;

        switch (e.Key)
        {
            case KeyCode.Alpha1:
                TryUseSkill(1);
                break;
            case KeyCode.Alpha2:
                TryUseSkill(2);
                break;
            case KeyCode.Alpha3:
                TryUseSkill(3);
                break;
            case KeyCode.Alpha4:
                TryUseSkill(4);
                break;
            case KeyCode.Alpha5:
                TryUseSkill(5);
                break;
        }
    }

    private void TryUseSkill(int skillNum)
    {
        skillNum--;

        INodeInput skill = NodeManager.Instance.Skills[skillNum];

        if (skill == null) return;

        if (skill.NodeVisualBehaviour.Complete)
        {
            skill.Activate();
        }
    }
}
