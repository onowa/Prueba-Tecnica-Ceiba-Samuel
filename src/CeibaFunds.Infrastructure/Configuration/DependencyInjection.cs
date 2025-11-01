using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using CeibaFunds.Domain.Interfaces;
using CeibaFunds.Infrastructure.Repositories;
using CeibaFunds.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CeibaFunds.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // AWS DynamoDB Configuration
        var awsConfig = new AmazonDynamoDBConfig();

        var endpoint = Environment.GetEnvironmentVariable("DynamoDB_Endpoint") ?? configuration["AWS:ServiceURL"];
        if (!string.IsNullOrEmpty(endpoint))
        {
            awsConfig.ServiceURL = endpoint;
        }

        services.AddSingleton<IAmazonDynamoDB>(provider =>
        {
            return new AmazonDynamoDBClient(
                Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") ?? "dummy",
                Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY") ?? "dummy",
                awsConfig);
        });

        services.AddSingleton<IDynamoDBContext, DynamoDBContext>();

        // Repository Registration
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IFundRepository, FundRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        // Service Registration
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}