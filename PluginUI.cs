using Dalamud.Bindings.ImGui;
using System.Net;
using System.Numerics;
using Veda;

namespace TeamcraftListMaker
{
    public class SelectAmountUI
    {
        public bool IsVisible;
        public bool IsSelectAmountVisible;
        public int SelectAmountX = 0;
        public int SelectAmountY = 0;
        public int AmountToAdd;
        public uint? SelectItemID;

        public void Draw()
        {
            if (!IsVisible || !ImGui.Begin("", ref IsVisible, ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize)) { return; }
            ImGui.SetWindowPos(new Vector2(SelectAmountX + 50, SelectAmountY + 50));
            ImGui.SetWindowFocus();
            ImGui.Text("Amount to add:");
            ImGui.SetNextItemWidth(100);
            if (ImGui.IsWindowAppearing()) { ImGui.SetKeyboardFocusHere(); }
            if (ImGui.InputInt("###Inputshit", ref AmountToAdd, 0, 0, flags:ImGuiInputTextFlags.EnterReturnsTrue))
            {
                if (AmountToAdd > 0)
                {
                    Plugin.AddItem(SelectItemID, AmountToAdd);
                    this.IsVisible = false;
                }
                else
                {
                    Plugin.Chat.Print(Functions.BuildSeString("Teamcraft List Maker", "Please select a number greater than 0.", ColorType.Error));
                }
            }
            //Not currently working cause ImGui sucks ass and won't return true on the above InputInt if the number isn't changed
            //ImGui.Checkbox("Don't reset", ref Plugin.PluginConfig.DoNotResetAmount);
            //if (ImGui.Button("OK"))
            //{
            //    if (AmountToAdd > 0)
            //    {
            //        Plugin.AddItem(SelectItemID, AmountToAdd);
            //        this.IsVisible = false;
            //    }
            //    else
            //    {
            //        Plugin.Chat.Print(Functions.BuildSeString("Teamcraft List Maker", "Please select a number greater than 0.", ColorType.Error));
            //    }
            //}
            //ImGui.SameLine();
            //ImGui.SetNextItemWidth(100);
            if (ImGui.Button("Cancel",new Vector2(100,20)))
            {
                this.IsVisible = false;
            }
            ImGui.End();
        }
    }

    public class ConfigUI
    {
        public bool IsVisible;
        public bool ShowSupport;
        public bool DontPostExportLink = false;

        public void Draw()
        {
            if (!IsVisible || !ImGui.Begin("Teamcraft List Maker Config", ref IsVisible, ImGuiWindowFlags.AlwaysAutoResize))
                return;
            if (ImGui.Button("Save"))
            {
                Plugin.PluginConfig.Save();
                this.IsVisible = false;
            }
            ImGui.SameLine();
            ImGui.Checkbox("Don't post exported link in chat", ref DontPostExportLink);
            //ImGui.Spacing();
            //ImGui.Indent(-275);
            if (ImGui.Button("Want to help support my work?"))
            {
                ShowSupport = !ShowSupport;
            }
            if (ImGui.IsItemHovered()) { ImGui.SetTooltip("Click me!"); }
            if (ShowSupport)
            {
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