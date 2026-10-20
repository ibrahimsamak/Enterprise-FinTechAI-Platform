resource "azurerm_static_web_app" "customer" {
  name                = "${var.prefix}-customer"
  resource_group_name = azurerm_resource_group.rg.name
  location            = "westeurope"
}

resource "azurerm_static_web_app" "operations" {
  name                = "${var.prefix}-operations"
  resource_group_name = azurerm_resource_group.rg.name
  location            = "westeurope"
}

resource "azurerm_cdn_frontdoor_profile" "fd" {
  name                = "${var.prefix}-fd"
  resource_group_name = azurerm_resource_group.rg.name
  sku_name            = "Standard_AzureFrontDoor"
}

resource "azurerm_cdn_frontdoor_endpoint" "customer" {
  name                     = "${var.prefix}-customer"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.fd.id
}

resource "azurerm_cdn_frontdoor_endpoint" "operations" {
  name                     = "${var.prefix}-operations"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.fd.id
}

resource "azurerm_cdn_frontdoor_origin_group" "api" {
  name                     = "api"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.fd.id
  load_balancing {}
  health_probe {
    path                = "/health/ready"
    protocol            = "Https"
    interval_in_seconds = 30
    request_type        = "GET"
  }
}

resource "azurerm_cdn_frontdoor_origin" "api" {
  name                           = "api"
  cdn_frontdoor_origin_group_id  = azurerm_cdn_frontdoor_origin_group.api.id
  enabled                        = true
  host_name                      = azurerm_container_app.api.ingress[0].fqdn
  origin_host_header             = azurerm_container_app.api.ingress[0].fqdn
  https_port                     = 443
  http_port                      = 80
  priority                       = 1
  weight                         = 1000
  certificate_name_check_enabled = true
}

resource "azurerm_cdn_frontdoor_origin_group" "customer_spa" {
  name                     = "customer-spa"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.fd.id
  load_balancing {}
}

resource "azurerm_cdn_frontdoor_origin" "customer_spa" {
  name                           = "customer-spa"
  cdn_frontdoor_origin_group_id  = azurerm_cdn_frontdoor_origin_group.customer_spa.id
  enabled                        = true
  host_name                      = azurerm_static_web_app.customer.default_host_name
  origin_host_header             = azurerm_static_web_app.customer.default_host_name
  https_port                     = 443
  http_port                      = 80
  priority                       = 1
  weight                         = 1000
  certificate_name_check_enabled = true
}

resource "azurerm_cdn_frontdoor_origin_group" "operations_spa" {
  name                     = "operations-spa"
  cdn_frontdoor_profile_id = azurerm_cdn_frontdoor_profile.fd.id
  load_balancing {}
}

resource "azurerm_cdn_frontdoor_origin" "operations_spa" {
  name                           = "operations-spa"
  cdn_frontdoor_origin_group_id  = azurerm_cdn_frontdoor_origin_group.operations_spa.id
  enabled                        = true
  host_name                      = azurerm_static_web_app.operations.default_host_name
  origin_host_header             = azurerm_static_web_app.operations.default_host_name
  https_port                     = 443
  http_port                      = 80
  priority                       = 1
  weight                         = 1000
  certificate_name_check_enabled = true
}

resource "azurerm_cdn_frontdoor_route" "customer_api" {
  name                          = "customer-api"
  cdn_frontdoor_endpoint_id     = azurerm_cdn_frontdoor_endpoint.customer.id
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.api.id
  cdn_frontdoor_origin_ids      = [azurerm_cdn_frontdoor_origin.api.id]
  patterns_to_match             = ["/payments", "/payments/*", "/customers", "/customers/*", "/hubs/*", "/signin-oidc"]
  supported_protocols           = ["Https"]
  forwarding_protocol           = "HttpsOnly"
}

resource "azurerm_cdn_frontdoor_route" "customer_spa" {
  name                          = "customer-spa"
  cdn_frontdoor_endpoint_id     = azurerm_cdn_frontdoor_endpoint.customer.id
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.customer_spa.id
  cdn_frontdoor_origin_ids      = [azurerm_cdn_frontdoor_origin.customer_spa.id]
  patterns_to_match             = ["/*"]
  supported_protocols           = ["Https"]
  forwarding_protocol           = "HttpsOnly"
}

resource "azurerm_cdn_frontdoor_route" "operations_api" {
  name                          = "operations-api"
  cdn_frontdoor_endpoint_id     = azurerm_cdn_frontdoor_endpoint.operations.id
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.api.id
  cdn_frontdoor_origin_ids      = [azurerm_cdn_frontdoor_origin.api.id]
  patterns_to_match             = ["/fraud-cases/*", "/graphql", "/hubs/*", "/hangfire", "/hangfire/*", "/signin-oidc", "/signout-callback-oidc"]
  supported_protocols           = ["Https"]
  forwarding_protocol           = "HttpsOnly"
}

resource "azurerm_cdn_frontdoor_route" "operations_spa" {
  name                          = "operations-spa"
  cdn_frontdoor_endpoint_id     = azurerm_cdn_frontdoor_endpoint.operations.id
  cdn_frontdoor_origin_group_id = azurerm_cdn_frontdoor_origin_group.operations_spa.id
  cdn_frontdoor_origin_ids      = [azurerm_cdn_frontdoor_origin.operations_spa.id]
  patterns_to_match             = ["/*"]
  supported_protocols           = ["Https"]
  forwarding_protocol           = "HttpsOnly"
}
