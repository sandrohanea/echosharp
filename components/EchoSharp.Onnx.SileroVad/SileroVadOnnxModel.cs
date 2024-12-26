// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Microsoft.ML.OnnxRuntime;

namespace EchoSharp.Onnx.SileroVad;
internal class SileroVadOnnxModel : IDisposable
{
    private readonly InferenceSession session;
    private static readonly long[] sampleRateInput = [SileroConstants.SampleRate];
    private readonly long[] stateShape = [2, 1, 128];
    private readonly long[] runningInputShape = [1, SileroConstants.BatchSize + SileroConstants.ContextSize];
    private readonly RunOptions runOptions;

    public SileroVadOnnxModel(string modelPath)
    {
        var sessionOptions = new SessionOptions
        {
            InterOpNumThreads = 1,
            IntraOpNumThreads = 1
        };

        session = new InferenceSession(modelPath, sessionOptions);
        runOptions = new RunOptions();
    }

    public void Dispose()
    {
        session?.Dispose();
    }

    public SileroInferenceState CreateInferenceState()
    {
        var state = new SileroInferenceState(session.CreateIoBinding());

        state.Binding.BindInput("state", OrtValue.CreateTensorValueFromMemory(state.State, stateShape));
        state.Binding.BindInput("sr", OrtValue.CreateTensorValueFromMemory(sampleRateInput, [1]));

        state.Binding.BindOutput("output", OrtValue.CreateTensorValueFromMemory(state.Output, [1, SileroConstants.OutputSize]));
        state.Binding.BindOutput("stateN", OrtValue.CreateTensorValueFromMemory(state.PendingState, stateShape));
        return state;
    }

    public float Call(Memory<float> input, SileroInferenceState state)
    {
        state.Binding.BindInput("input", OrtValue.CreateTensorValueFromMemory(OrtMemoryInfo.DefaultInstance, input, runningInputShape));
        // We need to swap the state and pending state to keep the state for the next inference
        // Zero allocation swap
        (state.State, state.PendingState) = (state.PendingState, state.State);

        session.RunWithBinding(runOptions, state.Binding);

        return state.Output[0];
    }
}
