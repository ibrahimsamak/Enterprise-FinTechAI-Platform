---
doc_id: FRD-004
title: Account Takeover and Device Anomaly Procedure
category: fraud
audience: internal
owner: Fraud Operations
version: "1.0"
effective_date: 2026-02-01
review_date: 2027-02-01
tags: [account takeover, ATO, device, login, credential, SIM swap, compromised account]
---

# Account Takeover and Device Anomaly Procedure

## 1. Purpose

This procedure explains how to recognize and respond to account takeover (ATO):
a criminal gaining control of a customer's online banking credentials and
moving money out of the account.

## 2. How account takeover usually happens

- Phishing emails or texts imitating FinAI Bank that capture login details.
- Credential stuffing: passwords reused from breaches on other websites.
- SIM swap: the fraudster ports the customer's phone number to receive one-time
  passcodes.
- Malware or remote-access tools installed on the customer's device, often
  after a fake "tech support" call.

## 3. Account takeover indicators

When investigating a flagged payment, check device and profile activity for:

- Login from a device never seen before on the account.
- Login IP geolocation in a different country or province from the customer's
  recent locations.
- Impossible travel: two logins from locations too far apart for the time
  between them.
- Change of password, email, phone number or mailing address in the last 7
  days, especially followed by a new payee.
- New payee added and paid within 24 hours.
- One-time passcode requested several times, or delivered to a recently
  changed phone number.
- Payment at unusual hours from a new device.

Two or more of these indicators together with a HIGH-risk payment strongly
suggest account takeover.

## 4. Response when account takeover is suspected

1. Escalate the fraud case (FRD-003 §6.2) - do not approve.
2. Request that Fraud Operations lock online banking access and revoke active
   sessions.
3. Contact the customer only through the phone number on file **before** the
   recent profile change.
4. Once the customer is verified, reset credentials and remove unknown devices
   and payees.
5. Request a recall of any funds already sent (success is not guaranteed for
   international transfers).
6. Record all steps in the case notes.

## 5. Customer reimbursement

Customers are protected against unauthorized transactions they did not make or
authorize, provided they protected their credentials and reported the issue
promptly. See the public Online Banking Security Guarantee in ACC-001 §6.
