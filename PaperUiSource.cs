using System;
using System.Collections.Generic;
using V12.Core.Input;
using V12.Core.Rendering;
using V12.Core.UI.Paper;

namespace V12.Paper;

/// <summary>
/// Paper implementation of the engine's <see cref="IUiSource"/>: lays out
/// world canvases, steps their surfaces and routes pointer/key input. Hosts
/// hand this to the frame producer, so the engine and renderers stay free of
/// Paper and Prowl types.
/// </summary>
public sealed class PaperUiSource : IUiSource, IDisposable
{
    private readonly PaperWorldCanvasSystem _canvases;
    private readonly Dictionary<long, PaperSurfaceInput> _input = new();
    private readonly Dictionary<long, PaperFrame> _frames = new();
    private bool _disposed;

    public PaperUiSource()
        : this((_, _, _) => new PaperCanvasRenderer())
    {
    }

    public PaperUiSource(Func<long, int, int, PaperCanvasRenderer> rendererFactory)
    {
        _canvases = new PaperWorldCanvasSystem(
            rendererFactory ?? throw new ArgumentNullException(nameof(rendererFactory)));
    }

    /// <summary>Underlying canvas surfaces (hosts that still use the legacy path).</summary>
    public PaperWorldCanvasSystem Canvases => _canvases;

    public IReadOnlyDictionary<long, PaperFrame> WorldCanvases => _frames;

    public IEnumerable<long> CanvasIds => _canvases.Surfaces.Keys;

    public void PrepareWorldCanvases(UIFrame frame)
    {
        ThrowIfDisposed();
        _canvases.EnsureSurfaces(frame);

        // Drop input bridges whose canvas disappeared.
        if (_input.Count > 0)
        {
            List<long>? stale = null;
            foreach (var id in _input.Keys)
            {
                if (_canvases.Surfaces.ContainsKey(id)) continue;
                (stale ??= new List<long>()).Add(id);
            }
            if (stale != null)
                foreach (var id in stale) _input.Remove(id);
        }
    }

    public void StepWorldCanvases()
    {
        ThrowIfDisposed();
        _canvases.Step();

        _frames.Clear();
        foreach (var pair in _canvases.Surfaces)
        {
            if (pair.Value.LastFrame is { } frame)
                _frames[pair.Key] = frame;
        }
    }

    public bool FeedCanvasPointer(long canvasId, float x, float y, bool down)
    {
        if (_disposed || !_canvases.Surfaces.TryGetValue(canvasId, out var surface))
            return false;

        surface.FeedPointer(x, y, down);
        return true;
    }

    public bool FeedCanvasInput(long canvasId, InputEvent evt)
    {
        if (_disposed || !_canvases.Surfaces.TryGetValue(canvasId, out var surface))
            return false;

        if (!_input.TryGetValue(canvasId, out var bridge))
            _input[canvasId] = bridge = new PaperSurfaceInput(surface.Paper.PaperInstance);

        return bridge.Feed(evt);
    }

    public bool ContainsCanvas(long canvasId) =>
        !_disposed && _canvases.Surfaces.ContainsKey(canvasId);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _input.Clear();
        _frames.Clear();
        _canvases.Dispose();
    }

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(PaperUiSource));
    }
}
