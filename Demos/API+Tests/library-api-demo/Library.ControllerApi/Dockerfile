# Stage 1  - BUILD: We need the .NET SDK image in order to take our code
# and build the api. We do not want to ship that SDK image, it's too big
# and we don't need it to RUN the API

# AS build names our stage, can be useful for later
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build 
WORKDIR /src

# Restore first. Project files change rarely, source files change constantly
# The restore layer is cached until packages change
COPY Library.ControllerApi/Library.ControllerApi.csproj Library.ControllerApi/
COPY Library.Data/Library.Data.csproj Library.Data/
RUN dotnet restore Library.ControllerApi/Library.ControllerApi.csproj

# Now we have our Nuget packages, we need our source code
COPY Library.ControllerApi/ Library.ControllerApi/
COPY Library.Data/ Library.Data/
RUN dotnet publish Library.ControllerApi/Library.ControllerApi.csproj -c Release -o /app/publish 

# Stage 2 - RUNTIME: Only the asp.net runtime + our published output
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Containers have their own local networking - and the ASP.NET image listens on 8080 by default 
# Any config (conn_string, JWT key, etc) comes from the runtime environment. Do not hardcode those
# into this file, this will make it to GH
EXPOSE 8080
ENTRYPOINT [ "dotnet", "Library.ControllerApi.dll" ]