using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeInfoDisplayer : ProjectBehaviour
{
    public static NodeInfoDisplayer Instance { get; private set; }
    private RectTransform rectTransform;
    private Image image;
    [SerializeField] private TMP_Text displayTitleText;
    [SerializeField] private TMP_Text displayInfoText;
    private NodeVisualBehaviour currentlyDisplayedNode = null;

    private float alphaChangeSpeed = 12f;
    private float currentAlpha;
    private bool addAlpha = false;

    private void Awake()
    {
        Instance = this;
        rectTransform = transform.parent.GetComponent<RectTransform>();
        image = GetComponent<Image>();
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
        displayTitleText.color = new Color(displayTitleText.color.r, displayTitleText.color.g, displayTitleText.color.b, 0f);
        displayInfoText.color = new Color(displayInfoText.color.r, displayInfoText.color.g, displayInfoText.color.b, 0f);
        currentAlpha = 0;
    }

    public void Display(string title, string info, NodeVisualBehaviour nodeVisualBehaviour)
    {
        if (currentlyDisplayedNode == null || nodeVisualBehaviour == currentlyDisplayedNode)
        {
            currentlyDisplayedNode = nodeVisualBehaviour;

            addAlpha = true;

            rectTransform.position = Input.mousePosition;
            displayTitleText.text = title;
            displayInfoText.text = info;
        }
    }

    private void Update()
    {
        rectTransform.position = Input.mousePosition;

        float targetAlpha = addAlpha ? 1.5f : 0f;

        if (currentAlpha == targetAlpha) return;

        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, alphaChangeSpeed * Time.deltaTime);

        if (currentAlpha < 0.02f)
        {
            currentAlpha = 0f;
        }
        else if (currentAlpha > 1.48f)
        {
            currentAlpha = 1.5f;
        }

        image.color = new Color(image.color.r, image.color.g, image.color.b, currentAlpha);
        displayTitleText.color = new Color(displayTitleText.color.r, displayTitleText.color.g, displayTitleText.color.b, currentAlpha);
        displayInfoText.color = new Color(displayInfoText.color.r, displayInfoText.color.g, displayInfoText.color.b, currentAlpha);
    }

    public void Deactivate(NodeVisualBehaviour nodeVisualBehaviour)
    {
        if (currentlyDisplayedNode != nodeVisualBehaviour) return;
        currentlyDisplayedNode = null;

        addAlpha = false;
    }

    //private void LateUpdate()
    //{
    //    IsDisplaying = false;
    //}
}