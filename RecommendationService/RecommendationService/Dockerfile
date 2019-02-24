FROM microsoft/dotnet:2.2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 52521
EXPOSE 44340

FROM microsoft/dotnet:2.2.2-sdk AS build
WORKDIR /src
COPY ["RecommendationService/RecommendationService.csproj", "RecommendationService/"]
RUN dotnet restore "RecommendationService/RecommendationService.csproj"
COPY . .
WORKDIR "/src/RecommendationService"
RUN dotnet build "RecommendationService.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "RecommendationService.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "RecommendationService.dll"]