// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using EchoSharp.Audio.Sink;
using EchoSharp.AzureAI.SpeechServices;
using EchoSharp.AzureAI.SpeechServices.RealTime;
using EchoSharp.AzureAI.SpeechServices.SpeechSynthesis;
using EchoSharp.NAudio;
using EchoSharp.Provisioning;
using EchoSharp.SpeechSynthesis;

Console.WriteLine(
    "This is a demonstration for text to speech. Write your desired text and press enter. If you want to exit, write \"exit\".");

var text = Console.ReadLine();
var provider = args.Length > 0 ? args[0] : "azure";
var speechProvisioner = GetSpeechSynthesisProvisioner(provider);

var speechSynthesisFactory = await speechProvisioner.ProvisionAsync();
var speechOptions = GetSpeechSynthesisOptions(provider);

while (text != "exit" && !string.IsNullOrEmpty(text))
{
    // Currently, the audio out is only supported by the NAudio component which is not supported on non-Windows platforms
    // For other platforms, we use a WaveFileSink to save the audio to a file
    await using IAudioSink speakerOutSink =
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new SpeakerOutSink()
            : new WaveFileSink($"output-{DateTime.Now.Ticks}.wav");

    using var speechSynthesizer = speechSynthesisFactory.Create(speechOptions);
    await speechSynthesizer.SynthesizeAsync(new SpeechSegment() { Text = text }, speakerOutSink);

    text = Console.ReadLine();
}

SpeechSynthesizerOptions GetSpeechSynthesisOptions(string providerName)
{
    return new SpeechSynthesizerOptions()
    {
        DefaultVoice =
            providerName switch
            {
                "azure" => "en-US-DavisNeural",
                _ => throw new NotImplementedException($"Provider {providerName} not implemented.")
            }
    };
}

ISpeechSynthesisProvisioner GetSpeechSynthesisProvisioner(string providerName)
{
    return providerName switch
    {
        "azure" => GetAzureSpeechSynthesisProvisioner(),
        _ => throw new NotImplementedException($"Provider {providerName} not implemented.")
    };
}

ISpeechSynthesisProvisioner GetAzureSpeechSynthesisProvisioner()
{
    var config = new AzureSpeechServicesConfig()
    {
        AzureRegion = "westeurope", //TODO: Replace with your region
        SubscriptionKey = "<your-subscription-key>" // TODO: Replace with your subscription key
    };

    return new AzureSpeechSynthesisProvisioner(config, new AzureSpeechSynthesizerOptions());
}
