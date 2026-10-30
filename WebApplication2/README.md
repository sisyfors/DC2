## How to Start Applications
Open Visual Studio 2022 or newer. Make sure to Configure the Startup Projects
and select WebApplication1. WebApplication1 needs to run first then WebApplication2.
When trying to run WebApplication1 and WebApplication2 you can launch the
http or https launch profile.

#### Ports
http port: http://localhost:5262
https port: https://localhost:7285

#### Where is the BaseUrl Configured
Its configured inside WebApplication2/appsettings.json

## Sign-In Details
For Customer:
For Administrator:
  Username: admin
  Password: Admin123!

## Changes from Part A
- Added a Role column to User
- In the DBSeeder, it creates 1 admin user with the administrator role
- Added new AuditEntry and new endpoints "api/audit"
- New AdminController with new endpoints like "api/users-list", "api/admin/users", etc
- Changed SearchByEmail() to search by to searchByAccountNumber() to be used in part b transaction searching
- Added Transfer Controller
- Added MoneyTransfer Model
- Added Password Change endpoint to user controller
- Aded GetAllUsers() to user controller
- Added Transaction Controller and associated endpoints inside


