using System;
using System.Collections.Generic;
using System.Text;
using Infrastructure.Persistence.SQLServer.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Application.UnitTests.Common
{
    public static class TestExtensions
    {
        public static Task<TResponse> SendAsync<TResponse>(
            this IServiceProvider serviceProvider,
            IRequest<TResponse> request
        )
        {
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            return mediator.Send(request);
        }

        public static Task SendAsync(this IServiceProvider serviceProvider, IRequest request)
        {
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            return mediator.Send(request);
        }

        public static IServiceCollection AddDatabase(
            this IServiceCollection services,
            TimeProvider timeProvider
        )
        {
            var databaseName = Guid.NewGuid().ToString();
            var root = new InMemoryDatabaseRoot();

            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: databaseName, root)
                .EnableServiceProviderCaching(false)
                .ConfigureWarnings(config =>
                    config.Ignore(InMemoryEventId.TransactionIgnoredWarning)
                )
                .Options;

            var context = new ApplicationDbContext(dbOptions);
            services.AddSingleton(context);
            context.Database.EnsureCreated();
            var users = context.Users.ToList();

            var roContext = new ReadOnlyDbContext(dbOptions);
            services.AddSingleton(roContext);
            var rwContext = new WritableDbContext(dbOptions, timeProvider);
            services.AddSingleton(rwContext);

            return services;
        }
    }
}
