namespace Microsoft.AspNetCore.Routing
{
    public interface ILinkGenerator
    {
        bool TryGetLink(LinkGeneratorContext context, out string link);

        string GetLink(LinkGeneratorContext context);
    }
}
