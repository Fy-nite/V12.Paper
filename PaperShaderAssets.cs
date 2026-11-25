using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using V12.Core.Rendering;

namespace V12.Paper;

/// <summary>
/// Built-in Paper UI shader assets. The assets live in this assembly so V12.Paper
/// owns its own visual language; the engine shader registry only resolves them.
/// </summary>
public static class PaperShaderAssets
{
    public const string PaperUri = "shader://v12.paper/paper";
    public const string TextUri = "shader://v12.paper/text";

    /// <summary>World-canvas variants. They recover canvas pixels from world
    /// space through the model matrix; the screen variants can use the raw
    /// model-space pixel vertices because their view is orthographic.</summary>
    public const string WorldPaperUri = "shader://v12.paper/world";
    public const string WorldTextUri = "shader://v12.paper/world-text";

    /// <summary>Solid/text world shader for small HUD panels. It keeps the font
    /// SDF but skips brush and scissor masks.</summary>
    public const string WorldFlatUri = "shader://v12.paper/world-flat";

    private static readonly object Gate = new();
    private static bool _registered;

    [ModuleInitializer]
    internal static void ModuleInit() => EnsureRegistered();

    public static void EnsureRegistered()
    {
        if (_registered)
            return;

        lock (Gate)
        {
            if (_registered)
                return;

            Register(PaperUri, "V12.Paper.Shaders.paper.v12shader");
            Register(TextUri, "V12.Paper.Shaders.text.v12shader");
            Register(WorldPaperUri, "V12.Paper.Shaders.paper_world.v12shader");
            Register(WorldTextUri, "V12.Paper.Shaders.text_world.v12shader");
            Register(WorldFlatUri, "V12.Paper.Shaders.world_flat.v12shader");
            _registered = true;
        }
    }

    private static void Register(string uri, string resourceName)
    {
        var assembly = typeof(PaperShaderAssets).Assembly;
        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
            throw new InvalidOperationException(
                $"Paper shader resource '{resourceName}' is missing from {assembly.GetName().Name}.");

        using var reader = new StreamReader(stream);
        string json = reader.ReadToEnd();
        ShaderAssetRegistry.Instance.RegisterJson(uri, json);
    }
}
