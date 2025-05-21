#nullable enable

using System;
using BloodCraftUI.UI.UniverseLib.UI.Panels;
using UnityEngine;

namespace BloodCraftUI.UI.CustomLib.Panel;

public class RectTransformDragger : IDragger
{
    // Instance

    public IPanelBase UIPanel { get; private set; }
    public bool AllowDrag => true;

    public RectTransform PanelRect { get; set; }
    public event Action? OnFinishDrag;


    // Common

    private Vector2 _initialMousePos;
    private Vector2 _initialValue;

    // Dragging
    public RectTransform DraggableArea { get; set; }
    public bool WasDragging { get; set; }

    public RectTransformDragger(IPanelBase uiPanel, RectTransform draggableArea)
    {
        UIPanel = uiPanel;
        DraggableArea = draggableArea;
        PanelRect = uiPanel.PanelRect;
    }

    public virtual void Update(MouseState.ButtonState state, Vector3 rawMousePos)
    {
        if (!AllowDrag || UIPanel.IsPinned) return;

        var resizePos = PanelRect.InverseTransformPoint(rawMousePos);
       
        var dragPos = DraggableArea.InverseTransformPoint(rawMousePos);
        var inDragPos = DraggableArea.rect.Contains(dragPos);

        if (state.HasFlag(MouseState.ButtonState.Clicked))
        {
            if (inDragPos)
            {
                UIPanel.SetActive(true);
                PanelManager.draggerHandledThisFrame = true;
            }

            // Resize with priority as actually shows an icon change (maybe show an icon for drag as well?)
            if (inDragPos)
            {
                OnBeginDrag();
            }
        }
        else if (state.HasFlag(MouseState.ButtonState.Down))
        {
            if (WasDragging)
            {
                PanelManager.draggerHandledThisFrame = true;
            }

            if (WasDragging)
            {
                OnDrag();
            }
        }
        else if (state.HasFlag(MouseState.ButtonState.Released) || state.HasFlag(MouseState.ButtonState.Up))
        {
            if (WasDragging)
            {
                OnEndDrag();
            }
        }

    }

    #region DRAGGING

    public virtual void OnBeginDrag()
    {
        PanelManager.wasAnyDragging = true;
        WasDragging = true;
        _initialMousePos = Input.mousePosition;
        _initialValue = PanelRect.anchoredPosition;
    }

    public virtual void OnDrag()
    {
        var mousePos = (Vector2)Input.mousePosition;

        var diff = mousePos - _initialMousePos;

        PanelRect.anchoredPosition = _initialValue + diff / UIPanel.Owner.Canvas.scaleFactor;

        UIPanel.EnsureValidPosition();
    }

    public virtual void OnEndDrag()
    {
        WasDragging = false;

        OnFinishDrag?.Invoke();
    }

    #endregion

    #region cOMPATIBILITY

    public void OnEndResize()
    {
    }
    public event Action? OnFinishResize;
    public bool WasResizing { get; set; }


    #endregion
}