﻿/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace OpenIddict.MongoDb.Tests
{
    public class OpenIddictMongoDbContextTests
    {
        [Fact]
        public async Task GetDatabaseAsync_ThrowsAnExceptionForCanceledToken()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            var options = Mock.Of<IOptionsMonitor<OpenIddictMongoDbOptions>>();
            var token = new CancellationToken(canceled: true);

            var context = new OpenIddictMongoDbContext(options, provider);

            // Act and assert
            var exception = await Assert.ThrowsAsync<TaskCanceledException>(async delegate
            {
                await context.GetDatabaseAsync(token);
            });

            Assert.Equal(token, exception.CancellationToken);
        }

        [Fact]
        public async Task GetDatabaseAsync_PrefersDatabaseRegisteredInOptionsToDatabaseRegisteredInDependencyInjectionContainer()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton(Mock.Of<IMongoDatabase>());

            var provider = services.BuildServiceProvider();

            var database = Mock.Of<IMongoDatabase>();
            var options = Mock.Of<IOptionsMonitor<OpenIddictMongoDbOptions>>(
                mock => mock.CurrentValue == new OpenIddictMongoDbOptions
                {
                    Database = database
                });

            var context = new OpenIddictMongoDbContext(options, provider);

            // Act and assert
            Assert.Same(database, await context.GetDatabaseAsync(CancellationToken.None));
        }

        [Fact]
        public async Task GetDatabaseAsync_ThrowsAnExceptionWhenDatabaseCannotBeFound()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            var options = Mock.Of<IOptionsMonitor<OpenIddictMongoDbOptions>>(
                mock => mock.CurrentValue == new OpenIddictMongoDbOptions
                {
                    Database = null
                });

            var context = new OpenIddictMongoDbContext(options, provider);

            // Act and assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async delegate
            {
                await context.GetDatabaseAsync(CancellationToken.None);
            });

            Assert.Equal(new StringBuilder()
                .AppendLine("No suitable MongoDB database service can be found.")
                .Append("To configure the OpenIddict MongoDB stores to use a specific database, use ")
                .Append("'services.AddOpenIddict().AddCore().UseMongoDb().UseDatabase()' or register an ")
                .Append("'IMongoDatabase' in the dependency injection container in 'ConfigureServices()'.")
                .ToString(), exception.Message);
        }

        [Fact]
        public async Task GetDatabaseAsync_UsesDatabaseRegisteredInDependencyInjectionContainer()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton(Mock.Of<IMongoDatabase>());

            var database = Mock.Of<IMongoDatabase>();
            services.AddSingleton(database);

            var provider = services.BuildServiceProvider();

            var options = Mock.Of<IOptionsMonitor<OpenIddictMongoDbOptions>>(
                mock => mock.CurrentValue == new OpenIddictMongoDbOptions
                {
                    Database = null
                });

            var context = new OpenIddictMongoDbContext(options, provider);

            // Act and assert
            Assert.Same(database, await context.GetDatabaseAsync(CancellationToken.None));
        }
    }
}
