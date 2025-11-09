using System;
using System.Linq;
using MenuChanger;
using MenuChanger.Extensions;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using RandomizerMod.Menu;
using UnityEngine;
using static RandomizerMod.Localization;

namespace RandoSBRCO
{
    public class MenuHolder
    {
        internal SmallButton JumpToRPButton;
        private static MenuHolder _instance = null;
        internal static MenuHolder Instance => _instance ??= new MenuHolder();
        
        internal MenuPage SBRCOMenu;
        internal MenuElementFactory<SBRCOSettings> MEF;
        internal VerticalItemPanel VIP;
        internal TextEntryField CodeEntryField;
        internal SmallButton CodeApplyButton;
        internal MenuLabel CharmListHeader;
        internal MenuLabel CharmList;

        public static void OnExitMenu()
        {
            _instance = null;
        }

        public static void Hook()
        {
            RandomizerMenuAPI.AddMenuPage(Instance.ConstructMenu, Instance.HandleButton);
            MenuChangerMod.OnExitMainMenu += OnExitMenu;
        }

        private Color ButtonColor() =>
            RandoSBRCO.Instance.GS.Enabled() ? Colors.TRUE_COLOR : Colors.DEFAULT_COLOR;

        private bool HandleButton(MenuPage landingPage, out SmallButton button)
        {
            JumpToRPButton = new SmallButton(landingPage, Localize("RandoSBRCO"));
            SBRCOMenu.BeforeGoBack += () => JumpToRPButton.Text.color = ButtonColor();
            JumpToRPButton.Text.color = ButtonColor();
            JumpToRPButton.AddHideAndShowEvent(landingPage, SBRCOMenu);
            button = JumpToRPButton;
            return true;
        }

        public void UpdateElements()
        {
            CodeEntryField.SetValue(RandoSBRCO.Instance.GS.SBRCode);
            try
            {
                var charms = RandoSBRCO.Instance.GS.GetCharmOrder();
                CharmList.Text.text = string.Join("\n", charms.Select(Data.Charm).Take(5));
                if (charms.Count > 5) CharmList.Text.text += "\n...";
            }
            catch (Exception e)
            {
                CharmList.Text.text = $"Error processing SBRCode:\n{e}";
            }

            var rs = RandomizerMod.RandomizerMod.GS.DefaultMenuSettings;
            if (rs.PoolSettings.Charms)
                CharmList.Text.text += "\nWARNING: Your charms are randomized!";
            if (rs.DuplicateItemSettings.Grimmchild || rs.DuplicateItemSettings.VoidHeart)
                CharmList.Text.text += "\nWARNING: Duping grimmchild/voidheart/sporeshroom is not advised!";
            if (rs.CostSettings.GrubTolerance + rs.CursedSettings.MaximumGrubsReplacedByMimics > RandoSBRCO.Instance.GS.DupeGrubs)
                CharmList.Text.text += "\nWARNING: (grub cost tolerance + extra mimics) > dupe grubs, generation may fail.";
        }

        private void ConstructMenu(MenuPage landingPage)
        {
            SBRCOMenu = new MenuPage(Localize("RandoSBRCO"), landingPage);
            MEF = new MenuElementFactory<SBRCOSettings>(SBRCOMenu, RandoSBRCO.Instance.GS);
            VIP = new VerticalItemPanel(SBRCOMenu, new Vector2(0, 400), 75f, true, MEF.Elements);

            CodeEntryField = new TextEntryField(SBRCOMenu, "SBRCode (\"base64 config\" from the generator)")
                { InputField = { textComponent = { horizontalOverflow = HorizontalWrapMode.Overflow}}};

            CodeApplyButton = new SmallButton(SBRCOMenu, "Apply Settings");
            CodeApplyButton.OnClick += () => RandoSBRCO.Instance.GS.SBRCode = CodeEntryField.Value;
            CodeApplyButton.OnClick += UpdateElements;
            
            CharmListHeader = new MenuLabel(SBRCOMenu, "Current Charm Order:", MenuLabel.Style.Body)
                { Text = { alignment = TextAnchor.UpperCenter}};
            
            CharmList = new MenuLabel(SBRCOMenu, "", MenuLabel.Style.Body)
                { Text = { alignment = TextAnchor.UpperCenter, lineSpacing = 0.9f}};

            VIP.Add(CodeEntryField);
            VIP.Add(CodeApplyButton);
            VIP.Add(CharmListHeader);
            VIP.Add(CharmList);
            
            Localize(MEF);
            Localize(CodeEntryField);
            Localize(CharmListHeader);
            Localize(CodeApplyButton);
            
            UpdateElements();
        }
    }
}
