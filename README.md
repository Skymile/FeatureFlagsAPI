Project runs on .NET 8.0.

The following packages are used:

    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.18" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.0.7" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />

Unit tests are on xUnit 2.5.3.

Running the app via `dotnet run` will start the web server with some test data.

Run `dotnet run clean` to start without any data.
