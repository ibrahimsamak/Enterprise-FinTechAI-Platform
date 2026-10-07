---
doc_id: FRD-002
title: Fraud Risk Indicators and Scoring Standard
category: fraud
audience: internal
owner: Fraud Risk Management
version: "1.4"
effective_date: 2026-01-15
review_date: 2027-01-15
tags: [risk score, rules engine, indicators, thresholds, fraud flag, risk tier]
---

# Fraud Risk Indicators and Scoring Standard

## 1. Purpose

This standard describes how the automated fraud rules engine scores outbound
payments, which risk tier each score maps to, and which additional indicators
the AI Fraud Agent and fraud officers must consider during an investigation.

## 2. Rule-based indicators

The rules engine evaluates three indicators on every outbound payment:

| Rule | Indicator | Condition | Reason text shown to officers |
|------|-----------|-----------|-------------------------------|
| R1 | High amount | Amount ≥ 5,000 | "Amount significantly above typical." |
| R2 | Unusual destination | Destination country is not `CA` | "Unusual destination: {country}." |
| R3 | Unusual time | Transaction hour between 0 and 5 (00:00–05:59) | "Unusual transaction time." |

## 3. Score calculation and risk tiers

The fraud score is the number of triggered rules divided by three.

| Rules triggered | Score | Risk tier | Outcome |
|-----------------|-------|-----------|---------|
| 0 | 0.00 | LOW | Payment proceeds, no case. |
| 1 | 0.33 | MEDIUM | Payment proceeds; logged for trend monitoring. |
| 2 | 0.67 | HIGH | Fraud case opened, `fraud.flagged` event published, AI investigation, officer review. |
| 3 | 1.00 | HIGH (critical) | As above, plus mandatory customer contact (FRD-001 §4.3). |

A payment is HIGH risk when its score is **0.66 or higher**, i.e. two or more
rules triggered.

## 4. Behavioural indicators for investigation

The rules engine is deliberately simple. During investigation, the AI Fraud
Agent and fraud officer must also weigh these indicators, gathered through the
authorized tools (`GetTransactionHistory`, `GetRecentLocations`,
`GetDeviceInfo`, `GetCustomerProfile`):

### 4.1 Indicators that increase risk

- Payment amount more than **3×** the customer's largest payment in the last
  90 days.
- First payment ever to this payee or to this destination country.
- Destination country not among the customer's recent locations.
- Login from a new device or new IP geolocation in the 24 hours before the
  payment (see FRD-004).
- Password, phone number or email changed in the 7 days before the payment.
- Several payments in quick succession (velocity): 3 or more outbound payments
  within 1 hour.
- Account balance drained: payment uses more than 80% of the available balance.
- Account opened less than 30 days ago.

### 4.2 Indicators that reduce risk

- Customer has paid the same payee before without dispute.
- Customer has a history of payments to the same country (for example, regular
  family remittances).
- Customer recently travelled to, or lives part-time in, the destination
  country according to their recent locations.
- The customer proactively notified the bank of the payment.

## 5. Writing the risk assessment

An investigation summary must state: the rule flags that fired, each
behavioural indicator found (with the evidence it came from), the indicators
checked and **not** found, and an overall assessment. Unsupported claims must
not be included - if a tool returned no data, say so.

## 6. Tuning

Thresholds in §2 are reviewed quarterly by Fraud Risk Management against false
positive and loss rates. Changes require a new version of this standard and a
matching change to the rules engine.
