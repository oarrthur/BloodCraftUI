using System.Collections.Generic;
using BloodCraftUI.Config;
using BloodCraftUI.Services;
using BloodCraftUI.UI.CustomLib.Controls;
using BloodCraftUI.UI.CustomLib.Panel;
using BloodCraftUI.UI.CustomLib.Util;
using BloodCraftUI.UI.ModContent.Data;
using BloodCraftUI.UI.UniverseLib.UI;
using BloodCraftUI.UI.UniverseLib.UI.Models;
using BloodCraftUI.UI.UniverseLib.UI.Panels;
using BloodCraftUI.Utils;
using UnityEngine;
using UnityEngine.UI;
using UIBase = BloodCraftUI.UI.UniverseLib.UI.UIBase;

namespace BloodCraftUI.UI.ModContent
{
    public class ContentPanel : ResizeablePanelBase
    {
        public override string PanelId => "CorePanel";

        public override int MinWidth => Settings.UseHorizontalContentLayout ? 340 : 100;
        public override int MinHeight => 15;
        public override Vector2 DefaultAnchorMin => new Vector2(0.5f, 0.5f);
        public override Vector2 DefaultAnchorMax => new Vector2(0.5f, 0.5f);
        public override Vector2 DefaultPivot => new Vector2(0.5f, 0.5f);
        public override Vector2 DefaultPosition => new Vector2(0f, Owner.Scaler.m_ReferenceResolution.y);
        public override bool CanDrag => true;
        public override PanelDragger.ResizeTypes CanResize => PanelDragger.ResizeTypes.None;
        public override PanelType PanelType => PanelType.Base;
        private GameObject _uiAnchor;
        private UIScaleSettingButton _scaleButtonData;
        private List<GameObject> _objectsList;
        private Toggle _pinToggle;
        public override float Opacity => Settings.UITransparency;
        
        // Altura fixa calculada: fontSize (14) + padding (16) = 30px
        private const int BUTTON_HEIGHT = 30;
        
        // Referências dos botões
        private static ButtonRef _bindLastButton;
        private static ButtonRef _callButton;
        private static ButtonRef _dismissButton;

        public ContentPanel(UIBase owner) : base(owner)
        {
        }

        protected override void ConstructPanelContent()
        {
            TitleBar.SetActive(false);
            _uiAnchor = Settings.UseHorizontalContentLayout
                ? UIFactory.CreateHorizontalGroup(ContentRoot, "UIAnchor", true, false, true, false)
                : UIFactory.CreateVerticalGroup(ContentRoot, "UIAnchor", false, false, true, false, padding: new Vector4(5,5,5,5));

            Dragger.DraggableArea = Rect;
            Dragger.OnEndResize();

            _objectsList = new List<GameObject>();

            if (CanDrag)
            {
                var pinButton = UIFactory.CreateToggle(_uiAnchor, "PinButton");
                UIFactory.SetLayoutElement(pinButton.GameObject, minHeight: 15, preferredHeight: 15, flexibleHeight: 0,
                    minWidth: 15, preferredWidth: 15, flexibleWidth: 0, ignoreLayout: false);
                
                pinButton.Toggle.isOn = false;
                pinButton.OnValueChanged += (value) => IsPinned = value;
                _pinToggle = pinButton.Toggle;
                pinButton.Text.text = " ";
            }

            var text = UIFactory.CreateLabel(_uiAnchor, "UIAnchorText", "CelemUI");
            UIFactory.SetLayoutElement(text.GameObject, 80, 25, 1, 1);
            _objectsList.Add(text.GameObject);

            // Botão para listar caixas
            /*
            var listBoxesButton = UIFactory.CreateButton(_uiAnchor, "ListBoxesButton", "Caixas");
            UIFactory.SetLayoutElement(listBoxesButton.GameObject, minHeight: 25, minWidth: 60);
            _objectsList.Add(listBoxesButton.GameObject);
            listBoxesButton.OnClick = () => {
                Plugin.UIManager.AddPanel(PanelType.BoxList);
                MessageService.DestroyBoxListMessages = true;
                MessageService.EnqueueMessage(MessageService.BCCOM_LISTBOXES1);
                TimerHelper.OneTickTimer(500, () => MessageService.DestroyBoxListMessages = false);
            };
            */

            if (Settings.IsBindButtonEnabled)
            {
                // Botão para vincular
                _bindLastButton = UIFactory.CreateButton(_uiAnchor, "FamBindLastButton", "Vincular");
                UIFactory.SetLayoutElement(_bindLastButton.GameObject, minHeight: BUTTON_HEIGHT, preferredHeight: BUTTON_HEIGHT, flexibleHeight: 0, minWidth: 80);
                _objectsList.Add(_bindLastButton.GameObject);
                _bindLastButton.OnClick = () =>
                {
                    _bindLastButton.Component.interactable = false;
                    MessageService.EnqueueMessage(MessageService.EMOTE_BINDUNBIND_SMART);
                    TimerHelper.OneTickTimer(2000, () => _bindLastButton.Component.interactable = true);
                };

                // Botão para chamar o familiar
                _callButton = UIFactory.CreateButton(_uiAnchor, "CallButton", "Chamar");
                UIFactory.SetLayoutElement(_callButton.GameObject, minHeight: BUTTON_HEIGHT, preferredHeight: BUTTON_HEIGHT, flexibleHeight: 0, minWidth: 72);
                _objectsList.Add(_callButton.GameObject);
                _callButton.OnClick = () =>
                {
                    _callButton.Component.interactable = false;
                    MessageService.EnqueueMessage(MessageService.EMOTE_CALLDISMISS);
                    TimerHelper.OneTickTimer(2000, () => _callButton.Component.interactable = true);
                };

                // Botão para dispensar o familiar
                _dismissButton = UIFactory.CreateButton(_uiAnchor, "DismissButton", "Dispensar");
                UIFactory.SetLayoutElement(_dismissButton.GameObject, minHeight: BUTTON_HEIGHT, preferredHeight: BUTTON_HEIGHT, flexibleHeight: 0, minWidth: 80);
                _objectsList.Add(_dismissButton.GameObject);
                _dismissButton.OnClick = () =>
                {
                    _dismissButton.Component.interactable = false;
                    MessageService.EnqueueMessage(MessageService.EMOTE_CALLDISMISS);
                    TimerHelper.OneTickTimer(2000, () => _dismissButton.Component.interactable = true);
                };

                var petSkillButton = UIFactory.CreateButton(_uiAnchor, "PetSkillButton", "Habilidade");
                UIFactory.SetLayoutElement(petSkillButton.GameObject, minHeight: BUTTON_HEIGHT, preferredHeight: BUTTON_HEIGHT, flexibleHeight: 0, minWidth: 86);
                _objectsList.Add(petSkillButton.GameObject);
                petSkillButton.OnClick = () =>
                {
                    petSkillButton.Component.interactable = false;
                    MessageService.EnqueueMessage(MessageService.EMOTE_CASTFAMILIARSKILL);
                    TimerHelper.OneTickTimer(2000, () => petSkillButton.Component.interactable = true);
                };
            }

            if (Settings.IsPrestigeButtonEnabled)
            {
                var prestigeButton = UIFactory.CreateButton(_uiAnchor, "PrestigeButton", "Prestigio");
                UIFactory.SetLayoutElement(prestigeButton.GameObject, minHeight: BUTTON_HEIGHT, preferredHeight: BUTTON_HEIGHT, flexibleHeight: 0, minWidth: 76);
                _objectsList.Add(prestigeButton.GameObject);
                prestigeButton.OnClick = () =>
                {
                    MessageService.EnqueueMessage(MessageService.BCCOM_PRESTIGEFAM);
                    prestigeButton.DisableWithTimer(2000);
                };
            }

            if (Settings.IsCombatButtonEnabled)
            {
                var combatToggle = UIFactory.CreateToggle(_uiAnchor, "FamToggleCombatButton");
                combatToggle.Text.text = "Combater";
                combatToggle.OnValueChanged += value =>
                {
                    MessageService.EnqueueMessage(MessageService.EMOTE_COMBATMODE);
                    combatToggle.DisableWithTimer(2000);
                };
                UIFactory.SetLayoutElement(combatToggle.GameObject, minHeight: BUTTON_HEIGHT, preferredHeight: BUTTON_HEIGHT, flexibleHeight: 0, minWidth: 110);
            }

            if (Settings.IsBoxManagementPanelEnabled)
            {
                var boxMgmtButton = UIFactory.CreateButton(_uiAnchor, "BoxMgmtButton", "Gerenciar");
                UIFactory.SetLayoutElement(boxMgmtButton.GameObject, minHeight: BUTTON_HEIGHT, preferredHeight: BUTTON_HEIGHT, flexibleHeight: 0, minWidth: 80);
                _objectsList.Add(boxMgmtButton.GameObject);
                boxMgmtButton.OnClick = () => { Plugin.UIManager.AddPanel(PanelType.BoxManagement); };
            }

            if (Settings.IsTradePanelEnabled)
            {
                var tradeButton = UIFactory.CreateButton(_uiAnchor, "TradeButton", "Trocar");
                UIFactory.SetLayoutElement(tradeButton.GameObject, minHeight: BUTTON_HEIGHT, preferredHeight: BUTTON_HEIGHT, flexibleHeight: 0, minWidth: 60);
                _objectsList.Add(tradeButton.GameObject);
                tradeButton.OnClick = () => { Plugin.UIManager.AddPanel(PanelType.TradePanel); };
            }

            if (Settings.EnableQuickActions)
            {
                var quickActionsButton = UIFactory.CreateButton(_uiAnchor, "QuickActionsButton", "Avançado");
                UIFactory.SetLayoutElement(quickActionsButton.GameObject, minHeight: BUTTON_HEIGHT, preferredHeight: BUTTON_HEIGHT, flexibleHeight: 0, minWidth: 80);
                _objectsList.Add(quickActionsButton.GameObject);
                quickActionsButton.OnClick = () => {
                    Plugin.UIManager.AddPanel(PanelType.AdvancedOptions);
                };
            }

            var scaleButton = UIFactory.CreateButton(_uiAnchor, "ScaleButton", "*");
            UIFactory.SetLayoutElement(scaleButton.GameObject, minHeight: BUTTON_HEIGHT, preferredHeight: BUTTON_HEIGHT, flexibleHeight: 0, minWidth: 25);
            _objectsList.Add(scaleButton.GameObject);
            _scaleButtonData = new UIScaleSettingButton();
            scaleButton.OnClick = () =>
            {
                _scaleButtonData.PerformAction();
                var panel = Plugin.UIManager.GetPanel<FamStatsPanel>();
                if(panel != null && panel.UIRoot.active)
                    panel.RecalculateHeight();
            };

            if (Plugin.IS_TESTING)
            {
                var b = UIFactory.CreateButton(_uiAnchor, "TestButton", "T");
                UIFactory.SetLayoutElement(b.GameObject, minHeight: BUTTON_HEIGHT, preferredHeight: BUTTON_HEIGHT, flexibleHeight: 0, minWidth: 25);
                _objectsList.Add(b.GameObject);
                b.OnClick = () => Plugin.UIManager.AddPanel(PanelType.TestPanel);
            }
        }

        protected override void LateConstructUI()
        {
            base.LateConstructUI();

            if (!Settings.UseHorizontalContentLayout)
                ForceRecalculateBasePanelWidth(_objectsList);
        }

        internal override void Reset()
        {
        }

        public override void Update()
        {
            base.Update();
        }
    }
}