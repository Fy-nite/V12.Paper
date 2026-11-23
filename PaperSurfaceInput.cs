using System;
using V12.Core.Input;
using V12.Core.UI.Paper;
using PPaper = global::Prowl.PaperUI.Paper;
using PKey = global::Prowl.PaperUI.PaperKey;

namespace V12.Paper
{
    /// <summary>
    /// Translates engine input events into one Paper instance's key/text state.
    /// Used for focused world canvases; the screen-space UI uses
    /// <see cref="PaperInputHandler"/> instead.
    /// </summary>
    public sealed class PaperSurfaceInput
    {
        private readonly PPaper _paper;

        public PaperSurfaceInput(PPaper paper)
        {
            _paper = paper ?? throw new ArgumentNullException(nameof(paper));
        }

        public PPaper Paper => _paper;

        /// <summary>Feeds a key or text event. Returns true when the event carried UI input.</summary>
        public bool Feed(InputEvent? evt)
        {
            if (evt == null) return false;

            switch (evt.Type)
            {
                case InputEventType.ButtonDown:
                case InputEventType.ButtonUp:
                    if (evt.Key is not { } key) return false;
                    var paperKey = MapKey(key);
                    if (paperKey == PKey.Unknown) return false;
                    _paper.SetKeyState(paperKey, evt.Type == InputEventType.ButtonDown);
                    return true;

                case InputEventType.Char:
                case InputEventType.Text:
                    if (evt.Character == '\0') return false;
                    _paper.AddInputCharacter(evt.Character.ToString());
                    return true;

                default:
                    return false;
            }
        }

        private static PKey MapKey(InputKey key)
        {
            if (key >= InputKey.D0 && key <= InputKey.D9)
            {
                int digit = key - InputKey.D0;
                string name = digit == 0 ? "Num0" : "Num" + digit;
                return Enum.TryParse<PKey>(name, true, out var num) ? num : PKey.Unknown;
            }

            return Enum.TryParse<PKey>(key.ToString(), true, out var parsed)
                ? parsed
                : PKey.Unknown;
        }
    }
}
