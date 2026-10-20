output "acr_login_server" {
  description = "Registry to docker push the api/workers/migrator images to."
  value       = azurerm_container_registry.acr.login_server
}

output "api_fqdn" {
  description = "Direct hostname of the API container app (behind Front Door in normal use)."
  value       = azurerm_container_app.api.ingress[0].fqdn
}

output "customer_url" {
  description = "Public customer portal URL (Front Door)."
  value       = "https://${azurerm_cdn_frontdoor_endpoint.customer.host_name}"
}

output "operations_url" {
  description = "Public operations portal URL (Front Door)."
  value       = "https://${azurerm_cdn_frontdoor_endpoint.operations.host_name}"
}

output "customer_swa_api_key" {
  description = "Deployment token for the customer Static Web App (swa deploy / CI)."
  value       = azurerm_static_web_app.customer.api_key
  sensitive   = true
}

output "operations_swa_api_key" {
  description = "Deployment token for the operations Static Web App."
  value       = azurerm_static_web_app.operations.api_key
  sensitive   = true
}

output "app_identity_client_id" {
  description = "Client id of the app managed identity (AZURE_CLIENT_ID in the container apps)."
  value       = azurerm_user_assigned_identity.app.client_id
}
