using System;
using System.Configuration;

/// <summary>
/// Represents SMTP and email configuration settings, typically loaded from Web.config.
/// Allows initialization from config, manual parameters, or partial overrides.
/// </summary>
public class EmailSettings
{
    public string SmtpHost { get; private set; }
    public int SmtpPort { get; private set; }
    public string SmtpUser { get; private set; }
    public string SmtpPass { get; private set; }
    public bool EnableSSL { get; private set; }
    public string SocketOption { get; private set; }
    public int Timeout { get; private set; }
    public string FromAddress { get; private set; }
    public string ToAddress { get; private set; }
    public string CcAddress { get; private set; } // New property for SysCCEmailAddress

    private bool isInitialized = false;
    /// <summary>
    /// Indicates whether the settings have been successfully initialized.
    /// </summary>
    public bool IsInitialized => isInitialized;

    /// <summary>
    /// Default constructor. Does not perform automatic loading.
    /// Call LoadFromConfig() or use CreateFromConfig() for safe initialization.
    /// </summary>
    public EmailSettings()
    {
        // by default the settings are loaded from web.config
        LoadFromConfig();
    }

    /// <summary>
    /// Initializes a new instance of the EmailSettings class using explicit parameter values,
    /// allowing configuration to be set programmatically instead of loaded from Web.config.
    /// </summary>
    public EmailSettings(string smtpHost, int smtpPort, string smtpUser, string smtpPass,
                         bool enableSSL, string socketOption, int timeout,
                         string fromAddress, string toAddress, string ccAddress)
    {
        SmtpHost = smtpHost;
        SmtpPort = smtpPort;
        SmtpUser = smtpUser;
        SmtpPass = smtpPass;
        EnableSSL = enableSSL;
        SocketOption = socketOption;
        Timeout = timeout;
        FromAddress = fromAddress;
        ToAddress = toAddress;
        CcAddress = ccAddress;

        isInitialized = true;
    }

    /// <summary>
    /// Loads values from Web.config AppSettings. Sets IsInitialized to true if successful.
    /// Throws if configuration is missing or malformed.
    /// </summary>
    public void LoadFromConfig()
    {
        try
        {
            FromAddress = ConfigurationManager.AppSettings["SysEmailFrom"];
            CcAddress = ConfigurationManager.AppSettings["SysCCEmailAddress"];
            if (string.IsNullOrWhiteSpace(CcAddress))
                CcAddress = FromAddress; // Fallback to FromAddress if not set

            ToAddress = ConfigurationManager.AppSettings["EMailLogIn"];
            SmtpUser = ConfigurationManager.AppSettings["EMailLogIn"];
            SmtpPass = ConfigurationManager.AppSettings["EMailPassword"];
            SmtpHost = ConfigurationManager.AppSettings["EMailSMTP"];
            SmtpPort = int.Parse(ConfigurationManager.AppSettings["EMailPort"]);
            EnableSSL = bool.Parse(ConfigurationManager.AppSettings["EMailSSLEnabled"]);
            SocketOption = ConfigurationManager.AppSettings["EmailSocketOption"];
            Timeout = int.Parse(ConfigurationManager.AppSettings["EmailTimeout"]);

            isInitialized = true;
        }
        catch
        {
            isInitialized = false;
            throw new ApplicationException("Error loading EmailSettings from Web.config.");
        }
    }

    /// <summary>
    /// Creates and loads EmailSettings from Web.config.
    /// Returns null if any error occurs during initialization.
    /// </summary>
    public static EmailSettings CreateFromConfig()
    {
        try
        {
            var settings = new EmailSettings();
            settings.LoadFromConfig();
            return settings.IsInitialized ? settings : null;
        }
        catch
        {
            return null;
        }
    }
    public void SetRecipient(string to) => ToAddress = to;

    /// <summary>
    /// Reinitializes EmailSettings by reloading from Web.config.
    /// </summary>
    public void Reload()
    {
        isInitialized = false;
        LoadFromConfig();
    }

    /// <summary>
    /// Manually sets configuration values using provided parameters.
    /// </summary>
    public void LoadFromParameters(string host, int port, string user, string pass,
                                   bool enableSSL, string socketOption, int timeout,
                                   string from, string to, string cc)
    {
        SmtpHost = host;
        SmtpPort = port;
        SmtpUser = user;
        SmtpPass = pass;
        EnableSSL = enableSSL;
        SocketOption = socketOption;
        Timeout = timeout;
        FromAddress = from;
        ToAddress = to;
        CcAddress = cc == null ? from : cc;  // if null send set to from 

        isInitialized = true;
    }

    /// <summary>
    /// Updates common fields such as credentials and address info without full reload.
    /// </summary>
    public void UpdateCommonSettings(string newUser, string newPass, string newFrom = null, string newTo = null, string newCc = null)
    {
        SmtpUser = newUser;
        SmtpPass = newPass;
        if (!string.IsNullOrEmpty(newFrom)) FromAddress = newFrom;
        if (!string.IsNullOrEmpty(newTo)) ToAddress = newTo;
        if (string.IsNullOrWhiteSpace(newCc))
            newCc = FromAddress; // Fallback to FromAddress if not set
        if (!string.IsNullOrEmpty(newCc)) CcAddress = newCc;
    }
}
