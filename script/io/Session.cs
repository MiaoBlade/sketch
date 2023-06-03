using Godot;
using System;


namespace Sketchpad
{
    public class Session
    {
        public static void loadSession(SketchPad pad)
        {
            if (!FileAccess.FileExists("user://session.sav"))
            {
                return;
            }
            using var savedGame = FileAccess.Open("user://session.sav", FileAccess.ModeFlags.Read);
            var color = new Color(savedGame.Get32());
            pad.currentLayer.setting.color = color;
            GD.Print($"Load setting from {savedGame.GetPathAbsolute()}");
        }
        public static void saveSession(SketchPad pad)
        {
            using var saveGame = FileAccess.Open("user://session.sav", FileAccess.ModeFlags.Write);
            if (saveGame == null)
            {
                GD.PrintErr(FileAccess.GetOpenError());
                return;
            }
            var color = pad.currentLayer.setting.color;
            saveGame.Store32(color.ToRgba32());

            GD.Print($"Save setting to {saveGame.GetPathAbsolute()}");
        }
    }
}