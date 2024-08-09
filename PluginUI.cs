using ImGuiNET;
using System.Numerics;
using Veda;

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
            if (!IsVisible || !ImGui.Begin("", ref IsVisible, ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize)) { return; }
            ImGui.SetWindowPos(new Vector2(SelectAmountX + 50, SelectAmountY + 50));
            ImGui.SetWindowFocus();
            ImGui.Text("Amount to add:");
            ImGui.SetNextItemWidth(100);
            if (ImGui.IsWindowAppearing()) { ImGui.SetKeyboardFocusHere(); }
            if (ImGui.InputInt("###Inputshit", ref AmountToAdd, 0, 0, ImGuiInputTextFlags.EnterReturnsTrue))
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
}