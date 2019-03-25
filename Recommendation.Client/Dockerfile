FROM microsoft/dotnet:2.2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 52521
EXPOSE 44340

FROM microsoft/dotnet:2.2.2-sdk AS build
WORKDIR /src
COPY ["Recommendation.Client/Recommendation.Client.csproj", "Recommendation.Client/"]
RUN dotnet restore "Recommendation.Client/Recommendation.Client.csproj"
COPY . .
WORKDIR "/src/Recommendation.Client"
RUN dotnet build "Recommendation.Client.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Recommendation.Client.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Recommendation.Client.dll"]