# RaceDay – Running Event Management System

## Project Overview

RaceDay is a web-based running event management system developed using C# and ASP.NET Core. It allows organisers to manage running events and participants to register, log in and explore available events.
.

The application uses an MVC web application connected to a separate backend Web API and SQL Server database.

## Features

### Public Users
- Browse available running events.
- View event dates, locations and descriptions.
- View event banners when available.

### Participants
- Register an account.
- Log in and log out.
- Access a participant dashboard.
- Browse events and view event details.

### Organisers
- Register an organiser account.
- Log in securely.
- Access an organiser dashboard.
- Create, update and delete events.
- Upload event banner images.

## Technologies Used

- C#
- ASP.NET Core MVC
- ASP.NET Core Web API
- Entity Framework Core
- Microsoft SQL Server
- JWT Authentication
- Bootstrap 5
- HTML and CSS
- Azure Blob Storage
- Azurite for local storage development
- Docker
- GitHub Actions
- Visual Studio

## Solution Structure

The solution contains three main projects:

```text
RaceDay.Part3/
│
├── RaceDay.API/
│   ├── Controllers/
│   ├── Data/
│   ├── Models/
│   ├── Services/
│   └── Program.cs
│
├── RaceDay.Web/
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   ├── Views/
│   ├── wwwroot/
│   └── Program.cs
│
├── RaceDay.API.Tests/
│
├── .github/
│   └── workflows/
│
├── Dockerfile
└── README.md
```

## Project Architecture

### RaceDay.API

The API manages backend operations, authentication, event management, database access and banner storage.

### RaceDay.Web

The MVC application provides the user interface. It uses:
- Models to represent application data.
- Views to display pages.
- Controllers to handle user requests.
- Services to communicate with the API.

### RaceDay.API.Tests

Contains automated tests for the API.

## Database

**Database:** `RaceDayDB`  
**Database platform:** Microsoft SQL Server

Entity Framework Core is used for database operations.

The database stores user accounts, participant and organiser information, event details and other application records.

## Authentication

RaceDay uses JSON Web Token (JWT) authentication for protected API requests.

The application supports two main user roles:
- Participant
- Organiser

Users authenticate through the API, which returns a token used for authorised requests.

## Azure Blob Storage

Azure Blob Storage functionality is used for event banner images.

The application supports uploading and retrieving event banners. Azurite can be used as a local storage emulator during development.

## Configuration

Before running the application, configure:

- SQL Server connection string.
- JWT authentication settings.
- Azure Blob Storage or Azurite connection string.
- Web application API base URL.

Sensitive configuration values should be stored using .NET User Secrets or environment variables and must not be committed to GitHub.

## Prerequisites

Install:
- Visual Studio with ASP.NET and web development support.
- .NET SDK matching the project target framework.
- Microsoft SQL Server.
- SQL Server Management Studio (optional).
- Docker Desktop.
- Git.
- Azurite, if required for local Blob Storage.

## Running Locally

1. Clone the RaceDay repository.
2. Open the solution in Visual Studio.
3. Restore NuGet packages.
4. Configure the database connection.
5. Ensure the `RaceDayDB` database is available.
6. Configure JWT and Blob Storage settings.
7. Configure the Web application's API base URL.
8. Build the solution.
9. Run both the API and Web projects.

Current local development addresses:

**Web application:**
`https://localhost:7063`

**API:**
`https://localhost:7001`

API events endpoint:

`https://localhost:7001/api/events`

The ports may differ depending on the local launch configuration.

## Docker

Docker is used to package applications into containers.

Ensure Docker Desktop is installed and running before building the application.

If the Dockerfile is located in the solution root, build the image using:

```bash
docker build -t raceday .
```

Run the container using the port configured in the Dockerfile:

```bash
docker run -d --name raceday -p 8080:8080 raceday
```

View running containers:

```bash
docker ps
```

View container logs:

```bash
docker logs raceday
```

Stop the container:

```bash
docker stop raceday
```

Remove the container:

```bash
docker rm raceday
```

**Note:** Docker commands and port mappings must match the actual Dockerfile and application configuration. Database and Blob Storage services must also be accessible from the container.

## GitHub Actions

GitHub Actions is used for continuous integration.

The workflow should:
1. Check out the repository.
2. Install the required .NET SDK.
3. Restore dependencies.
4. Build the API project.
5. Build the MVC Web project.
6. Run automated tests.

Workflow files are stored in:

`.github/workflows/`

## Testing

The API test project is:

`RaceDay.API.Tests`

Run automated tests from the solution directory:

```bash
dotnet test
```

The application should also be tested for registration, login, role-based access, event management, banner uploads and API communication.

## Demonstration Video

**YouTube demonstration link:**  
[Insert your unlisted YouTube video link here]

The demonstration should cover:
- Running the RaceDay application.
- Public event browsing.
- Participant functionality.
- Organiser functionality.
- MVC architecture.
- API and database integration.
- Azure Blob Storage integration.
- Docker configuration.

## Submission Checklist

- [ ] RaceDay source code uploaded to GitHub.
- [ ] README included.
- [ ] Dockerfile included.
- [ ] Docker configuration checked.
- [ ] GitHub Actions workflow included.
- [ ] Unit tests added and executed.
- [ ] YouTube demonstration uploaded as Unlisted.
- [ ] GitHub repository link submitted through ARC.

## Future Improvements

- Expanded participant enrolment functionality.
- Participant results and event history.
- Organiser reporting.
- Additional automated tests.
- Cloud deployment.
- Improved accessibility and error handling.

---

**Project:** RaceDay – Running Event Management System  
**Language:** C#  
**Framework:** ASP.NET Core  
**Database:** Microsoft SQL Server  
**Architecture:** MVC and Web API  
**Development Environment:** Visual Studio
