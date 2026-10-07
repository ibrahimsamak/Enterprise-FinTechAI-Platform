---
doc_id: FRD-001
title: High-Value International Transfer Fraud Policy
category: fraud
audience: internal
owner: Fraud Risk Management
version: "2.1"
effective_date: 2026-01-15
review_date: 2027-01-15
tags: [international transfer, wire, high value, fraud, outbound payment, review]
---

# High-Value International Transfer Fraud Policy

## 1. Purpose

This policy defines how FinAI Bank detects, reviews and decides on
high-value outbound international transfers that may be fraudulent. It applies
to every outbound payment whose destination country is outside Canada (`CA`),
whatever the channel (online banking, mobile app, API, branch).

## 2. Definitions

- **High-value transfer**: a single outbound payment of **CAD 5,000 or more**
  (or the equivalent in another currency at the bank's posted rate).
- **International transfer**: a payment whose destination country is not
  Canada (`CA`).
- **Unusual hours**: a payment initiated between **00:00 and 05:59** (transaction
  timestamp hour 0–5).
- **Flagged payment**: a payment the automated rules engine scores as high
  risk (see FRD-002 §3).
- **Fraud officer**: a member of the Fraud Operations team with the
  `fraud-officer` role who may approve or escalate flagged payments.

## 3. Why high-value international transfers carry elevated risk

High-value international transfers are the most common cash-out channel for
account takeover, business email compromise and authorized push payment scams
(see FRD-005). Once funds leave Canada, recovery through correspondent banks is
slow and rarely succeeds. For this reason FinAI Bank applies extra scrutiny
before such a payment is released.

## 4. Policy statements

### 4.1 Automated screening

Every outbound payment is screened by the fraud rules engine when the
`payment.created` event is consumed. A payment that meets two or more of the
following indicators is classified **HIGH risk** and a fraud case is opened:

1. Amount of CAD 5,000 or more.
2. Destination country outside Canada.
3. Initiated during unusual hours (00:00–05:59).

### 4.2 Investigation before release

A HIGH-risk international transfer must not be treated as cleared until a fraud
officer has reviewed the case. The AI Fraud Agent prepares an investigation
summary (FRD-003 §4); the officer makes the decision.

### 4.3 Combined indicators

When **all three** indicators are present (for example CAD 9,500 to Germany at
02:43), the fraud officer must attempt to contact the customer through a
verified channel (phone number on file, not a number provided in the payment
instructions) before approving.

### 4.4 First-time payees

A high-value international payment to a payee the customer has never paid
before is an additional risk indicator. The officer must confirm the payee
relationship with the customer before approval.

### 4.5 Elevated-risk jurisdictions

Payments to jurisdictions on the bank's elevated-risk country list (maintained
by Compliance, see CMP-003 §5) must be escalated to Compliance regardless of
the fraud score.

## 5. Decisions available to the fraud officer

| Decision | When to use |
|----------|-------------|
| **Approve** | Evidence supports a legitimate payment (customer confirmed, consistent history, known payee). |
| **Escalate** | Evidence is inconclusive, the customer cannot be reached, signs of account takeover or scam, or any money-laundering concern. Escalation sends the case to Compliance (CMP-002). |

Every decision must include written notes and is stored as an immutable audit
record (CMP-004).

## 6. Service levels

- HIGH-risk cases must be picked up within **15 minutes** during staffed hours
  and within **1 hour** outside staffed hours.
- Cases with all three indicators are prioritized above other HIGH-risk cases.

## 7. Customer communication

Fraud officers must never tell a customer that a payment was flagged by a
specific rule or threshold. Use the approved wording: *"We hold some payments
for an additional security review to protect your account."* Never disclose
that a suspicious transaction report is being considered (see CMP-002 §6,
tipping-off).
