namespace SpreadShare.SupportServices.Configuration.ConstraintAttributes
{
    /// <summary>
    /// Requires a setting to be not null.
    /// </summary>
    internal class Required : Constraint
    {
        /// <inheritdoc/>
        public override string OnError(string name, object value)
            => $"{name} must have a value";

        /// <inheritdoc/>
        protected override bool Predicate(object value) => value != null;
    }
}