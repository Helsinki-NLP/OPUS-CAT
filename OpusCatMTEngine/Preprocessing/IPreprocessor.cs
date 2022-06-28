namespace OpusCatMTEngine
{
    public interface IPreprocessor
    {
        string PreprocessSentence(string sentence);
        string PostprocessSentence(string rawTranslation);
    }
}