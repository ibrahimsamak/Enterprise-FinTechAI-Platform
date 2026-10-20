resource "azurerm_resource_group" "rg" {
  name     = "${var.prefix}-rg"
  location = var.location
}

resource "azurerm_user_assigned_identity" "app" {
  name                = "${var.prefix}-app-id"
  resource_group_name = azurerm_resource_group.rg.name
  location            = var.location
}

locals {
  app_roles = {
    acr_pull   = { scope = azurerm_container_registry.acr.id, role = "AcrPull" }
    kv_secrets = { scope = azurerm_key_vault.kv.id, role = "Key Vault Secrets User" }
    eventhubs  = { scope = azurerm_eventhub_namespace.bus.id, role = "Azure Event Hubs Data Owner" }
    blob       = { scope = azurerm_storage_account.docs.id, role = "Storage Blob Data Contributor" }
    signalr    = { scope = azurerm_signalr_service.signalr.id, role = "SignalR App Server" }
  }
}

resource "azurerm_role_assignment" "app" {
  for_each             = local.app_roles
  scope                = each.value.scope
  role_definition_name = each.value.role
  principal_id         = azurerm_user_assigned_identity.app.principal_id
}