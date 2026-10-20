resource "azurerm_log_analytics_workspace" "logs" {
  name                = "${var.prefix}-logs"
  resource_group_name = azurerm_resource_group.rg.name
  location            = var.location
  sku                 = "PerGB2018"
  retention_in_days   = 30
}

resource "azurerm_application_insights" "ai" {
  name                = "${var.prefix}-ai"
  resource_group_name = azurerm_resource_group.rg.name
  location            = var.location
  workspace_id        = azurerm_log_analytics_workspace.logs.id
  application_type    = "web"
}

resource "azurerm_monitor_action_group" "oncall" {
  name                = "${var.prefix}-oncall"
  resource_group_name = azurerm_resource_group.rg.name
  short_name          = "finaialert"

  email_receiver {
    name          = "oncall-email"
    email_address = var.oncall_email
  }
}

resource "azurerm_monitor_scheduled_query_rules_alert_v2" "dead_letters" {
  name                 = "${var.prefix}-dead-letters"
  resource_group_name  = azurerm_resource_group.rg.name
  location             = var.location
  scopes               = [azurerm_application_insights.ai.id]
  severity             = 2
  evaluation_frequency = "PT5M"
  window_duration      = "PT15M"
  criteria {
    query                   = <<-KQL
      dependencies | where name startswith "publish" and name endswith ".dlt"
    KQL
    time_aggregation_method = "Count"
    operator                = "GreaterThan"
    threshold               = 0
  }
  action { action_groups = [azurerm_monitor_action_group.oncall.id] }
}

resource "azurerm_monitor_scheduled_query_rules_alert_v2" "error_rate" {
  name                 = "${var.prefix}-error-rate"
  resource_group_name  = azurerm_resource_group.rg.name
  location             = var.location
  scopes               = [azurerm_application_insights.ai.id]
  severity             = 1
  evaluation_frequency = "PT5M"
  window_duration      = "PT15M"
  criteria {
    query                   = <<-KQL
      requests
      | summarize failed = countif(success == false), total = count() by bin(timestamp, 5m)
      | where total > 0 and todouble(failed) / total > 0.02
    KQL
    time_aggregation_method = "Count"
    operator                = "GreaterThan"
    threshold               = 0
  }
  action { action_groups = [azurerm_monitor_action_group.oncall.id] }
}

resource "azurerm_monitor_scheduled_query_rules_alert_v2" "agent_latency" {
  name                 = "${var.prefix}-agent-latency"
  resource_group_name  = azurerm_resource_group.rg.name
  location             = var.location
  scopes               = [azurerm_application_insights.ai.id]
  severity             = 2
  evaluation_frequency = "PT15M"
  window_duration      = "PT15M"
  criteria {
    query                   = <<-KQL
      dependencies
      | where name in ("fraud agent investigate", "support agent answer")
      | summarize p95 = percentile(duration, 95) by bin(timestamp, 15m)
      | where p95 > 20000
    KQL
    time_aggregation_method = "Count"
    operator                = "GreaterThan"
    threshold               = 0
  }
  action { action_groups = [azurerm_monitor_action_group.oncall.id] }
}

resource "azurerm_monitor_scheduled_query_rules_alert_v2" "sla_breaches" {
  name                 = "${var.prefix}-sla-breaches"
  resource_group_name  = azurerm_resource_group.rg.name
  location             = var.location
  scopes               = [azurerm_application_insights.ai.id]
  severity             = 2
  evaluation_frequency = "PT15M"
  window_duration      = "PT15M"
  criteria {
    query                   = <<-KQL
      customMetrics
      | where name == "finai.fraud.sla_breaches"
      | summarize total = sum(valueSum) by bin(timestamp, 15m)
      | where total > 0
    KQL
    time_aggregation_method = "Count"
    operator                = "GreaterThan"
    threshold               = 0
  }
  action { action_groups = [azurerm_monitor_action_group.oncall.id] }
}
