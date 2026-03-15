1- Entra a la carpeta del frontend desde la consola de vsc o powershell como admin en caso de no funcionar
cd Front-DashClass

2- Instala las dependencias
npm install

3- Corre el proyecto
npm run dev

4- Abre el navegador en: (aplican excepciones)
http://localhost:5173

COSAS DE appsettings.Development.json

{
"ConnectionStrings": {
"DefaultConnection": "Server=localhost;Port=3306;Database=lms_gamificacion;User=root;Password=Milaneza12345;"
},
"Logging": {
"LogLevel": {
"Default": "Information",
"Microsoft.AspNetCore": "Warning",
"Microsoft.EntityFrameworkCore.Database.Command": "Information"
}
},
"CorsSettings": {
"AllowedOrigins": [
"http://localhost:3000",
"http://localhost:5173",
"https://localhost:5173"
]
},
"ResendSettings": {
"ApiKey": "re_WsJamkoc_BijMjEs34Roh5xYCSPcvEGaG",
"FromEmail": "onboarding@resend.dev",
"FromName": "DashClass"
},
"AuthSettings": {
"Enable2FA": false,
"EnableEmailVerification": false
}
}

COSAS DE appsettings.json

{
"ConnectionStrings": {
"DefaultConnection": "Server=localhost;Port=3306;Database=lms_gamificacion;User=PLACEHOLDER_USER;Password=PLACEHOLDER_PASSWORD;CharSet=utf8mb4;"
},

"Logging": {
"LogLevel": {
"Default": "Information",
"Microsoft.AspNetCore": "Warning",
"Microsoft.EntityFrameworkCore": "Information"
}
},

"AllowedHosts": "\*",

"CorsSettings": {
"AllowedOrigins": [
"http://localhost:3000",
"http://localhost:5173"
]
},

"JwtSettings": {
"Secret": "PLACEHOLDER_JWT_SECRET_KEY_MIN_32_CHARACTERS",
"Issuer": "API-DashClass",
"Audience": "DashClass-Users",
"ExpirationMinutes": 60,
"RefreshTokenExpirationDays": 7
},

"GoogleOAuth": {
"ClientId": "PLACEHOLDER_GOOGLE_CLIENT_ID.apps.googleusercontent.com",
"ClientSecret": "PLACEHOLDER_GOOGLE_CLIENT_SECRET"
},

"FacebookOAuth": {
"AppId": "PLACEHOLDER_FACEBOOK_APP_ID",
"AppSecret": "PLACEHOLDER_FACEBOOK_APP_SECRET"
},

"MicrosoftOAuth": {
"ClientId": "PLACEHOLDER_MICROSOFT_CLIENT_ID",
"ClientSecret": "PLACEHOLDER_MICROSOFT_CLIENT_SECRET",
"TenantId": "common"
},

"RateLimiting": {
"EnableRateLimiting": true,
"LoginAttempts": {
"PermitLimit": 5,
"WindowMinutes": 15
},
"GlobalRequests": {
"PermitLimit": 100,
"WindowMinutes": 1
}
},

"FileStorage": {
"StorageType": "Local",
"LocalPath": "./uploads",
"MaxFileSizeMB": 10,
"AllowedExtensions": [".jpg", ".jpeg", ".png", ".pdf", ".docx", ".xlsx", ".pptx"]
},

"EmailSettings": {
"EnableEmail": false,
"SmtpServer": "smtp.gmail.com",
"SmtpPort": 587,
"SenderEmail": "noreply@dashclass.com",
"SenderName": "DashClass LMS",
"Username": "PLACEHOLDER_EMAIL_USERNAME",
"Password": "PLACEHOLDER_EMAIL_PASSWORD",
"EnableSsl": true
},

"QRCodeSettings": {
"DefaultIntervalSeconds": 10,
"MinIntervalSeconds": 5,
"MaxIntervalSeconds": 60,
"SecretKeyLength": 32,
"VerificationCodeLength": 6
},

"GamificationSettings": {
"DefaultPointsPerActivity": 50,
"MaxPointsPerActivity": 1000,
"PointsPerAttendance": 5,
"EnableTransfers": true,
"TransferCooldownMinutes": 60
}
}
