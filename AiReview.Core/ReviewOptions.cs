using System;
using System.IO;
using Newtonsoft.Json;

namespace AiReview.Core
{
    [Serializable]
    public sealed record ReviewOptions
    {
        public string MainPrompt { get; set; } = PromptDatabase.GeneralReviewPrompt;

        public static readonly ReviewOptions Default = new();

        public const string OptionsFileName = "luminaCode-options.json";


        public static bool TryLoad(string filePath, out ReviewOptions options)
        {
            options = Default;

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return false;

            try
            {
                var json = File.ReadAllText(filePath);
                options = JsonConvert.DeserializeObject<ReviewOptions>(json) ?? Default;
                return true;
            }
            catch
            {
                options = Default;
                return false;
            }
        }
    }
}