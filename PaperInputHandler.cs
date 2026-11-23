using System;
using V12.Core.Input;
using PPaper = global::Prowl.PaperUI.Paper;
using PMouse = global::Prowl.PaperUI.PaperMouseBtn;
using PKey = global::Prowl.PaperUI.PaperKey;

namespace V12.Paper;

/// <summary>
/// Feeds V12 input events into a Paper instance. Paper stays backend-agnostic;
/// this bridge is the only place that knows how to translate pointer, wheel,
/// key and text state into Paper's immediate-mode input API.
/// </summary>
public sealed class PaperInputHandler : IInputHandler
{
    private readonly InputService _input;
    private readonly PPaper _paper;

    public PaperInputHandler(InputService input, PPaper paper)
    {
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _paper = paper ?? throw new ArgumentNullException(nameof(paper));
    }

    public PPaper Paper => _paper;

    public void OnInputEvent(V12.Core.Input.InputEvent evt)
    {
        if (evt is null)
            return;

        switch (evt.Type)
        {
            // Consumption is decided from this Paper instance's own state, not
            // the shared capture flags: a world-canvas UI can own the pointer in
            // the same frame without the screen layer swallowing its events.
            case InputEventType.PointerMove:
                _paper.SetPointerState(PMouse.Unknown, evt.Pointer.X, evt.Pointer.Y, false, true);
                if (_paper.WantsCapturePointer)
                    evt.Handled = true;
                break;

            case InputEventType.ButtonDown:
            case InputEventType.ButtonUp:
                bool down = evt.Type == InputEventType.ButtonDown;
                if (evt.Key is { } key)
                {
                    var paperKey = MapKey(key);
                    if (paperKey != PKey.Unknown)
                        _paper.SetKeyState(paperKey, down);
                    if (_paper.WantsCaptureKeyboard || _paper.FocusedElementId != 0)
                        evt.Handled = true;
                }
                else
                {
                    var button = MapButton(evt.Button);
                    if (button != PMouse.Unknown)
                        _paper.SetPointerState(button, evt.Pointer.X, evt.Pointer.Y, down, false);
                    if (_paper.WantsCapturePointer)
                        evt.Handled = true;
                }
                break;

            case InputEventType.Axis:
                if (string.Equals(evt.Name, "ui_wheel", StringComparison.OrdinalIgnoreCase))
                {
                    _paper.SetPointerWheel(evt.Value);
                    if (_paper.WantsCapturePointer)
                        evt.Handled = true;
                }
                break;

            case InputEventType.Char:
            case InputEventType.Text:
                if (evt.Character != '\0')
                {
                    _paper.AddInputCharacter(evt.Character.ToString());
                    if (_paper.WantsCaptureKeyboard || _paper.FocusedElementId != 0)
                        evt.Handled = true;
                }
                break;
        }

        UpdateCaptureState();
    }

    private void UpdateCaptureState()
    {
        try
        {
            _input.UiCapturesPointer = _paper.WantsCapturePointer;
            _input.UiCapturesKeyboard =
                _paper.WantsCaptureKeyboard || _paper.FocusedElementId != 0;
        }
        catch
        {
            // Paper is immediate-mode; a transient state read should not break input.
        }
    }

    private static PMouse MapButton(PointerButton button) => button switch
    {
        PointerButton.Left => PMouse.Left,
        PointerButton.Right => PMouse.Right,
        PointerButton.Middle => PMouse.Middle,
        PointerButton.Button4 => PMouse.Button4,
        PointerButton.Button5 => PMouse.Button5,
        _ => PMouse.Unknown,
    };

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
