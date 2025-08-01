# Eventure - An Event Management Web Application

**Eventure** is a full-featured CRUD web application built with ASP.NET Core MVC that allows users to create, discover, and join events. This project was developed to demonstrate proficiency in building modern .NET applications with a focus on clean architecture, best programming practices, and a rich feature set.

**Live Demo Link:** `TODO`

## Table of Contents
- [Key Features](#key-features)
- [Technologies and Libraries](#technologies-and-libraries)
- [Project Architecture](#project-architecture)
- [Getting Started](#getting-started)
- [Screenshots](#screenshots)

## Key Features

The application offers a wide range of functionalities for both regular users and administrators.

### User Features:
*   **Account System:** Full user account lifecycle management, including registration, login, email confirmation, password reset, and profile editing.
*   **Event Management:** Users can create their own events, edit them, and delete them.
*   **Browsing and Searching:** Advanced event filtering by title, location, category, and date. Results are presented with pagination.
*   **Interactive Calendar:** An alternative event view presented as a fully functional calendar (monthly and weekly views) powered by **FullCalendar.js**.
*   **Joining Events:** An event registration system with validation for the maximum number of participants.
*   **Interactive Map:** The event location is displayed on a dynamic map thanks to an integration with **Leaflet.js** and **OpenStreetMap**.
*   **Comment System:** Event participants can engage in discussions through nested comment threads (replies to comments).
*   **Real-Time Private Messaging:** Users can have private conversations. New messages appear instantly without needing to refresh the page, implemented using **SignalR**.
*   **Notification System:** Users receive in-app notifications (e.g., for event updates) and email notifications.
*   **Profile Customization:** Ability to change username, phone number, and upload a custom **avatar** (profile picture).
*   **Recommendations:** The event details page displays suggestions for other events in the same category.

### Admin Panel Features:
*   **User Management:** An admin-exclusive panel allows for viewing a list of all users and locking/unlocking their accounts.
*   **Role Management:** The administrator can dynamically assign and revoke roles (e.g., promote a user to an admin role).
*   **Content Management:** Ability to delete any event in the system, bypassing standard ownership permissions.

## Technologies and Libraries

The project was built using a modern .NET technology stack.

### Backend:
*   **Framework:** ASP.NET Core 8
*   **Language:** C# 12
*   **Architecture:** MVC (Model-View-Controller)
*   **Data Access:** Entity Framework Core 8 (Code-First)
*   **Database:** SQL Server
*   **Authentication:** ASP.NET Core Identity
*   **Real-Time Communication:** SignalR
*   **Emailing:** MailKit
*   **Dependency Injection:** Built-in .NET DI Container

### Frontend:
*   **Templating:** Razor Pages
*   **Styling:** Bootstrap 5
*   **Interactivity:**
    *   **FullCalendar.js** - for the calendar view
    *   **Leaflet.js** - for interactive maps
    *   **JavaScript (ES6)** - for SignalR handling and dynamic UI elements

## Project Architecture

The application was designed with a clear separation of concerns and code cleanliness in mind.
*   **Controllers:** Responsible for handling HTTP requests and coordination. They are "thin" and delegate all business logic to the service layer.
*   **Services:** The heart of the application. They contain all business logic (e.g., creating an event, sending a message, filtering). They communicate with the database via the `DbContext`.
*   **Data:** Contains the `DbContext`, migrations, and model configuration (Fluent API).
*   **Models:** Definitions of database entities (POCO).
*   **ViewModels:** Dedicated models for views, ensuring a separation between the domain model and what is presented to the user.
*   **Hubs:** Communication centers for SignalR.

## Getting Started

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/your-username/Eventure.git
    ```
2.  **Configure the database connection:**
    *   In the `appsettings.json` file, update the `ConnectionStrings` to match your SQL Server setup.
3.  **Configure User Secrets:**
    *   Right-click the project in Visual Studio -> `Manage User Secrets`.
    *   Add the configuration for the email service (see example below):
    ```json
    {
      "EmailSettings": {
        "From": "your.system.email@example.com",
        "SmtpServer": "smtp.example.com",
        "Port": 587,
        "Username": "your.system.email@example.com",
        "Password": "YourAppPassword"
      }
    }
    ```
4.  **Apply migrations:**
    *   In the `Package Manager Console` in Visual Studio, run the command:
    ```powershell
    Update-Database
    ```
5.  **Run the application:**
    *   Press F5 in Visual Studio or use `dotnet run` in the terminal.

## Screenshots
