using System.Collections.Generic;
using BloodCraftUI.Config;
using BloodCraftUI.Services;
using BloodCraftUI.UI.CustomLib.Panel;
using BloodCraftUI.UI.CustomLib.Util;
using BloodCraftUI.UI.ModContent.Data;
using BloodCraftUI.UI.UniverseLib.UI;
using BloodCraftUI.UI.UniverseLib.UI.Models;
using BloodCraftUI.UI.UniverseLib.UI.Panels;
using BloodCraftUI.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace BloodCraftUI.UI.ModContent
{
    internal class AdvancedOptionsPanel : ResizeablePanelBase
    {
        public override string PanelId => "AdvancedOptionsPanel";
        public override int MinWidth => 380;
        public override int MinHeight => 420;
        public override Vector2 DefaultAnchorMin => new(0.5f, 0.5f);
        public override Vector2 DefaultAnchorMax => new(0.5f, 0.5f);
        public override Vector2 DefaultPivot => new(0.5f, 0.5f);
        public override bool CanDrag => true;
        public override PanelDragger.ResizeTypes CanResize => PanelDragger.ResizeTypes.All;
        public override PanelType PanelType => PanelType.AdvancedOptions;
        public override float Opacity => Settings.UITransparency;

        private readonly Dictionary<string, string> _shinyTypes = new()
        {
            {"Sangue", "sangue"},
            {"Caos", "caos"}, 
            {"Profana", "profana"},
            {"Ilusão", "ilusao"},
            {"Gelo", "gelo"},
            {"Tempestade", "tempestade"}
        };

        private readonly Dictionary<string, string> _toggleOptions = new()
        {
            {"Emotes", "emotes"},
            {"Shiny Visual", "shiny"}
        };

        public AdvancedOptionsPanel(UIBase owner) : base(owner)
        {
        }

        protected override void ConstructPanelContent()
        {
            SetTitle("🔧 Opções Avançadas");

            var scrollView = UIFactory.CreateScrollView(ContentRoot, "ScrollView", out GameObject scrollContent, out var scrollbar);
            UIFactory.SetLayoutElement(scrollView, flexibleWidth: 9999, flexibleHeight: 9999);

            // Seção de Shiny
            CreateShinySection(scrollContent);
            
            // Seção de Toggles/Opções
            CreateToggleSection(scrollContent);
            
            // Seção de Utilitários
            CreateUtilitiesSection(scrollContent);
        }

        private void CreateShinySection(GameObject parent)
        {
            var shinySection = UIFactory.CreateVerticalGroup(parent, "ShinySection", true, false, true, true, 10,
                new Vector4(15, 15, 15, 15), new Color(0.2f, 0.1f, 0.3f, 0.4f));
            UIFactory.SetLayoutElement(shinySection, minHeight: 140, flexibleWidth: 9999);

            // Título da seção
            var title = UIFactory.CreateLabel(shinySection, "ShinyTitle", "💎 CONFIGURAR SHINY DO FAMILIAR", 
                TMPro.TextAlignmentOptions.Center, Theme.DefaultText, 16);
            title.TextMesh.fontStyle = TMPro.FontStyles.Bold;
            UIFactory.SetLayoutElement(title.GameObject, minHeight: 30, flexibleWidth: 9999);

            // Grid de botões Shiny
            var shinyGrid = UIFactory.CreateVerticalGroup(shinySection, "ShinyGrid", false, false, true, true, 5);
            UIFactory.SetLayoutElement(shinyGrid, flexibleWidth: 9999, flexibleHeight: 0);

            // Primeira linha
            var shinyRow1 = UIFactory.CreateHorizontalGroup(shinyGrid, "ShinyRow1", false, false, true, true, 5);
            UIFactory.SetLayoutElement(shinyRow1, minHeight: 30, flexibleWidth: 9999);

            CreateShinyButton(shinyRow1, "Sangue", "#ff3333");
            CreateShinyButton(shinyRow1, "Caos", "#ff6600");
            CreateShinyButton(shinyRow1, "Profana", "#800080");

            // Segunda linha
            var shinyRow2 = UIFactory.CreateHorizontalGroup(shinyGrid, "ShinyRow2", false, false, true, true, 5);
            UIFactory.SetLayoutElement(shinyRow2, minHeight: 30, flexibleWidth: 9999);

            CreateShinyButton(shinyRow2, "Ilusão", "#ff1493");
            CreateShinyButton(shinyRow2, "Gelo", "#00bfff");
            CreateShinyButton(shinyRow2, "Tempestade", "#9932cc");

            // Info sobre custo
            var costInfo = UIFactory.CreateLabel(shinySection, "CostInfo", "💰 Requer Poeira Vampírica para aplicar/alterar", 
                TMPro.TextAlignmentOptions.Center, new Color(0.8f, 0.8f, 0.6f), 12);
            UIFactory.SetLayoutElement(costInfo.GameObject, minHeight: 20, flexibleWidth: 9999);
        }

        private void CreateShinyButton(GameObject parent, string displayName, string colorHex)
        {
            var btn = UIFactory.CreateButton(parent, $"Shiny{displayName}Btn", displayName);
            UIFactory.SetLayoutElement(btn.GameObject, minHeight: 30, flexibleWidth: 1);
            
            // Aplicar cor do tema
            if (ColorUtility.TryParseHtmlString(colorHex, out Color color))
            {
                btn.Component.GetComponent<Image>().color = new Color(color.r, color.g, color.b, 0.7f);
            }
            
            btn.OnClick = () => {
                if (_shinyTypes.TryGetValue(displayName, out string shinyType))
                {
                    MessageService.EnqueueMessage(string.Format(MessageService.BCCOM_CHOOSESHINY, shinyType));
                    btn.DisableWithTimer(3000);
                }
            };
        }

        private void CreateToggleSection(GameObject parent)
        {
            var toggleSection = UIFactory.CreateVerticalGroup(parent, "ToggleSection", true, false, true, true, 10,
                new Vector4(15, 15, 15, 15), new Color(0.1f, 0.2f, 0.3f, 0.4f));
            UIFactory.SetLayoutElement(toggleSection, minHeight: 120, flexibleWidth: 9999);

            // Título da seção
            var title = UIFactory.CreateLabel(toggleSection, "ToggleTitle", " ALTERNAR OPÇÕES", 
                TMPro.TextAlignmentOptions.Center, Theme.DefaultText, 16);
            title.TextMesh.fontStyle = TMPro.FontStyles.Bold;
            UIFactory.SetLayoutElement(title.GameObject, minHeight: 30, flexibleWidth: 9999);

            // Botões de toggle
            var toggleRow1 = UIFactory.CreateHorizontalGroup(toggleSection, "ToggleRow1", false, false, true, true, 10);
            UIFactory.SetLayoutElement(toggleRow1, minHeight: 35, flexibleWidth: 9999);

            var emotesToggle = UIFactory.CreateButton(toggleRow1, "EmotesToggleBtn", "😊 Alternar Emotes");
            UIFactory.SetLayoutElement(emotesToggle.GameObject, minHeight: 35, flexibleWidth: 1);
            emotesToggle.OnClick = () => {
                MessageService.EnqueueMessage(string.Format(MessageService.BCCOM_TOGGLEOPTIONS, "emotes"));
                emotesToggle.DisableWithTimer(1500);
            };

            var shinyToggle = UIFactory.CreateButton(toggleRow1, "ShinyToggleBtn", " Alternar Shiny Visual");
            UIFactory.SetLayoutElement(shinyToggle.GameObject, minHeight: 35, flexibleWidth: 1);
            shinyToggle.OnClick = () => {
                MessageService.EnqueueMessage(string.Format(MessageService.BCCOM_TOGGLEOPTIONS, "shiny"));
                shinyToggle.DisableWithTimer(1500);
            };

            var toggleRow2 = UIFactory.CreateHorizontalGroup(toggleSection, "ToggleRow2", false, false, true, true, 10);
            UIFactory.SetLayoutElement(toggleRow2, minHeight: 35, flexibleWidth: 9999);

            var unitToggle = UIFactory.CreateButton(toggleRow2, "UnitToggleBtn", "Familiares Unidade");
            UIFactory.SetLayoutElement(unitToggle.GameObject, minHeight: 35, flexibleWidth: 1);
            unitToggle.OnClick = () => {
                MessageService.EnqueueMessage(MessageService.BCCOM_UNITTYPE);
                unitToggle.DisableWithTimer(1500);
            };

            var interactModeButton = UIFactory.CreateButton(toggleRow2, "InteractModeBtn", "Modo Interacao");
            UIFactory.SetLayoutElement(interactModeButton.GameObject, minHeight: 35, flexibleWidth: 1);
            interactModeButton.OnClick = () => {
                MessageService.EnqueueMessage(MessageService.EMOTE_INTERACTMODE);
                interactModeButton.DisableWithTimer(1500);
            };
        }

        private void CreateUtilitiesSection(GameObject parent)
        {
            var utilSection = UIFactory.CreateVerticalGroup(parent, "UtilSection", true, false, true, true, 10,
                new Vector4(15, 15, 15, 15), new Color(0.2f, 0.3f, 0.1f, 0.4f));
            UIFactory.SetLayoutElement(utilSection, minHeight: 140, flexibleWidth: 9999);

            // Título da seção
            var title = UIFactory.CreateLabel(utilSection, "UtilTitle", "🛠️ UTILITÁRIOS E AÇÕES", 
                TMPro.TextAlignmentOptions.Center, Theme.DefaultText, 16);
            title.TextMesh.fontStyle = TMPro.FontStyles.Bold;
            UIFactory.SetLayoutElement(title.GameObject, minHeight: 30, flexibleWidth: 9999);

            // Linha 1: Habilidades e Emotes
            var utilRow1 = UIFactory.CreateHorizontalGroup(utilSection, "UtilRow1", false, false, true, true, 5);
            UIFactory.SetLayoutElement(utilRow1, minHeight: 35, flexibleWidth: 9999);

            var changeAbilityBtn = UIFactory.CreateButton(utilRow1, "ChangeAbilityBtn", "🎯 Trocar Habilidade");
            UIFactory.SetLayoutElement(changeAbilityBtn.GameObject, minHeight: 35, flexibleWidth: 1);
            changeAbilityBtn.OnClick = () => {
                MessageService.EnqueueMessage(MessageService.BCCOM_CHANGEABILITY);
                changeAbilityBtn.DisableWithTimer(2500);
            };

            var listEmotesBtn = UIFactory.CreateButton(utilRow1, "ListEmotesBtn", "📋 Listar Emotes");
            UIFactory.SetLayoutElement(listEmotesBtn.GameObject, minHeight: 35, flexibleWidth: 1);
            listEmotesBtn.OnClick = () => {
                MessageService.EnqueueMessage(MessageService.BCCOM_LISTEMOTES);
                listEmotesBtn.DisableWithTimer(1000);
            };

            // Linha 2: Ações críticas
            var utilRow2 = UIFactory.CreateHorizontalGroup(utilSection, "UtilRow2", false, false, true, true, 5);
            UIFactory.SetLayoutElement(utilRow2, minHeight: 35, flexibleWidth: 9999);

            var resetBtn = UIFactory.CreateButton(utilRow2, "ResetBtn", "Resetar Familiares");
            UIFactory.SetLayoutElement(resetBtn.GameObject, minHeight: 35, flexibleWidth: 1);
            resetBtn.Component.GetComponent<Image>().color = new Color(0.8f, 0.4f, 0.2f, 0.8f);
            resetBtn.OnClick = () => {
                MessageService.EnqueueMessage(MessageService.BCCOM_RESETFAMILIARS);
                resetBtn.DisableWithTimer(4000);
            };

            var prestigeBtn = UIFactory.CreateButton(utilRow2, "PrestigeBtn", " Prestígio");
            UIFactory.SetLayoutElement(prestigeBtn.GameObject, minHeight: 35, flexibleWidth: 1);
            prestigeBtn.Component.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.8f, 0.8f);
            prestigeBtn.OnClick = () => {
                MessageService.EnqueueMessage(MessageService.BCCOM_PRESTIGEFAM);
                prestigeBtn.DisableWithTimer(2000);
            };

            // Info adicional
            var warningInfo = UIFactory.CreateLabel(utilSection, "WarningInfo", 
                " Ações em laranja são irreversíveis - use com cuidado!", 
                TMPro.TextAlignmentOptions.Center, new Color(0.9f, 0.6f, 0.3f), 11);
            UIFactory.SetLayoutElement(warningInfo.GameObject, minHeight: 20, flexibleWidth: 9999);
        }

        internal override void Reset()
        {
            // Reset se necessário
        }

        protected override void OnClosePanelClicked()
        {
            SetActive(false);
        }
    }
}