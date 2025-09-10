using System.Collections.Generic;
using System.Linq;
using BloodCraftUI.Config;
using BloodCraftUI.Services;
using BloodCraftUI.UI.CustomLib.Cells;
using BloodCraftUI.UI.CustomLib.Cells.Handlers;
using BloodCraftUI.UI.CustomLib.Panel;
using BloodCraftUI.UI.ModContent.Data;
using BloodCraftUI.UI.UniverseLib.UI;
using BloodCraftUI.UI.UniverseLib.UI.Models;
using BloodCraftUI.UI.UniverseLib.UI.Panels;
using BloodCraftUI.UI.UniverseLib.UI.Widgets.ScrollView;
using BloodCraftUI.Utils;
using UnityEngine;

namespace BloodCraftUI.UI.ModContent
{
    internal class BoxListPanel : ResizeablePanelBase
    {
        public override string PanelId => "BoxList";
        public override int MinWidth => 340;
        public override int MinHeight => 180;
        public override Vector2 DefaultAnchorMin => new Vector2(0.5f, 0.5f);
        public override Vector2 DefaultAnchorMax => new Vector2(0.5f, 0.5f);
        public override Vector2 DefaultPivot => new Vector2(0.5f, 1f);
        public override bool CanDrag => true;
        public override PanelDragger.ResizeTypes CanResize => PanelDragger.ResizeTypes.All;
        public override PanelType PanelType => PanelType.BoxList;
        public override float Opacity => Settings.UITransparency;

        public BoxListPanel(UIBase owner) : base(owner)
        {
            SetTitle("Lista de Caixas");
        }

        public void AddListEntry(string name)
        {
            if (string.IsNullOrEmpty(name) || _dataList.Any(a => a.Name.Equals(name)))
                return;
            
            _dataList.Add(new FamBoxData { Name = name });
            
            // Garante que o refresh seja feito na thread principal
            if (_scrollDataHandler != null && _scrollPool != null)
            {
                _scrollDataHandler.RefreshData();
                _scrollPool.Refresh(true, true);
            }
        }

        protected override void LateConstructUI()
        {
            base.LateConstructUI();
            RunUpdateCommand();
        }

        protected override void OnClosePanelClicked()
        {
            SetActive(false);
        }

        protected override void ConstructPanelContent()
        {
            _scrollDataHandler = new ButtonListHandler<FamBoxData, ButtonCell>(_scrollPool, GetEntries, SetCell, ShouldDisplay, OnCellClicked);
            _scrollPool = UIFactory.CreateScrollPool<ButtonCell>(ContentRoot, "ContentList", out GameObject scrollObj,
                out _, new Color(0.03f, 0.03f, 0.03f, Opacity));
            _scrollPool.Initialize(_scrollDataHandler);
            UIFactory.SetLayoutElement(scrollObj, flexibleHeight: 9999);
        }

        internal override void Reset()
        {
            //Object.Destroy(UIRoot);
            _dataList.Clear();
            if (_scrollDataHandler != null && _scrollPool != null)
            {
                _scrollDataHandler.RefreshData();
                _scrollPool.Refresh(true, true);
            }
        }

        private void RunUpdateCommand()
        {
            EnableAllButtons(false);
            MessageService.EnqueueMessage(MessageService.BCCOM_LISTBOXES1);
            TimerHelper.OneTickTimer(3000, () =>
            {
                EnableAllButtons(true);
            });
        }

        private void EnableAllButtons(bool value)
        {
            foreach (var a in _scrollPool.CellPool)
                a.Button.Component.interactable = value;
        }

        public override void SetActive(bool active)
        {
            var shouldUpdateData = _isInitialized && active && Enabled == false;
            _isInitialized = true;
            base.SetActive(active);
            if (shouldUpdateData)
                RunUpdateCommand();
        }

        #region ScrollPool handling

        private ScrollPool<ButtonCell> _scrollPool;
        private ButtonListHandler<FamBoxData, ButtonCell> _scrollDataHandler;
        private readonly List<FamBoxData> _dataList = new();
        private bool _isInitialized;

        private class FamBoxData
        {
            public string Name { get; set; }
        }

        private void OnCellClicked(int dataIndex)
        {
            if (_dataList == null || dataIndex < 0 || dataIndex >= _dataList.Count)
                return;
            
            try
            {
                var famBox = _dataList[dataIndex];
                if (famBox != null && !string.IsNullOrEmpty(famBox.Name))
                {
                    Plugin.UIManager.AddPanel(PanelType.BoxContent, famBox.Name);
                }
            }
            catch
            {
                // Ignora erros de clique em índices inválidos
            }
        }

        private bool ShouldDisplay(FamBoxData data, string filter) => true;
        private List<FamBoxData> GetEntries() => _dataList ?? new List<FamBoxData>();

        private void SetCell(ButtonCell cell, int index)
        {
            if (cell == null || _dataList == null || index < 0 || index >= _dataList.Count)
            {
                if (cell != null)
                    cell.Disable();
                return;
            }
            
            try
            {
                var data = _dataList[index];
                if (data != null && !string.IsNullOrEmpty(data.Name))
                {
                    cell.Button.ButtonText.text = data.Name;
                }
                else
                {
                    cell.Disable();
                }
            }
            catch
            {
                cell.Disable();
            }
        }

        #endregion
    }
}
