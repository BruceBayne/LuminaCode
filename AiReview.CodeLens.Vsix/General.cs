using System.ComponentModel;
using Community.VisualStudio.Toolkit;

namespace AiReview.CodeLens.Vsix;

public class BetterNamingSettingsModel : BaseOptionModel<BetterNamingSettingsModel>
{
    [Category("General")]
    [DisplayName("Enabled")]
    [Description("Enable BetterNaming code lens provider")]
    [DefaultValue(true)]
    public bool Enabled { get; set; } = true;


    [Category("General")]
    [DisplayName("API Endpoint")]
    [Description("URL containing the OpenAI API endpoint and request format ({0}=version, {1}=request)")]
    [DefaultValue("https://api.openai.com/{0}/{1}")]
    public string ApiEndpoint { get; set; } = "https://api.openai.com/{0}/{1}";

    [Category("General")]
    [DisplayName("API Key")]
    [Description("AI Studio utilizes Chat GPT API, to use this extension create an API Key and add it here.")]
    public string ApiKey { get; set; }

    [Category("Custom Model")]
    [DisplayName("Custom Language Model ID")]
    [Description("Provide the language model ID to use. (Example: deepseek-coder)")]
    [DefaultValue("")]
    public string CustomLanguageModel { get; set; } = "";
}

public class VsCodeReviewSettingsModel : BaseOptionModel<VsCodeReviewSettingsModel>
{
    [Category("General")]
    [DisplayName("Enabled")]
    [Description("Enable CodeReview code lens provider")]
    [DefaultValue(true)]
    public bool Enabled { get; set; } = true;


    [Category("General")]
    [DisplayName("API Endpoint")]
    [Description("URL containing the OpenAI API endpoint and request format ({0}=version, {1}=request)")]
    [DefaultValue("https://api.openai.com/{0}/{1}")]
    public string ApiEndpoint { get; set; } = "https://api.openai.com/{0}/{1}";

    [Category("General")]
    [DisplayName("API Key")]
    [Description("AI Studio utilizes Chat GPT API, to use this extension create an API Key and add it here.")]
    public string ApiKey { get; set; }

    [Category("Custom Model")]
    [DisplayName("Custom Language Model ID")]
    [Description("Provide the language model ID to use. (Example: deepseek-coder)")]
    [DefaultValue("")]
    public string CustomLanguageModel { get; set; } = "";
}

public class General : BaseOptionModel<General>
{
    [Category("General")]
    [DisplayName("Enabled")]
    [Description("Globally enable/disable LuminaCode")]
    [DefaultValue(true)]
    public bool Enabled { get; set; } = true;


    [Category("General")]
    [DisplayName("Language Model")]
    [Description("Chat language model")]
    [DefaultValue(ChatLanguageModel.ChatGPTTurbo)]
    public ChatLanguageModel LanguageModel { get; set; } = ChatLanguageModel.ChatGPTTurbo;


    [Category("General")]
    [DisplayName("API Endpoint")]
    [Description("URL containing the OpenAI API endpoint and request format ({0}=version, {1}=request)")]
    [DefaultValue("https://api.openai.com/{0}/{1}")]
    public string ApiEndpoint { get; set; } = "https://api.openai.com/{0}/{1}";

    [Category("General")]
    [DisplayName("API Key")]
    [Description("AI Studio utilizes Chat GPT API, to use this extension create an API Key and add it here.")]
    public string ApiKey { get; set; }

    [Category("Custom Model")]
    [DisplayName("Custom Language Model ID")]
    [Description("Provide the language model ID to use. (Example: deepseek-coder)")]
    [DefaultValue("")]
    public string CustomLanguageModel { get; set; } = "";
}