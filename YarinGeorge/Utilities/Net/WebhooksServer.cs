using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace YarinGeorge.Utilities.Net
{
    public class WebhooksServer : IDisposable
    {
        private readonly HttpListener _server;

        public WebhooksServer(int Port)
        {
            _server = new HttpListener();
            _server.Prefixes.Add("http://*:" + Port + "/");
        }

        public EventHandler<HttpListenerContext> OnRequest;

        public void Start()
        {
            _server.Start();
            RunServerAsync(); // do not await
        }

        private async Task RunServerAsync()
        {
            while (_server.IsListening)
            {
                var context = await _server.GetContextAsync();
                OnRequest?.Invoke(this, context);
            }
        }

        public async Task WaitUntilDisposed()
        {
            while (!_disposed)
            {
                await Task.Delay(200);
            }
        }

        private bool _disposed;
        public void Dispose()
        {
            _disposed = true;
            ((IDisposable)_server)?.Dispose();
        }
    }
}
