# HealthAid: Digital Healthcare Platform for Palestine

![Build Status](https://img.shields.io/badge/Build-Passing-success?style=for-the-badge&logo=github)
![Platform](https://img.shields.io/badge/Platform-.NET%208-purple?style=for-the-badge&logo=dotnet)
![Database](https://img.shields.io/badge/Database-SQL%20Server-red?style=for-the-badge&logo=microsoftsqlserver)
![License](https://img.shields.io/badge/License-MIT-blue?style=for-the-badge)

> **"Bridging the gap between patients, doctors, and humanitarian aid in crisis zones."**

---

## Introduction

**HealthAid** is an enterprise-grade RESTful API designed to serve as a digital lifeline for the healthcare sector in Palestine. Amidst infrastructure challenges and limited accessibility, this platform connects patients with medical professionals through telemedicine, facilitates emergency response via geo-location, and ensures transparent management of humanitarian aid.

This project was developed as the Capstone Project for the **Advanced Software Engineering Course (Fall 2025)** under the supervision of **Dr. Amjad AbuHassan**.

---

## Key Features

### Telemedicine & Clinical Care

- **Smart Scheduling:** Advanced appointment booking system with conflict detection.
- **Digital Prescriptions:** Secure issuance and tracking of medicines.
- **AI Symptom Checker:** Integrated OpenAI logic to triage patients before consultation.

### Emergency Response (Real-Time)

- **SOS Alerts:** One-click emergency distress signal with precise GPS coordinates.
- **Geo-Spatial Dispatch:** Automatically finds and notifies the nearest responders within a 5km radius using SignalR.

### Humanitarian Aid & Logistics

- **Sponsorships:** Case-based funding system for critical surgeries.
- **Medicine Inventory:** Centralized pharmacy search engine to locate scarce medications.
- **Transparent Donations:** Financial audit trail linked to Stripe payment gateway.

---

## Technical Architecture

The system is built on a robust **N-Tier Architecture**, ensuring separation of concerns and scalability.

- **Framework:** ASP.NET Core 8 Web API
- **Database:** SQL Server (Entity Framework Core)
- **Design Principles:** SOLID, Dependency Injection (DI), Repository Pattern
- **Documentation:** Swagger / OpenAPI (Full interactive documentation)

### Tech Stack

| Component          | Technology                |
| :----------------- | :------------------------ |
| **Backend Core**   | .NET 8 (C#)               |
| **Real-Time**      | SignalR                   |
| **Security**       | JWT Authentication & RBAC |
| **Mapping**        | AutoMapper                |
| **AI Integration** | OpenAI API Logic          |
| **Payments**       | Stripe Integration        |

---

## Installation & Setup

Follow these steps to run the project locally:

### Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB or Express)
- Visual Studio 2022

### Steps

1.  **Clone the repository:**

    ```bash
    git clone https://github.com/Sara-Samara/HealthAidProj.git
    cd HealthAidProj
    ```

2.  **Configure Database:**
    Update `appsettings.json` connection string if needed.

    ```json
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HealthAidDB;Trusted_Connection=True;"
    ```

3.  **Apply Migrations:**

    ```bash
    dotnet ef database update
    ```

4.  **Run the Application:**

    ```bash
    dotnet run
    ```

    _The API will start, and the database will be automatically seeded with demo data (Doctors, Patients, etc)._

5.  **Explore Documentation:**
    Navigate to `https://localhost:7068/swagger` to test the 280+ endpoints.

---

## Documentation (Wiki)

For detailed architectural decisions, ERD diagrams, and logic explanation, please refer to the **[Project Wiki](../../wiki)**.

---

## Contributors

- **Sara Samara**
- **Shereen abubeeh**
- **Shahd Hanbali**
- **Aya Dwaikat**

---

_© 2025 HealthAid Project. All Rights Reserved._
