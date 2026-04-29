using './main.bicep'

param appBaseName = 'skill-matrix-llm'
param environment = 'prod'

// Use a production-grade App Service Plan SKU (supports VNet Integration)
param aspSkuName = 'P1v3'

// PostgreSQL — larger SKU, zone-redundant HA
param postgresSkuName = 'Standard_D2s_v3'
param postgresSkuTier = 'GeneralPurpose'
param postgresStorageSizeGB = 128
param postgresAdminLogin = 'skill-matrix-llm-admin'

param keycloakAuthority = 'https://keycloak.example.com/realms/skill-matrix-llm'
param keycloakApiAudience = 'skill-matrix-llm-api'
param keycloakFrontendClientId = 'skill-matrix-llm-frontend'
param keycloakIssuerUrl = 'https://keycloak.example.com/realms/skill-matrix-llm'

param logRetentionDays = 90

// Enable zone-redundant HA and VNet integration for production
param enableZoneRedundancy = true
param enableVnet = true
