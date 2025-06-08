using System;

namespace AiReview.Core.LLM;

[Serializable]
public abstract class BasicAnswer
{
    public string RawRequest { get; set; }
    public string RawResponse { get; set; }

    public float Temperature { get; set; }
    public string LLmProps { get; set; }
    public TimeSpan Duration { get; set; }


    public float TokensPerSecond => RawResponse != null && Duration.TotalSeconds > 0
        ? (float)(RawResponse.Length / Duration.TotalSeconds)
        : 0;
}