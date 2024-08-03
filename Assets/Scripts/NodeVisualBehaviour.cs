using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Assets.Models;
using TMPro;

public class NodeVisualBehaviour : ProjectBehaviour/*, IPointerEnterHandler, IPointerExitHandler*/
{
    private static Color hoverColor = Color.grey;
    private static Color selectedColor = Color.white;
    private static Color normalColor = new Color(0.15f, 0.15f, 0.15f, 1);
    private static Color completeChainColor = Color.green;
    private static Color incompleteChainColor = Color.red;

    private NodeDragger nodeDragger;
    public INode Node;
    //private Button button;
    private RectTransform rectTransform;
    private bool insideRect = false;
    //private static List<NodeVisualBehaviour> nodeVisualBehaviourRects = new List<NodeVisualBehaviour>();
    private static List<RectTransform> rectTransforms = new List<RectTransform>();
    public Outline Outline;
    public bool Selected;
    public bool InChain;
    public bool Complete;
    [SerializeField] private TMP_Text topBarText;

    //private bool didDragPrevFrame = false;
    //private bool mousePressedBeforeEntering = false;

    //public void OnPointerEnter(PointerEventData eventData)
    //{
    //    Debug.Log("Enter");
    //    insideRect = true;
    //    ((IPointerEnterHandler)button).OnPointerEnter(eventData);
    //}

    //public void OnPointerExit(PointerEventData eventData)
    //{
    //    Debug.Log("Exit");
    //    if (!Input.GetMouseButton(0) && !didDragPrevFrame && nodeDragger.GetCurrentlyGettingDraggedNode() == this)
    //    {
    //        insideRect = false;
    //        nodeDragger.SetIsDraggingNode(false);
    //    }

    //    ((IPointerExitHandler)button).OnPointerExit(eventData);
    //}

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        nodeDragger = this.transform.parent.parent.gameObject.GetComponent<NodeDragger>();
        Outline = this.transform.GetChild(0).GetComponent<Outline>();
        //OnPointerClick( += StartDragging;
        //button.onClick.AddListener(StartDragging);
        //nodeVisualBehaviourRects.Add(this);
        rectTransforms.Add(rectTransform);
    }

    private void Update()
    {
        CheckIfMouseInsideRect();

        TryToDrag();

        //if (insideRect)
        //{
        //    transform.GetChild(0).gameObject.GetComponent<Image>().color = Color.green;
        //}
        //else
        //{
        //    transform.GetChild(0).gameObject.GetComponent<Image>().color = Color.red;
        //}



        if (rectTransform.position.x > Screen.width)
        {
            rectTransform.position = new Vector3(Screen.width, rectTransform.position.y, rectTransform.position.z);
        }
        else if (rectTransform.position.x < 0)
        {
            rectTransform.position = new Vector3(0, rectTransform.position.y, rectTransform.position.z);
        }

        if (rectTransform.position.y > Screen.height)
        {
            rectTransform.position = new Vector3(rectTransform.position.x, Screen.height, rectTransform.position.z);
        }
        else if (rectTransform.position.y < 0)
        {
            rectTransform.position = new Vector3(rectTransform.position.x, 0, rectTransform.position.z);
        }
    }

    public bool CheckIfTouchingOtherNode()
    {
        for (int i = 0; i < rectTransforms.Count; i++)
        {
            if (rectTransforms[i] != this.rectTransform)
            {
                if (CheckIfOverlaps(rectTransforms[i]))
                {
                    if (rectTransforms[i].gameObject.TryGetComponent<NodeVisualBehaviour>(out NodeVisualBehaviour node))
                    {
                        return NodeManager.Instance.TryAttachNodes(node.Node, Node);
                    }
                }
            }
        }

        //if (selectable.gameObject.TryGetComponent<NodeHook>(out NodeHook nodeHook))
        //{
        //    if (rectTransform.rect.Overlaps(selectable.gameObject.GetComponent<RectTransform>().rect))
        //    {
        //        return nodeHook.TryAttachInputNode(Node);
        //    }
        //}
        //if (selectable.gameObject.TryGetComponent<INode>(out INode node))
        //{
        //    if (rectTransform.rect.Overlaps(selectable.gameObject.GetComponent<RectTransform>().rect))
        //    {
        //        return NodeManager.Instance.TryAttachNodes(node, Node);
        //    }
        //}

        return false;
    }

    private void CheckIfMouseInsideRect()
    {
        Vector2 minCorner = new Vector2(rectTransform.position.x, rectTransform.position.y) + new Vector2(rectTransform.rect.min.x / 1920 * Screen.width, rectTransform.rect.min.y / 1080 * Screen.height);
        Vector2 maxCorner = new Vector2(rectTransform.position.x, rectTransform.position.y) + new Vector2(rectTransform.rect.max.x / 1920 * Screen.width, rectTransform.rect.max.y / 1080 * Screen.height);
        Vector2 mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        //Debug.Log("minCorner: " + minCorner);
        //Debug.Log("maxCorner: " + maxCorner);
        //Debug.Log("mousePos: " + mousePos);

        if (minCorner.x < mousePos.x && maxCorner.x > mousePos.x &&
            minCorner.y < mousePos.y && maxCorner.y > mousePos.y)
        {
            insideRect = true;
            if (!Selected)
            {
                if (InChain)
                {
                    if (Complete)
                    {
                        Outline.effectColor = completeChainColor;
                    }
                    else
                    {
                        Outline.effectColor = incompleteChainColor;
                    }
                }
                else
                {
                    Outline.effectColor = hoverColor;
                }
            }
            else
            {
                Outline.effectColor = selectedColor;
            }
        }
        else
        {
            insideRect = false;
            if (!Selected)
            {
                if (InChain)
                {
                    if (Complete)
                    {
                        Outline.effectColor = completeChainColor;
                    }
                    else
                    {
                        Outline.effectColor = incompleteChainColor;
                    }
                }
                else
                {
                    Outline.effectColor = normalColor;
                }
            }
            else
            {
                Outline.effectColor = selectedColor;
            }
        }
    }

    private void TryToDrag()
    {
        //Vector3 offset = this.transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (insideRect && Input.GetMouseButton(0))
        {
            nodeDragger.TryToGetDragged(this);
        }
    }

    public RectTransform GetRectTransform()
    {
        return this.rectTransform;
    }

    public void SetRectPosition(Vector3 position)
    {
        rectTransform.position = position;

        if (rectTransform.position.x > Screen.width)
        {
            rectTransform.position = new Vector3 (Screen.width, rectTransform.position.y, rectTransform.position.z);
        }
        else if (rectTransform.position.x < 0)
        {
            rectTransform.position = new Vector3(0, rectTransform.position.y, rectTransform.position.z);
        }

        if (rectTransform.position.y > Screen.height)
        {
            rectTransform.position = new Vector3(rectTransform.position.x, Screen.height, rectTransform.position.z);
        }
        else if (rectTransform.position.y < 0)
        {
            rectTransform.position = new Vector3(rectTransform.position.x, 0, rectTransform.position.z);
        }


        if (Node.ConnectedNode != null)
        {
            Node.ConnectedNode.NodeVisualBehaviour.SetRectPosition(position + new Vector3(0, -this.rectTransform.rect.height / 1080 * Screen.height, 0));
        }
    }


    /// <summary>
    /// Blue = Input, 
    /// Green = Modifier, 
    /// Cyan = Output
    /// </summary>
    /// <param name="color"></param>

    public void SetColor(Color color)
    {
        transform.GetChild(0).gameObject.GetComponent<Image>().color = color;
    }

    private bool CheckIfOverlaps(RectTransform otherRectTransform)
    {
        float thisMaxX = rectTransform.position.x + rectTransform.rect.xMax / 1080 * Screen.height;
        float thisMinX = rectTransform.position.x + rectTransform.rect.xMin / 1080 * Screen.height;

        float otherMaxX = otherRectTransform.position.x + otherRectTransform.rect.xMax / 1920 * Screen.width;
        float otherMinX = otherRectTransform.position.x + otherRectTransform.rect.xMin / 1920 * Screen.width;

        float thisMaxY = rectTransform.position.y + rectTransform.rect.yMax / 1080 * Screen.height;
        float thisMinY = rectTransform.position.y + rectTransform.rect.yMin / 1080 * Screen.height;

        float otherMaxY = otherRectTransform.position.y + otherRectTransform.rect.yMax / 1920 * Screen.width;
        float otherMinY = otherRectTransform.position.y + otherRectTransform.rect.yMin / 1920 * Screen.width;

        if (thisMaxX > otherMinX && thisMinX < otherMaxX && thisMaxY > otherMinY && thisMinY < otherMaxY)
        {
            return true;
        }

        return false;
    }

    public void SetTopBarText(string text)
    {
        topBarText.text = text;
    }
}
