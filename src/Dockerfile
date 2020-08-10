# Stage 1
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

WORKDIR /src
COPY . .
RUN find -type d -name bin -prune -exec rm -rf {} \; && find -type d -name obj -prune -exec rm -rf {} \;
RUN dotnet restore ./InMemoryIdentityApp.sln
RUN dotnet build   ./InMemoryIdentityApp.sln -c Debug --no-restore
RUN dotnet test    ./InMemoryIdentityApp.sln -c Debug --no-restore --no-build
RUN dotnet build   ./InMemoryIdentityApp.sln -c Release --no-restore