var builder = DistributedApplication.CreateBuilder(args);

var geonosisAuth = builder.AddProject<Projects.Geonosis_Auth>("geonosis-auth");

var geonosisApi = builder.AddProject<Projects.Geonosis_Api>("geonosis-api")
    .WaitFor(geonosisAuth);

builder.AddProject<Projects.Geonosis_Ui>("geonosis-ui")
    .WaitFor(geonosisAuth)
    .WaitFor(geonosisApi);

builder.Build().Run();
