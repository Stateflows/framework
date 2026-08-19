namespace Stateflows.Common.Interfaces;

public interface IElementMetadataBuilder<out TReturn>
{
    TReturn AddMetadata(string key, object value);
}