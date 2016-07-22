# Run the Sample with the CLI

First, clone this repo.

    git clone git@github.com:openiddict/openiddict-samples.git
    
Second, start the Mvc Server Sample.

    cd samples/Mvc/Mvc.Server   
    dotnet restore
    dotnet run --server.urls="http://localhost:54540"
    
Third, in another command prompt, start the Mvc Client Sample.

    cd samples/Mvc/Mvc.Client
    dotnet restore
    dotnet run --server.urls="http://localhost:53507"

Fourth, navigate to `localhost:53507` in a web browser.

**Note:** For the server to work, we need either to install SQL Server Express or to use the ASP.NET Core in memory datastore.
