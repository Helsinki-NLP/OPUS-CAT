namespace OpusCatTranslationProvider
{
    internal interface IElgCredentials
    {
        string AccessToken { get; set; }
        string RefreshToken { get; set; }
    }
}