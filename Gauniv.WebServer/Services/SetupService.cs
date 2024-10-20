using Gauniv.WebServer.Data;
using Gauniv.WebServer.Websocket;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace Gauniv.WebServer.Services
{
    public class SetupService : IHostedService
    {
        private ApplicationDbContext? applicationDbContext;
        private readonly IServiceProvider serviceProvider;
        private Task? task;

        public SetupService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = serviceProvider.CreateScope()) // this will use `IServiceScopeFactory` internally
            {
                applicationDbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                if(applicationDbContext is null)
                {
                    throw new Exception("ApplicationDbContext is null");
                }

                if (applicationDbContext.Database.GetPendingMigrations().Any())
                {
                    applicationDbContext.Database.Migrate();
                }

                // Ajouter ici les données que vous insérer dans votre DB au démarrage

                return Task.CompletedTask;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
