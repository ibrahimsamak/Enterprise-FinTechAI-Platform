import { Routes } from '@angular/router';
import { DevLogin, roleGuard } from 'shared-ui';

export const routes: Routes = [

    {path: 'login', component:DevLogin, data:{role:'Customer', heading:'FinAI - Customer'}},
    {
        path: '', canActivate: [roleGuard('Customer')], children:[
        {path: 'pay', loadComponent:()=> import('./payments/make-payment').then(m=> m.MakePayment)},
        { path: 'pay/:id', loadComponent: () => import('./payments/payment-status').then(m => m.PaymentStatus) },
        { path: 'assistant', loadComponent: () => import('./assistant/assistant').then(m => m.Assistant) },
        { path: '', pathMatch: 'full', redirectTo: 'pay' },
    ]},
    { path: "**", redirectTo:'' },
];
