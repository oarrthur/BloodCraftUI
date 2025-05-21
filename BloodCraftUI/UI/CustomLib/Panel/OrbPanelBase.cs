using System;
using System.Linq;
using System.Runtime.CompilerServices;
using BloodCraftUI.UI.ModContent.Data;
using BloodCraftUI.UI.UniverseLib.UI;
using BloodCraftUI.UI.UniverseLib.UI.Models;
using BloodCraftUI.UI.UniverseLib.UI.Panels;
using BloodCraftUI.Utils;
using UnityEngine;
using UnityEngine.UI;
using UIBase = BloodCraftUI.UI.UniverseLib.UI.UIBase;

namespace BloodCraftUI.UI.CustomLib.Panel;

public abstract class OrbPanelBase : UIBehaviourModel, IPanelBase
{
    public UIBase Owner { get; }
    public abstract PanelType PanelType { get; }
    public abstract string PanelId { get; }
    private string PanelConfigKey => $"{PanelType}{PanelId}".Replace("'", "").Replace("\"", "");
    private bool ApplyingSaveData { get; set; } = true;
    public IDragger Dragger { get; internal set; }
    public virtual bool IsPinned { get; protected set; }

    public RectTransform PanelRect { get; protected set; }

    public abstract Vector2 DefaultAnchorMin { get; }
    public abstract Vector2 DefaultAnchorMax { get; }
    public virtual Vector2 DefaultPivot => Vector2.one * 0.5f;
    public virtual Vector2 DefaultPosition { get; }

    protected OrbPanelBase(UIBase owner)
    {
        Owner = owner;
        ConstructUI();
        Owner.Panels.AddPanel(this);
    }

    protected virtual void ConstructUI()
    {
        
    }

    public override void SetActive(bool active)
    {
        if (Enabled != active)
            base.SetActive(active);

        if (!active)
        {
            Dragger.WasDragging = false;
        }
        else
        {
            UIRoot.transform.SetAsLastSibling();
            Owner.Panels.InvokeOnPanelsReordered();
        }
    }

    public void SetActiveOnly(bool active)
    {
        if (Enabled != active)
            base.SetActive(active);

        if (!active)
        {
            Dragger.WasDragging = false;
        }
        else
        {
            UIRoot.transform.SetAsLastSibling();
            Owner.Panels.InvokeOnPanelsReordered();
        }
    }

    public override void Destroy()
    {
        Owner.Panels.RemovePanel(this);
        base.Destroy();
    }

    public void EnsureValidSize()
    {
    }

    protected virtual void ConstructPanelContent()
    {
    }

    /// <summary>
    /// Intended to be called when leaving a server to ensure joining the next can build up the UI correctly again
    /// </summary>
    internal abstract void Reset();

    public virtual void OnFinishDrag()
    {
        SaveInternalData();
    }

    private void SaveInternalData()
    {
        if (ApplyingSaveData) return;

        SetSaveDataToConfigValue();
    }

    private void SetSaveDataToConfigValue()
    {
        Plugin.Instance.Config.Bind("Panels", PanelConfigKey, "", "Serialized panel data").Value = ToSaveData();
    }

    private string ToSaveData()
    {
        try
        {
            return string.Join("|", new string[]
            {
                PanelRect.RectAnchorsToString(),
                PanelRect.RectPositionToString(),
                IsPinned.ToString()
            });
        }
        catch (Exception ex)
        {
            LogUtils.LogWarning($"Exception generating Panel save data: {ex}");
            return "";
        }
    }

    private void ApplySaveData()
    {
        var data = Plugin.Instance.Config.Bind("Panels", PanelConfigKey, "", "Serialized panel data").Value;
        // Load from the old key if the new key is empty. This ensures a good transition to the new format, while not losing existing config.
        // This is deprecated and should be removed in a later version.
        if (string.IsNullOrEmpty(data))
        {
            data = Plugin.Instance.Config.Bind("Panels", $"{PanelType}", "", "Serialized panel data").Value;
        }
        ApplySaveData(data);
    }

    private void ApplySaveData(string data)
    {
        if (string.IsNullOrEmpty(data))
            return;
        string[] split = data.Split('|');

        try
        {
            PanelRect.SetAnchorsFromString(split[0]);
            PanelRect.SetPositionFromString(split[1]);
            if (split.Length > 2 && bool.TryParse(split[2], out var pinned))
                IsPinned = pinned;
            EnsureValidPosition();
        }
        catch
        {
            LogUtils.LogWarning("Invalid or corrupt panel save data! Restoring to default.");
            SetDefaultSizeAndPosition();
            SetSaveDataToConfigValue();
        }
    }

    protected virtual void LateConstructUI()
    {
        ApplyingSaveData = true;

        SetDefaultSizeAndPosition();

        // apply panel save data or revert to default
        try
        {
            ApplySaveData();
        }
        catch (Exception ex)
        {
            LogUtils.LogError($"Exception loading panel save data: {ex}");
            SetDefaultSizeAndPosition();
        }

        ApplyingSaveData = false;
    }

    public virtual void SetDefaultSizeAndPosition()
    {
        PanelRect.localPosition = DefaultPosition;
        PanelRect.pivot = DefaultPivot;

        PanelRect.anchorMin = DefaultAnchorMin;
        PanelRect.anchorMax = DefaultAnchorMax;

        LayoutRebuilder.ForceRebuildLayoutImmediate(PanelRect);

        EnsureValidPosition();

        Dragger.OnEndResize();
    }

    public virtual void EnsureValidPosition()
    {
        var scale = UniversalUI.uiBases.First().Panels.PanelHolder.GetComponent<RectTransform>().localScale.x;
        // Prevent panel going outside screen bounds
        Vector2 pos = PanelRect.anchoredPosition;
        Vector2 dimensions = Owner.Scaler.referenceResolution / scale;
        float halfW = dimensions.x * 0.5f;
        float halfH = dimensions.y * 0.5f;

        // Account for localScale by multiplying width and height
        float scaledWidth = PanelRect.rect.width;
        float scaledHeight = PanelRect.rect.height;

        // Calculate min/max positions accounting for scaled dimensions
        float minPosX = -halfW + scaledWidth * 0.5f;
        float maxPosX = halfW - scaledWidth * 0.5f;
        float minPosY = -halfH + scaledHeight * 0.5f;
        float maxPosY = halfH - scaledHeight * 0.5f;

        // Apply clamping to keep the panel within screen bounds
        pos.x = Math.Clamp(pos.x, minPosX, maxPosX);
        pos.y = Math.Clamp(pos.y, minPosY, maxPosY);
        PanelRect.anchoredPosition = pos;
    }
}