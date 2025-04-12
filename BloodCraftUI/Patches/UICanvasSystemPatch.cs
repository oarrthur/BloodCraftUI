using BloodCraftUI.NewUI;
using BloodCraftUI.NewUI.UniverseLib.UI;
using HarmonyLib;
using ProjectM.UI;

namespace BloodCraftUI.Patches;

public static class UICanvasSystemPatch
{
    private static bool hudEnabled = false;
    [HarmonyPatch(typeof(UICanvasSystem), "UpdateHideIfDisabled")]
    [HarmonyPostfix]
    private static void UICanvasSystemPostfix(UICanvasBase canvas)
    {
        if (!UIFactory.PlayerHUDCanvas)
        {
            UIFactory.PlayerHUDCanvas = canvas.CharacterHUDs.gameObject;
        }

        if (!canvas.HUDMenuParent.gameObject.active || !BCUIManager.IsInitialized) return;
        var anyChildActive = false;
        for (var i = 0; i < canvas.HUDMenuParent.childCount && !anyChildActive; i++)
        {
            anyChildActive |= canvas.HUDMenuParent.GetChild(i).gameObject.active;
        }

        // If there is a child of HUDMenuParent active, then we want to hide our UI. Check if we match state then switch if needed.
        if (anyChildActive != hudEnabled) return;

        hudEnabled = !anyChildActive;
        BCUIManager.SetActive(hudEnabled);
    }
}