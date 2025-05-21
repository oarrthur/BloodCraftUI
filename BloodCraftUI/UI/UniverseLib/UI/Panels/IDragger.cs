#nullable enable
using System;
using BloodCraftUI.UI.CustomLib;
using UnityEngine;

namespace BloodCraftUI.UI.UniverseLib.UI.Panels;

public interface IDragger
{
    bool WasDragging { get; set; }
    bool WasResizing { get; set; }
    RectTransform PanelRect { get; set; }
    RectTransform DraggableArea { get; set; }
    event Action OnFinishResize;
    event Action OnFinishDrag;
    void OnEndResize();
    void Update(MouseState.ButtonState state, Vector3 mousePos);
}