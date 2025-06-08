using System;
using System.IO;
using AiReview.Core.LLM.Naming;
using AiReview.Core.LLM.Review;
using Newtonsoft.Json;

namespace AiReview.Core.LLM;

[Serializable]
public sealed record LuminaCodeProjectOptions
{
    public ReviewOptions ReviewOptions = new ReviewOptions();
    public BetterNamingOptions BetterNamingOptions = new BetterNamingOptions();

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