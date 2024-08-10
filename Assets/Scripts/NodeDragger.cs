using UnityEditor;
using UnityEngine;

public class NodeDragger : ProjectBehaviour
{
    private bool isDraggingNode = false;
    private NodeVisualBehaviour currentlyGettingDraggedNode;
    public NodeVisualBehaviour CurrentlySelectedNode;

    private void Start()
    {
        PlayerInput.Instance.AlphaKeyPressed += AlphaKeyPressed;
        OnInventoryOpened += NodeDragger_OnInventoryClosed;
        OnInventoryClosed += NodeDragger_OnInventoryOpened;
    }

    private void NodeDragger_OnInventoryOpened(object sender, System.EventArgs e)
    {

    }

    private void NodeDragger_OnInventoryClosed(object sender, System.EventArgs e)
    {

    }

    private void Update()
    {
        if (PauseGame) return;
        if (!InventoryOpen) return;

        if (Input.GetMouseButtonDown(0) && CurrentlySelectedNode != null)
        {
            CurrentlySelectedNode.Selected = false;
            CurrentlySelectedNode = null;
        }

        if (Input.GetMouseButton(0) && isDraggingNode)
        {
            currentlyGettingDraggedNode.Node.TryDetachNode();

            currentlyGettingDraggedNode.SetRectPosition(Input.mousePosition);

            currentlyGettingDraggedNode.transform.SetAsLastSibling();
        }
        else if (isDraggingNode)
        {
            CurrentlySelectedNode = currentlyGettingDraggedNode;
            CurrentlySelectedNode.Selected = true;
            isDraggingNode = false;

            if (currentlyGettingDraggedNode.CheckIfTouchingOtherNode())
            {
                RectTransform rectTransform = currentlyGettingDraggedNode.Node.PrevConnectedNode.NodeVisualBehaviour.GetRectTransform();
                currentlyGettingDraggedNode.SetRectPosition(rectTransform.position + new Vector3(0, -rectTransform.rect.height / 1080 * Screen.height, 0));
            }
        }
    }

    private void AlphaKeyPressed(object sender, PlayerInput.AlphaKeyPressedEventArgs e)
    {
        if (!InventoryOpen) return;

        if (CurrentlySelectedNode == null) return;

        if (!(CurrentlySelectedNode.Node is INodeInput)) return;

        switch (e.Key)
        {
            case KeyCode.Alpha1:
                CurrentlySelectedNode.AssignKey(e.Key);
                NodeManager.Instance.SetSkill(1, CurrentlySelectedNode.Node as INodeInput);
                break;
            case KeyCode.Alpha2:
                CurrentlySelectedNode.AssignKey(e.Key);
                NodeManager.Instance.SetSkill(2, CurrentlySelectedNode.Node as INodeInput);
                break;
            case KeyCode.Alpha3:
                CurrentlySelectedNode.AssignKey(e.Key);
                NodeManager.Instance.SetSkill(3, CurrentlySelectedNode.Node as INodeInput);
                break;
            case KeyCode.Alpha4:
                CurrentlySelectedNode.AssignKey(e.Key);
                NodeManager.Instance.SetSkill(4, CurrentlySelectedNode.Node as INodeInput);
                break;
            case KeyCode.Alpha5:
                CurrentlySelectedNode.AssignKey(e.Key);
                NodeManager.Instance.SetSkill(5, CurrentlySelectedNode.Node as INodeInput);
                break;
        }
    }

    public bool IsDraggingNode()
    {
        return isDraggingNode;
    }

    //public void SetIsDraggingNode(bool isDraggingNode)
    //{
    //    this.isDraggingNode = isDraggingNode;
    //}

    //public NodeVisualBehaviour GetCurrentlyGettingDraggedNode()
    //{
    //    return currentlyGettingDraggedNode;
    //}

    public bool TryToGetDragged(NodeVisualBehaviour nodeVisualBehaviour)
    {
        if (currentlyGettingDraggedNode == nodeVisualBehaviour)
        {
            isDraggingNode = true;
            return true;
        }

        if (!isDraggingNode)
        {
            isDraggingNode = true;
            currentlyGettingDraggedNode = nodeVisualBehaviour;
            return true;
        }

        return false;
    }

    //public void SetCurrentlyGettingDraggedNode(NodeVisualBehaviour nodeVisualBehaviour)
    //{
    //    currentlyGettingDraggedNode = nodeVisualBehaviour;
    //}
}
