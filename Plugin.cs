using Dalamud.Game;
using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using ImGuiNET;
using Lumina.Data.Parsing.Scd;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using Veda;
using static FFXIVClientStructs.FFXIV.Common.Component.BGCollision.MeshPCB;
using static TeamcraftListMaker.ContextMenuService;

namespace TeamcraftListMaker
{
    public class Plugin : IDalamudPlugin
    {
        public string Name => "Teamcraft List Maker";

        [PluginService] public static IDalamudPluginInterface PluginInterface { get; set; }
        [PluginService] public static ICommandManager Commands { get; set; }
        [PluginService] public static ICondition Conditions { get; set; }
        [PluginService] public static IDataManager DataManager { get; set; }
        [PluginService] public static IFramework Framework { get; set; }
        [PluginService] public static IGameGui GameGui { get; set; }
        [PluginService] public static IKeyState KeyState { get; set; }
        [PluginService] public static IChatGui Chat { get; set; }
        [PluginService] public static IClientState ClientState { get; set; }
        [PluginService] public static IPartyList PartyList { get; set; }
        [PluginService] public static IPluginLog PluginLog { get; set; }
        [PluginService] internal static IContextMenu ContextMenu { get; private set; } = null!;

        public static Configuration PluginConfig { get; set; }
        private PluginCommandManager<Plugin> CommandManager;
        public static PluginUI ui;

        private ContextMenuService CMSShit = new ContextMenuService();
        public static string CraftingListLocation;

        public Plugin(IDalamudPluginInterface pluginInterface, IChatGui chat, IPartyList partyList, ICommandManager commands, IDataManager Data)
        {
            PluginInterface = pluginInterface;
            PartyList = partyList;
            Chat = chat;
            DataManager = Data;

            // Get or create a configuration object
            PluginConfig = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            PluginConfig.Initialize(PluginInterface);

            ui = new PluginUI();
            PluginInterface.UiBuilder.Draw += new System.Action(ui.Draw);
            //PluginInterface.UiBuilder.OpenConfigUi += () =>
            //{
            //    PluginUI ui = Plugin.ui;
            //    ui.IsVisible = !ui.IsVisible;
            //};

            ContextMenu.OnMenuOpened += OnContextMenuOpened;

            CraftingListLocation = Path.Combine(PluginInterface.AssemblyLocation.DirectoryName, "craftinglist.txt");

            // Load all of our commands
            CommandManager = new PluginCommandManager<Plugin>(this, commands);
        }

        private void OnContextMenuOpened(IMenuOpenedArgs args)
        {
            try
            {
                uint? ItemID;
                ItemID = CMSShit.GetGameObjectItemId(args);
                ItemID %= 500000;
                //ExcelSheet<RecipeLookup> Recipe = DataManager.GetExcelSheet<RecipeLookup>()!;
                //Chat.Print(Recipe.GetRow((uint)ItemID).RowId.ToString());
                //Chat.Print($"{itemId}");

                if (ItemID != null && IsCraftable((uint)ItemID))
                {
                    var menuItem = new MenuItem();
                    menuItem.Name = "Teamcraft";
                    menuItem.Prefix = SeIconChar.HighQuality;
                    menuItem.PrefixColor = 31;
                    menuItem.IsSubmenu = true;
                    menuItem.OnClicked += clickedArgs => PopulateCraftingListOptions(clickedArgs, ItemID);
                    args.AddMenuItem(menuItem);
                }
            }
            catch (Exception ex)
            {
                Chat.Print("An error has occured - " + ex.ToString());
            }
        }

        private void PopulateCraftingListOptions(IMenuItemClickedArgs clickedArgs, uint? ItemID = null)
        {
            try
            {
                if (!File.Exists(CraftingListLocation))
                {
                    File.WriteAllText(CraftingListLocation, "");
                }
                var menuItems = new List<MenuItem>();
                var CraftingListCount = File.ReadLines(CraftingListLocation).Where(x => x.IsNullOrWhitespace() == false).Count();
                var newButton = new MenuItem();
                if (CheckListForItem(ItemID))
                {
                    newButton = new MenuItem();
                    newButton.Name = "Remove from List";
                    newButton.Prefix = SeIconChar.QuestRepeatable;
                    newButton.PrefixColor = 25;
                    newButton.OnClicked += args => RemoveItem(ItemID);
                }
                else
                {
                    newButton.Name = "Add to List";
                    newButton.IsSubmenu = true;
                    newButton.Prefix = SeIconChar.BoxedPlus;
                    newButton.PrefixColor = 45;
                    newButton.OnClicked += args => AddItemMenu(args, ItemID);
                }
                menuItems.Add(newButton);
                if (CraftingListCount > 0)
                {
                    var ExportItemListButton = new MenuItem();
                    if (CraftingListCount == 1)
                    {
                        ExportItemListButton.Name = "Export List (" + CraftingListCount + " item)";
                    }
                    else
                    {
                        ExportItemListButton.Name = "Export List (" + CraftingListCount + " items)";
                    }
                    ExportItemListButton.Prefix = SeIconChar.ArrowRight;
                    ExportItemListButton.PrefixColor = 37;
                    ExportItemListButton.OnClicked += args => ExportItemList();
                    menuItems.Add(ExportItemListButton);

                    var ClearItemListButton = new MenuItem();
                    if (CraftingListCount == 1)
                    {
                        ClearItemListButton.Name = "Clear List (" + CraftingListCount + " item)";
                    }
                    else
                    {
                        ClearItemListButton.Name = "Clear List (" + CraftingListCount + " items)";
                    }
                    ClearItemListButton.Prefix = SeIconChar.Cross;
                    ClearItemListButton.PrefixColor = 17;
                    ClearItemListButton.OnClicked += args => ClearItemList();
                    menuItems.Add(ClearItemListButton);
                }
                clickedArgs.OpenSubmenu(menuItems);
            }
            catch (Exception ex)
            {
                Chat.Print("An error has occured - " + ex.ToString());
            }
        }

        private void AddItemMenu(IMenuItemClickedArgs clickedArgs, uint? itemId = null)
        {
            var menuItems = new List<MenuItem>();
            var newButton = new MenuItem();
            newButton.Name = "1";
            newButton.OnClicked += args => AddItem(itemId, 1);
            menuItems.Add(newButton);

            newButton = new MenuItem();
            newButton.Name = "Specify amount";
            newButton.OnClicked += SpecifyAmountArgs => SpecifyAmountPopup(SpecifyAmountArgs, itemId);
            menuItems.Add(newButton);
            clickedArgs.OpenSubmenu(menuItems);
        }

        public static bool CheckListForItem(uint? ItemID)
        {
            using (StreamReader sr = File.OpenText(CraftingListLocation))
            {
                string s = String.Empty;
                while (!String.IsNullOrWhiteSpace(s = sr.ReadLine()))
                {
                    if (s.Split(',')[0] == ItemID.ToString())
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public static unsafe void SpecifyAmountPopup(IMenuItemClickedArgs clickedArgs, uint? itemId = null)
        {
            try
            {
                ui.SelectItemID = itemId;
                var ContextMenuPtr = GameGui.GetAddonByName("ContextMenu", 1);
                var addonContextMenu = (AddonContextMenu*)ContextMenuPtr;
                //if (!Plugin.PluginConfig.DoNotResetAmount) { ui.AmountToAdd = 1; }
                ui.AmountToAdd = 1;
                ui.SelectAmountX = addonContextMenu->X;
                ui.SelectAmountY = addonContextMenu->Y;
                ui.IsVisible = true;
            }
            catch (Exception ex)
            {
                Chat.Print("An error has occured - " + ex.ToString());
            }
        }

        public static void ExportItemList()
        {
            string StringBeforeEncode = "";
            using (StreamReader sr = File.OpenText(CraftingListLocation))
            {
                string s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    if (!String.IsNullOrWhiteSpace(s)) { StringBeforeEncode += s; }
                }
            }
            StringBeforeEncode = StringBeforeEncode.Remove(StringBeforeEncode.Length - 1, 1).Trim();
            //Chat.Print("String before encoding: \"" + StringBeforeEncode + "\"");
            string WebsiteURL = "https://ffxivteamcraft.com/import/" + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(StringBeforeEncode));
            Functions.OpenWebsite(WebsiteURL);
            Chat.Print(Functions.BuildSeString("Teamcraft List Maker", "List exported to " + WebsiteURL, ColorType.Teamcraft));
        }

        public static void ClearItemList()
        {
            File.WriteAllText(CraftingListLocation, "");
            Chat.Print(Functions.BuildSeString("Teamcraft List Maker", "List cleared!", ColorType.Teamcraft));
        }

        public static void AddItem(uint? ItemID, int Quantity)
        {
            File.AppendAllText(CraftingListLocation, ItemID + ",null," + Quantity + ";" + Environment.NewLine);
            Item RetrievedItem = GetItemInfo((uint)ItemID);
            Chat.Print(Functions.BuildSeString("Teamcraft List Maker", "Added " + Quantity + " " + RetrievedItem.Name.ToString() + " to the list!", ColorType.Teamcraft));
            //if (Quantity > 1)
            //{
            //    Chat.Print(Functions.BuildSeString("Teamcraft List Maker", "Added " + Quantity + " " + Item.Name + "s to the list!", ColorType.Teamcraft));
            //}
            //else
            //{
            //    Chat.Print(Functions.BuildSeString("Teamcraft List Maker", "Added " + Quantity + " " + Item.Name + " to the list!", ColorType.Teamcraft));
            //}
        }
        public static void RemoveItem(uint? ItemID)
        {
            File.WriteAllLines(CraftingListLocation, File.ReadLines(CraftingListLocation).Where(x => !x.StartsWith(ItemID + ",")).ToList());
            Item RetrievedItem = GetItemInfo((uint)ItemID);
            Chat.Print(Functions.BuildSeString("Teamcraft List Maker", "Removed " + RetrievedItem.Name.ToString() + " from the list!", ColorType.Teamcraft));
        }

        public static Item GetItemInfo(uint ItemID)
        {
            ExcelSheet<Item> ItemSheet = DataManager.GetExcelSheet<Item>()!;
            return ItemSheet.GetRow((uint)ItemID);
        }

        public static bool IsCraftable(uint ItemID)
        {
            ExcelSheet<RecipeLookup> Recipe = DataManager.GetExcelSheet<RecipeLookup>()!;
            Recipe.TryGetRow(ItemID, out RecipeLookup RandomShit);
            if (RandomShit.RowId != 0)
            {
                return true;
            }   
            else
            {
                return false;
            }
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            CommandManager.Dispose();

            PluginInterface.SavePluginConfig(PluginConfig);

            PluginInterface.UiBuilder.Draw -= ui.Draw;
            PluginInterface.UiBuilder.OpenConfigUi -= () =>
            {
                PluginUI ui = Plugin.ui;
                ui.IsVisible = !ui.IsVisible;
            };

            ContextMenu.OnMenuOpened -= OnContextMenuOpened;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}