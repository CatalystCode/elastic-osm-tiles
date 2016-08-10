
namespace Microsoft.PCT.TestingFramework
{
    using System;
    using System.Threading.Tasks;
    using VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Utilities to assert exceptions in unit tests.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "It's short and sweet.")]
    public static class AssertEx
    {
        /// <summary>
        /// Asserts that an exception of the given type is thrown from the action.
        /// </summary>
        /// <typeparam name="TException">Type of exception.</typeparam>
        /// <param name="action">The action that should throw an exception.</param>
        /// <param name="assert">The hook to assert correctness of the exception.</param>
        public static void Throws<TException>(Action action, Action<TException> assert) where TException : Exception
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (assert == null)
                throw new ArgumentNullException(nameof(assert));

            try
            {
                action();
            }
            catch (TException ex)
            {
                assert(ex);
                return;
            }

            Assert.Fail();
        }

        /// <summary>
        /// Asserts that an exception of the given type is thrown from the asynchronous action.
        /// </summary>
        /// <typeparam name="TException">Type of exception.</typeparam>
        /// <param name="action">A task to await the action that should throw an exception.</param>
        /// <param name="assert">The hook to assert correctness of the exception.</param>
        public static async Task Throws<TException>(Func<Task> action, Action<TException> assert) where TException : Exception
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (assert == null)
                throw new ArgumentNullException(nameof(assert));

            try
            {
                await action();
            }
            catch (TException ex)
            {
                assert(ex);
                return;
            }

            Assert.Fail();
        }

        /// <summary>
        /// Asserts that an exception of the given type is thrown from the action.
        /// </summary>
        /// <typeparam name="TException">Type of exception.</typeparam>
        /// <param name="action">The action that should throw an exception.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
        public static void Throws<TException>(Action action) where TException : Exception
        {
            Throws<TException>(action, _ => { });
        }

        /// <summary>
        /// Asserts that an exception of the given type is thrown from the asynchronous action.
        /// </summary>
        /// <typeparam name="TException">Type of exception.</typeparam>
        /// <param name="action">A task to await the action that should throw an exception.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "By design.")]
        public static Task Throws<TException>(Func<Task> action) where TException : Exception
        {
            return Throws<TException>(action, _ => { });
        }
    }
}
