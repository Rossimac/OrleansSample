var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage").RunAsEmulator(c => c.WithImageTag("3.33.0"));
var clusteringTable = storage.AddTables("clustering");
var grainStorage = storage.AddBlobs("grain-state");

var orleans = builder.AddOrleans("default")
    .WaitFor(storage)
    .WithClustering(clusteringTable)
    .WithGrainStorage("Default", grainStorage);

var silo = builder.AddProject<Projects.Silo>("silo")
    .WaitFor(storage)
    .WithReference(orleans)
    .WithReplicas(1);

builder.AddProject<Projects.Client>("frontend")
    .WaitFor(silo)
    .WithReference(orleans.AsClient())
    .WithExternalHttpEndpoints()
    .WithReplicas(1);

builder.Build().Run();