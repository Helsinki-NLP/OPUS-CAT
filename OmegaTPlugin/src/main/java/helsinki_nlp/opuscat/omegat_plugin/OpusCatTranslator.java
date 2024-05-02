
package helsinki_nlp.opuscat.omegat_plugin;

import org.omegat.util.HttpConnectionUtils;
import org.omegat.util.Log;

import java.util.Map;
import java.util.TreeMap;
import java.util.concurrent.ExecutionException;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;


public class OpusCatTranslator extends OpusCatTranslatorBase {



    private String urlTranslate;
    private final ObjectMapper mapper = new ObjectMapper();

    public OpusCatTranslator(OpusCatPlugin parent, String translateEndpointUrl) {
        super(parent);
        urlTranslate = translateEndpointUrl;
    }

    protected String requestLanguagePairs(String url)
    {
        try {
            String langUrl = url+"/MTRestService/ListSupportedLanguagePairs";
            String res = HttpConnectionUtils.get(langUrl, null, null);
            
            return res;
        } catch (Exception ex) {
            return null;
        }
    }
    
    @Override
    protected String requestTranslate(String langFrom, String langTo, String text) throws Exception {
        Map<String, String> p = new TreeMap<>();
        
        String url = urlTranslate;
        Map<String, String> query = createRequest(text, langFrom, langTo);
        try {
            String res = HttpConnectionUtils.get(url, query, p);
            JsonNode root = mapper.readValue(res, JsonNode.class);
            JsonNode translation = root.get("translation");

            if (translation == null) {
                return null;
            }

            return translation.asText();
        } catch (Exception ex) {
            return null;
        }
    }

    /**
     * Method for test.
     * @param url alternative url.
     */
    public void setUrl(String url) {
        urlTranslate = url;
    }

    /**
     * Create request and return as json string.
     */
    protected Map<String, String> createRequest(String trText, String srcLang, String trgLang) throws JsonProcessingException {
        Map<String, String> param = new TreeMap<>();
        param.put("input", trText);
        param.put("srcLangCode", srcLang);
        param.put("trgLangCode", trgLang);
        param.put("tokenCode", "0");
        param.put("modelTag", "");
        return param;
    }
}
