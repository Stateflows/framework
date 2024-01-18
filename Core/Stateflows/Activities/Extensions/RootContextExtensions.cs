using Stateflows.Activities.Streams;
using Stateflows.Activities.Context.Classes;
using Stateflows.Activities.Models;

namespace Stateflows.Activities.Extensions
{
    //internal static class RootContextExtensions
    //{
    //    public static Stream GetStream(this RootContext context, Element element)
    //    {
    //        Stream stream = null;

    //        lock (context.Streams)
    //        {
    //            if (!context.Streams.TryGetValue(element.Identifier, out stream))
    //            {
    //                stream = new Stream();
    //                context.Streams.Add(element.Identifier, stream);
    //            }
    //        }

    //        return stream;
    //    }
    //}
}
