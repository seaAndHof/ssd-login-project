# Subject: Designing a Centralized Login Portal with Zero Trust, OAuth2, and MFA

**Course:** Secure Software Development

**Students :** Nicklas Kramer & Anders Hoffmann 

---

## First Run
To run this project the first time, you wont get it working without setting up the database.

Run the 'dotnet ef database update' command to get the database up and running.

## 1. Introduction and problem

## 2. Theory

### 2.1 Federated authentication: OAuth2 and OpenID Connect

### 2.2 Cryptographic primitives: password hashing and JWT signing
This project uses EF Core identity to manage users. EF Core Identity automatically hashses any passwords using
HMAC-SHA256 and uses a 128 bit random salt. This way no passwords are ever stored in plain text, and password sent
through HTTPS are encrypted.

Any authenticated user gets sent a JWT token with claims through the HTTPS respone to their browser,
which is stored as a cookie named "jwt". This cookie expires after 1 hour, prompting the user to log in again.

### 2.3 Zero Trust architecture

### 2.4 Multi-Factor Authentication (MFA/2FA)

### 2.5 Role-Based Access Control (RBAC)

This project is using Role-Based Access Control to dictate which roles has access to certain areas of the system.
In this project this is done using claims that exist within the JWT, which the frontend can read and see which role
that the authenticated user has.

Any requests to the backend are also being role checked and authorizing whether or not the user has permission to
access the attempted endpoint. Access denied is shown with a 403 Forbidden error.

### 2.6 Threat modeling the login flow (STRIDE)

### 2.7 Discussion points for exam

## 3. Practical work

### 3.1 The login portal

### 3.2 OAuth2/OIDC SSO flow

### 3.3 Password hashing and JWT issuance

### 3.4 MFA enrollment and verification

### 3.5 RBAC enforcement

### 3.6 Countermeasures against session hijacking

## 4. Conclusion

## 5. Sources
