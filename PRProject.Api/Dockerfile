FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["PRProject.Api/PRProject.Api.csproj", "PRProject.Api/"]
COPY ["PRProject.Bll/PRProject.Bll.csproj", "PRProject.Bll/"]
COPY ["PRProject.Dal/PRProject.Dal.csproj", "PRProject.Dal/"]
COPY ["PRProject.Domain/PRProject.Domain.csproj", "PRProject.Domain/"]
COPY ["PRProject.Common/PRProject.Common.csproj", "PRProject.Common/"]

RUN dotnet restore "PRProject.Api/PRProject.Api.csproj"

COPY . .
WORKDIR "/src/PRProject.Api"
RUN dotnet publish "PRProject.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "PRProject.Api.dll"]