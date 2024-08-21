
package helsinki_nlp.opuscat.omegat_plugin;

import org.omegat.core.Core;
import org.omegat.core.machinetranslators.BaseCachedTranslate;
import org.omegat.gui.exttrans.IMachineTranslation;
import org.omegat.gui.exttrans.MTConfigDialog;
import org.omegat.util.CredentialsManager;
import org.omegat.util.Language;
import org.omegat.util.OStrings;
import org.omegat.util.Preferences;
import org.omegat.util.StringUtil;
import org.omegat.util.Log;

import javax.swing.*;
import java.awt.*;
import java.awt.event.ActionEvent;
import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;
import java.util.ResourceBundle;


public class OpusCatPlugin extends BaseCachedTranslate implements IMachineTranslation {

    protected static final String ALLOW_OPUSCAT = "allow_opuscat";

    protected static final String PROPERTY_MT_ENGINE_URL = "opuscat.engine_url";

    protected static final String PROPERTY_MT_ENGINE_PORT = "opuscat.engine_port";

    
    private static final ResourceBundle BUNDLE = ResourceBundle.getBundle("OpusCatBundle");

    private OpusCatTranslatorBase translator = null;

    /**
     * Constructor of the connector.
     */
    public OpusCatPlugin() {
        super();
    }

    /**
     * Utility function to get a localized message.
     * @param key bundle key.
     * @return a localized string.
     */
    static String getString(String key) {
        return BUNDLE.getString(key);
    }

    /**
     * Register plugin into OmegaT.
     */
    @SuppressWarnings("unused")
    public static void loadPlugins() {
        String requiredVersion = "5.8.0";
        String requiredUpdate = "0";
        try {
            Class<?> clazz = Class.forName("org.omegat.util.VersionChecker");
            Method compareVersions =
                    clazz.getMethod("compareVersions", String.class, String.class, String.class, String.class);
            if ((int) compareVersions.invoke(clazz, OStrings.VERSION, OStrings.UPDATE, requiredVersion, requiredUpdate)
                    < 0) {
                Core.pluginLoadingError("OPUS-CAT plugin cannot be loaded because OmegaT Version "
                        + OStrings.VERSION + " is lower than required version " + requiredVersion);
                return;
            }
        } catch (ClassNotFoundException
                | NoSuchMethodException
                | IllegalAccessException
                | InvocationTargetException e) {
            Core.pluginLoadingError(
                    "OPUS-CAT plugin cannot be loaded because this OmegaT version is not supported");
            return;
        }
        Core.registerMachineTranslationClass(OpusCatPlugin.class);
    }

    /**
     * Unregister plugin.
     * Currently not supported.
     */
    @SuppressWarnings("unused")
    public static void unloadPlugins() {}

    /**
     * Return a name of the connector.
     * @return connector name.
     */
    public String getName() {
        return getString("MT_ENGINE_OPUSCAT");
    }

    @Override
    protected String getPreferenceName() {
        return ALLOW_OPUSCAT;
    }

    /**
     * Store a credential. Credentials are stored in temporary system properties and, if
     * <code>temporary</code> is <code>false</code>, in the program's persistent preferences encoded in
     * Base64. Retrieve a credential with {@link #getCredential(String)}.
     *
     * @param id
     *            ID or key of the credential to store
     * @param value
     *            value of the credential to store
     * @param temporary
     *            if <code>false</code>, encode with Base64 and store in persistent preferences as well
     */
    protected void setCredential(String id, String value, boolean temporary) {
        System.setProperty(id, value);
        if (temporary) {
            CredentialsManager.getInstance().store(id, "");
        } else {
            CredentialsManager.getInstance().store(id, value);
        }
    }

    /**
     * Retrieve a credential with the given ID. First checks temporary system properties, then falls back to
     * the program's persistent preferences. Store a credential with
     * {@link #setCredential(String, String, boolean)}.
     *
     * @param id
     *            ID or key of the credential to retrieve
     * @return the credential value in plain text
     */
    protected String getCredential(String id) {
        String property = System.getProperty(id);
        if (property != null) {
            return property;
        }
        return CredentialsManager.getInstance().retrieve(id).orElse("");
    }

    @Override
    protected String translate(Language sLang, Language tLang, String text) throws Exception {
        translator = new OpusCatTranslator(this,this.GetTranslateEndpointUrl());
        String translation = translator.translate(sLang, tLang, text);
        if (translation != null)
        {
            return translation;
        }
        else {
            Log.logWarningRB("MT_ENGINE_NO_RESPONSE");
            return null;
        }
    }

    public String GetTranslateEndpointUrl() {
        String url = Preferences.getPreferenceDefault(PROPERTY_MT_ENGINE_URL,"localhost");
        String port = Preferences.getPreferenceDefault(PROPERTY_MT_ENGINE_PORT,"8500");
        if (url == null || port == null) {
            return "";
        }
        else {
            return String.format("%s:%s/MTRestService/TranslateJson", url, port);
        }

    }

    @Override
    public boolean isConfigurable() {
        return true;
    }

    //Add http protocol if name does not include one yet
    private static String validateUrl(String url) {
        if (!url.matches("^https?://.*")) {
            url = "http://" + url;
        }
        return url;
    }

    @Override
    public void showConfigurationUI(Window parent) {

        MTConfigDialog dialog = new MTConfigDialog(parent, getName()) {
            @Override
            protected void onConfirm() {

                Preferences.setPreference(
                        PROPERTY_MT_ENGINE_URL, OpusCatPlugin.validateUrl(panel.valueField1.getText()));
                Preferences.setPreference(
                        PROPERTY_MT_ENGINE_PORT, panel.valueField2.getText().trim());
            }


        };
        int height = dialog.panel.getFont().getSize();
        //dialog.panel.valueField1.setPreferredSize(new Dimension(height * 24, height * 2));
        dialog.panel.valueLabel1.setText(getString("MT_ENGINE_URL"));
        dialog.panel.valueField1.setText(Preferences.getPreferenceDefault(PROPERTY_MT_ENGINE_URL, "localhost"));
        dialog.panel.valueField1.setPreferredSize(new Dimension(height * 12, height * 2));
        dialog.panel.valueLabel2.setText(getString("MT_ENGINE_PORT"));
        dialog.panel.valueField2.setText(Preferences.getPreferenceDefault(PROPERTY_MT_ENGINE_PORT, "8500"));
        dialog.panel.valueField2.setPreferredSize(new Dimension(height * 12, height * 2));

        //Test connection button adapted from https://github.com/omegat-org/moses-plugin.git
        JLabel messageLabel = new JLabel();
        JButton testButton = new JButton(BUNDLE.getString("MT_ENGINE_TEST_BUTTON"));
        testButton.addActionListener(e -> {
            messageLabel.setText(BUNDLE.getString("MT_ENGINE_TEST_TESTING"));
            String host = OpusCatPlugin.validateUrl(dialog.panel.valueField1.getText().trim());
            String port = dialog.panel.valueField2.getText().trim();
            new SwingWorker<String, Void>() {
                @Override
                protected String doInBackground() throws Exception {
                    String url = String.format("%s:%s", host, port);
                    Log.logWarningRB(url);
                    OpusCatTranslator translator = new OpusCatTranslator(null,null);
                    String response = translator.requestLanguagePairs(url);
                    Log.logWarningRB(response);
                    
                    if (response != null) {
                        return BUNDLE.getString("MT_ENGINE_TEST_RESULT_OK");
                    } else {
                        return BUNDLE.getString("MT_ENGINE_TEST_RESULT_NO_TRANSLATE");
                    }
                }

                @Override
                protected void done() {
                    String message;
                    try {
                        message = get();
                    } catch (Exception e) {
                        message = e.getLocalizedMessage();
                    }
                    messageLabel.setText(message);
                }
            }.execute();

        });
        JPanel testPanel = new JPanel();
        testPanel.setLayout(new BoxLayout(testPanel, BoxLayout.LINE_AXIS));
        testPanel.add(testButton);
        testPanel.add(messageLabel);
        testPanel.setAlignmentX(0);
        dialog.panel.itemsPanel.add(testPanel);

        dialog.show();
    }
}
