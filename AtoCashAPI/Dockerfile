#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80


FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . ./
#COPY ["AtoCashAPI/CashAdvanceApprNotificationEmail.html", "./"]
#COPY ["AtoCashAPI/ClaimApprovedandSettled.html", "./"]
#COPY ["AtoCashAPI/ExpApprNotificationEmail.html", "./"]
#COPY ["AtoCashAPI/TravelApprNotificationEmail.html", "./"]

COPY ["AtoCashAPI/CashAdvanceApprNotificationEmail.html", "AtoCashAPI/"]
COPY ["AtoCashAPI/ClaimApprovedandSettled.html", "AtoCashAPI/"]
COPY ["AtoCashAPI/ExpApprNotificationEmail.html", "AtoCashAPI/"]
COPY ["AtoCashAPI/ConfirmEmail.html", "AtoCashAPI/"]
COPY ["AtoCashAPI/TravelApprNotificationEmail.html", "AtoCashAPI/"]


COPY ["AtoCashAPI/AtoCashAPI.csproj", "AtoCashAPI/"]
RUN dotnet restore "AtoCashAPI/AtoCashAPI.csproj"
COPY . .
COPY ["EmailService/EmailService.csproj", "EmailService/"]
RUN dotnet restore "EmailService/EmailService.csproj"
COPY . .

WORKDIR "/src/AtoCashAPI"
RUN dotnet build "AtoCashAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AtoCashAPI.csproj" -c Release -o /app/publish


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AtoCashAPI.dll"]