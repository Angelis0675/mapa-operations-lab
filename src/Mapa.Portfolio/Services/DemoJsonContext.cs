using System.Text.Json.Serialization;

namespace Mapa.Portfolio.Services;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(DemoState))]
internal partial class DemoJsonContext : JsonSerializerContext { }
