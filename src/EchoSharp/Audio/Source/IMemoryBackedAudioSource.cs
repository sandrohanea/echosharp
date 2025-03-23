// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Audio.Source;

internal interface IMemoryBackedAudioSource
{
    public bool StoresFloats { get; }

    public bool StoresBytes { get; }
}
