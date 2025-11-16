using AiReview.Core.LLM.Naming;
using AiReview.Core.LLM.PurityInspector;
using AiReview.Core.LLM.Review;
using Newtonsoft.Json;
using System;
using System.IO;

namespace AiReview.Core.LLM;

[Serializable]
public sealed record LuminaCodeProjectOptions
{
    public ReviewOptions ReviewOptions = new ReviewOptions();
    public BetterNamingOptions BetterNamingOptions = new BetterNamingOptions();
    public PurityInspectorOptions PureInspectorOptions = new PurityInspectorOptions();

    public static readonly LuminaCodeProjectOptions Default = new();

    public const string OptionsFileName = "luminaCode-options.json";


    public static bool TryLoad(string filePath, out LuminaCodeProjectOptions options)
    {
        options = Default;

        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return false;

        try
        {
            var json = File.ReadAllText(filePath);
            options = JsonConvert.DeserializeObject<LuminaCodeProjectOptions>(json) ?? Default;
            return true;
        }
        catch
        {
            options = Default;
            return false;
        }
    }
}