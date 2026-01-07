# syntax=docker/dockerfile:1

## isso foi usado pra hospedar no Render

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Copia VoltsBackend.sln para /src/VoltsBackend.sln
COPY VoltsBackend.sln ./
COPY Volts.Api/Volts.Api.csproj Volts.Api/
COPY Volts.Application/Volts.Application.csproj Volts.Application/
COPY Volts.Infrastructure/Volts.Infrastructure.csproj Volts.Infrastructure/
COPY Volts.Domain/Volts.Domain.csproj Volts.Domain/
RUN dotnet restore Volts.Api/Volts.Api.csproj
# Copia tudo do projeto local na imagem para o dirétorio (workdir) /src
COPY . .
RUN dotnet publish Volts.Api/Volts.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Production or Development
## usar appsettings.Production.json ou o Development
# ENV ASPNETCORE_ENVIRONMENT=Development 
ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8080
EXPOSE 8080
COPY --from=build /app/publish .
CMD ["sh", "-c", "dotnet Volts.Api.dll --urls http://0.0.0.0:$PORT"]