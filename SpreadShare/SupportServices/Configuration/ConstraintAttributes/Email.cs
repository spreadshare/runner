using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace SpreadShare.SupportServices.Configuration.ConstraintAttributes
{
    /// <summary>
    /// Imposes that a string must be a valid email address.
    /// </summary>
    internal class Email : Constraint
    {
        /// <summary>
        /// Gets or sets the host name which the email address should match  (e.g. google.com) (default disabled).
        /// </summary>
        public string HostName { get; set; }

        /// <inheritdoc/>
        protected override Type InputType => typeof(string);

        /// <inheritdoc/>
        protected override IEnumerable<string> GetErrors(string name, object value)
        {
            MailAddress mailAddress;
            try
            {
                mailAddress = new MailAddress((string)value);
            }
            catch
            {
                mailAddress = new MailAddress("failure@error.com");
            }

            if ((string)value != mailAddress.Address || HostName != null || HostName == mailAddress.Host)
            {
                yield return $"{name} has value {value}, which is not a valid email address {(HostName != null ? $", given host name must be {HostName}" : string.Empty)}.";
            }
        }
    }
}