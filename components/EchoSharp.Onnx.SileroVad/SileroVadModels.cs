// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning;

namespace EchoSharp.Onnx.SileroVad;

/// <summary>
/// This is a provisioning model that can be obtained from <seealso cref="SileroVadModels"/>
/// </summary>
public class SileroVadModel(Uri uri, ProvisioningModel.ArchiveTypes archiveType, string archiveHash, string integrityHash, long archiveSize, long maxFileSize) : ProvisioningModel(uri, archiveType, archiveHash, integrityHash, archiveSize, maxFileSize);

/// <summary>
/// Collection of provisioning models for Silero VAD.
/// </summary>
public static class SileroVadModels
{
    public static readonly SileroVadModel Op15_16k = new(
         new("https://github.com/sandrohanea/silero-vad/raw/refs/tags/v1/src/silero_vad/data/silero_vad_16k_op15.onnx"),
         ProvisioningModel.ArchiveTypes.None,
         "LYYEwUZ8ETrbFFdlsy9GYymUvwFefL18fs8BORGgk2oRBMFNwcmwebOPUnAYSE6N6grC/DuTrlUtSmSlVdQZdA==",
         "ixiEok1mqOKHdrmM8WIMtuPL834AcRi7J2tQkKDfVwpAS8Mn2CCd07HJSkXuK5QgQLZfZ8qROB32FNvwl/6cRA==",
         1289603,
         1289603);

    public static readonly SileroVadModel Half = new(
        new("https://github.com/sandrohanea/silero-vad/raw/refs/tags/v1/src/silero_vad/data/silero_vad_half.onnx"),
        ProvisioningModel.ArchiveTypes.None,
        "TpLdfDy6XyN6yg8Iulj9pOcF5nhwc5VMMTu78+hUBMo1iLngDuDrNb7o+7Zlp8ya0fevrTMHxuk11jllfybWlA==",
        "PJPRjnIs47XHhncvsO3T6XESMIMzhBYJ6g13yF5a6RaWJe8uKzmIpB794fSo91PKtfXlNQ+buGweZklWjRvcbw==",
        1280395,
        1280395);

    public static readonly SileroVadModel Full = new(
        new("https://github.com/sandrohanea/silero-vad/raw/refs/tags/v1/src/silero_vad/data/silero_vad.onnx"),
        ProvisioningModel.ArchiveTypes.None,
        "R9bOuVQ1yvgEng6hek3ZVYDopZUJdux3CWLFU05k8upxzhlpVxAhBP4BSivfp2bzI6teRra+sfxG2xKWIimJEw==",
        "P+yofTXsePE+cKdGnjOd3mCK/BCaUkrZPcGfN6DBM/eYfzKCw1AMvR0krCpY/XYIW8HkJFqJPg/EL1FwWWHc0g==",
        2327524,
        2327524);
}
