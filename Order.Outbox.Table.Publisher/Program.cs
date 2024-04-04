using Order.Outbox.Table.Publisher.Jobs;
using Quartz;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddQuartz(configurator =>
{
    JobKey jobKey = new("OrderOutboxPublishJon");
    configurator.AddJob<OrderOutboxPublishJob>(options => options.WithIdentity(jobKey));

    TriggerKey triggerKey = new("OrderOutboxPublishTrigger");
    configurator.AddTrigger(options => options.ForJob(jobKey)
    .WithIdentity(triggerKey)
    .StartAt(DateTime.UtcNow)
    .WithSimpleSchedule(builder => builder.WithIntervalInSeconds(5)
    .RepeatForever())
    );
});

builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

//Order.Outbox.Table.Publisher amacý belirli periyotlarla OrderOutboxes tablosundaki verileri kontrol edip, belirli bir kurala göre RabbitMQ'ya mesaj göndermektir.
var host = builder.Build();
host.Run();
