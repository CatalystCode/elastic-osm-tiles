
namespace Microsoft.PCT.Reactive
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    /// Container for either a value or exception.
    /// </summary>
    /// <typeparam name="T">Type of value contained.</typeparam>
    public struct Exceptional<T> : IEquatable<Exceptional<T>>
    {
        private readonly T _value;
        private readonly Exception _exception;
        private readonly bool _hasValue;

        internal Exceptional(T value)
        {
            _value = value;
            _exception = null;
            _hasValue = true;
        }

        internal Exceptional(Exception exception)
        {
            _value = default(T);
            _exception = exception;
            _hasValue = false;
        }

        /// <summary>
        /// The value.
        /// </summary>
        public T Value { get { return _value; } }

        /// <summary>
        /// The exception.
        /// </summary>
        public Exception Exception { get { return _exception; } }

        /// <summary>
        /// <b>true</b> if the instance has a value.
        /// </summary>
        public bool HasValue { get { return _hasValue; } }

        /// <summary>
        /// Checks for equality between two instances.
        /// </summary>
        /// <param name="other">The other instance.</param>
        /// <returns>
        /// <b>true</b> if the instances are equal, <b>false</b> otherwise.
        /// </returns>
        public bool Equals(Exceptional<T> other)
        {
            return (HasValue && EqualityComparer<T>.Default.Equals(Value, other.Value))
                || (!HasValue && EqualityComparer<Exception>.Default.Equals(Exception, other.Exception));
        }

        /// <summary>
        /// Checks for equality between two objects.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>
        /// <b>true</b> if the objects are equal, <b>false</b> otherwise.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is Exceptional<T>)
            {
                return Equals((Exceptional<T>)obj);
            }

            return false;
        }

        /// <summary>
        /// Gets a hash code for the instance.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return HasValue
                ? EqualityComparer<T>.Default.GetHashCode(_value)
                : EqualityComparer<Exception>.Default.GetHashCode(_exception);
        }

        /// <summary>
        /// Gets a string representation of the instance.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return typeof(T).IsValueType
                ? ToStringValueType()
                : ToStringReferenceType();
        }

        private string ToStringValueType()
        {
            var format = "Exceptional({0})";
            return HasValue
                ? string.Format(CultureInfo.InvariantCulture, format, Value)
                : string.Format(CultureInfo.InvariantCulture, format, Exception);
        }

        private string ToStringReferenceType()
        {
            var format = "Exceptional({0})";
            var value = ((object)Value) ?? "null";
            return HasValue
                ? string.Format(CultureInfo.InvariantCulture, format, value)
                : string.Format(CultureInfo.InvariantCulture, format, Exception);
        }

        /// <summary>
        /// Checks for equality between two instances.
        /// </summary>
        /// <param name="first">The first instance.</param>
        /// <param name="second">The second instance.</param>
        /// <b>true</b> if the instances are equal, <b>false</b> otherwise.
        public static bool operator ==(Exceptional<T> first, Exceptional<T> second)
        {
            return first.Equals(second);
        }

        /// <summary>
        /// Checks for inequality between two instances.
        /// </summary>
        /// <param name="first">The first instance.</param>
        /// <param name="second">The second instance.</param>
        /// <b>true</b> if the instances are not equal, <b>false</b> otherwise.
        public static bool operator !=(Exceptional<T> first, Exceptional<T> second)
        {
            return !first.Equals(second);
        }
    }
}
