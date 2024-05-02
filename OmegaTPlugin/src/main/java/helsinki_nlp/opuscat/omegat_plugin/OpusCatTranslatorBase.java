
package helsinki_nlp.opuscat.omegat_plugin;

import org.omegat.util.Language;


public abstract class OpusCatTranslatorBase {

    protected final OpusCatPlugin parent;

    public OpusCatTranslatorBase(OpusCatPlugin parent) {
        this.parent = parent;
    }



    /**
     * translate text.
     * @param sLang source langauge.
     * @param tLang target language.
     * @param text source text.
     * @return translated text.
     * @throws Exception when connection error.
     */
    protected synchronized String translate(Language sLang, Language tLang, String text) throws Exception {
        String langFrom = sLang.getLanguage();
        String langTo = tLang.getLanguage();
        return requestTranslate(langFrom, langTo, text);
    }

    protected abstract String requestTranslate(String langFrom, String langTo, String text) throws Exception;
}
