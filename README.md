# University-Tuition-Api
University Tuition API – SE4458 Midterm Project

for the video: https://youtu.be/sRjQwHiE_Cs

for the photos: büyük sistem

1. Project Source Code
GitHub Repository:
https://github.com/dilagencaga/se4458-university-tuition-api

2. Project Description

This project is a university tuition management system built with .NET 8 Web API.
It contains two main components:

1️⃣ UniversityGateway
Acts as an API Gateway using Ocelot
Routes all requests to the backend services
Handles logging, request tracing, and lightweight filtering

2️⃣ UniversityTuitionApi
Core backend service
Supports:
Student management
Tuition record management
Payment operations
Admin operations
JWT-Based Authentication (Login/Register)

3. Design, Assumptions & Architecture
🧩 3.1 System Design Overview

The system follows a microservice-like layered design:

Client → UniversityGateway (Ocelot) → UniversityTuitionApi → Database (PostgreSQL)

🔹 UniversityGateway
Uses Ocelot configuration
Central entry point
Simplifies routes and reduces backend exposure
Logs every request + response
Can be extended for rate limiting, load balancing, or auth validation

🔹 UniversityTuitionApi (Main Service)
Layered architecture:
Controllers → Handle endpoints
Models → Entities such as Student, Payment, TuitionRecord
DTOs → Clean request/response models
Config → JWT configurations
Data → EF Core DbContext

🧰 Used Technologies
.NET 8 Web API
Ocelot Gateway
Entity Framework Core
PostgreSQL
RESTful API principles
JWT Authentication
Swagger UI

4. Assumptions

Bu projeyi tasarlarken aşağıdaki varsayımlar kabul edilmiştir:
Her öğrencinin birden fazla ödeme kaydı olabilir.
Admin role tüm CRUD işlemlerine erişebilir.
Authentication için JWT Token kullanılır ve token her istek için header üzerinden gönderilir.
Ödemeler sadece "successful" olarak kaydedilir — geri ödeme veya provizyon işlenmez.
Gateway yalnızca backend’e yönlendirme yapar, iş kuralı içermez.
Veri modeli yalnızca ödev kapsamında gereksinim duyulan alanlarla sınırlıdır.

5. Issues Encountered & Solutions
   
❗ Issue 1 — Git/GitHub conflict
Problem: Local repo ve GitHub’daki eski repo arasında çakışma oldu.
Çözüm: Repo yeniden başlatıldı, git push --force ile temiz kurulum yapıldı.

❗ Issue 2 — Project folder misalignment
Problem: Proje dosyaları yanlış klasör altına karıştı.
Çözüm: Solution Explorer’dan projeler kaldırıldı, dosyalar yeniden taşındı ve .csproj tekrar eklendi.

❗ Issue 3 — Swagger not starting
Problem: Gateway ayağa kalktı ama API açılmadı.
Çözüm: UniversityTuitionApi projesi StartUp olarak seçildi + HTTPS yönlendirmesi düzenlendi.

❗ Issue 4 — Ocelot configuration error
Problem: Yanlış path eşlemeleri → 404 döndü.
Çözüm: ocelot.json manual düzenlenip doğru downstream portları yazıldı.

6. Data Model (ER Diagram)

Aşağıdaki ER diyagramı proje veri modelini gösterir:

+------------------+         +---------------------+
|     Student      | 1     ∞ |    TuitionRecord    |
+------------------+---------+---------------------+
| Id (PK)          |         | Id (PK)             |
| FirstName        |         | StudentId (FK)      |
| LastName         |         | Amount              |
| Email            |         | Semester            |
| Phone            |         | Status              |
+------------------+         +---------------------+

                  ∞
                  |
                  |

+------------------+         +---------------------+
|     Payment      |   ∞   1 |    TuitionRecord    |
+------------------+---------+---------------------+
| Id (PK)          |
| TuitionRecordId  |
| Amount           |
| Date             |
+------------------+


7. API Endpoints Summary
🔐 Authentication
Method	Endpoint	Description
POST	/auth/login	Login & get JWT token
POST	/auth/register	Register new admin or user
🎓 Students
Method	Endpoint	Description
GET	/students	List students
POST	/students	Add student
PUT	/students/{id}	Update student
DELETE	/students/{id}	Remove student
💳 Tuition Records
Method	Endpoint	Description
GET	/tuition	List all
POST	/tuition	Add record
PUT	/tuition/{id}	Update record
💰 Payments
Method	Endpoint	Description
GET	/payments	List all
POST	/payments	Add payment

9. Swagger Documentation

Swagger UI automatically loads at:

➡ https://localhost:7243/swagger/index.html
