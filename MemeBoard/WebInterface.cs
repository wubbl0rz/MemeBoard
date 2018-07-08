using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;
using System.Reflection;

namespace MemeBoard
{
    public class WebInterface
    {
        private readonly MemeRepo repo;
        private IWebHost host;
        private readonly string webroot = Path.Combine(Path.GetTempPath(), "MemeBoard");

        public event Action<Meme> MemeClicked;

        public bool IsRunning { get; private set; }

        public WebInterface(MemeRepo repo)
        {
            this.repo = repo;
            this.repo.Updated += () => this.SendUpdate();
        }

        public void Start()
        {
            this.host = this.Build();
            this.host.Start();
            this.IsRunning = true;
        }

        public void Stop()
        {
            this.host?.StopAsync().Wait();
            this.IsRunning = false;
        }

        public void SendUpdate()
        {
            if (!this.IsRunning)
                return;
            
            var hub = this.host.Services.GetService<IHubContext<MemeHub>>();
            hub.Clients.All.SendAsync("Update", this.repo.Memes);
        }

        public IWebHost Build()
        {
            return WebHost.CreateDefaultBuilder().
                ConfigureServices(services =>
                {
                    services.AddTransient(_ => this);
                    services.AddSignalR().AddJsonProtocol(o =>
                        o.PayloadSerializerSettings.Converters.Add(new StringEnumConverter()));
                }).
                Configure(app =>
                {
                    app.UseStaticFiles(new StaticFileOptions
                    {
                        FileProvider = new PhysicalFileProvider(this.repo.Path),
                        RequestPath = "/img"
                    });


                    app.UseStaticFiles(new StaticFileOptions
                    {
                        FileProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly()),
                        RequestPath = "/html"
                    });


                    app.UseSignalR(c => c.MapHub<MemeHub>("/MemeHub"));
                }).
                UseUrls($"http://*:5001/").
                Build();
        }

        class MemeHub : Hub
        {
            private readonly WebInterface parent;

            public MemeHub(WebInterface parent)
            {
                this.parent = parent;
            }
            
            public override Task OnConnectedAsync()
            {
                this.Clients.Caller.SendAsync("Update", this.parent.repo.Memes);
                return base.OnConnectedAsync();
            }
            
            public void MemeClicked(string path)
            {
                var meme = this.parent.repo.Memes.First(m => m.Path == path);
                this.parent.MemeClicked?.Invoke(meme);
            }
        }
    }
}
