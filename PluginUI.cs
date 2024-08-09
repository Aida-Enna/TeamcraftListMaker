using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game;
using Dalamud.Plugin;
using ImGuiNET;
using ImGuiScene;
using System;
using System.Diagnostics;
using System.Net;
using System.Numerics;
using TwitchLib.Api.Helix;
using TwitchLib.Client.Models;
using Veda;
using Dalamud.Configuration;
using Dalamud.Interface.Utility;

namespace TeamcraftListMaker
{
    public class PluginUI
    {
        public bool IsVisible;
        public bool ShowSupport;
        public bool IsSelectAmountVisible;
        public int SelectAmountX = 0;
        public int SelectAmountY = 0;
        public int AmountToAdd;
        public uint? SelectItemID;
        public void Draw()
        {
            if (!IsVisible || !ImGui.Begin("", ref IsVisible, ImGuiWindowFlags.NoDecoration)) { return; }
            ImGui.SetWindowPos(new Vector2(SelectAmountX+50, SelectAmountY+50));
            ImGui.SetWindowFocus();
            ImGui.Text("Amount to add:");
            ImGui.SetNextItemWidth(100);
            if (ImGui.IsWindowAppearing()) { ImGui.SetKeyboardFocusHere(); }
            if (ImGui.InputInt("###Inputshit", ref AmountToAdd,0,0,ImGuiInputTextFlags.EnterReturnsTrue))
            {
                Plugin.AddItem(SelectItemID, AmountToAdd);
                this.IsVisible = false;
            }
            ImGui.Checkbox("Don't reset", ref Plugin.PluginConfig.DoNotResetAmount);
            if (ImGui.Button("OK"))
            {
                Plugin.AddItem(SelectItemID, AmountToAdd);
                this.IsVisible = false;
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                this.IsVisible = false;
            }
            ImGui.End();
        }
    }
}
