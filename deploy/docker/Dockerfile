FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG PROJECT
WORKDIR /src
COPY . .
RUN dotnet publish "$PROJECT" -c Release -o /app /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
ARG DLL
ENV APP_DLL=$DLL
WORKDIR /app
COPY --from=build /app .

COPY knowledge-base ./knowledge-base
ENV ASPNETCORE_HTTP_PORTS=8080
USER $APP_UID
ENTRYPOINT ["sh", "-c", "exec dotnet $APP_DLL \"$@\"", "--"]

