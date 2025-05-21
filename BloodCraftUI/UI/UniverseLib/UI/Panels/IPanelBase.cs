using BloodCraftUI.UI.ModContent.Data;
using UnityEngine;

namespace BloodCraftUI.UI.UniverseLib.UI.Panels;

public interface IPanelBase
{
    PanelType PanelType { get; }
    string PanelId { get; }
    bool Enabled { get; set; }
    RectTransform PanelRect { get; }
    GameObject UIRoot { get; }
    UIBase Owner { get; }
    IDragger Dragger { get; }
    bool IsPinned { get; }

    void Destroy();
    void EnsureValidSize();
    void EnsureValidPosition();
    void SetActive(bool active);
    void SetActiveOnly(bool active);
}