// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning;
using EchoSharp.SpeechTranscription;

namespace EchoSharp.OpenAI.Whisper;

public class OpenAIWhisperSpeechTranscriporProvisioner(OpenAiWhisperSpeechTranscriptorConfig config) : ISpeechTranscriptorProvisioner
{
    private const string openApiKeyEnvVarName = "OPENAI_API_KEY";

    public async Task<ISpeechTranscriptorFactory> ProvisionAsync(CancellationToken cancellationToken = default)
    {
        var factory = config.AudioClient is not null
            ? new OpenAIWhisperSpeechTranscriptorFactory(config.AudioClient, config.Temperature)
            : config.ApiKey is not null
                ? new OpenAIWhisperSpeechTranscriptorFactory(config.ApiKey, config.Temperature)
                : GetEnvVariableFactory();

        return await factory.WarmUpAsync(config.WarmUp, cancellationToken);
    }

    private OpenAIWhisperSpeechTranscriptorFactory GetEnvVariableFactory()
    {
        var envVariable = Environment.GetEnvironmentVariable(openApiKeyEnvVarName);
        return envVariable is not null
            ? new OpenAIWhisperSpeechTranscriptorFactory(envVariable, config.Temperature)
            : throw new InvalidOperationException($"The required API key for OpenAI Whisper is missing. Ensure that either the environment variable '{openApiKeyEnvVarName}' is set or that 'config.ApiKey' or 'config.AudioClient' is provided in the configuration.");
    }
}
