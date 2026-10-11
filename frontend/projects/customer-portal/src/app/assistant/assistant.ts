import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SupportChatService, SupportCitation } from './support-chat.service';

interface ChatTurn {
  role: 'user' | 'assistant';
  text: string;
  citations: SupportCitation[];
  streaming: boolean;
  failed: boolean;
}

@Component({
  selector: 'app-assistant',
  imports: [FormsModule],
  template: `
    <main class="page" style="max-width: 44rem">
      <h1>Assistant</h1>
      <p class="hint">Answers come only from FinAI's public help documents, with citations. It can't see your account.</p>

      <section class="log">
        @for (t of turns(); track $index) {
          <article class="card" [class.mine]="t.role === 'user'">
            <div class="text" [class.error]="t.failed">{{ t.text }}@if (t.streaming) {<span class="cursor">▍</span>}</div>
            @if (t.citations.length) {
              <ul class="hint">
                @for (c of t.citations; track c.docId + c.section) { <li>[{{ c.docId }} §{{ c.section }}] {{ c.title }}</li> }
              </ul>
            }
          </article>
        } @empty {
          <div class="suggest">
            @for (q of suggestions; track q) { <button type="button" (click)="question = q; send()">{{ q }}</button> }
          </div>
        }
      </section>

      <form class="ask" (ngSubmit)="send()">
        <input name="question" [(ngModel)]="question" placeholder="Ask about payments, holds, limits..."
               maxlength="1000" [disabled]="busy()" />
        <button type="submit" [disabled]="busy() || !question.trim()">Send</button>
      </form>
    </main>
  `,
  styles: [`
    .log { display: grid; gap: .75rem; margin-block: 1rem; }
    .mine { justify-self: end; max-width: 80%; }
    .text { white-space: pre-wrap; }
    .cursor { animation: blink 1s steps(1) infinite; } @keyframes blink { 50% { opacity: 0; } }
    .ask { display: flex; gap: .5rem; } .ask input { flex: 1; }
    .suggest { display: flex; flex-wrap: wrap; gap: .5rem; }
  `],
})
export class Assistant {
    private readonly chat = inject(SupportChatService);
    readonly suggestions = ['Why would a payment be held for review?', 'How long does an international transfer take?'];
    readonly turns = signal<ChatTurn[]>([]);
    readonly busy = signal(false);
    question = '';

    async send():  Promise<void>{
        const q = this.question.trim();
        if (!q || this.busy()) return;
        this.question = '';
        this.busy.set(true);

        this.turns.update(t=>[
            ...t,
            {role: 'user', text:q, citations:[], streaming: false, failed: false},
            {role: 'assistant', text:'', citations:[], streaming:true, failed: false}
        ])
        try{
            await this.chat.ask(q, item=> this.updateLast(t=>({
                ...t,
                text: t.text + (item.delta ?? ''),
                citations: item.citations ?? t.citations,
            })))    
        }
        catch(e){
            const msg = e instanceof Error ? e.message : String(e);
            this.updateLast(t => ({ ...t, failed: true, text: `Assistant unavailable: ${msg}` }));
        }
        finally{
            this.updateLast( t=> ({...t, streaming:false}))
            this.busy.set(false);
        }
    }

    private updateLast(change: (t: ChatTurn) => ChatTurn): void{
        this.turns.update(list=> [...list.slice(0,-1), change(list[list.length-1])])
    }
}
