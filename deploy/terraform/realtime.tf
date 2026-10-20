resource "azurerm_signalr_service" "signalr" {
  name                = "${var.prefix}-signalr"
  resource_group_name = azurerm_resource_group.rg.name
  location            = var.location
  service_mode        = "Default"
  local_auth_enabled  = false
  sku {
    name     = "Standard_S1"
    capacity = 1
  }
}
