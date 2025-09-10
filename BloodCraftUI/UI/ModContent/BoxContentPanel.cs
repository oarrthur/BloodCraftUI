using System.Collections.Generic;
using BloodCraftUI.Config;
using BloodCraftUI.Services;
using BloodCraftUI.UI.CustomLib.Cells.Handlers;
using BloodCraftUI.UI.CustomLib.Panel;
using BloodCraftUI.UI.CustomLib.Util;
using BloodCraftUI.UI.ModContent.CustomElements;
using BloodCraftUI.UI.ModContent.Data;
using BloodCraftUI.UI.UniverseLib.UI;
using BloodCraftUI.UI.UniverseLib.UI.Models;
using BloodCraftUI.UI.UniverseLib.UI.Panels;
using BloodCraftUI.UI.UniverseLib.UI.Widgets.ScrollView;
using BloodCraftUI.Utils;
using ProjectM;
using UnityEngine;

namespace BloodCraftUI.UI.ModContent
{
    internal class BoxContentPanel : ResizeablePanelBase
    {
        public override string PanelId { get; }
        public override int MinWidth => 340;
        public override int MinHeight => 220;
        public override Vector2 DefaultAnchorMin => new(0.5f, 0.5f);
        public override Vector2 DefaultAnchorMax => new(0.5f, 0.5f);
        public override Vector2 DefaultPivot => new Vector2(0.5f, 1f);
        public override bool CanDrag => true;
        public override PanelDragger.ResizeTypes CanResize => PanelDragger.ResizeTypes.All;
        public override PanelType PanelType => PanelType.BoxContent;
        public override float Opacity => Settings.UITransparency;

        private readonly string _boxName;
        private bool _isInitialized;
        private ToggleRef _deleteToggle;

        public BoxContentPanel(UIBase owner, string name) : base(owner)
        {
            PanelId = name;
            SetTitle(name);
            _boxName = name;
        }

        protected override void LateConstructUI()
        {
            base.LateConstructUI();
            SendUpdateCommand();
        }

        public override void SetActive(bool active)
        {
            var shouldUpdateData = _isInitialized && active && Enabled == false;
            _isInitialized = true;
            base.SetActive(active);
            if (shouldUpdateData)
                SendUpdateCommand();
        }

        protected override void OnClosePanelClicked()
        {
            SetActive(false);
        }

        #region Commands
        public void SendUpdateCommand()
        {
            if (string.IsNullOrEmpty(_boxName))
                return;

            EnableAllButtons(false);
            MessageService.EnqueueMessage(string.Format(MessageService.BCCOM_SWITCHBOX, _boxName));
            TimerHelper.OneTickTimer(1000, () =>
            {
                try
                {
                    if (Plugin.IS_TESTING)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            AddListEntry(i, $"Test Familiar {i}", AbilitySchoolType.Unholy);
                        }
                    }

                    MessageService.EnqueueMessage(MessageService.BCCOM_BOXCONTENT);
                    MessageService.BoxContentFlag = true;
                }
                finally
                {
                    EnableAllButtons(true);
                }
            });
        }

        private void SendBindCommand(int number)
        {
            EnableAllButtons(false);
            MessageService.EnqueueMessage(MessageService.BCCOM_UNBINDFAM);
            TimerHelper.OneTickTimer(3000, () =>
            {
                try
                {
                    var message = string.Format(MessageService.BCCOM_BINDFAM, number);
                    Settings.LastBindCommand = message;
                    MessageService.EnqueueMessage(message);
                }
                finally
                {
                    EnableAllButtons(true);
                }
            });
        }

        private void SendDeleteCommand(int number)
        {
            EnableAllButtons(false);
            MessageService.EnqueueMessage(string.Format(MessageService.BCCOM_DELETEFAM, number));
            TimerHelper.OneTickTimer(2000, () => EnableAllButtons(true));
        }

        #endregion

        public void AddListEntry(int number, string name, AbilitySchoolType? schoolType)
        {
            _dataList.Add(new FamDataListItem { Number = number, Name = name, SpellSchool = schoolType });
            _scrollDataHandler.RefreshData();
            _scrollPool.Refresh(true);
        }

        protected override void ConstructPanelContent()
        {
            // Seção de controles superiores
            var topControlsGroup = UIFactory.CreateVerticalGroup(ContentRoot, "TopControls", false, false, true, true, 3,
                new Vector4(5, 5, 5, 5), new Color(1, 1, 1, 0));
            UIFactory.SetLayoutElement(topControlsGroup, minHeight: 60, flexibleWidth: 9999);

            // Linha 1: Toggle de delete e botões de ação rápida
            var controlsRow1 = UIFactory.CreateHorizontalGroup(topControlsGroup, "ControlsRow1", false, false, true, true, 5);
            UIFactory.SetLayoutElement(controlsRow1, minHeight: 25, flexibleWidth: 9999);

            _deleteToggle = UIFactory.CreateToggle(controlsRow1, "ToggleDelete", text: "Excluir");
            UIFactory.SetLayoutElement(_deleteToggle.GameObject, minWidth: 80, minHeight: 25, flexibleWidth: 3);
            _deleteToggle.Toggle.isOn = false;
            _deleteToggle.OnValueChanged += (value) =>
            {
                foreach (var a in _scrollPool.CellPool)
                {
                    a.DeleteButton.SetEnabled(value);
                }
            };

            // Botão de procurar familiar
            var searchBtn = UIFactory.CreateButton(controlsRow1, "SearchBtn", "Buscar");
            UIFactory.SetLayoutElement(searchBtn.GameObject, minHeight: 25, minWidth: 80, flexibleWidth: 1);
            searchBtn.OnClick = () => {
                // Abre o painel de gerenciamento focado na busca
                Plugin.UIManager.AddPanel(PanelType.BoxManagement);
            };

            // Botão de ações rápidas
            var actionsBtn = UIFactory.CreateButton(controlsRow1, "ActionsBtn", "Habilidade");
            UIFactory.SetLayoutElement(actionsBtn.GameObject, minHeight: 25, minWidth: 80, flexibleWidth: 1);
            actionsBtn.OnClick = () => {
                MessageService.EnqueueMessage(MessageService.BCCOM_CHANGEABILITY);
                actionsBtn.DisableWithTimer(2000);
            };

            // Linha 2: Funcionalidades avançadas
            var controlsRow2 = UIFactory.CreateHorizontalGroup(topControlsGroup, "ControlsRow2", false, false, true, true, 5);
            UIFactory.SetLayoutElement(controlsRow2, minHeight: 25, flexibleWidth: 9999);

            var shinyBtn = UIFactory.CreateButton(controlsRow2, "ShinyBtn", "Shiny");
            UIFactory.SetLayoutElement(shinyBtn.GameObject, minHeight: 25, flexibleWidth: 1);
            shinyBtn.OnClick = () => {
                // Exemplo: aplicar shiny de sangue por padrão
                MessageService.EnqueueMessage(string.Format(MessageService.BCCOM_CHOOSESHINY, "sangue"));
                shinyBtn.DisableWithTimer(3000);
            };

            var tradeBtn = UIFactory.CreateButton(controlsRow2, "TradeBtn", "Negociar");
            UIFactory.SetLayoutElement(tradeBtn.GameObject, minHeight: 25, flexibleWidth: 1);
            tradeBtn.OnClick = () => {
                Plugin.UIManager.AddPanel(PanelType.TradePanel);
            };

            var moveBtn = UIFactory.CreateButton(controlsRow2, "MoveBtn", "Transferir");
            UIFactory.SetLayoutElement(moveBtn.GameObject, minHeight: 25, flexibleWidth: 1);
            moveBtn.OnClick = () => {
                Plugin.UIManager.AddPanel(PanelType.BoxManagement);
            };

            // Lista de familiares (scroll pool)
            _scrollDataHandler = new BoxContentListHandler<FamDataListItem, BoxContentCell>(_scrollPool, GetEntries, SetCell, ShouldDisplay, OnCellClicked, OnDeleteClicked);
            _scrollPool = UIFactory.CreateScrollPool<BoxContentCell>(ContentRoot, "ContentList", out GameObject scrollObj,
                out _, new Color(0.03f, 0.03f, 0.03f, Theme.Opacity));
            _scrollPool.Initialize(_scrollDataHandler);
            UIFactory.SetLayoutElement(scrollObj, flexibleHeight: 9999);
        }

        internal override void Reset()
        {
            _dataList.Clear();
        }

        private void EnableAllButtons(bool value)
        {
            _deleteToggle.SetEnabled(value);

            foreach (var a in _scrollPool.CellPool)
            {
                a.ContentButton.SetEnabled(value);
                if(!value || _deleteToggle.Toggle.isOn)
                    a.DeleteButton.SetEnabled(value);
            }
        }

        #region ScrollPool handling

        private static ScrollPool<BoxContentCell> _scrollPool;
        private static BoxContentListHandler<FamDataListItem, BoxContentCell> _scrollDataHandler;

        private List<FamDataListItem> GetEntries() => _dataList;

        private bool ShouldDisplay(FamDataListItem data, string filter) => true;

        private void OnCellClicked(int dataIndex)
        {
            var fam = _dataList[dataIndex];
            SendBindCommand(fam.Number);
        }

        private void OnDeleteClicked(int dataIndex)
        {
            var fam = _dataList[dataIndex];
            SendDeleteCommand(fam.Number);
            _dataList.RemoveAt(dataIndex);
            _scrollDataHandler.RefreshData();
            _scrollPool.Refresh(true);
        }

        private void SetCell(BoxContentCell cell, int index)
        {
            if (index < 0 || index >= _dataList.Count)
            {
                cell.Disable();
                return;
            }

            var data = _dataList[index];
            cell.ContentButton.ButtonText.text = data.Name;
        }

        private readonly List<FamDataListItem> _dataList = new();

        public class FamDataListItem
        {
            public int Number { get; set; }
            public string Name { get; set; }
            public AbilitySchoolType? SpellSchool { get; set; }
        }
        #endregion
    }
}
