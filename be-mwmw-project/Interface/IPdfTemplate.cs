public interface IPdfTemplate
{
        string GenerateHtml();
        string GenerateHtml<T>(T data) where T : class;
}
