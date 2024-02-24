using Stateflows.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Stateflows.Common.Transport.Classes
{
    public class StateflowsRequestConverter : JsonConverter<StateflowsRequest>
    {
        public override StateflowsRequest Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var jsonDocument = JsonDocument.ParseValue(ref reader);
            var jsonText = jsonDocument.RootElement.GetRawText();

            return StateflowsJsonConverter.DeserializeObject<StateflowsRequest>(jsonText);
        }

        public override void Write(Utf8JsonWriter writer, StateflowsRequest value, JsonSerializerOptions options)
        {
        }
    }
}
