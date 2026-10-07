---
doc_id: CMP-003
title: Sanctions Screening and Elevated-Risk Jurisdiction Policy
category: compliance
audience: internal
owner: Sanctions Compliance
version: "1.3"
effective_date: 2026-01-01
review_date: 2027-01-01
tags: [sanctions, screening, OFAC, SEMA, elevated-risk country, jurisdiction, FATF]
---

# Sanctions Screening and Elevated-Risk Jurisdiction Policy

## 1. Purpose

This policy ensures FinAI Bank does not process payments for sanctioned
persons, entities or countries, and applies extra controls to payments
involving elevated-risk jurisdictions.

## 2. Applicable sanctions lists

Customers, payees and payments are screened against:

- Canadian sanctions under the *Special Economic Measures Act* (SEMA), the
  *Justice for Victims of Corrupt Foreign Officials Act* and the *Criminal
  Code* listed entities.
- United Nations Security Council consolidated list.
- Other lists adopted by the bank's risk appetite (for example US OFAC and UK
  lists for correspondent-banking reasons).

## 3. When screening happens

- At customer onboarding and whenever customer details change.
- Every night against list updates (re-screening of the full customer base).
- On every international payment: the payee name, payee bank and destination
  country are screened before release.

## 4. Handling a potential match

1. The payment is held automatically.
2. Sanctions Compliance reviews the match within **4 business hours**.
3. A false positive is cleared with documented reasons.
4. A true match is frozen and reported as required by law. The payment is
   never released.

Fraud officers must not approve a payment that has an open sanctions alert.

## 5. Elevated-risk jurisdictions

Compliance maintains an internal **elevated-risk country list**, based on the
FATF "high-risk jurisdictions subject to a call for action", FATF "jurisdictions
under increased monitoring", Canadian ministerial directives and the bank's own
risk assessment. The list is reviewed after every FATF plenary.

Payments to or from a country on this list must:

- Be escalated to Compliance regardless of fraud score (FRD-001 §4.5).
- Have a documented business or personal purpose.
- Trigger enhanced due diligence when they are recurring (CMP-001 §4).
