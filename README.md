# About

The purpose of the library is to help manage SQL Server connections between tenants.

Below is the database model used in this library.
![Screenshot from 2025-01-25 15-28-20](https://github.com/user-attachments/assets/6fbd631d-e637-4db4-b057-c02c1ba38edb)

# Prerequisites

## Install Package
`dotnet nuget add source https://nuget.pkg.github.com/chizer1/index.json --name DbLocator --username <YourGitHubUsername> --password <YourGitHubPAT>`

## Database setup

First, you need an instance of SQL Server running. For local development, you can either:
   - Use the SQL Server image in this repository by running `docker compose up` from the root. This requires Docker Desktop to be installed (https://docs.docker.com/get-started/get-docker/)
   - Install SQL Server directly on your machine (https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

# How to use / examples

After getting everything setup, you will now be able to initialize the base DbLocator object:
```
DbLocator locator = new("YourConnectionString");
```

Initializing the DbLocator object will automatically run the EF core migration scripts and create the necessary database if not created yet.

- Example repository implementing this class library: [https://github.com/chizer1/DbLocatorExample](https://github.com/chizer1/DbLocatorExample)
