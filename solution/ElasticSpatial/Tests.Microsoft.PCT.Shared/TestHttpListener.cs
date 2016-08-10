
namespace Tests.Microsoft.PCT.Shared
{
    using global::Microsoft.PCT.Reactive;
    using System;
    using System.Linq;
    using System.Net;
    using System.Reactive.Threading.Tasks;
    using System.Threading;
    using System.Threading.Tasks;

    public class TestHttpListener : IDisposable
    {
        private readonly Action<HttpListenerResponse> createResponse;
        private readonly HttpListener listener;
        private readonly CancellationTokenSource tokenSource;
        private readonly Task loop;

        public TestHttpListener(Action<HttpListenerResponse> createResponse, params string[] prefixes)
        {
            this.createResponse = createResponse;
            listener = new HttpListener();
            tokenSource = new CancellationTokenSource();

            StartListener(prefixes);

            loop = Run();
        }

        public string ServiceName
        {
            get
            {
                return listener.DefaultServiceNames.Cast<string>().First();
            }
        }

        private async Task Run()
        {
            var canceledTask = tokenSource.Token.ToObservable().ToTask();
            var bytes = new byte[0];

            while (true)
            {
                var contextTask = listener.GetContextAsync();
                var contextOrCanceledTask = await Task.WhenAny(contextTask, canceledTask);
                if (contextOrCanceledTask == canceledTask)
                {
                    break;
                }

                var context = await contextTask;
                createResponse(context.Response);
                context.Response.Close(bytes, false);
            }
        }

        public void Dispose()
        {
            tokenSource.Cancel();
            tokenSource.Dispose();
            loop.Wait();
            listener.Stop();
            listener.Close();
        }

        private void StartListener(string[] prefixes)
        {
            foreach (var prefix in prefixes)
            {
                listener.Prefixes.Add(prefix);
            }

            listener.Start();
        }
    }
}
