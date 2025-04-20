namespace DiarioPersonalApi.Services
{
    public interface IHashService
    {
        string Hash(string plainText);
        bool Verify(string plainText, string hashed);
    }
}
