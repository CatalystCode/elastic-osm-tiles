
namespace Tests.Microsoft.PCT.Reactive
{
    using global::Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Runtime.ExceptionServices;
    using System.Threading.Tasks;

    /// <summary>
    /// Utilities for running integration tests as inconclusive.
    /// </summary>

    [TestClass]
    public static class IntegrationTest
    {
        /// <summary>
        /// Perform an action. If it throws an exception or fails an assertion, report the failure as inconclusive.
        /// </summary>
        /// <param name="action">The action to run.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is intented to catch anything the action may throw, which would otherwise trigger a failure.")]
        public static void Run(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                action();
            }
            catch (AssertFailedException failure)
            {
                HandleFailure(failure);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        /// <summary>
        /// Perform an asynchronous action. If it throws an exception or fails an assertion, report the failure as inconclusive.
        /// </summary>
        /// <param name="action">A task to await the action.</param>
        public static async Task Run(Func<Task> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                await action();
            }
            catch (AssertFailedException failure)
            {
                HandleFailure(failure);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private static void HandleInconclusiveFailure(AssertFailedException failure)
        {
#if DEBUG
            // This is rethrow semantics from outside a catch block.
            ExceptionDispatchInfo.Capture(failure).Throw();
#else
            Assert.Inconclusive("Test failed with message: {0}{1} Stack trace: {2}",
                failure.Message,
                Environment.NewLine,
                failure.StackTrace);
#endif
        }

        private static void HandleInconclusiveException(Exception ex)
        {
#if DEBUG
            // This is rethrow semantics from outside a catch block.
            ExceptionDispatchInfo.Capture(ex).Throw();
#else
            Assert.Inconclusive("Test failed with exception: {0}", ex);
#endif
        }

        private static void HandleFailure(AssertFailedException failure)
        {
            // This is rethrow semantics from outside a catch block.
            ExceptionDispatchInfo.Capture(failure).Throw();
        }

        private static void HandleException(Exception ex)
        {
            // This is rethrow semantics from outside a catch block.
            ExceptionDispatchInfo.Capture(ex).Throw();
        }
    }
}
