---
doc_id: CMP-004
title: Record Keeping and Audit Trail Policy
category: compliance
audience: internal
owner: Compliance and Internal Audit
version: "1.1"
effective_date: 2026-01-01
review_date: 2027-01-01
tags: [audit, record keeping, retention, immutable, decision log, AI audit]
---

# Record Keeping and Audit Trail Policy

## 1. Purpose

This policy defines what FinAI Bank must record about transactions, fraud
cases and decisions, how long those records are kept, and how they are
protected from change.

## 2. What must be recorded

- Customer identification records and risk ratings (CMP-001).
- Every payment, including the fraud score and rule flags.
- Every fraud case: AI investigation summary, evidence, policy cited.
- Every human decision: who decided, when, the decision (Approve / Escalate)
  and the written notes.
- Every AI agent invocation: model, prompt version, tools called and their
  results, retrieved policy chunks, and the final output.
- Every regulatory report and its supporting analysis (CMP-002).

## 3. Immutability

Decision and audit records are **append-only**. A record is never edited or
deleted; a correction is made by adding a new record that references the
original. Access to the audit store is restricted to Compliance and Internal
Audit.

## 4. Retention periods

| Record | Minimum retention |
|--------|-------------------|
| Customer identification and account records | 5 years after the account is closed |
| Transaction records, fraud cases, decisions | 5 years from the date of the transaction |
| Regulatory reports (STR, LCTR, EFTR) | 5 years from filing |
| AI agent traces | 5 years when attached to a fraud case; 90 days otherwise |

## 5. Traceability

Each payment carries a trace identifier from the API request through Kafka,
the fraud worker, the AI agent and the officer's decision, so that the full
history of any case can be reconstructed end to end.
