using BloodCraftUI.UI.CustomLib.Panel;
using BloodCraftUI.UI.CustomLib.Util;
using BloodCraftUI.UI.ModContent.Data;
using BloodCraftUI.UI.UniverseLib.UI;
using UnityEngine;
using UIBase = BloodCraftUI.UI.UniverseLib.UI.UIBase;
using BloodCraftUI.Utils;
using ProjectM;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using MS.Internal.Xml.XPath;

namespace BloodCraftUI.UI.ModContent
{
    internal class OrbFamPanel: OrbPanelBase
    {
        private GameObject _uiRoot;
        private BloodOrbElement _bloodOrbElement;
        public override PanelType PanelType => PanelType.OrbFamStats;
        public override string PanelId => nameof(OrbFamPanel);
        public override Vector2 DefaultAnchorMin => new Vector2(0.5f, 0.5f);
        public override Vector2 DefaultAnchorMax => new Vector2(0.5f, 0.5f);
        public override Vector2 DefaultPivot => new Vector2(0.5f, 0.5f);
        public override GameObject UIRoot => _uiRoot;

        public OrbFamPanel(UIBase owner) : base(owner)
        {
        }

        protected override void ConstructUI()
        {

            _bloodOrbElement = CopyBloodOrb(Owner.Panels.PanelHolder.transform);
            _uiRoot = _bloodOrbElement.GameObject;
            PanelRect = _bloodOrbElement.GameObject.GetComponent<RectTransform>();

            var customRoot = UIFactory.CreateUIObject("FamOrbRoot", _bloodOrbElement.GameObject);
            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(customRoot, true, true, true, true);

            var labelRef = UIFactory.CreateLabel(customRoot, "NameLabel", "Very Fancy Name", color: Theme.DefaultText,
                fontSize: 16, outlineColor: Color.black, outlineWidth: 0.05f);
            //UIFactory.SetLayoutElement(labelRef.GameObject, 200, 25, flexibleWidth: 9999, flexibleHeight: 9999);
            //labelRef.GameObject.transform.localPosition = new Vector3(-100f, 80f, 0f);

            // Center it horizontally and position it above the parent
            var textRectTransform = labelRef.GameObject.GetComponent<RectTransform>();
            textRectTransform.anchorMin = new Vector2(0.5f, 1);
            textRectTransform.anchorMax = new Vector2(0.5f, 1);
            textRectTransform.pivot = new Vector2(0.5f, 0);
            textRectTransform.anchoredPosition = new Vector2(0, 0); // Position at the top center
            textRectTransform.sizeDelta = new Vector2(104, 30); // Width matches parent, height as needed



            var hpRef = UIFactory.CreateLabel(customRoot, "HpLabel", "1032/1032", color: Theme.DefaultText,
                fontSize: 12, outlineColor: Color.black, outlineWidth: 0.05f);
            UIFactory.SetLayoutElement(hpRef.GameObject, 200, 25, flexibleWidth: 9999, flexibleHeight: 9999);
            hpRef.GameObject.transform.localPosition = new Vector3(-100f, 0f, 0f);



            Dragger = new RectTransformDragger(this, _bloodOrbElement.BloodCoreRect);
            Dragger.OnFinishDrag += OnFinishDrag;

            /*var halfHeight = PanelRect.rect.height * .5f;
            labelRef.GameObject.transform.localPosition =
                new Vector3(labelRef.GameObject.transform.localPosition.x, halfHeight + 25f, 0f);
            */
            SetDefaultSizeAndPosition();

        }

        private BloodOrbElement CopyBloodOrb(Transform parent)
        {
            var source = UnityHelper.FindInHierarchy("BloodOrbParent|BloodOrb");
            return new BloodOrbElement(source, parent);
        }


        internal override void Reset()
        {
        }

    }

    public class BloodOrbElement
    {
        public GameObject GameObject { get; }
        public GameObject BloodCoreObject { get; }
        public RectTransform BloodCoreRect{ get; }
        private readonly Image _bloodImage;

        public BloodOrbElement(GameObject source, Transform parent)
        {
            GameObject = Object.Instantiate(source, parent);
            var c1 = GameObject.transform.FindChild("BlackBackground");
            BloodCoreObject = c1.transform.FindChild("Blood").gameObject;
            BloodCoreRect = BloodCoreObject.GetComponent<RectTransform>();

            var toRemove1 = GameObject.GetComponent<BloodOrbComponent>();
            Object.Destroy(toRemove1);
            var toRemove2 = BloodCoreObject.GetComponent<ValidUiRaycastTarget>();
            Object.Destroy(toRemove2);
            var toRemove3 = BloodCoreObject.GetComponent<EventTrigger>();
            Object.Destroy(toRemove3);

            _bloodImage = BloodCoreObject.GetComponent<Image>();

            var material = new Material(_bloodImage.material.shader);
            material.CopyPropertiesFromMaterial(_bloodImage.material);
            _bloodImage.material = material;
            _bloodImage.SetMaterialDirty();
        }

        public void SetLevel(float level)
        {
            _bloodImage.material.SetFloat("_LiquidLevel", level);

        }
    }
}
