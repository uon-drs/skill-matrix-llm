using './main.bicep'

param appBaseName = 'skill-matrix-llm'
param environment = 'uat'

param aspSkuName = 'B2'

param postgresSkuName = 'Standard_B2ms'
param postgresSkuTier = 'Burstable'
param postgresStorageSizeGB = 64
param postgresAdminLogin = 'skill-matrix-llm-admin'

param keycloakAuthority = 'https://keycloak.example.com/realms/skill-matrix-llm'
param keycloakApiAudience = 'skill-matrix-llm-api'
param keycloakFrontendClientId = 'skill-matrix-llm-frontend'
param keycloakIssuerUrl = 'https://keycloak.example.com/realms/skill-matrix-llm'

param logRetentionDays = 60

param enableZoneRedundancy = false
param enableVnet = false
