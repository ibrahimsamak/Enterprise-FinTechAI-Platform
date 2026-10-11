export interface CreatePayment {
  accountId: string;
  amount: number;
  currency: string;
  country: string;
}

export type PaymentStatusValue = 'Pending' | 'OnHold' | 'Completed' | 'Rejected';

export interface Payment {
  id: string;
  accountId: string;
  amount: number;
  currency: string;
  country: string;
  status: PaymentStatusValue;
}