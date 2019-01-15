using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Dawn;
using SpreadShare.Models.Exceptions;
using SpreadShare.Models.Poco;

namespace SpreadShare.SupportServices.SettingsServices
{
    /// <summary>
    /// Settings regarding administrative monitoring.
    /// </summary>
    internal sealed class AdministratorSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdministratorSettings"/> class.
        /// </summary>
        /// <param name="adminSettingsRaw">The adminSettingsRaw object to parse.</param>
        public AdministratorSettings(AdministratorSettingsPoco adminSettingsRaw)
        {
            Guard.Argument(adminSettingsRaw).NotNull(nameof(adminSettingsRaw));

            Guard.Argument(adminSettingsRaw.AdminEmail)
                .Require<InvalidConfigurationException>(
                    x => !string.IsNullOrEmpty(x),
                    x => $"{nameof(x)} is required for {nameof(AdministratorSettings)}")
                .Require<InvalidEmailException>(
                    IsValidEmail,
                    x => $"AdminEmail is not a valid email address: {x}");

            Guard.Argument(adminSettingsRaw.AdminPassword)
                .Require<InvalidConfigurationException>(
                    x => !string.IsNullOrEmpty(x),
                    x => $"{nameof(x)} is required for {nameof(AdministratorSettings)}");

            Guard.Argument(adminSettingsRaw.Recipients)
                .NotNull(nameof(Recipients))
                .Require<InvalidEmailException>(
                    x => x.TrueForAll(IsValidEmail),
                    _ => "Not all recipients are valid email addresses");

            AdminEmail = new MailAddress(adminSettingsRaw.AdminEmail);
            Guard.Argument(AdminEmail).Require<InvalidEmailException>(
                x => x.Host == "gmail.com",
                x => "AdminEmail must be a gmail address");

            AdminPassword = adminSettingsRaw.AdminPassword;
            Recipients = adminSettingsRaw.Recipients.Select(x => new MailAddress(x)).ToList();
        }

        /// <summary>
        /// Gets the list of Recipients mail addresses.
        /// </summary>
        public List<MailAddress> Recipients { get; }

        /// <summary>
        /// Gets the Admin mail address.
        /// </summary>
        public MailAddress AdminEmail { get; }

        /// <summary>
        /// Gets the Admin password.
        /// </summary>
        public string AdminPassword { get; }

        private static bool IsValidEmail(string input)
        {
            try
            {
                return input == new MailAddress(input).Address;
            }
            catch
            {
                return false;
            }
        }
    }
}
