#nullable enable

using System;
using System.Collections.Generic;
using BloodCraftUI.UI.CustomLib;
using UnityEngine;

namespace BloodCraftUI.UI.UniverseLib.UI.Panels;

public class PanelDragger : IDragger
{
    // Static

    private const int ResizeThickness = 10;

    // Instance

    public PanelBase UIPanel { get; private set; }
    public bool AllowDrag => UIPanel.CanDrag;
    public ResizeTypes AllowResize => UIPanel.CanResize;

    public RectTransform PanelRect { get; set; }
    public event Action? OnFinishResize;
    public event Action? OnFinishDrag;

    // Common

    private Vector2 _initialMousePos;
    private Vector2 _initialValue;

    // Dragging
    public RectTransform DraggableArea { get; set; }
    public bool WasDragging { get; set; }

    // Resizing
    public bool WasResizing { get; set; }
    private bool IsHoverResize { get; set; }
    private bool WasHoveringResize =>
        PanelManager.resizeCursor != null &&
        PanelManager.resizeCursor.activeInHierarchy;

    private ResizeTypes _currentResizeType = ResizeTypes.None;
    private ResizeTypes _lastResizeHoverType;
    private Rect _totalResizeRect;

    public PanelDragger(PanelBase uiPanel, RectTransform draggableArea)
    {
        UIPanel = uiPanel;
        DraggableArea = draggableArea;
        PanelRect = uiPanel.PanelRect;

        UpdateResizeCache();
    }

    public virtual void Update(MouseState.ButtonState state, Vector3 rawMousePos)
    {
        if(UIPanel.IsPinned) return;

        if (!(AllowDrag || AllowResize > 0)) return;

        Vector3 resizePos = PanelRect.InverseTransformPoint(rawMousePos);
        ResizeTypes type = GetResizeType(resizePos);
        bool inResizePos = type != ResizeTypes.None;

        Vector3 dragPos = DraggableArea.InverseTransformPoint(rawMousePos);
        bool inDragPos = DraggableArea.rect.Contains(dragPos);

        if (state.HasFlag(MouseState.ButtonState.Clicked))
        {
            if (inDragPos || inResizePos)
            {
                UIPanel.SetActive(true);
                PanelManager.draggerHandledThisFrame = true;
            }

            // Resize with priority as actually shows an icon change (maybe show an icon for drag as well?)
            if (inResizePos)
            {
                OnBeginResize(type);
            }
            else if (inDragPos)
            {
                OnBeginDrag();
            }
        }
        else if (state.HasFlag(MouseState.ButtonState.Down))
        {
            if (WasDragging || WasResizing)
            {
                PanelManager.draggerHandledThisFrame = true;
            }

            if (WasDragging)
            {
                OnDrag();
            }
            else if (WasResizing)
            {
                OnResize();
            }
        }
        else if (state.HasFlag(MouseState.ButtonState.Released) || state.HasFlag(MouseState.ButtonState.Up))
        {
            if (WasDragging)
            {
                OnEndDrag();
            }
            else if (WasResizing)
            {
                OnEndResize();
            }

            if (WasHoveringResize)
            {
                if (inResizePos)
                {
                    OnHoverResize(type);
                }
                else
                {
                    OnHoverResizeEnd();
                }
            }
        }
        else // mouse moving when not clicked
        {
            if (inResizePos)
            {
                IsHoverResize = true;
                OnHoverResize(type);
            }
            else if (!WasResizing && IsHoverResize)
            {
                OnHoverResizeEnd();
            }
        }

        if (WasHoveringResize && PanelManager.resizeCursor)
            UpdateHoverImagePos();
    }

    #region DRAGGING

    public virtual void OnBeginDrag()
    {
        PanelManager.wasAnyDragging = true;
        WasDragging = true;
        _initialMousePos = UIPanel.Owner.Panels.MousePosition;
        _initialValue = PanelRect.anchoredPosition;
    }

    public virtual void OnDrag()
    {
        var mousePos = (Vector2)UIPanel.Owner.Panels.MousePosition;

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

    #region RESIZE

    private readonly Dictionary<ResizeTypes, Rect> _resizeMask = new()
    {
        { ResizeTypes.Top, default },
        { ResizeTypes.Left, default },
        { ResizeTypes.Right, default },
        { ResizeTypes.Bottom, default },
    };

    [Flags]
    public enum ResizeTypes : ulong
    {
        None = 0,
        Top = 1,
        Left = 2,
        Right = 4,
        Bottom = 8,
        TopLeft = Top | Left,
        TopRight = Top | Right,
        BottomLeft = Bottom | Left,
        BottomRight = Bottom | Right,
        Vertical = Top | Bottom,
        Horizontal = Left | Right,
        All = Top | Left | Right | Bottom,
    }

    private const int DoubleThickness = ResizeThickness * 2;

    private void UpdateResizeCache()
    {
        _totalResizeRect = new Rect(PanelRect.rect.x - ResizeThickness + 1,
            PanelRect.rect.y - ResizeThickness + 1,
            PanelRect.rect.width + DoubleThickness - 2,
            PanelRect.rect.height + DoubleThickness - 2);

        // calculate the four cross-sections to use as flags
        if (AllowResize.HasFlag(ResizeTypes.Bottom))
        {
            _resizeMask[ResizeTypes.Bottom] = new Rect(
                _totalResizeRect.x,
                _totalResizeRect.y,
                _totalResizeRect.width,
                ResizeThickness);
        }

        if (AllowResize.HasFlag(ResizeTypes.Left))
        {
            _resizeMask[ResizeTypes.Left] = new Rect(
                _totalResizeRect.x,
                _totalResizeRect.y,
                ResizeThickness,
                _totalResizeRect.height);
        }

        if (AllowResize.HasFlag(ResizeTypes.Top))
        {
            _resizeMask[ResizeTypes.Top] = new Rect(
                _totalResizeRect.x,
                PanelRect.rect.y + PanelRect.rect.height - 2,
                _totalResizeRect.width,
                ResizeThickness);
        }

        if (AllowResize.HasFlag(ResizeTypes.Right))
        {
            _resizeMask[ResizeTypes.Right] = new Rect(
                _totalResizeRect.x + PanelRect.rect.width + ResizeThickness - 2,
                _totalResizeRect.y,
                ResizeThickness,
                _totalResizeRect.height);
        }
    }

    protected virtual bool MouseInResizeArea(Vector2 mousePos)
    {
        return _totalResizeRect.Contains(mousePos);
    }

    private ResizeTypes GetResizeType(Vector2 mousePos)
    {
        // Calculate which part of the resize area we're in, if any.

        var mask = ResizeTypes.None;

        if (_resizeMask[ResizeTypes.Top].Contains(mousePos))
            mask |= ResizeTypes.Top;
        else if (_resizeMask[ResizeTypes.Bottom].Contains(mousePos))
            mask |= ResizeTypes.Bottom;

        if (_resizeMask[ResizeTypes.Left].Contains(mousePos))
            mask |= ResizeTypes.Left;
        else if (_resizeMask[ResizeTypes.Right].Contains(mousePos))
            mask |= ResizeTypes.Right;

        return mask;
    }

    public virtual void OnHoverResize(ResizeTypes resizeType)
    {
        if (WasHoveringResize && _lastResizeHoverType == resizeType)
            return;

        // we are entering resize, or the resize type has changed.

        _lastResizeHoverType = resizeType;

        if (PanelManager.resizeCursorUIBase != null)
            PanelManager.resizeCursorUIBase.Enabled = true;
        if (PanelManager.resizeCursor == null)
            return;

        PanelManager.resizeCursor.SetActive(true);

        // set the rotation for the resize icon
        float iconRotation = 0f;
        switch (resizeType)
        {
            case ResizeTypes.TopRight:
            case ResizeTypes.BottomLeft:
                iconRotation = 45f; break;
            case ResizeTypes.Top:
            case ResizeTypes.Bottom:
                iconRotation = 90f; break;
            case ResizeTypes.TopLeft:
            case ResizeTypes.BottomRight:
                iconRotation = 135f; break;
        }

        Quaternion rot = PanelManager.resizeCursor.transform.rotation;
        rot.eulerAngles = new Vector3(0, 0, iconRotation);
        PanelManager.resizeCursor.transform.rotation = rot;

        UpdateHoverImagePos();
        PanelManager.draggerHandledThisFrame = true;
    }

    // update the resize icon position to be above the mouse
    private void UpdateHoverImagePos()
    {
        Vector3 mousePos = UIPanel.Owner.Panels.MousePosition;
        RectTransform rect = UIPanel.Owner.RootRect;
        if (PanelManager.resizeCursorUIBase != null)
            PanelManager.resizeCursorUIBase.SetOnTop();

        if (PanelManager.resizeCursor != null)
            PanelManager.resizeCursor.transform.localPosition = rect.InverseTransformPoint(mousePos);
    }

    public virtual void OnHoverResizeEnd()
    {
        IsHoverResize = false;
        if (PanelManager.resizeCursorUIBase != null)
            PanelManager.resizeCursorUIBase.Enabled = false;
        if (PanelManager.resizeCursor != null)
            PanelManager.resizeCursor.SetActive(false);
    }

    public virtual void OnBeginResize(ResizeTypes resizeType)
    {
        _currentResizeType = resizeType;
        _initialMousePos = UIPanel.Owner.Panels.MousePosition;
        _initialValue = new Vector2(PanelRect.rect.width, PanelRect.rect.height);
        WasResizing = true;
        PanelManager.Resizing = true;

        var newPivot = new Vector2(
            _currentResizeType.HasFlag(ResizeTypes.Left) ? 1 : 0,
            _currentResizeType.HasFlag(ResizeTypes.Bottom) ? 1 : 0
        );

        PanelRect.SetPivot(newPivot);
    }

    public virtual void OnResize()
    {
        Vector3 mousePos = UIPanel.Owner.Panels.MousePosition;
        Vector2 diff = (_initialMousePos - (Vector2)mousePos) / UIPanel.Owner.Canvas.scaleFactor;

        var width = _initialValue.x;
        var height = _initialValue.y;
        if (_currentResizeType.HasFlag(ResizeTypes.Left))
        {
            width = Math.Max(width + diff.x, UIPanel.MinWidth);
        }
        else if (_currentResizeType.HasFlag(ResizeTypes.Right))
        {
            width = Math.Max(width - diff.x, UIPanel.MinWidth);
        }

        if (_currentResizeType.HasFlag(ResizeTypes.Top))
        {
            height = Math.Max(height - diff.y, UIPanel.MinHeight);
        }
        else if (_currentResizeType.HasFlag(ResizeTypes.Bottom))
        {
            height = Math.Max(height + diff.y, UIPanel.MinHeight);
        }

        PanelRect.sizeDelta = new Vector2(width, height);
    }

    public virtual void OnEndResize()
    {
        WasResizing = false;
        PanelManager.Resizing = false;
        try { OnHoverResizeEnd(); } catch { }

        PanelRect.SetPivot(UIPanel.DefaultPivot);
        UpdateResizeCache();
        OnFinishResize?.Invoke();
    }

    #endregion
}