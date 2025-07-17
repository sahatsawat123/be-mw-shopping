public interface IPdfService
{
    Task<byte[]> CreateAsync(string html);
}