using System;
using UnityEngine;
using Assets.Models;
using UnityEngine.UI;

public class NodeManager : ProjectBehaviour
{
    public static NodeManager Instance;
    private Inventory inventory = new Inventory();
    //public List<INodeInput> storedInputs = new List<INodeInput>();
    //public List<INodeModifier> storedModifiers = new List<INodeModifier>();
    //public List<INodeOutput> storedOutputs = new List<INodeOutput>();
    private Transform childTransform;
    //[SerializeField] private NodeHook hook1;
    //[SerializeField] private NodeHook hook2;
    //[SerializeField] private NodeHook hook3;

    //private int skillSlots = 5;

    public INodeInput[] Skills = new INodeInput[5];

    //private void OnDisable()
    //{
    //    for(int i = 0; i < storedInputs.Count; i++)
    //    {
    //        for (int j = 0; j < Skills.Length; j++)
    //        {
    //            if (storedInputs[i] == Skills[j])
    //            {
    //                storedInputs[i] = null;
    //            }
    //        }
    //    }
    //}

    private void Awake()
    {
        Instance = this;

        childTransform = this.transform.GetChild(0);
        //NodeChain1.NodeHook = hook1;
        //NodeChain2.NodeHook = hook2;
        //NodeChain3.NodeHook = hook3;

        //hook1.AttachedNodeChain = NodeChain1;
        //hook2.AttachedNodeChain = NodeChain2;
        //hook3.AttachedNodeChain = NodeChain3;

        CreateNewInputNode<ShootProjectileInput>();
        CreateNewInputNode<TestInput>();
        CreateNewModifierNode<TestModifier>();
        CreateNewModifierNode<TestModifier>();
        CreateNewOutputNode<TestOutput>();
        CreateNewOutputNode<TestOutput>();

        //storedInputs.Add(n1);

        //storedModifiers.Add(n4);
        //storedModifiers.Add(n5);

        //storedOutputs.Add(n6);
        //storedOutputs.Add(n7);

        //inventory.StoreNode(n1);
        //inventory.StoreNode(n2);
        //inventory.StoreNode(n3);

        //inventory.StoreNode(n4);
        //inventory.StoreNode(n5);

        //InputNode1 = transform.GetChild(0).GetComponent<INodeInput>();
        //NodeChain1.InputNode = InputNode1;
        //NodeChain2.InputNode = InputNode2;
        //NodeChain3.InputNode = InputNode3;

        //NodeChain1.OutputNode = OutputNode1;
        //NodeChain2.OutputNode = OutputNode2;
        //NodeChain3.OutputNode = OutputNode3;
    }

    private void Start()
    {
        //PlayerInput.Instance.Mouse0Pressed += ReceiveEventMouse0;
        OnInventoryOpened += NodeManager_OnInventoryOpened;
        OnInventoryClosed += NodeManager_OnInventoryClosed;
        childTransform.gameObject.SetActive(false);
    }

    private void NodeManager_OnInventoryOpened(object sender, EventArgs e)
    {
        childTransform.gameObject.SetActive(true);
    }

    private void NodeManager_OnInventoryClosed(object sender, EventArgs e)
    {
        childTransform.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (PauseGame) return;
        if (!InventoryOpen) return;

        for (int i = 0; i < Skills.Length; i++)
        {
            if (Skills[i] != null)
            {
                if (Skills[i].SendHeartBeat())
                {
                    Skills[i].NodeVisualBehaviour.Complete = true;
                }
                else
                {
                    Skills[i].NodeVisualBehaviour.Complete = false;
                }
            }
        }
    }

    public T CreateNewInputNode<T>() where T : INodeInput, new()
    {
        float width = Screen.width / 3;
        float height = Screen.height / 3;
        Vector2 pos = new Vector2(UnityEngine.Random.Range(-width, width) + Screen.width / 2, UnityEngine.Random.Range(-height, height) + Screen.height / 2);

        GameObject nodeObject = Instantiate(GameManager.NodePrefab, pos, Quaternion.identity, childTransform);

        T node = new T();
        node.CreateNode(nodeObject.GetComponent<NodeVisualBehaviour>());
        return node;
    }

    public T CreateNewModifierNode<T>() where T : INodeModifier, new()
    {
        float width = Screen.width / 3;
        float height = Screen.height / 3;
        Vector2 pos = new Vector2(UnityEngine.Random.Range(-width, width) + Screen.width / 2, UnityEngine.Random.Range(-height, height) + Screen.height / 2);

        GameObject nodeObject = Instantiate(GameManager.NodePrefab, pos, Quaternion.identity, childTransform);

        T node = new T();
        node.CreateNode(nodeObject.GetComponent<NodeVisualBehaviour>());
        return node;
    }

    public T CreateNewOutputNode<T>() where T : INodeOutput, new()
    {
        float width = Screen.width / 3;
        float height = Screen.height / 3;
        Vector2 pos = new Vector2(UnityEngine.Random.Range(-width, width) + Screen.width / 2, UnityEngine.Random.Range(-height, height) + Screen.height / 2);

        GameObject nodeObject = Instantiate(GameManager.NodePrefab, pos, Quaternion.identity, childTransform);

        T node = new T();
        node.CreateNode(nodeObject.GetComponent<NodeVisualBehaviour>());
        return node;
    }

    //public void CreateNewNode<T>(T nodeType) where T : Component
    //{
    //    GameObject nodeObject = Instantiate<GameObject>(GameManager.NodePrefab, this.transform);
    //    var node = nodeObject.AddComponent<T>();
    //}

    public bool TryAttachNodes(INode topNode, INode attachNode)
    {
        if (topNode is INodeInput)
        {
            if (attachNode is INodeInput)
            {
                return false;
            }
            else if (attachNode is INodeModifier)
            {
                return topNode.TryAttachNode(attachNode);
            }
            else if (attachNode is INodeOutput)
            {
                return topNode.TryAttachNode(attachNode);
            }
        }
        else if(topNode is INodeModifier)
        {
            if (attachNode is INodeInput)
            {
                return false;
            }
            else if (attachNode is INodeModifier)
            {
                return topNode.TryAttachNode(attachNode);
            }
            else if (attachNode is INodeOutput)
            {
                return topNode.TryAttachNode(attachNode);
            }
        }
        else if (topNode is INodeOutput)
        {
            return false;
        }

        Debug.LogWarning("!!! Unknown node with no correct Interface !!!");

        return false;
    }

    public void SetSkill(int skillNum, INodeInput node)
    {
        skillNum--;

        if (Skills[skillNum] == node)
        {
            node.NodeVisualBehaviour.InChain = false;
            node.NodeVisualBehaviour.UnAssignKey();
            Skills[skillNum] = null;
            return;
        }

        for (int i = 0; i < Skills.Length; i++)
        {
            if (node == Skills[i])
            {
                Skills[i] = null;
            }
        }

        if (Skills[skillNum] != null)
        {
            Skills[skillNum].NodeVisualBehaviour.InChain = false;
            Skills[skillNum].NodeVisualBehaviour.UnAssignKey();
        }

        Skills[skillNum] = node;
        node.NodeVisualBehaviour.InChain = true;

        if (node.SendHeartBeat())
        {
            node.NodeVisualBehaviour.Complete = true;
        }
        else
        {
            node.NodeVisualBehaviour.Complete = false;
        }
    }
}
