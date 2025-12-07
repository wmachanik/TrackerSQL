using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Web;
using System.Text;
using TrackerSQL.Resources;


namespace TrackerSQL.Classes
{
    public static class MessageProvider
    {
        public static string Get(string key)
        {
            return Resources.Messages.ResourceManager.GetString(key) ?? key;
        }

        public static string Format(string key, params object[] args)
        {
            string message = Get(key);
            return string.Format(message, args);
        }

        public static string GetWithType(string key, MessageType type)
        {
            return $"{type}: {Get(key)}";
        }

        public static string FormatWithType(string key, MessageType type, params object[] args)
        {
            return $"{type}: {Format(key, args)}";
        }

        /// <summary>
        /// Gets a personalized email signature including the current user's name
        /// </summary>
        public static string GetEmailSignature()
        {
            string userName = HttpContext.Current?.User?.Identity?.Name;
            
            // If no user found, return standard signature
            if (string.IsNullOrEmpty(userName))
            {
                return Get(MessageKeys.Email.SignatureNoUser);
            }

            // Get friendly name (remove domain if present)
            string friendlyName = userName.Split('\\').Last();
            
            // Convert to proper case
            friendlyName = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo
                .ToTitleCase(friendlyName.ToLower());

            return Format(MessageKeys.Email.SignatureTemplate, friendlyName);
        }

        /// <summary>
        /// Gets a personalized email signature with a specific name
        /// </summary>
        public static string GetEmailSignature(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return Get(MessageKeys.Email.SignatureNoUser);
            }

            return Format(MessageKeys.Email.SignatureTemplate, name);
        }

        /// <summary>
        /// Gets a formatted HTML email using multiple message keys
        /// </summary>
        public static string GetFormattedHtmlEmail(params object[] keyAndValues)
        {
            try
            {
                var emailBuilder = new StringBuilder();
                emailBuilder.AppendLine("<html><body>");
                emailBuilder.AppendLine("<div style='font-family: Calibri, Arial, sans-serif; max-width: 600px; margin: 0 auto;'>");
                
                // Add logo header
                emailBuilder.AppendLine("<div style='text-align: center; margin-bottom: 30px;'>");
                emailBuilder.AppendLine("<img src='https://tracker.quaffee.co.za/images/logo/QuaffeeLogoSmall.jpg' alt='Quaffee Logo' style='max-width: 200px;' />");
                
                // Process message keys with their formatting parameters
                for (int i = 0; i < keyAndValues.Length; i++)
                {
                    if (keyAndValues[i] is string messageKey)
                    {
                        string message = Get(messageKey);
                        
                        // If there are parameters following this key, use them for formatting
                        var formatParams = new List<object>();
                        for (int j = i + 1; j < keyAndValues.Length && !(keyAndValues[j] is string && keyAndValues[j].ToString().Contains(".")); j++)
                        {
                            formatParams.Add(keyAndValues[j]);
                        }
                        
                        if (formatParams.Any())
                        {
                            message = string.Format(message, formatParams.ToArray());
                            i += formatParams.Count; // Skip the used parameters
                        }
                        
                        emailBuilder.AppendLine(message);
                    }
                }
                
                emailBuilder.AppendLine("</div>");
                emailBuilder.AppendLine("</body></html>");
                
                return emailBuilder.ToString();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Email, $"MessageProvider: Error building formatted HTML email: {ex.Message}");
                return "<html><body><p>Error building email content.</p></body></html>";
            }
        }
    }

}