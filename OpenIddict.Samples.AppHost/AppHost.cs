var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Geonosis_Ui>("geonosis-ui");

builder.Build().Run();
