var builder = DistributedApplication.CreateBuilder(args);

var server = builder.AddProject<Projects.Geonosis_Auth>("geonosis-auth");

var resource = builder.AddProject<Projects.Geonosis_Api>("geonosis-api")
    .WaitFor(server);

builder.AddProject<Projects.Geonosis_Ui>("geonosis-ui")
    .WaitFor(server)
    .WaitFor(resource);

var app = builder.Build();
await app.RunAsync();
