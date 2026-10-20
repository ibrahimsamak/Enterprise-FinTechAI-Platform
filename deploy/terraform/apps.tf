resource "azurerm_container_app_environment" "env" {
  name                       = "${var.prefix}-env"
  resource_group_name        = azurerm_resource_group.rg.name
  location                   = var.location
  log_analytics_workspace_id = azurerm_log_analytics_workspace.logs.id
}

locals {
  common_env = {
    AZURE_CLIENT_ID                       = azurerm_user_assigned_identity.app.client_id
    AZURE__KEYVAULTURI                    = azurerm_key_vault.kv.vault_uri
    APPLICATIONINSIGHTS_CONNECTION_STRING = azurerm_application_insights.ai.connection_string
    ConnectionStrings__Postgres           = "Host=${azurerm_postgresql_flexible_server.pg.fqdn};Database=finai;Username=${azurerm_user_assigned_identity.app.name};SSL Mode=Require"
    ConnectionStrings__Redis              = "${azurerm_redis_cache.redis.hostname}:6380,ssl=true,abortConnect=false"
    Kafka__BootstrapServers               = "${azurerm_eventhub_namespace.bus.name}.servicebus.windows.net:9093"
    Kafka__Auth                           = "EntraId"
    AzureAd__TenantId                     = data.azurerm_client_config.current.tenant_id
    AzureAd__ClientId                     = var.entra_api_client_id
    AzureAd__Audience                     = "api://${var.entra_api_client_id}"
    AZURE__SIGNALR__ENDPOINT              = "https://${azurerm_signalr_service.signalr.hostname}"
    OPENAI__CHATMODEL                     = "gpt-4o"
    OPENAI__EMBEDDINGMODEL                = "text-embedding-3-small"
  }
}

resource "azurerm_container_app" "api" {
  name                         = "${var.prefix}-api"
  container_app_environment_id = azurerm_container_app_environment.env.id
  resource_group_name          = azurerm_resource_group.rg.name
  revision_mode                = "Single"

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.app.id]
  }
  registry {
    server   = azurerm_container_registry.acr.login_server
    identity = azurerm_user_assigned_identity.app.id
  }
  ingress {
    external_enabled = true
    target_port      = 8080
    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }
  template {
    min_replicas = 2
    max_replicas = 6
    container {
      name   = "api"
      image  = "${azurerm_container_registry.acr.login_server}/finai-api:${var.image_tag}"
      cpu    = 0.5
      memory = "1Gi"
      dynamic "env" {
        for_each = local.common_env
        content {
          name  = env.key
          value = env.value
        }
      }
      liveness_probe {
        transport = "HTTP"
        port      = 8080
        path      = "/health/live"
      }
      readiness_probe {
        transport = "HTTP"
        port      = 8080
        path      = "/health/ready"
      }
    }
    http_scale_rule {
      name                = "http"
      concurrent_requests = "50"
    }
  }
}

resource "azurerm_container_app" "workers" {
  name                         = "${var.prefix}-workers"
  container_app_environment_id = azurerm_container_app_environment.env.id
  resource_group_name          = azurerm_resource_group.rg.name
  revision_mode                = "Single"
  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.app.id]
  }
  registry {
    server   = azurerm_container_registry.acr.login_server
    identity = azurerm_user_assigned_identity.app.id
  }
  template {
    min_replicas = 1
    max_replicas = 4
    container {
      name   = "workers"
      image  = "${azurerm_container_registry.acr.login_server}/finai-workers:${var.image_tag}"
      cpu    = 1.0
      memory = "2Gi"
      dynamic "env" {
        for_each = local.common_env
        content {
          name  = env.key
          value = env.value
        }
      }
    }
  }
}

resource "azurerm_container_app_job" "migrator" {
  name                         = "${var.prefix}-migrator"
  container_app_environment_id = azurerm_container_app_environment.env.id
  resource_group_name          = azurerm_resource_group.rg.name
  location                     = var.location
  replica_timeout_in_seconds   = 600
  manual_trigger_config {
    parallelism              = 1
    replica_completion_count = 1
  }
  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.app.id]
  }
  registry {
    server   = azurerm_container_registry.acr.login_server
    identity = azurerm_user_assigned_identity.app.id
  }
  template {
    container {
      name   = "migrator"
      image  = "${azurerm_container_registry.acr.login_server}/finai-migrator:${var.image_tag}"
      cpu    = 0.5
      memory = "1Gi"
      env {
        name  = "AZURE_CLIENT_ID"
        value = azurerm_user_assigned_identity.app.client_id
      }
      env {
        name  = "ConnectionStrings__Postgres"
        value = local.common_env["ConnectionStrings__Postgres"]
      }
    }
  }
}
