
namespace Microsoft.PCT.Reactive
{
    using System;

    /// <summary>
    /// Factory methods for <see cref="Exceptional{T}"/>.
    /// </summary>
    public static class Exceptional
    {
        /// <summary>
        /// Create an instance of <see cref="Exceptional{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The instance.</returns>
        public static Exceptional<T> ToExceptional<T>(this T value)
        {
            return new Exceptional<T>(value);
        }

        /// <summary>
        /// Create an instance of <see cref="Exceptional{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of value.</typeparam>
        /// <param name="exception">The exception.</param>
        /// <returns>The instance.</returns>
        public static Exceptional<T> ToExceptional<T>(this Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            return new Exceptional<T>(exception);
        }
    }
}
