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
using RazorLight;
using Microsoft.CodeAnalysis;

namespace MemeBoard
{
    public class WebInterface
    {
        private readonly MemeRepo repo;
        private IWebHost host;

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
            var response = this.repo.Memes.ToList();
            response.Sort((a, b) => a.Name.CompareTo(b.Name));
            hub.Clients.All.SendAsync("Update", response);
        }

        public IWebHost Build()
        {
            return WebHost.CreateDefaultBuilder().
                ConfigureServices(services =>
                {
                    services.AddTransient(_ => this);
                    services.AddTransient(_ => this.repo);
                    services.AddSignalR().AddJsonProtocol(o => 
                        o.PayloadSerializerSettings.Converters.Add(new StringEnumConverter()));
                    services.AddSignalR();
                    services.AddMvc();
                }).
                Configure(app =>
                {
                    var env = app.ApplicationServices.GetService<IHostingEnvironment>();

                    app.UseStaticFiles(new StaticFileOptions()
                    {
                        FileProvider = new PhysicalFileProvider(this.repo.Path),
                        RequestPath = "/img"
                    });

                    app.UseSignalR(config => config.MapHub<MemeHub>("/MemeHub"));

                    app.UseMvc();
                    
                    StaticFileOptions fileOptions = new StaticFileOptions()
                    {
                        FileProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly(), "MemeBoard.Web.client")
                    };
                    
                    app.UseSpa(spa =>
                    {
                        if (env.IsDevelopment())
                        {
                            spa.ApplicationBuilder.UseDeveloperExceptionPage();
                            spa.UseProxyToSpaDevelopmentServer("http://127.0.0.1:5500");
                        }

                        spa.Options.DefaultPageStaticFileOptions = fileOptions;
                    });
                    app.UseSpaStaticFiles(fileOptions);
                }).
#if DEBUG
                UseEnvironment("development").
#else
                UseEnvironment("production").
#endif
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
                this.parent.SendUpdate();
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
