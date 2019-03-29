using System;
using System.Collections.Generic;
using System.Linq;
using SpreadShare.Models.Trading;

namespace SpreadShare.SupportServices.Configuration.ConstraintAttributes
{
    /// <summary>
    /// Imposes that a certain candle width value is compatible with the configured value.
    /// Compatible meaning, that it is divisible by (and equal/larger) the configured value.
    /// </summary>
    internal class CompatibleCandleWidth : Constraint
    {
        /// <inheritdoc/>
        protected override Type InputType => typeof(string);

        /// <inheritdoc/>
        protected override IEnumerable<string> GetErrors(string name, object value)
        {
            var failures = new ParsesToEnum(typeof(CandleWidth)).Validate(name, value).ToList();
            if (failures.Count > 0)
            {
                foreach (var failure in failures)
                {
                    yield return failure;
                }

                yield break;
            }

            int width = (int)Enum.Parse<CandleWidth>((string)value);

            try
            {
                var unused = Configuration.Instance.CandleWidth;
            }
            catch
            {
                yield break;
            }

            if (width < (int)Configuration.Instance.CandleWidth
                || width % (int)Configuration.Instance.CandleWidth != 0)
            {
                yield return
                    $"{name} has value {value}, which cannot be used in conjunction with the configured {Configuration.Instance.CandleWidth}";
            }
        }
    }
}