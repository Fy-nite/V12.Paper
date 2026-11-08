using System;
using V12.Core.Input;
using V12.Core.Rendering;
using V12.Core.UI.Paper;

namespace V12.Paper;

/// <summary>
/// Engine-side Paper layout and input surface. It owns no Godot objects and
/// emits a backend-neutral PaperFrame after each step.
/// </summary>
public sealed class PaperUiSystem : IDisposable
{
    private readonly InputService _input;
    private readonly PaperCanvasRenderer _canvas;
    private readonly ProwlPaperRenderer _paper;
    private readonly PaperInputHandler _inputHandler;
    private bool _disposed;

    public PaperUiSystem(InputService input, int width, int height)
    {
        _input = input ?? throw new ArgumentNullException(nameof(input));
        PaperShaderAssets.EnsureRegistered();

        _canvas = new PaperCanvasRenderer();
        _canvas.SetViewport(Math.Max(1, width), Math.Max(1, height));
        _paper = new ProwlPaperRenderer(_canvas, Math.Max(1, width), Math.Max(1, height), 1f);
        _inputHandler = new PaperInputHandler(input, _paper.PaperInstance);
        _input.RegisterHandler(_inputHandler);
    }

    public PaperCanvasRenderer Canvas => _canvas;
    public ProwlPaperRenderer Paper => _paper;
    public PaperFrame LastFrame => _paper.LastPaperFrame;

    public void Apply(UIFrame frame)
    {
        ThrowIfDisposed();
        _paper.ApplyUI(frame);
        _paper.step();
    }

    public void Resize(int width, int height)
    {
        ThrowIfDisposed();
        _paper.Resize(Math.Max(1, width), Math.Max(1, height), 1f);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _input.UnregisterHandler(_inputHandler);
        _paper.Resize(1, 1, 1f);
        _canvas.Dispose();
    }

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(PaperUiSystem));
    }
}
