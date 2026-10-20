resource "azurerm_postgresql_flexible_server" "pg" {
  name                = "${var.prefix}-pg"
  resource_group_name = azurerm_resource_group.rg.name
  location            = var.location
  version             = "16"
  sku_name            = "B_Standard_B1ms"
  storage_mb          = 32768
  zone                = "1"

  authentication {
    active_directory_auth_enabled = true
    password_auth_enabled         = false
    tenant_id                     = data.azurerm_client_config.current.tenant_id
  }
}

resource "azurerm_postgresql_flexible_server_configuration" "extensions" {
  name      = "azure.extensions"
  server_id = azurerm_postgresql_flexible_server.pg.id
  value     = "VECTOR"
}

resource "azurerm_postgresql_flexible_server_database" "finai" {
  name      = "finai"
  server_id = azurerm_postgresql_flexible_server.pg.id
}

resource "azurerm_postgresql_flexible_server_active_directory_administrator" "app" {
  server_name         = azurerm_postgresql_flexible_server.pg.name
  resource_group_name = azurerm_resource_group.rg.name
  tenant_id           = data.azurerm_client_config.current.tenant_id
  object_id           = azurerm_user_assigned_identity.app.principal_id
  principal_name      = azurerm_user_assigned_identity.app.name
  principal_type      = "ServicePrincipal"
}

resource "azurerm_postgresql_flexible_server_firewall_rule" "azure" {
  name             = "allow-azure-services"
  server_id        = azurerm_postgresql_flexible_server.pg.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

resource "azurerm_redis_cache" "redis" {
  name                = "${var.prefix}-redis"
  resource_group_name = azurerm_resource_group.rg.name
  location            = var.location
  capacity            = 0
  family              = "C"
  sku_name            = "Basic"
  minimum_tls_version = "1.2"
  redis_configuration {
    active_directory_authentication_enabled = true
  }
}

resource "azurerm_redis_cache_access_policy_assignment" "app" {
  name               = "finai-app"
  redis_cache_id     = azurerm_redis_cache.redis.id
  access_policy_name = "Data Owner"
  object_id          = azurerm_user_assigned_identity.app.principal_id
  object_id_alias    = "finai-app"
}

resource "azurerm_eventhub_namespace" "bus" {
  name                         = "${var.prefix}-bus"
  resource_group_name          = azurerm_resource_group.rg.name
  location                     = var.location
  sku                          = "Standard"
  capacity                     = 1
  local_authentication_enabled = false
}

locals {
  topics = ["payment.created", "payment.cleared", "fraud.flagged", "fraud.case.resolved"]
}

resource "azurerm_eventhub" "topic" {
  for_each          = toset(concat(local.topics, [for t in local.topics : "${t}.dlt"]))
  name              = each.value
  namespace_id      = azurerm_eventhub_namespace.bus.id
  partition_count   = 4
  message_retention = 1
}

resource "azurerm_storage_account" "docs" {
  name                            = replace("${var.prefix}docs", "-", "")
  resource_group_name             = azurerm_resource_group.rg.name
  location                        = var.location
  account_tier                    = "Standard"
  account_replication_type        = "LRS"
  shared_access_key_enabled       = false
  allow_nested_items_to_be_public = false
}

resource "azurerm_key_vault" "kv" {
  name                      = "${var.prefix}-kv"
  resource_group_name       = azurerm_resource_group.rg.name
  location                  = var.location
  tenant_id                 = data.azurerm_client_config.current.tenant_id
  sku_name                  = "standard"
  enable_rbac_authorization = true
}

resource "azurerm_container_registry" "acr" {
  name                = replace("${var.prefix}acr", "-", "")
  resource_group_name = azurerm_resource_group.rg.name
  location            = var.location
  sku                 = "Basic"
  admin_enabled       = false
}
