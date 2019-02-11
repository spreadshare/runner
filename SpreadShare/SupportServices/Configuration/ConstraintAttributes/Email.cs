using System;
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
        public override string OnError(string name, object value)
            => $"{name} has value {value}, which is not a valid email address {(HostName != null ? $", given host name must be {HostName}" : string.Empty)}.";

        /// <inheritdoc/>
        protected override bool Predicate(object value)
        {
            try
            {
                var mailAddress = new MailAddress((string)value);
                return (string)value == mailAddress.Address && (HostName is null || HostName == mailAddress.Host);
            }
            catch
            {
                return false;
            }
        }
    }
}