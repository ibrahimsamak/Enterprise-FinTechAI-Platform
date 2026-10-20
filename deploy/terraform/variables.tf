variable "prefix" {
  description = "Short name prefixed to every resource (e.g. finai, finaidev). Keep it short and lowercase: it feeds globally-unique names for ACR, Storage and Postgres."
  type        = string

  validation {
    condition     = can(regex("^[a-z][a-z0-9-]{1,18}$", var.prefix))
    error_message = "prefix must be 2-19 chars, lowercase letters/digits/hyphens, starting with a letter."
  }
}

variable "location" {
  description = "Azure region for all resources, e.g. westeurope, eastus."
  type        = string
  default     = "westeurope"
}

variable "image_tag" {
  description = "Tag of the api/workers/migrator images in ACR. CI sets this to the git SHA. On the very first apply no image exists yet, so push images first or temporarily point the app images at a public placeholder."
  type        = string
  default     = "latest"
}

variable "entra_api_client_id" {
  description = "Client ID (GUID) of the Entra app registration for the API. Used as AzureAd__ClientId and the 'api://<id>' audience."
  type        = string
}

variable "github_repo" {
  description = "GitHub repository in 'owner/repo' form, used for the GitHub Actions federated credential subject."
  type        = string

  validation {
    condition     = can(regex("^[^/]+/[^/]+$", var.github_repo))
    error_message = "github_repo must be in 'owner/repo' form, e.g. ibrahimsamak/Enterprise-FinTechAI-Platform."
  }
}

variable "oncall_email" {
  description = "Email address that receives the monitoring alerts."
  type        = string
  default     = "oncall@example.com"
}
