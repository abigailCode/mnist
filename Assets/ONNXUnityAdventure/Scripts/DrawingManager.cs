/*
    Created by Unity Adventure
    Copyright (C) 2023 Unity Adventure. All Rights Reserved.
*/
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DrawingManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool SendImageToModel;
    public int brushSize = 7;

    public GameObject drawingPanel;
    public ONNXManager onnxManager;

    private Color drawColor = Color.white;
    private Color backgroundColor = Color.black;

    private bool isDrawing = false;
    private bool isErasing = false;
    private RectTransform rectTransform;
    private Image drawingImage;
    private Vector2 prevMousePosition;
    private Texture2D drawingTexture;
    [HideInInspector]
    public Texture2D inputTexture;


    private void Start()
    {
        rectTransform = drawingPanel.GetComponent<RectTransform>();
        drawingImage = drawingPanel.GetComponent<Image>();

        drawingTexture = new Texture2D((int)rectTransform.rect.width, (int)rectTransform.rect.height);
        drawingImage.sprite = Sprite.Create(drawingTexture, new Rect(0, 0, drawingTexture.width, drawingTexture.height), Vector2.zero);

        ResetCanvas();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            isDrawing = true;
            prevMousePosition = Vector2.zero;
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            isErasing = true;
            prevMousePosition = Vector2.zero;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            isDrawing = false;
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            isErasing = false;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ResetCanvas();
        }

        if (isDrawing || isErasing)
        {
            Vector2 localCursor;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out localCursor))
            {
                Vector2 mousePos = new Vector2(localCursor.x + rectTransform.rect.width * 0.5f, localCursor.y + rectTransform.rect.height * 0.5f);

                if (prevMousePosition != Vector2.zero)
                {
                    Color colorToUse = isDrawing ? drawColor : backgroundColor;
                    DrawLine(prevMousePosition, mousePos, colorToUse, brushSize);
                }

                prevMousePosition = mousePos;
            }
        }
    }

    public void LateUpdate()
    {
        inputTexture = GetInputTexture();
        if (!SendImageToModel)
        {
            return;
        }
        onnxManager.Process(inputTexture);
    }

    private void DrawLine(Vector2 start, Vector2 end, Color color, int size)
    {
        Texture2D texture = drawingTexture;
        int x0 = (int)start.x;
        int y0 = (int)start.y;
        int x1 = (int)end.x;
        int y1 = (int)end.y;

        int dy = y1 - y0;
        int dx = x1 - x0;
        int stepx, stepy;

        if (dy < 0) { dy = -dy; stepy = -1; }
        else { stepy = 1; }
        if (dx < 0) { dx = -dx; stepx = -1; }
        else { stepx = 1; }
        dy <<= 1;
        dx <<= 1;

        texture.SetPixel(x0, y0, color);

        if (dx > dy)
        {
            int fraction = dy - (dx >> 1);
            while (x0 != x1)
            {
                if (fraction >= 0)
                {
                    y0 += stepy;
                    fraction -= dx;
                }
                x0 += stepx;
                fraction += dy;

                DrawCircle(x0, y0, size, color);
            }
        }
        else
        {
            int fraction = dx - (dy >> 1);
            while (y0 != y1)
            {
                if (fraction >= 0)
                {
                    x0 += stepx;
                    fraction -= dy;
                }
                y0 += stepy;
                fraction += dx;

                DrawCircle(x0, y0, size, color);
            }
        }

        texture.Apply();
    }

    private void DrawCircle(int x, int y, int radius, Color color)
    {
        for (int i = -radius; i <= radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                if (i * i + j * j <= radius * radius)
                {
                    int xPos = x + i;
                    int yPos = y + j;

                    if (xPos >= 0 && xPos < drawingTexture.width && yPos >= 0 && yPos < drawingTexture.height)
                    {
                        drawingTexture.SetPixel(xPos, yPos, color);
                    }
                }
            }
        }
    }

    private void ResetCanvas()
    {
        for (int x = 0; x < drawingTexture.width; x++)
        {
            for (int y = 0; y < drawingTexture.height; y++)
            {
                drawingTexture.SetPixel(x, y, backgroundColor);
            }
        }
        drawingTexture.Apply();
    }

    public Texture2D GetInputTexture()
    {
        int inputWidth = 28;
        int inputHeight = 28;
        var inputTexture = new Texture2D(inputWidth, inputHeight, TextureFormat.R8, false);

        for (int x = 0; x < inputWidth; x++)
        {
            for (int y = 0; y < inputHeight; y++)
            {
                float scaleX = (float)drawingTexture.width / inputWidth;
                float scaleY = (float)drawingTexture.height / inputHeight;
                int sourceX = Mathf.FloorToInt(x * scaleX);
                int sourceY = Mathf.FloorToInt(y * scaleY);

                Color sourceColor = drawingTexture.GetPixel(sourceX, sourceY);
                inputTexture.SetPixel(x, y, sourceColor);
            }
        }

        inputTexture.Apply();
        return inputTexture;
    }
}
