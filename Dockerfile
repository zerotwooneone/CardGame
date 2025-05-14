# Stage 1: Build the Angular frontend
# Use a Node image that matches the version used by your Angular project
FROM node:20 as build-angular
WORKDIR /app/client

# Copy package files and install dependencies
COPY CardGame.Web/ClientApp/package.json CardGame.Web/ClientApp/package-lock.json ./
RUN npm install

# Copy the rest of the Angular app source code
COPY CardGame.Web/ClientApp/. .

# Build the Angular app for production
# The output path is usually 'dist/your-app-name' inside the container
RUN npm run build -- --configuration production

# Stage 2: Build the ASP.NET Core backend
# Use the .NET SDK image matching your project's target framework
FROM mcr.microsoft.com/dotnet/sdk:9.0 as build-dotnet
WORKDIR /src

# Copy solution and project files
COPY *.sln .
COPY CardGame.Domain/*.csproj ./CardGame.Domain/
COPY CardGame.Domain.Tests/*.csproj ./CardGame.Domain.Tests/
COPY CardGame.Application/*.csproj ./CardGame.Application/
COPY CardGame.Infrastructure/*.csproj ./CardGame.Infrastructure/
COPY CardGame.Web/*.csproj ./CardGame.Web/
COPY CodeGenerator/*.csproj ./CodeGenerator/
COPY GeneratorAttributes/*.csproj ./GeneratorAttributes/
COPY CodeGenConsoleTest/*.csproj ./CodeGenConsoleTest/
# Add other projects if you have more

# Restore dependencies for all projects
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Set environment variable to signal we are inside a container
# This will be used by the Condition in CardGame.Web.csproj
ENV DOTNET_RUNNING_IN_CONTAINER=true

# *** Explicitly add dotnet tools directory to PATH ***
# This ensures the 'dotnet' command can be found by the ENTRYPOINT,
# even if it wasn't added correctly by the base image in this specific environment.
ENV PATH="${PATH}:/usr/share/dotnet"

# Publish the ASP.NET Core app (Release configuration)
WORKDIR "/src/CardGame.Web"
# Output to /app/publish
RUN dotnet publish "CardGame.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Create the final runtime image
# Use the ASP.NET Core runtime image matching your project's target framework
FROM mcr.microsoft.com/dotnet/aspnet:9.0 as final
WORKDIR /app

# Copy the published backend app from the build-dotnet stage
COPY --from=build-dotnet /app/publish .

# Copy the built Angular app from the build-angular stage
# Adjust the source path ('dist/card-game.web/browser') based on your angular.json output path
# The destination './wwwroot' assumes your ASP.NET Core app is configured to serve static files from wwwroot
COPY --from=build-angular /app/client/dist/card-game-client/browser ./wwwroot

# Copy static assets from the ASP.NET Core project's wwwroot (from the publish step)
# This ensures any assets managed by the backend project are also included.
# If your assets are solely part of the Angular build, this line might be redundant
# or could be more specific, e.g., COPY --from=build-dotnet /app/publish/wwwroot/assets ./wwwroot/assets
COPY --from=build-dotnet /app/publish/wwwroot ./wwwroot

# Expose the port Kestrel will listen on inside the container (usually 80 or 8080 for HTTP)
# This depends on your ASP.NET Core configuration (ASPNETCORE_URLS environment variable or appsettings)
EXPOSE 8080

# *** Explicitly add dotnet tools directory to PATH ***
# This ensures the 'dotnet' command can be found by the ENTRYPOINT,
# even if it wasn't added correctly by the base image in this specific environment.
ENV PATH="${PATH}:/usr/share/dotnet"

# Define the entry point for the container
ENTRYPOINT ["dotnet", "CardGame.Web.dll"]
