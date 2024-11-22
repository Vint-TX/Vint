[![Discord](https://img.shields.io/discord/1196050317607972934?style=for-the-badge&label=Discord&link=https%3A%2F%2Fdiscord.gg%2F2ZdJYbj7pt)](https://discord.gg/2ZdJYbj7pt)

# Vint

This is the first open-source server for the TankiX game

Issues and pull-requests are acceptable

## Building

1. Install [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
2. Clone this repository
3. Type `dotnet build` into the terminal in the directory with the server

## Setup

1. Install [MariaDB Server](https://mariadb.org/download/?t=mariadb&p=mariadb&r=11.2.2)
2. Create database and user for the server (see [database.json](./Resources/database.json))
3. Import database schema (see [Vint.sql](./Vint.sql))
4. Start the server

## Information

Server uses port 8080 for the HTTP Static server, 5050 for the TCP Game server and 5051 for the local HTTP REST API
server
