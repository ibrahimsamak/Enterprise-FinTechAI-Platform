---
doc_id: FRD-003
title: Fraud Case Investigation and Resolution Procedure
category: fraud
audience: internal
owner: Fraud Operations
version: "1.2"
effective_date: 2026-02-01
review_date: 2027-02-01
tags: [fraud case, investigation, human in the loop, approve, escalate, case summary, procedure]
---

# Fraud Case Investigation and Resolution Procedure

## 1. Purpose

This procedure describes the life cycle of a fraud case at FinAI Bank, from the
moment the rules engine flags a payment to the fraud officer's final decision.
It also defines the format of the AI Fraud Agent's investigation summary.

## 2. Case life cycle

1. **Detection**: the rules engine scores the payment HIGH risk (FRD-002 §3).
2. **Case opened**: a fraud case is created for the payment and a
   `fraud.flagged` event is published.
3. **AI investigation**: the AI Fraud Agent gathers evidence through authorized
   tools, retrieves relevant policy and writes a case summary with a
   recommended action.
4. **Alert**: the case appears live in the Operations Portal for users with the
   `fraud-officer` role.
5. **Human review**: a fraud officer reviews the evidence, contacts the
   customer where required, and decides **Approve** or **Escalate**.
6. **Audit**: the decision, notes, officer identity and timestamp are written
   as an immutable audit record (CMP-004).

## 3. Roles and responsibilities

| Role | Responsibility |
|------|----------------|
| Rules engine | Scores every outbound payment; opens cases for HIGH risk. |
| AI Fraud Agent | Gathers evidence, cites policy, **recommends** an action. It never approves, blocks or reverses a payment. |
| Fraud officer | Makes and records the decision. Accountable for the outcome. |
| Compliance | Receives escalated cases; decides on suspicious transaction reporting (CMP-002). |

## 4. AI investigation summary format

The AI Fraud Agent's summary must follow this structure:

1. **Transaction**: amount, currency, destination country, timestamp.
2. **Rule flags**: the rules that fired, quoted from the rules engine.
3. **Evidence gathered**: what each tool returned (transaction history,
   recent locations, device information, profile changes). State explicitly
   when a tool returned nothing.
4. **Risk indicators**: indicators found and not found, per FRD-002 §4.
5. **Policy basis**: the policy sections relied on, cited as `doc_id §section`
   (for example *FRD-001 §4.3*).
6. **Recommended action**: `APPROVE` or `ESCALATE`, plus required next steps
   (for example "contact customer on file number before approval").
7. **Confidence**: LOW, MEDIUM or HIGH, with a one-line reason.

The summary must not include policy that was not retrieved, and must not
include customer data beyond what the tools returned.

## 5. Officer review checklist

Before deciding, the fraud officer confirms:

- [ ] The AI summary's evidence matches the transaction details on screen.
- [ ] Customer contact attempted through a verified channel when required
      (FRD-001 §4.3, §4.4).
- [ ] No signs of account takeover (FRD-004 §3).
- [ ] No signs of a scam where the customer is being coached (FRD-005).
- [ ] Destination is not on the elevated-risk list (CMP-003 §5).

## 6. Decision guidance

### 6.1 Approve

Approve when the customer confirms the payment through a verified channel and
the evidence is consistent with their history, or when strong risk-reducing
indicators (FRD-002 §4.2) outweigh the rule flags.

### 6.2 Escalate

Escalate when any of the following apply:

- The customer cannot be reached within the service level.
- The customer denies making the payment (account takeover - also follow
  FRD-004 §4).
- The customer confirms the payment but shows signs of being scammed.
- There is any indication of money laundering, structuring or sanctions
  exposure.
- The officer disagrees with the AI recommendation and the evidence is
  inconclusive.

## 7. Disagreeing with the AI

The AI recommendation is advisory. An officer may decide against it, but must
record why in the decision notes. Disagreements are sampled weekly to improve
the agent's prompts and evaluation set.
