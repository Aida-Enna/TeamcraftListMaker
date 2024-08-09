using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Data.Parsing.Scd;

namespace TeamcraftListMaker;

public class ContextMenuService
{
    //Code copied from https://github.com/Critical-Impact/InventoryTools/blob/8b52fd6eec34bfae78388caea84b2170da838bee/InventoryTools/Services/ContextMenuService.cs#L110
    //Thank you!
    private readonly IGameGui _gameGui = Plugin.GameGui;
    public const int SatisfactionSupplyItemIdx = 0x54;
    public const int SatisfactionSupplyItem1Id = 0x80 + 1 * 0x3C;
    public const int SatisfactionSupplyItem2Id = 0x80 + 2 * 0x3C;
    public const int ContentsInfoDetailContextItemId = 0x17CC;
    public const int RecipeNoteContextItemId = 0x398;
    public const int AgentItemContextItemId = 0x28;
    public const int GatheringNoteContextItemId = 0xA0;
    public const int ItemSearchContextItemId = 0x17D0;
    public const int ItemSearchContextItemId2 = 0x3384;
    public const int ChatLogContextMenuType = ChatLogContextItemId + 0x8;
    public const int ChatLogContextItemId = 0x950;

    public const int SubmarinePartsMenuContextItemId = 0x54;
    public const int ShopExchangeItemContextItemId = 0x54;
    public const int ShopContextMenuItemId = 0x54;
    public const int ShopExchangeCurrencyContextItemId = 0x54;
    public const int HWDSupplyContextItemId = 0x38C;
    public const int GrandCompanySupplyListContextItemId = 0x54;
    public const int GrandCompanyExchangeContextItemId = 0x54;

    public uint? GetGameObjectItemId(IMenuOpenedArgs args)
    {
        var item = args.AddonName switch  
        {
            null => HandleNulls(),
            "Shop" => GetObjectItemId("Shop", ShopContextMenuItemId),
            "GrandCompanySupplyList" => GetObjectItemId("GrandCompanySupplyList", GrandCompanySupplyListContextItemId),
            "GrandCompanyExchange" => GetObjectItemId("GrandCompanyExchange", GrandCompanyExchangeContextItemId),
            "ShopExchangeCurrency" => GetObjectItemId("ShopExchangeCurrency", ShopExchangeCurrencyContextItemId),
            "SubmarinePartsMenu" => GetObjectItemId("SubmarinePartsMenu", SubmarinePartsMenuContextItemId),
            "ShopExchangeItem" => GetObjectItemId("ShopExchangeItem", ShopExchangeItemContextItemId),
            "ContentsInfoDetail" => GetObjectItemId("ContentsInfo", ContentsInfoDetailContextItemId),
            "RecipeNote" => GetObjectItemId("RecipeNote", RecipeNoteContextItemId),
            "RecipeTree" => GetObjectItemId(AgentById(AgentId.RecipeItemContext), AgentItemContextItemId),
            "RecipeMaterialList" => GetObjectItemId(AgentById(AgentId.RecipeItemContext), AgentItemContextItemId),
            "RecipeProductList" => GetObjectItemId(AgentById(AgentId.RecipeItemContext), AgentItemContextItemId),
            "GatheringNote" => GetObjectItemId("GatheringNote", GatheringNoteContextItemId),
            "ItemSearch" => HandleItemSearch(args),
            "ChatLog" => GetObjectItemId("ChatLog", ChatLogContextItemId),
            _ => null,
        };

        if (args.AddonName == "ChatLog" &&
            (item >= 1500000 || GetObjectItemId("ChatLog", ChatLogContextMenuType) != 3))
        {
            return null;
        }

        if (item == null)
        {
            var guiHoveredItem = _gameGui.HoveredItem;
            if (guiHoveredItem >= 2000000 || guiHoveredItem == 0) return null;
            item = (uint)guiHoveredItem % 500_000;
        }

        return item;
    }

    private uint GetObjectItemId(uint itemId)
    {
        if (itemId > 500000)
            itemId -= 500000;

        return itemId;
    }

    private unsafe uint? GetObjectItemId(IntPtr agent, int offset)
        => agent != IntPtr.Zero ? GetObjectItemId(*(uint*)(agent + offset)) : null;

    private uint? GetObjectItemId(string name, int offset)
        => GetObjectItemId(_gameGui.FindAgentInterface(name), offset);

    private unsafe uint? HandleSatisfactionSupply()
    {
        var agent = _gameGui.FindAgentInterface("SatisfactionSupply");
        if (agent == IntPtr.Zero)
            return null;

        var itemIdx = *(byte*)(agent + SatisfactionSupplyItemIdx);
        return itemIdx switch
        {
            1 => GetObjectItemId(*(uint*)(agent + SatisfactionSupplyItem1Id)),
            2 => GetObjectItemId(*(uint*)(agent + SatisfactionSupplyItem2Id)),
            _ => null,
        };
    }
    private unsafe uint? HandleHWDSupply()
    {
        var agent = _gameGui.FindAgentInterface("HWDSupply");
        if (agent == IntPtr.Zero)
            return null;

        return GetObjectItemId(*(uint*)(agent + HWDSupplyContextItemId));
    }
    private unsafe uint? HandleItemSearch(IMenuArgs args)
    {
        //"ItemSearch" => GetObjectItemId(args.AgentPtr, ItemSearchContextItemId),
        var agent = _gameGui.FindAgentInterface("ItemSearch");
        if (agent == IntPtr.Zero)
            return null;

        if (GetObjectItemId(args.AgentPtr, ItemSearchContextItemId) == 0)
        {
            return GetObjectItemId(AgentById(AgentId.ItemSearch), ItemSearchContextItemId2);
        }
        else
        {
            return GetObjectItemId(args.AgentPtr, ItemSearchContextItemId);
        }
    }

    private uint? HandleNulls()
    {
        var itemId = HandleSatisfactionSupply() ?? HandleHWDSupply();
        return itemId;
    }

    private unsafe IntPtr AgentById(AgentId id)
    {
        var uiModule = (UIModule*)_gameGui.GetUIModule();
        var agents = uiModule->GetAgentModule();
        var agent = agents->GetAgentByInternalId(id);
        return (IntPtr)agent;
    }
}