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
