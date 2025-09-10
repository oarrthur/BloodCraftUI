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
    internal class BoxManagementPanel : ResizeablePanelBase
    {
        public override string PanelId => "BoxManagementPanel";
        public override int MinWidth => 400;
        public override int MinHeight => 350;
        public override Vector2 DefaultAnchorMin => new(0.5f, 0.5f);
        public override Vector2 DefaultAnchorMax => new(0.5f, 0.5f);
        public override Vector2 DefaultPivot => new(0.5f, 0.5f);
        public override bool CanDrag => true;
        public override PanelDragger.ResizeTypes CanResize => PanelDragger.ResizeTypes.All;
        public override PanelType PanelType => PanelType.BoxManagement;
        public override float Opacity => Settings.UITransparency;

        private InputFieldRef _boxNameInput;
        private InputFieldRef _currentBoxInput;
        private InputFieldRef _newBoxNameInput;
        private InputFieldRef _familiarNameInput;
        private InputFieldRef _searchInput;
        private InputFieldRef _shinyTypeInput;

        public BoxManagementPanel(UIBase owner) : base(owner)
        {
        }

        protected override void ConstructPanelContent()
        {
            SetTitle("Gerenciar Caixas e Familiares");

            var scrollView = UIFactory.CreateScrollView(ContentRoot, "ScrollView", out GameObject scrollContent, out var scrollbar);
            UIFactory.SetLayoutElement(scrollView, flexibleWidth: 9999, flexibleHeight: 9999);

            // Seção de gerenciamento de caixas
            CreateBoxManagementSection(scrollContent);
            
            // Seção de gerenciamento de familiares
            CreateFamiliarManagementSection(scrollContent);
            
            // Seção de opções especiais
            CreateSpecialOptionsSection(scrollContent);
            
            // Seção de acesso rápido
            CreateQuickAccessSection(scrollContent);
        }

        private void CreateBoxManagementSection(GameObject parent)
        {
            var boxSection = UIFactory.CreateVerticalGroup(parent, "BoxSection", true, false, true, true, 5, 
                new Vector4(10, 10, 10, 10), Theme.PanelBackground);
            UIFactory.SetLayoutElement(boxSection, minHeight: 150, flexibleWidth: 9999);

            // Título da seção
            var title = UIFactory.CreateLabel(boxSection, "BoxTitle", "🗂️ GERENCIAR CAIXAS", 
                TMPro.TextAlignmentOptions.Center, Theme.DefaultText, 16);
            UIFactory.SetLayoutElement(title.GameObject, minHeight: 25, flexibleWidth: 9999);

            // Linha 1: Adicionar caixa
            var addBoxRow = UIFactory.CreateHorizontalGroup(boxSection, "AddBoxRow", false, false, true, true, 5);
            UIFactory.SetLayoutElement(addBoxRow, minHeight: 25, flexibleWidth: 9999);

            _boxNameInput = UIFactory.CreateInputField(addBoxRow, "BoxNameInput", "Nome da nova caixa...");
            UIFactory.SetLayoutElement(_boxNameInput.GameObject, minHeight: 25, flexibleWidth: 7);

            var addBoxBtn = UIFactory.CreateButton(addBoxRow, "AddBoxBtn", "Adicionar Caixa");
            UIFactory.SetLayoutElement(addBoxBtn.GameObject, minHeight: 25, minWidth: 120);
            addBoxBtn.OnClick = () => {
                if (!string.IsNullOrEmpty(_boxNameInput.Text))
                {
                    MessageService.EnqueueMessage(string.Format(MessageService.BCCOM_ADDBOX, _boxNameInput.Text));
                    _boxNameInput.Text = "";
                    addBoxBtn.DisableWithTimer(2000);
                }
            };

            // Linha 2: Renomear caixa
            var renameBoxRow = UIFactory.CreateHorizontalGroup(boxSection, "RenameBoxRow", false, false, true, true, 5);
            UIFactory.SetLayoutElement(renameBoxRow, minHeight: 25, flexibleWidth: 9999);

            _currentBoxInput = UIFactory.CreateInputField(renameBoxRow, "CurrentBoxInput", "Nome atual...");
            UIFactory.SetLayoutElement(_currentBoxInput.GameObject, minHeight: 25, flexibleWidth: 3);

            _newBoxNameInput = UIFactory.CreateInputField(renameBoxRow, "NewBoxNameInput", "Novo nome...");
            UIFactory.SetLayoutElement(_newBoxNameInput.GameObject, minHeight: 25, flexibleWidth: 3);

            var renameBtn = UIFactory.CreateButton(renameBoxRow, "RenameBtn", "Renomear");
            UIFactory.SetLayoutElement(renameBtn.GameObject, minHeight: 25, minWidth: 80);
            renameBtn.OnClick = () => {
                if (!string.IsNullOrEmpty(_currentBoxInput.Text) && !string.IsNullOrEmpty(_newBoxNameInput.Text))
                {
                    MessageService.EnqueueMessage(string.Format(MessageService.BCCOM_RENAMEBOX, _currentBoxInput.Text, _newBoxNameInput.Text));
                    _currentBoxInput.Text = "";
                    _newBoxNameInput.Text = "";
                    renameBtn.DisableWithTimer(2000);
                }
            };

            // Linha 3: Deletar caixa (com aviso)
            var deleteBoxRow = UIFactory.CreateHorizontalGroup(boxSection, "DeleteBoxRow", false, false, true, true, 5);
            UIFactory.SetLayoutElement(deleteBoxRow, minHeight: 25, flexibleWidth: 9999);

            var deleteBoxInput = UIFactory.CreateInputField(deleteBoxRow, "DeleteBoxInput", "Nome da caixa para deletar...");
            UIFactory.SetLayoutElement(deleteBoxInput.GameObject, minHeight: 25, flexibleWidth: 7);

            var deleteBoxBtn = UIFactory.CreateButton(deleteBoxRow, "DeleteBoxBtn", " Deletar");
            UIFactory.SetLayoutElement(deleteBoxBtn.GameObject, minHeight: 25, minWidth: 120);
            deleteBoxBtn.Component.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f, 0.8f);
            deleteBoxBtn.OnClick = () => {
                if (!string.IsNullOrEmpty(deleteBoxInput.Text))
                {
                    MessageService.EnqueueMessage(string.Format(MessageService.BCCOM_DELETEBOX, deleteBoxInput.Text));
                    deleteBoxInput.Text = "";
                    deleteBoxBtn.DisableWithTimer(3000);
                }
            };
        }

        private void CreateFamiliarManagementSection(GameObject parent)
        {
            var famSection = UIFactory.CreateVerticalGroup(parent, "FamiliarSection", true, false, true, true, 5,
                new Vector4(10, 10, 10, 10), Theme.PanelBackground);
            UIFactory.SetLayoutElement(famSection, minHeight: 150, flexibleWidth: 9999);

            // Título da seção
            var title = UIFactory.CreateLabel(famSection, "FamiliarTitle", "🐾 GERENCIAR FAMILIARES", 
                TMPro.TextAlignmentOptions.Center, Theme.DefaultText, 16);
            UIFactory.SetLayoutElement(title.GameObject, minHeight: 25, flexibleWidth: 9999);

            // Linha 1: Procurar familiar
            var searchRow = UIFactory.CreateHorizontalGroup(famSection, "SearchRow", false, false, true, true, 5);
            UIFactory.SetLayoutElement(searchRow, minHeight: 25, flexibleWidth: 9999);

            _searchInput = UIFactory.CreateInputField(searchRow, "SearchInput", "Nome do familiar para procurar...");
            UIFactory.SetLayoutElement(_searchInput.GameObject, minHeight: 25, flexibleWidth: 7);

            var searchBtn = UIFactory.CreateButton(searchRow, "SearchBtn", "🔍 Procurar");
            UIFactory.SetLayoutElement(searchBtn.GameObject, minHeight: 25, minWidth: 120);
            searchBtn.OnClick = () => {
                if (!string.IsNullOrEmpty(_searchInput.Text))
                {
                    MessageService.EnqueueMessage(string.Format(MessageService.BCCOM_SEARCHFAMILIAR, _searchInput.Text));
                    searchBtn.DisableWithTimer(1000);
                }
            };

            // Linha 2: Vínculo automático
            var smartBindRow = UIFactory.CreateHorizontalGroup(famSection, "SmartBindRow", false, false, true, true, 5);
            UIFactory.SetLayoutElement(smartBindRow, minHeight: 25, flexibleWidth: 9999);

            _familiarNameInput = UIFactory.CreateInputField(smartBindRow, "FamiliarNameInput", "Nome do familiar para vincular...");
            UIFactory.SetLayoutElement(_familiarNameInput.GameObject, minHeight: 25, flexibleWidth: 7);

            var smartBindBtn = UIFactory.CreateButton(smartBindRow, "SmartBindBtn", " Vínculo Auto");
            UIFactory.SetLayoutElement(smartBindBtn.GameObject, minHeight: 25, minWidth: 120);
            smartBindBtn.OnClick = () => {
                if (!string.IsNullOrEmpty(_familiarNameInput.Text))
                {
                    MessageService.EnqueueMessage(string.Format(MessageService.BCCOM_SMARTBIND, _familiarNameInput.Text));
                    _familiarNameInput.Text = "";
                    smartBindBtn.DisableWithTimer(2000);
                }
            };

            // Linha 3: Mover familiar para caixa
            var moveRow = UIFactory.CreateHorizontalGroup(famSection, "MoveRow", false, false, true, true, 5);
            UIFactory.SetLayoutElement(moveRow, minHeight: 25, flexibleWidth: 9999);

            var moveBoxInput = UIFactory.CreateInputField(moveRow, "MoveBoxInput", "Nome da caixa destino...");
            UIFactory.SetLayoutElement(moveBoxInput.GameObject, minHeight: 25, flexibleWidth: 7);

            var moveBtn = UIFactory.CreateButton(moveRow, "MoveBtn", "📦 Mover Familiar");
            UIFactory.SetLayoutElement(moveBtn.GameObject, minHeight: 25, minWidth: 120);
            moveBtn.OnClick = () => {
                if (!string.IsNullOrEmpty(moveBoxInput.Text))
                {
                    MessageService.EnqueueMessage(string.Format(MessageService.BCCOM_MOVEFAMILIAR, moveBoxInput.Text));
                    moveBoxInput.Text = "";
                    moveBtn.DisableWithTimer(2000);
                }
            };
        }

        private void CreateSpecialOptionsSection(GameObject parent)
        {
            var specialSection = UIFactory.CreateVerticalGroup(parent, "SpecialSection", true, false, true, true, 5,
                new Vector4(10, 10, 10, 10), Theme.PanelBackground);
            UIFactory.SetLayoutElement(specialSection, minHeight: 120, flexibleWidth: 9999);

            // Título da seção
            var title = UIFactory.CreateLabel(specialSection, "SpecialTitle", " OPÇÕES ESPECIAIS", 
                TMPro.TextAlignmentOptions.Center, Theme.DefaultText, 16);
            UIFactory.SetLayoutElement(title.GameObject, minHeight: 25, flexibleWidth: 9999);

            // Linha 1: Escolher Shiny e Trocar Habilidade
            var shinyRow = UIFactory.CreateHorizontalGroup(specialSection, "ShinyRow", false, false, true, true, 5);
            UIFactory.SetLayoutElement(shinyRow, minHeight: 25, flexibleWidth: 9999);

            _shinyTypeInput = UIFactory.CreateInputField(shinyRow, "ShinyTypeInput", "sangue/caos/profana/ilusao/gelo/tempestade");
            UIFactory.SetLayoutElement(_shinyTypeInput.GameObject, minHeight: 25, flexibleWidth: 5);

            var shinyBtn = UIFactory.CreateButton(shinyRow, "ShinyBtn", "💎 Shiny");
            UIFactory.SetLayoutElement(shinyBtn.GameObject, minHeight: 25, minWidth: 80);
            shinyBtn.OnClick = () => {
                if (!string.IsNullOrEmpty(_shinyTypeInput.Text))
                {
                    MessageService.EnqueueMessage(string.Format(MessageService.BCCOM_CHOOSESHINY, _shinyTypeInput.Text));
                    _shinyTypeInput.Text = "";
                    shinyBtn.DisableWithTimer(2000);
                }
            };

            var changeAbilityBtn = UIFactory.CreateButton(shinyRow, "ChangeAbilityBtn", "🎯 Trocar Habilidade");
            UIFactory.SetLayoutElement(changeAbilityBtn.GameObject, minHeight: 25, minWidth: 120);
            changeAbilityBtn.OnClick = () => {
                MessageService.EnqueueMessage(MessageService.BCCOM_CHANGEABILITY);
                changeAbilityBtn.DisableWithTimer(2000);
            };

            // Linha 2: Botões de ação
            var actionRow = UIFactory.CreateHorizontalGroup(specialSection, "ActionRow", false, false, true, true, 5);
            UIFactory.SetLayoutElement(actionRow, minHeight: 25, flexibleWidth: 9999);

            var unitTypeBtn = UIFactory.CreateButton(actionRow, "UnitTypeBtn", "⚔️ Alternar Unidades");
            UIFactory.SetLayoutElement(unitTypeBtn.GameObject, minHeight: 25, flexibleWidth: 2);
            unitTypeBtn.OnClick = () => {
                MessageService.EnqueueMessage(MessageService.BCCOM_UNITTYPE);
                unitTypeBtn.DisableWithTimer(1000);
            };

            var listEmotesBtn = UIFactory.CreateButton(actionRow, "ListEmotesBtn", "😊 Listar Emotes");
            UIFactory.SetLayoutElement(listEmotesBtn.GameObject, minHeight: 25, flexibleWidth: 2);
            listEmotesBtn.OnClick = () => {
                MessageService.EnqueueMessage(MessageService.BCCOM_LISTEMOTES);
                listEmotesBtn.DisableWithTimer(1000);
            };

            var resetBtn = UIFactory.CreateButton(actionRow, "ResetBtn", "🔄 Resetar Familiares");
            UIFactory.SetLayoutElement(resetBtn.GameObject, minHeight: 25, flexibleWidth: 2);
            resetBtn.Component.GetComponent<Image>().color = new Color(0.8f, 0.5f, 0.2f, 0.8f);
            resetBtn.OnClick = () => {
                MessageService.EnqueueMessage(MessageService.BCCOM_RESETFAMILIARS);
                resetBtn.DisableWithTimer(3000);
            };
        }

        private void CreateQuickAccessSection(GameObject parent)
        {
            var quickSection = UIFactory.CreateVerticalGroup(parent, "QuickAccessSection", true, false, true, true, 5,
                new Vector4(10, 10, 10, 10), Theme.PanelBackground);
            UIFactory.SetLayoutElement(quickSection, minHeight: 80, flexibleWidth: 9999);

            // Título da seção
            var title = UIFactory.CreateLabel(quickSection, "QuickTitle", "ACESSO RAPIDO", 
                TMPro.TextAlignmentOptions.Center, Theme.DefaultText, 16);
            UIFactory.SetLayoutElement(title.GameObject, minHeight: 25, flexibleWidth: 9999);

            // Linha de botões de acesso rápido
            var quickRow = UIFactory.CreateHorizontalGroup(quickSection, "QuickRow", false, false, true, true, 5);
            UIFactory.SetLayoutElement(quickRow, minHeight: 35, flexibleWidth: 9999);

            var boxListBtn = UIFactory.CreateButton(quickRow, "QuickBoxListBtn", "Ver Caixas");
            UIFactory.SetLayoutElement(boxListBtn.GameObject, minHeight: 35, flexibleWidth: 1);
            boxListBtn.OnClick = () => {
                // Primeiro abre o painel
                Plugin.UIManager.AddPanel(PanelType.BoxList);
                // Ativa flag para destruir mensagens de caixas
                MessageService.DestroyBoxListMessages = true;
                // Depois dispara o comando para listar as caixas
                MessageService.EnqueueMessage(MessageService.BCCOM_LISTBOXES1);
                // Desativa flag após um tempo
                TimerHelper.OneTickTimer(5000, () => MessageService.DestroyBoxListMessages = false);
            };

            var statusBtn = UIFactory.CreateButton(quickRow, "QuickStatusBtn", "Ver Status");
            UIFactory.SetLayoutElement(statusBtn.GameObject, minHeight: 35, flexibleWidth: 1);
            statusBtn.OnClick = () => {
                // Primeiro abre o painel
                Plugin.UIManager.AddPanel(PanelType.FamStats);
                // Ativa flag para destruir mensagens de status
                MessageService.DestroyFamStatsMessages = true;
                // Depois dispara o comando para mostrar status do familiar
                MessageService.EnqueueMessage(MessageService.BCCOM_FAMSTATS);
                // Desativa flag após um tempo
                TimerHelper.OneTickTimer(5000, () => MessageService.DestroyFamStatsMessages = false);
            };
        }

        internal override void Reset()
        {
            // Limpar campos quando necessário
            _boxNameInput.Text = "";
            _currentBoxInput.Text = "";
            _newBoxNameInput.Text = "";
            _familiarNameInput.Text = "";
            _searchInput.Text = "";
            _shinyTypeInput.Text = "";
        }

        protected override void OnClosePanelClicked()
        {
            SetActive(false);
        }
    }
}