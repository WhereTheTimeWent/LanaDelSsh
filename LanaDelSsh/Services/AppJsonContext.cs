using System.Collections.Generic;
using System.Text.Json.Serialization;
using LanaDelSsh.Models;

namespace LanaDelSsh.Services;

[JsonSerializable(typeof(List<SshConnection>))]
[JsonSerializable(typeof(AppSettings))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class AppJsonContext : JsonSerializerContext { }
