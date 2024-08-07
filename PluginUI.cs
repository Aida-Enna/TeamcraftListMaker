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

namespace TeamcraftListMaker
{
    public class PluginUI
    {
        public bool IsVisible;
        public bool ShowSupport;
        public void Draw()
        {
            if (!IsVisible || !ImGui.Begin("Teamcraft List Maker Config", ref IsVisible, ImGuiWindowFlags.AlwaysAutoResize))
                return;
            ImGui.Text("Some Text");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(40);
            ImGui.DragInt("", ref Plugin.PluginConfig.MaxClassLevel, 1, 1, 201);
            ImGui.Checkbox("Some checkbox", ref Plugin.PluginConfig.EmptyVariable);

            if (ImGui.Button("Save"))
            {
                Plugin.PluginConfig.Save();
                this.IsVisible = false;
            }

            ImGui.SameLine();
            ImGui.Indent(200);

            if (ImGui.Button("Want to help support my work?"))
            {
                ShowSupport = !ShowSupport;
            }
            if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Click me!"); }

            if (ShowSupport)
            {
                ImGui.Indent(-200);
                ImGui.Text("Here are the current ways you can support the work I do.\nEvery bit helps, thank you! Have a great day!");
                ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(0.19f, 0.52f, 0.27f, 1));
                if (ImGui.Button("Donate via Paypal"))
                {
                    Functions.OpenWebsite("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=QXF8EL4737HWJ");
                }
                ImGui.PopStyleColor();
                ImGui.SameLine();
                ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(0.95f, 0.39f, 0.32f, 1));
                if (ImGui.Button("Become a Patron"))
                {
                    Functions.OpenWebsite("https://www.patreon.com/bePatron?u=5597973");
                }
                ImGui.PopStyleColor();
                ImGui.SameLine();
                ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(0.25f, 0.67f, 0.87f, 1));
                if (ImGui.Button("Support me on Ko-Fi"))
                {
                    Functions.OpenWebsite("https://ko-fi.com/Y8Y114PMT");
                }
                ImGui.PopStyleColor();
            }
            ImGui.End();
        }
    }
}
