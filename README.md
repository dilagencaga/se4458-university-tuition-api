# AddressBookApi
# AddressBookApi – SE4458 Assignment 2

This project is a simple RESTful API built using ASP.NET Core 8 for managing an Address Book.

## 📘 Features
- CRUD operations for contacts (Create, Read, Update, Delete)
- Search contacts by name, email, or tag
- In-memory data seeding with sample contacts
- Swagger UI documentation
- Dockerized and deployed on Render

## 🚀 Live Deployment
[View Swagger UI](https://addressbookapi-b0hd.onrender.com/swagger)

## 🧠 Design & Assumptions
- The API uses an **in-memory data store** (no real database) for simplicity.
- A `ContactsController` handles all endpoints under `/api/Contacts`.
- Models and DTOs are separated for cleaner architecture.

## ⚙️ Tools & Technologies
- ASP.NET Core 8
- C#
- Docker
- Render for deployment
- Swagger / OpenAPI

## 🧩 Example Data
- Alan Turing — Work  
- Marie Curie — Family  
- Katherine Johnson — Phone  
- Charles Darwin — Tag

## 🧑‍💻 Issues Encountered
- Deployment initially failed due to missing `Dockerfile`.
- Solved by placing Dockerfile in project root and updating Render path.

## 🔗 Repository
[GitHub Repository](https://github.com/dilagencaga/AddressBookApi)
