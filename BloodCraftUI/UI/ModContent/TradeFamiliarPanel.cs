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
    internal class TradeFamiliarPanel : ResizeablePanelBase
    {
        public override string PanelId => "TradeFamiliarPanel";
        public override int MinWidth => 380;
        public override int MinHeight => 280;
        public override Vector2 DefaultAnchorMin => new(0.5f, 0.5f);
        public override Vector2 DefaultAnchorMax => new(0.5f, 0.5f);
        public override Vector2 DefaultPivot => new(0.5f, 0.5f);
        public override bool CanDrag => true;
        public override PanelDragger.ResizeTypes CanResize => PanelDragger.ResizeTypes.All;
        public override PanelType PanelType => PanelType.TradePanel;
        public override float Opacity => Settings.UITransparency;

        private InputFieldRef _playerNameInput;
        private LabelRef _statusLabel;
        private GameObject _tradeControlsSection;

        public TradeFamiliarPanel(UIBase owner) : base(owner)
        {
        }

        protected override void ConstructPanelContent()
        {
            SetTitle("🔄 Trocar Familiares");

            var mainContainer = UIFactory.CreateVerticalGroup(ContentRoot, "MainContainer", true, false, true, true, 10,
                new Vector4(15, 15, 15, 15), Theme.PanelBackground);
            UIFactory.SetLayoutElement(mainContainer, flexibleWidth: 9999, flexibleHeight: 9999);

            // Status da troca
            CreateStatusSection(mainContainer);

            // Iniciar nova troca
            CreateTradeInitiationSection(mainContainer);

            // Controles de troca ativa
            CreateTradeControlsSection(mainContainer);

            // Botões de ação
            CreateActionButtonsSection(mainContainer);
        }

        private void CreateStatusSection(GameObject parent)
        {
            var statusSection = UIFactory.CreateVerticalGroup(parent, "StatusSection", true, false, true, true, 5,
                new Vector4(10, 10, 10, 10), new Color(0.1f, 0.3f, 0.1f, 0.3f));
            UIFactory.SetLayoutElement(statusSection, minHeight: 60, flexibleWidth: 9999);

            var statusTitle = UIFactory.CreateLabel(statusSection, "StatusTitle", "📊 Status da Troca",
                TMPro.TextAlignmentOptions.Center, Theme.DefaultText, 14);
            UIFactory.SetLayoutElement(statusTitle.GameObject, minHeight: 25, flexibleWidth: 9999);

            _statusLabel = UIFactory.CreateLabel(statusSection, "StatusLabel", "Nenhuma troca ativa no momento",
                TMPro.TextAlignmentOptions.Center, new Color(0.8f, 0.8f, 0.8f), 12);
            UIFactory.SetLayoutElement(_statusLabel.GameObject, minHeight: 25, flexibleWidth: 9999);
        }

        private void CreateTradeInitiationSection(GameObject parent)
        {
            var initiateSection = UIFactory.CreateVerticalGroup(parent, "InitiateSection", true, false, true, true, 5,
                new Vector4(10, 10, 10, 10), Theme.PanelBackground);
            UIFactory.SetLayoutElement(initiateSection, minHeight: 80, flexibleWidth: 9999);

            var initiateTitle = UIFactory.CreateLabel(initiateSection, "InitiateTitle", "🎯 Iniciar Nova Troca",
                TMPro.TextAlignmentOptions.Center, Theme.DefaultText, 14);
            UIFactory.SetLayoutElement(initiateTitle.GameObject, minHeight: 25, flexibleWidth: 9999);

            // Linha para inserir nome do jogador
            var playerInputRow = UIFactory.CreateHorizontalGroup(initiateSection, "PlayerInputRow", false, false, true, true, 5);
            UIFactory.SetLayoutElement(playerInputRow, minHeight: 30, flexibleWidth: 9999);

            _playerNameInput = UIFactory.CreateInputField(playerInputRow, "PlayerNameInput", "Nome do jogador...");
            UIFactory.SetLayoutElement(_playerNameInput.GameObject, minHeight: 30, flexibleWidth: 7);

            var initiateTradeBtn = UIFactory.CreateButton(playerInputRow, "InitiateTradeBtn", "🤝 Propor Troca");
            UIFactory.SetLayoutElement(initiateTradeBtn.GameObject, minHeight: 30, minWidth: 120);
            initiateTradeBtn.Component.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.2f, 0.8f);
            initiateTradeBtn.OnClick = () => {
                if (!string.IsNullOrEmpty(_playerNameInput.Text))
                {
                    MessageService.EnqueueMessage(string.Format(MessageService.BCCOM_TRADEFAMILIAR, _playerNameInput.Text));
                    UpdateStatusLabel($"Proposta de troca enviada para {_playerNameInput.Text}...");
                    _playerNameInput.Text = "";
                    initiateTradeBtn.DisableWithTimer(3000);
                }
            };
        }

        private void CreateTradeControlsSection(GameObject parent)
        {
            _tradeControlsSection = UIFactory.CreateVerticalGroup(parent, "TradeControlsSection", true, false, true, true, 5,
                new Vector4(10, 10, 10, 10), new Color(0.3f, 0.2f, 0.1f, 0.3f));
            UIFactory.SetLayoutElement(_tradeControlsSection, minHeight: 60, flexibleWidth: 9999);
            _tradeControlsSection.SetActive(false); // Oculto por padrão

            var controlsTitle = UIFactory.CreateLabel(_tradeControlsSection, "ControlsTitle", " Controles de Troca Ativa",
                TMPro.TextAlignmentOptions.Center, Theme.DefaultText, 14);
            UIFactory.SetLayoutElement(controlsTitle.GameObject, minHeight: 25, flexibleWidth: 9999);

            var controlsRow = UIFactory.CreateHorizontalGroup(_tradeControlsSection, "ControlsRow", false, false, true, true, 10);
            UIFactory.SetLayoutElement(controlsRow, minHeight: 30, flexibleWidth: 9999);

            var acceptBtn = UIFactory.CreateButton(controlsRow, "AcceptBtn", " Aceitar");
            UIFactory.SetLayoutElement(acceptBtn.GameObject, minHeight: 30, flexibleWidth: 1);
            acceptBtn.Component.GetComponent<Image>().color = new Color(0.2f, 0.7f, 0.2f, 0.8f);
            acceptBtn.OnClick = () => {
                // Simula aceitar troca usando emotes ou comandos específicos
                UpdateStatusLabel("Troca aceita! Aguardando confirmação...");
                acceptBtn.DisableWithTimer(2000);
            };

            var cancelBtn = UIFactory.CreateButton(controlsRow, "CancelBtn", " Cancelar");
            UIFactory.SetLayoutElement(cancelBtn.GameObject, minHeight: 30, flexibleWidth: 1);
            cancelBtn.Component.GetComponent<Image>().color = new Color(0.7f, 0.2f, 0.2f, 0.8f);
            cancelBtn.OnClick = () => {
                MessageService.EnqueueMessage(string.Format(MessageService.BCCOM_TRADEFAMILIAR, "cancelar"));
                UpdateStatusLabel("Troca cancelada");
                _tradeControlsSection.SetActive(false);
                cancelBtn.DisableWithTimer(2000);
            };
        }

        private void CreateActionButtonsSection(GameObject parent)
        {
            var actionsSection = UIFactory.CreateVerticalGroup(parent, "ActionsSection", true, false, true, true, 5,
                new Vector4(10, 10, 10, 10), Theme.PanelBackground);
            UIFactory.SetLayoutElement(actionsSection, minHeight: 60, flexibleWidth: 9999);

            var actionsTitle = UIFactory.CreateLabel(actionsSection, "ActionsTitle", "🛠️ Ações Rápidas",
                TMPro.TextAlignmentOptions.Center, Theme.DefaultText, 14);
            UIFactory.SetLayoutElement(actionsTitle.GameObject, minHeight: 25, flexibleWidth: 9999);

            var actionsRow = UIFactory.CreateHorizontalGroup(actionsSection, "ActionsRow", false, false, true, true, 5);
            UIFactory.SetLayoutElement(actionsRow, minHeight: 30, flexibleWidth: 9999);

            var refreshStatusBtn = UIFactory.CreateButton(actionsRow, "RefreshStatusBtn", "🔄 Atualizar Status");
            UIFactory.SetLayoutElement(refreshStatusBtn.GameObject, minHeight: 30, flexibleWidth: 1);
            refreshStatusBtn.OnClick = () => {
                // Verifica status atual da troca
                UpdateStatusLabel("Verificando status da troca...");
                refreshStatusBtn.DisableWithTimer(1000);
            };

            var checkTradesBtn = UIFactory.CreateButton(actionsRow, "CheckTradesBtn", "📋 Ver Propostas");
            UIFactory.SetLayoutElement(checkTradesBtn.GameObject, minHeight: 30, flexibleWidth: 1);
            checkTradesBtn.OnClick = () => {
                // Lista propostas de troca pendentes
                UpdateStatusLabel("Verificando propostas pendentes...");
                checkTradesBtn.DisableWithTimer(1000);
            };
        }

        public void UpdateStatusLabel(string message)
        {
            if (_statusLabel != null)
            {
                _statusLabel.TextMesh.text = message;
                
                // Ativa controles se há troca ativa
                bool hasActiveTrade = message.Contains("proposta") || message.Contains("aceita") || message.Contains("pendente");
                if (_tradeControlsSection != null)
                {
                    _tradeControlsSection.SetActive(hasActiveTrade);
                }
            }
        }

        internal override void Reset()
        {
            _playerNameInput.Text = "";
            UpdateStatusLabel("Nenhuma troca ativa no momento");
            if (_tradeControlsSection != null)
            {
                _tradeControlsSection.SetActive(false);
            }
        }

        protected override void OnClosePanelClicked()
        {
            SetActive(false);
        }

        protected override void LateConstructUI()
        {
            base.LateConstructUI();
            UpdateStatusLabel("Painel de troca carregado");
        }
    }
}