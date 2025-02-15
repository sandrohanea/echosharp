// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning;
using Whisper.net.Ggml;

namespace EchoSharp.Whisper.net;

public static class WhisperNetModels
{
    public static ProvisioningModel GetGgmlModel(QuantizationType quantizationType, GgmlType ggmlType)
    {
        return quantizationType switch
        {
            #region NoQuantization
            QuantizationType.NoQuantization => ggmlType switch
            {
                GgmlType.Base => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/classic/ggml-base.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "rHydS7/W8wHTseloPkt1e6ZbVmp2FVAroz6b34/GlrgX3VAb0K3OnkoQ/UGy9BSL/NmIUHo9jELvx++tROts5w==",
                        "Y73oT5uiqjglaLe8KcF4fJYJGosPwnWCQbySYuOhGHPD1+gIl5V/rdSrUpzQh2Z36Tmd2eGMbsexpqmCW6SfFQ==",
                        147951465,
                        147951465),
                GgmlType.BaseEn => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/classic/ggml-base.en.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "kEXFYY5tq3ebSVqeY5eT3m62/W3EyF7U4eMbq2dj0MoKQL+J8IQVTEdl0VGtx/7OtWxOjJBXje4oUChvCG2Xcg==",
                        "SrgHOJ1vjj+1+DUV76b19V/BLHdm2zONolk9vDcSbOUvAPp3d65WrCC5DlZwSHFc1/SgJt8GAPjc7juJm0UAHA==",
                        147964211,
                        147964211),
                GgmlType.LargeV1 => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/classic/ggml-large-v1.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "sF+WqSDXsevv48mNiGNWQHy/7cAW53eWtZZ78RRLzTQHcIxZ0q6fZ0p960iji4WwaihbxEhBjmnmlhhBq41kqA==",
                        "qCmBr6AmRyqRzSBLoOCUQ2GEDwGixdawehv/H1Rc93ZLGRQ6YJJgTidIeechJAI0zi5frh8LZJmCykc+fABmRQ==",
                        3094623691,
                        3094623691),
                GgmlType.LargeV2 => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/classic/ggml-large-v2.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "/TX8Edx+WjG9orh+N8xPSB0ukJCHp6AiwwJqumtmxCIdrQJGOmcG0W3wXxeuSPaGgmT864TCUdI5vgm5D2trDQ==",
                        "MA0bHWMO7cniU0redqJ/d5Q6o7ApVldAg52/99WBjJHRlWAlzu21DPFXRJ3hjml+r1KBEoogKFuoSjFyNfwQpg==",
                        3094623691,
                        3094623691),
                GgmlType.LargeV3 => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/classic/ggml-large-v3.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "5++wQ9pdBkkPCaOthMzbZ8jvu/lTCbgOPdZ0VGhVwoH9G4876V1rynqstCKpiZvaIdZaUVWyy4PAPBVxviEQKg==",
                        "wiKhEhJp7/0EsiXt60oz7NZKzgYDg6huRmYCveaN0rddTolPbZYW5E1XOveRuf8JvipGMWZhDEvsGtXEXDYn3Q==",
                        3095033483,
                        3095033483),
                GgmlType.LargeV3Turbo => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/classic/ggml-large-v3-turbo.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "+JWtOcgfIcOo9h2uY9T7KOGdRlZiJtFBrzp45GNxPX1Evx/1EQNNTPAVir3uDWrJkIHXIF2KNX7vBuBYu0F7tA==",
                        "xE3p4C1LBx2j8341N8aRPfqeA/puS41PHMHgIES9pe32jBGej3MkRMOHUU3ikCgCqfzAPj3ZK+EFN7MWbPmVjg==",
                        1624555275,
                        1624555275),
                GgmlType.Medium => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/classic/ggml-medium.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "eXadiTj8AUSBXIl7NE/EGK6hdY8jj/o3YlDzB2pW+0D2fSfHLdiaRjQ0X0ase46J2p81QZQyHsXe/OUDqNjrqQ==",
                        "+7rFMdA/tgTQCdeOaJ3qb9fbJyoj5o+xN1NDbtL/iFGWefGSR+Kh0wC/rGdogy/gLVc8GvlkGTTs3kYqzdyv5A==",
                        1533763059,
                        1533763059),
                GgmlType.MediumEn => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/classic/ggml-medium.en.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "yUvHMD8LOMNr/v5GbjJ0A49SL8Q1xt/WrpAkyT8oh8z839kRN+lY/mBDnNoSTpzDn9GJgBaVfHusN4kCUVYTJA==",
                        "QxVD/HOAT/by25sU6ACo4EsHJes6VRHFNLlV2Jrp93gJZTgBl/fxJwyisDv3RhCVZKLIP5YVTPRmhXNSY2IABQ==",
                        1533774781,
                        1533774781),
                GgmlType.Small => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/classic/ggml-small.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "h5SBVGraq+/oMi51eWcdPgqh8BSiIXYeMQDmCCemlslHmLHAel8UXTafUqh4U7+HAKMIeV82YB8loGvFU40QdQ==",
                        "zC2cRrGsCsEE2fUgFWeW5x3iq/B6yXRbdX3Vp4mp9Z7eIOFazx284QUVZy5NZQVRhrkri80Dzu+Esvl6L2LZCw==",
                        487601967,
                        487601967),
                GgmlType.SmallEn => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/classic/ggml-small.en.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "Up12ONMy7SVUndDfL5z6Ekip36RuARtlFhm4dEMUbdr7VNPl4lXcZcJ7eF7qISKALEccQyOomgq/9RcED5rCsg==",
                        "BC8L5vsitJ+rU6GQMX5bxFQmL3Uyo0EA/Fxt/04SkWPsOsvwLc/sN6XWvFaVHJUPzIBwtEF+fN9Dh17ccGFMiA==",
                        487614201,
                        487614201),
                GgmlType.Tiny => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/classic/ggml-tiny.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "iBnYOKQwrv+neOB0bnD0oubytsn+fXQxkfvcQAKE9iSYKxsonc21JR9zK8If6rIFogMvhaRSF8jjLthsDXww6Q==",
                        "mGvVfpQy+gpNpYGWyvYP9KeAlqeRhLnZAR9czXH6AF7Nu7lUT5n2tU7mdq81QVyfTcOnbQZhisWTvA0IXww19A==",
                        77691713,
                        77691713),
                GgmlType.TinyEn => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/classic/ggml-tiny.en.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "GtJUJYkezKt8Ap+1eOIn8GLe3HlrSx3CGogfd4IEKdaX+gIR8xd6pTpA4dcDTlNDajiUUJA1JuWtg/xm0Lrnzw==",
                        "IBZCFbH9Wx4jzFOOqJ7ns1o/Qca/qEbbVarkxye9Z/faTFR1TVfs/tzF0y8zKblBtxrQ954HuBz8FaiyHYt/ig==",
                        77704715,
                        77704715),
                _ => throw new NotSupportedException("This ggml type is not supported.")
            },
            #endregion
            #region Q4_0
            QuantizationType.Q4_0 => ggmlType switch
            {
                GgmlType.Base => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q4_0/ggml-base.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "PYpdm2V2SGyYtZVjrES953zsO3bRqJZ1xO8W2JfuMNm8mWy+E5zmfgJ7D7AxsntAhDfikYVOJgTOTSnvinjnHw==",
                        "uTVmbc9D4IkeH5OorSvjjdaCZFvoy8xOBUOlQFobGcEZ3j+HkU5hJLrO28d22WxG44xw4jL61rNL+jJGC2gQQQ==",
                        46471049,
                        46471049),
                GgmlType.BaseEn => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q4_0/ggml-base.en.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "S5HiP9GCr/y5Em22Cw5g6I5fPz/U/TKTchk7WMV2nJeSXwSzmhqw1zZGsDm1uzVM8Ve+WjYxbleZDsIfWo1SvA==",
                        "HttQm5BXQja1jD3TK3VlW/k6ndTSrTN+e5np4+2zaFZDiHouzL/giLF+6ubYxiGxh9fHHGM3FcwNbpGHmna9Bw==",
                        46484531,
                        46484531),
                GgmlType.LargeV1 => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q4_0/ggml-large-v1.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "7eTDcwqqXxY7C/QWodRwWpeJ1bxbNejpRzXVolZq5EWrg4OxEHEV155y+Vb1W32rNdNVej8yFvfnbjx8emyUlw==",
                        "cedGOT5Sj21U9VjhIyCvVm731OpkhMBDtVIcq3wPF7eRv9odCEOWMBwicqEtPnui3ZSjykxEuqlIWSJG/7yhoA==",
                        888932891,
                        888932891),
                GgmlType.LargeV2 => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q4_0/ggml-large-v2.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "D3PXSewJsSiYLPHLU/2pZmayzWuCd4CNkikbT9Y292hAai9afcBwewULrP93vrayazR27OBetua8yc0ERh3fvg==",
                        "VkLVRHMdehNU3FJFDl5RlHUDkSfTQyptWwrcUOCnkGH0PU0l/lLd/QUyJ+mqDw0+0GDZScPMSfmkJMOOCAW3xA==",
                        888932891,
                        888932891),
                GgmlType.LargeV3 => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q4_0/ggml-large-v3.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "qIb4ETyd0yKLMdg4iWDMFro+233NTnNDxhWlJt9wf+sYU69+dl8MKUZIJtIxD3ehYVJmQOklfGmtu+wC1eP7rQ==",
                        "xveTMNh8pcC7hHR8fMyoVeAQ7vx51jrYLULzi6eoJS7/ZcQf/pa8FIlZm+Qx6K6Qr0z/U4jKW7OTZGArZcE82w==",
                        889340843,
                        889340843),
                GgmlType.LargeV3Turbo => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q4_0/ggml-large-v3-turbo.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "tBPvoBHePnp9T0fk2NR8m4nNIiicAHso+iO6KnKUH3GOyKEr2fA0Vm9HuUtDV1eGeEoh9gyeTZYsJj4gUPuHwg==",
                        "IFrN6Bl+NuI/t43RmoWdrbYpxTz5rd/Mvtw7Vc+qj94AfbMzjyzVY3jxPNsoVRWlXYTeELlUurpWGEhrUZNpkg==",
                        473992235,
                        473992235),
                GgmlType.Medium => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q4_0/ggml-medium.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "V6wUnxk8MSRMW0W+I52cG3vEERCVwWdUpFaVEtNt/8NLUCh6b+Ua8lFeZ3XIhXrWXgRDkuITHHG7p73OcXun7w==",
                        "KGGq6hcBpe3nEF8OCbobD4T4O3MSemeUKBW4Y92Iukz763ToZGPTtQgxQ7q8zXQObQjcsqWWZ8VwIBOt16TK4g==",
                        444493363,
                        444493363),
                GgmlType.MediumEn => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q4_0/ggml-medium.en.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "ZYL4LeLIoGzSWmNRhIAO2MaidNuz36OMzacR1Ww8dLOkSzauaMzjdsfcIRWfepXEBP54BBpB8DjIvyXujYdctw==",
                        "Ef1zhIUxbpo70zTwkcqliGIuxTOllwRZddmbctmqmWvP3r4ZjpOKHIEsBiEAcgOBohS9FpuqbisnkDJwrb0CvQ==",
                        444506557,
                        444506557),
                GgmlType.Small => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q4_0/ggml-small.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "LFxXkka1aYQMCa+u/PCWnYH9PQv1k06CtOwNST76Kn6lw6pkIGDGDhAhKLf4glHI2ut8XdDzD6ZiHbqHIB/oVw==",
                        "84VSXgMCVNqqyGmQOlGu4Ft2YhgGaeNE0OfnKrFHXcZD+Udm0x6Las1wmMN3+B4CfePtAS3F4Cj+s6lpNUnNcw==",
                        145458015,
                        145458015),
                GgmlType.SmallEn => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q4_0/ggml-small.en.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "vqGpcPjf4ASgEUqZKB0DALQnb9mkPi3JRzgXhbwEVjou7Czr4PAVi9Im8G/KPN3ed+VCNDCXLzpRbzA6IQy3/Q==",
                        "AaKRAPpkCJP5viyiOWD/kvLhbo9af4j8jeO/7sX7sjcS8nveybXcu3YUNxJhvPGwJM5UnV58K4zbiE9UIjbjJQ==",
                        145471353,
                        145471353),
                GgmlType.Tiny => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q4_0/ggml-tiny.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "RoevJUFK/xVjViyDgwyk6EWr7TY6tnczF4MGwYB40I5FLFQpVrW9Xi7y4P8wPnrnfdMWjjnz+aDnhuJR2aQ7iw==",
                        "boQuQ2HhdYsqQyQz9vDOkVPbgxBzAF/QPlPTrCHBImZHk08XhBl1myxh8fNYHwEeC3DhC92Aacyzz9I4a/oa/g==",
                        25321817,
                        25321817),
                GgmlType.TinyEn => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q4_0/ggml-tiny.en.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "Lzk7cbHrSxOPtK3P8RW+oL+YjxlMOa5hq6UACfGsKk5TAzvgxHjcn1PYE+02YOjb7voJwjwrtNitgrHKHR3rzg==",
                        "uxFust5WPJKRuSHIHmXtPsC9AH9HQwvMqVu2ze8ARs19EQmfPueWlNblNCSdusOtokXBoQkAxlvwLthnsiTdaQ==",
                        25335371,
                        25335371),
                _ => throw new NotSupportedException("This ggml type is not supported.")
            },
            #endregion
            #region Q4_1
            QuantizationType.Q4_1 => ggmlType switch
            {
                GgmlType.Base => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q4_1/ggml-base.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "fohaXEE7fIhYgK0NlX8lqS2UUkyxcGwsv28tcN1BNfCumnmOFe4DYvlu41jQEhGhQk5QDfhrlHhEkI1OmAnBDA==",
                        "YHZpu0jpUCwCbpolOz6s5awd+n7vlgHiX2deQ1HNFCmCz7HqvHGknVVtKRUeNId6N7mRNSzjdE7fxd+htLqv5A==",
                        50883241,
                        50883241),
                GgmlType.BaseEn => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q4_1/ggml-base.en.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "/83xQOz3ca+we1rJyCuzJNWyCwU11dO2Vqgmc+x4MtXPEWeklujGlGWSoOhd37IEa9bEFzecdqIzHd5U/c2cBQ==",
                        "16yDKSyBsG1qCqY9WTwwil71dYRSXTBmG7cQNBCabnp9vNtgHKYJoKLOY0vBVYk7ywQLCEENQFtW0jF2GnRhlQ==",
                        50896691,
                        50896691),
                GgmlType.LargeV1 => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q4_1/ggml-large-v1.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "yJhPgrR7KNGf/I7EDX14xAB8eLiZvxxAvZY5Mp4mHla5dT0NpFHxAAVL1gozEAO9DsjFWhZGpcbC9Bsh3WVWxA==",
                        "SlW8u1fTnbMvSeRRsYnG+9vKPn42K7V7dXBx6f2TCwg4TRVm1oUZZE+8klK4z21u8ZIZlb0PHyyP8MDS/GQW6A==",
                        984832491,
                        984832491),
                GgmlType.LargeV2 => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q4_1/ggml-large-v2.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "MQTZwGtU3qF/yOFpm15Mqmd7Ju2dngAISCj+GEpDRtce0B+TqFa4EYfiSy8vdNBJhIN7HDzYOzAgku11nIcA6g==",
                        "lmFZU/4xlTavucLlNtVW6T42SJ92k3crLV2zz/YbmzeL3malqJ9tIPC7SZB7/Zc0cFth57f8ydkMGgMRpMbxdA==",
                        984832491,
                        984832491),
                GgmlType.LargeV3 => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q4_1/ggml-large-v3.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "/bIlu6DQD/x5Z/X5iCn4TxJ/VPrHfmisUXKX3FPK4lNVarPGzeKM6+X6EAIjNjWjyI6VVEsdC9B22JUQKW410A==",
                        "/4H23ZHEJBZbW2ZnzxD6u6wDJw1IWHORHImBVPWSIRtSpOXRpJUamW5of0I86gLPRPXoHVn7D/mSalFVxnc2fQ==",
                        985240523,
                        985240523),
                GgmlType.LargeV3Turbo => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q4_1/ggml-large-v3-turbo.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "FvlLaL5SNB8VlKUyK4Wcu3/eHLI1Y+LQd8KdXGudnW3RYUuV99KGJMzS2Xz37HDQvl8OkzcLkILJrzI+LW6GMQ==",
                        "u+mI3Q2HbJ44MO7ZTGul5APJqJj/+YrkesOcAEwoBQwXsTbEhuPRonlo5YYdKrAb7+qY/D+rrEgKoL3S7kCdyg==",
                        524016715,
                        524016715),
                GgmlType.Medium => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q4_1/ggml-medium.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "9Q1T5/yU7Tlg9rpOA8+bkXEsP/1/JIcBDT9DZ8uC+RXE9fXelxQeVmi/6xEEklITuSaG6mxE2bu3xBZ6pVWERQ==",
                        "7sSjkVJKAblZGeC369XFg+rxxYmRwmB0QuTacbzypNdAnmgMhuWWrBxqq7oQFzbl6ZeSL+7Ud1yOyHY51YNgkw==",
                        491852915,
                        491852915),
                GgmlType.MediumEn => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q4_1/ggml-medium.en.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "tftzrdCq5id9AZ6CFNsE+mJdyEm7HcwQijXhQamIpBJlbOGRVxVk2V9fu5vkUB+cd/QSXvBdTx0nNLj+n8xbQQ==",
                        "a480UUlSOULgbKaWqznAEwUBvutqdoyycGuktL92aL+2eR/Y+2DKi/kKwNyJtw+gxyl9Zsk15Tu2ufQFQJCi0A==",
                        491866045,
                        491866045),
                GgmlType.Small => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q4_1/ggml-small.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "IGe6GdlPAkQKHsRhvFy9s1JhATudhDkB4lM06Z2yJRLn+7NUMLOdMDU3giiYSKyiqcFv8h1pmE6+RJH5zlxE5Q==",
                        "46uGeUZV3UELHCsjTfDYB55jIdSVM3ErdG4pzKLaBn2p9OTbUGTHdENPIjgMk5NCBvqJnNBEgVJfjeSf/+sb4A==",
                        160333839,
                        160333839),
                GgmlType.SmallEn => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q4_1/ggml-small.en.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "ybzEF5NEfl9+uN0blrmn523R81CGdFA8eiFNxQHf315cW01mQSzViubL5uf5IquAoyYj21Qh1et5aD+/73YGMw==",
                        "vH2YMk5Co3r5ydss9SLgSHl4zNVntgumAOTmhxmU9X4twbukFm+6g/Oeb1wwpEfUaWv9spwBcMtGGG15uhyWxg==",
                        160347129,
                        160347129),
                GgmlType.Tiny => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q4_1/ggml-tiny.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "SKHR4xhdfjeJnLRwPIt3XFDLEv1kYYMY8UQh57SaKq0X+m6t0+Xb/9GHtucqM969qptn6Dz66vqYA3mKYsgN5w==",
                        "U1DYKTIWPE8Y0EygYyg51EqcjLMrY6z/pRyc6IxwwU4bpqGXEsXSHGnpMSv37TTa6+GAAJQZeuU2gISSoMtOLA==",
                        27598769,
                        27598769),
                GgmlType.TinyEn => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q4_1/ggml-tiny.en.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "SfQW4tLEpAIigUoc3iQx2lToHcsO6cN7qDTG+Ih+qAzV91vXRG2eVnJbeWlO5fgo16J+XxWa4saEZbS0L30+hg==",
                        "mC5g6imWcr92q1K10SkTvm0V2ruSvqrI/Ed3DVNZNGOgpsds1DZTZAMhKV298cU5bkmGk+B0X6RGwJqmO1Y87g==",
                        27612299,
                        27612299),
                _ => throw new NotSupportedException("This ggml type is not supported.")
            },
            #endregion
            #region Q5_0
            QuantizationType.Q5_0 => ggmlType switch
            {
                GgmlType.Base => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q5_0/ggml-base.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "8P0gdYY3zjAh3FEfNvTiQGK3gJdDhx2N9dstJiHNFkcJKqwONaGCiKSmHN445wn1csU/WjjA3ulR1zJ/siQ2uQ==",
                        "gnEiX3kNGm3Z0ka8kzi2ilbMlcFjktB+Y5RQxvT9CSELvoodZSiuDr6KWjOqa0JtPMYa/sUcJ6a4U3wHV4ocJA==",
                        55295433,
                        55295433),
                GgmlType.BaseEn => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q5_0/ggml-base.en.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "bvFyySy/2102k0877bap28DLfQkHOFdfCytdGbiSffJP5wXssH0lZi0RFnVJ+y+qJ/mQBwL4dO4x3QME9X/Zmw==",
                        "Qp+a6suG6q93pqFZUSRIhnEZWRWUzKYi2beWbPn15aaDNoyhz5kLdIExVAGM6XgQiIRGBH9h6ct4/8lnsC3o0Q==",
                        55308851,
                        55308851),
                GgmlType.LargeV1 => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q5_0/ggml-large-v1.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "aOW/tFtYMHhJnzcuWfGclth6c3latiUxxbcnKorMy5477ds0yPNjM15/uuRzmyqcpPe4VYOqLJQZkx5Gxa6b+A==",
                        "31BVOozmJuvEjg8gjsGzdWNwgaMB6o984Lo/FBGLsbG9HJXlNPCchtVtcrAQ1F15r4UXD0+bd6YrBWkYjuuzXg==",
                        1080732091,
                        1080732091),
                GgmlType.LargeV2 => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q5_0/ggml-large-v2.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "Jz22l3K7F00Bk12clZ73ZaCniNlWJdB3amBNngIg3HhrYgP0tDVbp7REnsY73TW+bVtwcUno4Djyg0NHdrj/HA==",
                        "Au/3u9rhX2cOL43bfusyugkg5TCSvduPHMG0vKkYjiourcTXm84o77H3aDXcsbRFA/0jfCCS+FErj6oFYh552Q==",
                        1080732091,
                        1080732091),
                GgmlType.LargeV3 => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q5_0/ggml-large-v3.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "1nSaaGvRCukWBr2+XaCf1B/SRRC3xk3I07C5pqWYVWn9BaYe4ftlh4vTXxcG3VxyHBOtk8UKkHPA08jtTFvDDQ==",
                        "p2lzH7h6QPCtDOTlBgMzcbiJGpeWwa9YiIfWo7XA0absTOwssRLyF1hmjx/hLr5MFgRcldd/14ZtF7q0t424EA==",
                        1081140203,
                        1081140203),
                GgmlType.LargeV3Turbo => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q5_0/ggml-large-v3-turbo.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "Po+/1tMnmCGJR8E7GHrbwT8QZEc6CuO4UHl3U8TN3aCl4aplCxieIFt8hHBrrES5supi7hbQ367VmaaWn/KXdg==",
                        "dz7rfB5Zd8LYkC72ye6//C8OkSmcTyMFhvQcfv7ToHYhuehfspD2T2GRBzgtrFg73o1EqYe/1K1y0b62RfeS6w==",
                        574041195,
                        574041195),
                GgmlType.Medium => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q5_0/ggml-medium.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "c6v2jxlGUxGkz5sIRxbLyq9qNk2kb+GGnByUeGd8rBrUAZUN680MTzMdDycY3IYM1AmGhJZsyYtj/TK5qGkmog==",
                        "P1srIKef3uDHHRgSYT5A87VbNlsL+frPy3vawyPpxW9Za/DK9MWPGsSUim1a6bEh0U3Db+cmtFWXImHYwZFCuQ==",
                        539212467,
                        539212467),
                GgmlType.MediumEn => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q5_0/ggml-medium.en.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "DrflfK2aYGJvQlkQnhQE6l6SweJDLpG5lnlvPlhpf7xq+1HpbQwsRcxjcNTALS17GhBQ1QARBuL8Z6SRX/BGBA==",
                        "xjWMRJSQ12cHRy0DCuup0LgiHmlztRjAfA1SuIjGujbYmCIHxNir2AT2dr9zAMlEgm5agYunT8BwlzJYPhVeFA==",
                        539225533,
                        539225533),
                GgmlType.Small => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q5_0/ggml-small.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "+u+oaXCni/Tzp6mQrc3MquMGwjmut3bAWZ/XViKtqBd0rsRdr1nAb3hV4TNZH3NdYEJPZrn+FqCYAlOwm64V4w==",
                        "nX9WLlElmq9yN0U6+Dqhyo/gAFVO5be2uGJUObTgCQEJiolB2ourUK+LRMoj8V8MM24O6Uz85bu16hkIIQjBbg==",
                        175209663,
                        175209663),
                GgmlType.SmallEn => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q5_0/ggml-small.en.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "TX7972sdZW1BQJvw27cPXgXrw3IUlNrmKw16kH7874o4N7u+aI2mWv9k7IYMm5ubUvuJ4g/hdYdhFTq4lzdsgg==",
                        "WQKDckVT+Od3wGjVCu5OpXE/WEKZmEnnCzqirV16ldc0bY11k0aclGaGsjeMZ+rcprYwF6WwaIE/0dl/5duv5Q==",
                        175222905,
                        175222905),
                GgmlType.Tiny => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q5_0/ggml-tiny.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "r+Pp9qzRW88uiaAI9XDK66XzQh3DsqDbZ0xq9Y/H7a8VylFPA1cr36KzjRuLNOple90kspqKy5i7W2wVDGfvWA==",
                        "fJB9ePLPB5AymbKdVOEarzb2Zutp+dY5O6SeIb09w5v5pQqvpqFai2jybm55B0guTByuafNN0lqohrPbzmgdVg==",
                        29875721,
                        29875721),
                GgmlType.TinyEn => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q5_0/ggml-tiny.en.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "yRC7dMQPM65FHvIPNzKeQFbPkpCASUFUOuQjO76PxRRlEkqtligh8K5nnIV7oXl5MNp1fJQKP1xsthRNToCKuQ==",
                        "4+8cPoep0NDFVb45LNVj5ytoEFAcJ54VdDjO0CkLPSOSnyWF2Fx7S6KIlXaPXRe1nTadJ+11VLDmQvkdg3wUQA==",
                        29889227,
                        29889227),
                _ => throw new NotSupportedException("This ggml type is not supported.")
            },
            #endregion
            #region Q8_0
            QuantizationType.Q8_0 => ggmlType switch
            {
                GgmlType.Base => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q8_0/ggml-base.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "GVOcwxW2td1RATZZEADIwbftiGNESOrngec26Ov/koD3swg4AYibvyS3k6p86bcVsiTwwpCk1wOR/KH8gKM2LA==",
                        "0SlCMkmjexyr35nbVqeF2bJx1k5T/h1H4KfGRJt/rS1qGvR8Lz4PAYjP1HcZVOvy40VhDBjhRB4lYZ2wBRkraQ==",
                        81768585,
                        81768585),
                GgmlType.BaseEn => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q8_0/ggml-base.en.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "HJxTOtTLUYtbmemA1CetgH1MKFtjFSuhOalP5pW8UHSECgT3wiaciZSbnjt2MuMcojXLTxONpGJg8mCmiCEjpA==",
                        "VnOKegn2aDuDt/tpM2joyyuxaBFE2o3AB5fP5ILcUsGqBQFOyVOaucReWdWmf3wNrzF6zAcFn0rVQVVScYgO2Q==",
                        81781811,
                        81781811),
                GgmlType.LargeV1 => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q8_0/ggml-large-v1.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "/a9X7pIcVl6KhvGiQq26U5STNdAyryvJgy16+5KWzvh+D5RwMHVmDlfwEsUZ4tYFRKUbMCNs3XgsiHe24WEfig==",
                        "zh0GprKjeSlM7rvMkIG2+ipUXUHo+MIipZ4DFO090SuRXKlRegN4uqqye27MM6L8wfGNcqo4NqMyUJutwm9Nog==",
                        1656129691,
                        1656129691),
                GgmlType.LargeV2 => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q8_0/ggml-large-v2.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "0Rcei03i34wUvdQ/t7txx4N1icbv1LOEgz4Bmp+ArKTkd0o0K+5LKZpZFSq/uuOJ8JTu2ax3B3YOlh5N/S2dtA==",
                        "RvE5ISHGSusPsv4GlYhkAAw3KCGzHiGdxG9MnJ5YfoETch7Tw5eXC4eaBKH5zA2PuL/Gj4pu+FmMKRBQ0Lh/oQ==",
                        1656129691,
                        1656129691),
                GgmlType.LargeV3 => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q8_0/ggml-large-v3.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "pJkcyNghpgzQSpVgwi7db4Y4f0MoWGNjwK5ZO+nNd8onCl585LMz8150H4y9uOGNzQOpMpY4WUxZtyWgiMYSCg==",
                        "i6z6Oy4PY056kiAdD4oxIfkWYdhCUx6qXV1SkqlHJ4eGjrNCLIwXcMJJ+EfxDhgap2aoS0/WnSGH9O02FZHDhQ==",
                        1656538283,
                        1656538283),
                GgmlType.LargeV3Turbo => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q8_0/ggml-large-v3-turbo.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "SPdrryZmew3rIf6BqKh4sWGUo9Y9Bip+90MsiU/WLnI2f5JvNu9+QIVJoxtXIHrh0jjU/9woM6j1S4ufAfxyHg==",
                        "qAbdfHp/UPUt1uubjeNERbVr3umqujCFPmq3lcMvDe4r8cX6KCzjQTmO+6ffk0YPwmPL9ut9ddrxeM3s2y75QQ==",
                        874188075,
                        874188075),
                GgmlType.Medium => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q8_0/ggml-medium.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "KDgEd/RLwa0rpgcel5AnTkQkNwm+Jbh/QkyIBP0uTehVpw0o9uQppv/616HmCf6SkxJ2u3si9z6iCqYn59ctLQ==",
                        "Z3Xw3aWt52ucBPEi0hi+aCPXTjrKPWLYx61KmoYP0hbmILmxxQZKqhru5FEiypVjH9HV+Jqte5YJe+AJ09iKOw==",
                        823369779,
                        823369779),
                GgmlType.MediumEn => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q8_0/ggml-medium.en.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "PW2XPA7hbkGoClkprpewkf+A8qxale94Ebka+SjPvcxjUw9AQ4iAxCyu9fw1ZvWZQ3oNLlKnU3gRgN+TpbFQjQ==",
                        "i9ITbfVSljFWPudWnKBVaiogwxT/mUHcjggMVJyC0QSHNzrlOATS6uaO+4QN451/HVyDULWKQCxHleKC8TYRbg==",
                        823382461,
                        823382461),
                GgmlType.Small => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q8_0/ggml-small.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "QmZGSAO8/t2wvNpA70C/XAcEzTT7y1quoWrrkOHilzOck+tABaaUcFlA3IVlh9YgiCPo9to3ycKOea3pvde7pA==",
                        "WpDVgsDOomyuriuNV+jNoGcsp36WckmQd6O0GqNBamo4wsbOQIc8vHRezPzQj7abzAuE2QXtEQLDr6IjNCQIJw==",
                        264464607,
                        264464607),
                GgmlType.SmallEn => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q8_0/ggml-small.en.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "c9lHNM3okC9TvxQdbb+Gy0BFriDlJgfYPfIGTPrbTnAVIsagHw/JiyKTmri/XdanzcclgpVy5D0cWKSQ0INWOA==",
                        "suVkM4i+bSS/fpm6gl1xeGiAJZDzCsvs/MbkwC38osA0AbMQZc815OP/Oe24nvOsXkJdVHxnhztMI0h/kqvtxw==",
                        264477561,
                        264477561),
                GgmlType.Tiny => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q8_0/ggml-tiny.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "PiTO/g9VescqHQ8ZKgDkx0h9gNoapudxJgn+GQro/SWIfnKusXimtlcBPA8kgWrwoEGV68iGvly16t5DG0pJ1Q==",
                        "suzPfNQXWD8YV2sjS5S5963ntkexfC0fM68AxduqoYA56Fyij2sHahzD81FtNXPDGu5JXFUzzxpjnE86yR9OnQ==",
                        43537433,
                        43537433),
                GgmlType.TinyEn => new ProvisioningModel(
                        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/q8_0/ggml-tiny.en.bin"),
                        ProvisioningModel.ArchiveTypes.None,
                        "tlYPv3IwkskCmnAHxePwE+EZXnSR5rdGRX7CVWTafBRWUfrAtdz9Rhz4DuB7hccsmwDbL1KKr2bNQiJEFUGqKA==",
                        "EAj1hjKqRDEzGSchoNlCGbRBQPGC5RXrz5W+iF4OGmuxCM1iZhg/iEdVjhV5TiKEKOKXaKAp4z7HZc9vvyTXiA==",
                        43550795,
                        43550795),
                _ => throw new NotSupportedException("This ggml type is not supported.")
            },
            #endregion
            _ => throw new NotSupportedException("This quantization type is not supported.")
        };
    }

    public static ProvisioningModel GetOpenVinoModel(GgmlType ggmlType)
    {
        return ggmlType switch
        {
            GgmlType.Base => new ProvisioningModel(
                new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/openvino/ggml-base-encoder.zip"),
                ProvisioningModel.ArchiveTypes.Zip,
                "lhfglBjXqYccCO2OrDRsfe/ijrkgwubos4nMDct1gSL95iyrnV+xrw+tTpCsIUW4GmODPNkam6bbnQhR8mXkhw==",
                "0a6sMWAjz1uvCh6ERFHa4mVIUqtBAb3vfqtNC4t2ARZZpn8kZxzkNXF7r4/2i2RxbeB3JgFq3HL2gkOYlMCjuQ==",
                48353200L,
                82362540L),
            GgmlType.BaseEn => new ProvisioningModel(
                new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/openvino/ggml-base.en-encoder.zip"),
                ProvisioningModel.ArchiveTypes.Zip,
                "85/uSr2jwqHUfRkZAsWwhHirI/kwRP4TyfBnx/QfXOYPArNEdVIcyTL+BiAVGcUoMc+PX91xbTGsw26u42Qa0g==",
                "JCSzo26zA3AVf1K3Xi5Dh9WGEk4crhQuraFrg4ypytD1R75nHuH19my4LO60bQ3c4ZKrOWfm0veqLLxiqkJMCg==",
                48405638,
                82362540),
            GgmlType.LargeV1 => new ProvisioningModel(
                new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/openvino/ggml-large-v1-encoder.zip"),
                ProvisioningModel.ArchiveTypes.Zip,
                "7F9PC+8ZOqvbpSjFWDNI1kM0Mx5IOIAgLqAtvqGmJ5HP+kQpe4bq7TolfMB1sud1quAcbMkcxf0FXQRja7FGpQ==",
                "sDc/wxV1EWhXjse1LGXg9ZCpws/i3lHb04ptWXrMdScVjw+ojVzttmxLhAro8cXzv7cOmtohaX9nup9lySt62Q==",
                1442214406,
                2547138732),
            GgmlType.LargeV2 => new ProvisioningModel(
                new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/openvino/ggml-large-v2-encoder.zip"),
                ProvisioningModel.ArchiveTypes.Zip,
                "1uHem4DhFY5W2pBi9FRo6nY4seouDli6p2TQfBemx2DOl2uN0WflPDFe262Op567CuOOrlrmEE7Vc+dZbVlpDQ==",
                "nuZ4JwAIiNtRI/adgTl7DtN+Pwr/82s7L8vgVh1v6lgoCFWn53iMRJ+PnsEc2HFobLeXx5jt0qqyo6X5zx0jNQ==",
                1430959788,
                2547138732),
            GgmlType.LargeV3 => new ProvisioningModel(
                new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/openvino/ggml-large-v3-encoder.zip"),
                ProvisioningModel.ArchiveTypes.Zip,
                "WEOp/K1JBA6nNz26sfynUflLlHUgPNTEQRUib07mfjJPYcbJZIpP86Ti5WWb+pxIdinWHnYk5uM/7jmmI955/g==",
                "C3bDkz6GTP6Y8U28Wtz2efT7KQ+745qGJMoqVwfdcvmttVEBtztZ3NX0FoY5CjC68zqy22jOWd6p7Kfk52ScZg==",
                1422595014,
                2547876012),
            GgmlType.LargeV3Turbo => new ProvisioningModel(
                new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/openvino/ggml-large-v3-turbo-encoder.zip"),
                ProvisioningModel.ArchiveTypes.Zip,
                "tZAmOGJ4grbOVLL3MzTNtuj+C9UfzDY6yz1xUMPCV41y2+ugtDS/nfwapu3iU66Sq6OZ6kkOciWYkWru3NeZqg==",
                "dJH/usGXV7+3Xk42588wJpe5xbHb+1X36QJS0q7O3RCIvPbnT7CyqaOqwy3zVDhM+QjzUIUpYN4tKPOsPdfq5Q==",
                1417669689,
                2547876012),
            GgmlType.Medium => new ProvisioningModel(
                new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/openvino/ggml-medium-encoder.zip"),
                ProvisioningModel.ArchiveTypes.Zip,
                "6dx5agRmJffEelEZBYFTol4wgKuG8p3yATFyzuTKLeM2N2YnL8kref87vEq9pVFSd1CkaUrfBpvwRF9YUosq2Q==",
                "UA8K8MXVCPxc4IXIKm+n4GvAhLdFfaIpuKPkiebPeQ8Aj6pOAGCl79Y9DkupknsNouQAB20SavUI/8gX0Nav5Q==",
                693590294,
                1228865708),
            GgmlType.MediumEn => new ProvisioningModel(
                new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/openvino/ggml-medium.en-encoder.zip"),
                ProvisioningModel.ArchiveTypes.Zip,
                "xEDvaJhUw8sGcXe+iigtLOkZsNYraNJfny1YVITLobNNNkk7tNrj0IJsz1f9vRDGVM9tR6BBPCkf95t11fqYlg==",
                "dGRBDyaNPuCe5FX4JoR3cJUW5s72Rr8WqKanc4HfhL4rY2VMBDr9vdaxhHhlsKpYWYUSnSSgVGaIfOfpXf8TgA==",
                691698228,
                1228865708),
            GgmlType.Small => new ProvisioningModel(
                new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/openvino/ggml-small-encoder.zip"),
                ProvisioningModel.ArchiveTypes.Zip,
                "Tf6ZVBK0q32R+jKgM5wsBbJpmH1kRdFzGdNFCRZe8IT2MZEgukS7RVtXsZ4VAvNL5RXijt+hMK/HnbbGOUaELQ==",
                "Q2sBZ7oPB9NHMz9mkPeJ+HDBfU7HrMkS92uC94EJGiLOZrsIYJhCq02y/0/otYXGtY9Ea/P12EkH+/Y+h6+Eqg==",
                200372770,
                352616620),
            GgmlType.SmallEn => new ProvisioningModel(
                new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/openvino/ggml-small.en-encoder.zip"),
                ProvisioningModel.ArchiveTypes.Zip,
                "6hoCW57uHGFiL20rHDAmlFMvAoc2D4/So6bx92aW49PGGxRxwFMRNdDjaeZtbFF6uQ3ISnXC+jXB02zkM6j2UQ==",
                "wCiDdg4IpxmEqu0gG/vJEaZ1Xz09CQByUmcaB19nvAiajp6jR8jP7dO7hbbMsg/dmU+H4zLGL+wYx51lvOMA/A==",
                199944513,
                352616620),
            GgmlType.Tiny => new ProvisioningModel(
                new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/openvino/ggml-tiny-encoder.zip"),
                ProvisioningModel.ArchiveTypes.Zip,
                "VpszFM3MJR1QKA47wecZIx+s8ldTHAfteHsAJHr8EJ5a7JeNsmgW4ylrgtZdYUNb3wFBDaCBZ94SaV9AiWzfVA==",
                "FX0ehdAayOte9b6WDNhYnzHpMdJeK+dmiypy4JzZ2+Xmmwpbp5uoNrC19KeYRv9nrIxCzm+kHXN+OeB79QIOlA==",
                18566606,
                32833708),
            GgmlType.TinyEn => new ProvisioningModel(
                new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/openvino/ggml-tiny.en-encoder.zip"),
                ProvisioningModel.ArchiveTypes.Zip,
                "7jp9FsxVFW5kOROAur57XJYv3bRmlpzgSbGXWZDIU9nqewszpscWT+5U5VeUbm6ucfeNW/fUs5BVzTTzw075pg==",
                "TjlJV3ZXgsSnlCDizSk6mXwbQ2KNQpcDbjnr0deoK+VBX4/QuF551XFkBp84vaj8KPOgEr+jnGRZNn1/8RyWJw==",
                18566922,
                32833708),
            _ => throw new NotSupportedException("This ggml type is not supported.")
        };
    }

    public static ProvisioningModel GetCoreMlModel(GgmlType ggmlType)
    {
        return ggmlType switch
        {
            GgmlType.Base => new ProvisioningModel(
                    new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/coreml/ggml-base-encoder.zip"),
                    ProvisioningModel.ArchiveTypes.Zip,
                    "cZaQQV3XBYAn8YIgvp8MLLevuTng3qRfSSjJa2n9ziWQdiuS99GyeWiYQFMhou6c7mz0oazjKMXEp1o5l4GAcA==",
                    "JF7g/vpbZ4haH8yKfUkj3pPS6B6ASg3bzLeTwaASXTE01dqgEdVfXrlNLDoD1/QZLoaX2mqsRBkLmmPSLfGt0A==",
                    37923432,
                    41193984),
            GgmlType.BaseEn => new ProvisioningModel(
                    new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/coreml/ggml-base.en-encoder.zip"),
                    ProvisioningModel.ArchiveTypes.Zip,
                    "xmgTozTJsbssB6t+lMnaQ7+B/vChKV0BFwKViwRpPeSgjZhYzgLaPsi1TJy39RQoM7e7NlcNPFTzAXN5dhtdYA==",
                    "4gEJD7piVVXeG2VIpNGJHlPWPSMewROO+MV+aJZr3INJEHWwSJbKFTCvf0MuTzs66dpp9vDfmN7cfkBfuXSWbA==",
                    37951516,
                    41193984),
            GgmlType.LargeV1 => new ProvisioningModel(
                    new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/coreml/ggml-large-v1-encoder.zip"),
                    ProvisioningModel.ArchiveTypes.Zip,
                    "aE0BovUXFIUkd245GGNgccUYzkdyn1yq9sVfT1r48MDx3I06PV3ztqDTeMc1YBCG5lmLzlYL4+O2RtkC8urmHw==",
                    "i9zuWQ84IsKtjiX3c3W04MSQ3/zZEUVniGh8PWddsHWl6ckMfWJRINwm5E+ryX6Wfvk1Zq/Uzyf6+cgsFOmZRw==",
                    1176729022,
                    1273684480),
            GgmlType.LargeV2 => new ProvisioningModel(
                    new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/coreml/ggml-large-v2-encoder.zip"),
                    ProvisioningModel.ArchiveTypes.Zip,
                    "hTN29q0PzYkEEH1wzkuKrI33y+NyE40OP1ljWIiAs3prgoEmJzZUeQXgEVe6PqEb78h04pEtwWn0al6HGOjm6g==",
                    "kJLDsvP9ZzEZfzCr6MTC5TlP2neJnx1Vf0YK2OGBu4oqguB65O44dbqhPhlAkLqPVnpmz8H/qQpZQ9SRP60eHQ==",
                    1174646410,
                    1273684480),
            GgmlType.LargeV3 => new ProvisioningModel(
                    new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/coreml/ggml-large-v3-encoder.zip"),
                    ProvisioningModel.ArchiveTypes.Zip,
                    "PsQnoWElgw7l5+quEgAE6Ne8diBYoterThY81FRZHvnpGwvtA7kG3y4pMumf/pJp3YpjmArKl8ljEnVy/qXzSw==",
                    "MtuVBeVb+4RxdiDgSeTqKJFrmk6omVpizOpd3hSEaDDQ8e7YYkoCkwrp62veuhJglqM24dN11IhaUT9gySd6AA==",
                    1174646437,
                    1273684480),
            GgmlType.LargeV3Turbo => new ProvisioningModel(
                    new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/coreml/ggml-large-v3-turbo-encoder.zip"),
                    ProvisioningModel.ArchiveTypes.Zip,
                    "1n/tyna3mLWcEEAQYGCUCWHmBEDo4+EC3ujkb4yMHt/dpQUf/GHqWm0/+TwjHKAH8yXlDpISWd0Az2HangW/7Q==",
                    "jSedt7MjsVyJKaLQD/juCn/+G8xoSZL1DF3GHuv/zVhXTJxzY4qBAfgqnOsDpO/SAkLUSZJ0qRRVSPTTWATEDA==",
                    1173945483,
                    1273969152),
            GgmlType.Medium => new ProvisioningModel(
                    new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/coreml/ggml-medium-encoder.zip"),
                    ProvisioningModel.ArchiveTypes.Zip,
                    "OcrwQ8KLepqoTdqvkiM7X9DuGaxYMMqdbtAa8gG11tzVkdv28DQZ1n2nYX7LG1+Hqpd/37McVU/eIytPQuVw2Q==",
                    "4mj2QUmfmoR4hBOyt+RFKKnQhCe0D/O8PwhiD1cYwZ0udxKHv+yEOLd73af5UodWTqVZGv2kuPVB6oHC/YhueQ==",
                    567831848,
                    614507008),
            GgmlType.MediumEn => new ProvisioningModel(
                    new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/coreml/ggml-medium.en-encoder.zip"),
                    ProvisioningModel.ArchiveTypes.Zip,
                    "7cNPhTmkM7DjIEZE6Ttnyp6BgTSHfzKg0ytlWHHtlzUhmh95EJ9p8yWHG1ztdVJtO2qC8AH6CUnBPw4GtaSVrg==",
                    "f8ZduPcXEeTX5xVYyzeVApf0ncPq8kF8PTwjX/KS88e6BZIR57ob9NBFX5XxSAOGt5Noa4V5rjvO57LopLKTbA==",
                    566995437,
                    614507008),
            GgmlType.Small => new ProvisioningModel(
                    new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/coreml/ggml-small-encoder.zip"),
                    ProvisioningModel.ArchiveTypes.Zip,
                    "Aw/25P8iAJ9CPhOiSnn7IuL/6SxTgbGOkCZu8c3Uokwfr/ZEgDtnBSA31S9c0bs7pcxb7kT24YP8EgdnULiODw==",
                    "B6sJrpqgLR7LxSJ2S+EhelprgN3q7ErpZdo9Y8ppFXyaR7ghQzwvg5tovFRxB2HPTbRaiTa5wS9Tv1DHu2yHsg==",
                    163084531,
                    176339456),
            GgmlType.SmallEn => new ProvisioningModel(
                    new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/coreml/ggml-small.en-encoder.zip"),
                    ProvisioningModel.ArchiveTypes.Zip,
                    "Vsp2qoKJRreJQbvfZla/S9/hs0A6KFk5xy3Eiwyn8+bYMwEYg5BXQNvywCEyPFN6KHfbvls9olFEZ4LdAAvqyg==",
                    "bKiPFxgoy7JeBRLwtNUHYTKzxnZcNoxfZ9CYtavYWz7BASfe/z6wgQP03bGrfS22f81ecOnusu9fNI0SnMeggA==",
                    162953802,
                    176339456),
            GgmlType.Tiny => new ProvisioningModel(
                    new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/coreml/ggml-tiny-encoder.zip"),
                    ProvisioningModel.ArchiveTypes.Zip,
                    "Z5+2KhHfKJWvA+CZNBWaHtPD1pp0SZqbBhctfMY6dI+RVFMrKFe3A3sr3rBOuwdhG0iC5eeBPkTetcMteoEnSg==",
                    "soT0jeUsltOlyze9jXfDqZRAZLcr/Rs6iSAPzlKTwlGPmi97vQums3acryQTjJO0HZKHjV249Y9RkMtH0ikGAg==",
                    15037894,
                    16424448),
            GgmlType.TinyEn => new ProvisioningModel(
                    new("https://huggingface.co/sandrohanea/whisper.net/resolve/v3/coreml/ggml-tiny.en-encoder.zip"),
                    ProvisioningModel.ArchiveTypes.Zip,
                    "CnM9mRbNjCtyC6Z+MRgW0cyd14wqLRY09vv9rT3g+P/Fgn7YlYLM6Zf22CrHO/azzloWQqszr2BQ5H5lrWRkCw==",
                    "vwjXfaYKYtmIEKvsrBCygMUrIokbjCoi+mMEEMoYU40qbZweuEL9w47V3Y0eubkCCN8MKLF1cwLPgxSih686Xw==",
                    15035070,
                    16424448),
            _ => throw new NotSupportedException("This ggml type is not supported.")
        };
    }

}
