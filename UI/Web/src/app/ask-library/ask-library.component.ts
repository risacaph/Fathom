import {ChangeDetectionStrategy, Component, computed, DestroyRef, inject, OnInit, signal} from '@angular/core';
import {FormControl, ReactiveFormsModule, Validators} from '@angular/forms';
import {RouterLink} from '@angular/router';
import {TranslocoModule} from '@jsverse/transloco';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {AccountService} from '../_services/account.service';
import {AiService, AiStatus, RagAnswer, SemanticSearchResult} from '../_services/ai.service';

type AskMode = 'search' | 'ask';

/**
 * "Ask Your Library" — the user-facing surface for F6. Two modes:
 *  - Search: meaning-based search returning ranked text snippets.
 *  - Ask: a retrieval-augmented answer grounded in the user's own library, with source citations.
 */
@Component({
  selector: 'app-ask-library',
  standalone: true,
  imports: [ReactiveFormsModule, TranslocoModule, RouterLink],
  templateUrl: './ask-library.component.html',
  styleUrl: './ask-library.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AskLibraryComponent implements OnInit {
  private readonly aiService = inject(AiService);
  protected readonly accountService = inject(AccountService);
  private readonly destroyRef = inject(DestroyRef);

  readonly status = signal<AiStatus | null>(null);
  readonly statusLoaded = signal(false);
  readonly mode = signal<AskMode>('ask');
  readonly isLoading = signal(false);
  readonly hasRun = signal(false);
  readonly errored = signal(false);

  readonly results = signal<SemanticSearchResult[]>([]);
  readonly answer = signal<RagAnswer | null>(null);

  readonly queryControl = new FormControl('', [Validators.required, Validators.minLength(2)]);

  /** Ask needs a chat model; Search additionally needs an embedding model. */
  readonly canAsk = computed(() => this.status()?.enabled === true);
  readonly canSearch = computed(() => this.status()?.canEmbed === true);
  readonly anyAi = computed(() => this.canAsk() || this.canSearch());

  ngOnInit(): void {
    this.aiService.getStatus().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: status => {
        this.status.set(status);
        this.statusLoaded.set(true);
        // Prefer the richer mode that's available.
        this.mode.set(status.canEmbed ? 'search' : 'ask');
      },
      error: () => {
        this.statusLoaded.set(true);
      }
    });
  }

  setMode(mode: AskMode): void {
    if (this.mode() === mode) return;
    this.mode.set(mode);
    this.results.set([]);
    this.answer.set(null);
    this.hasRun.set(false);
    this.errored.set(false);
  }

  run(): void {
    const value = (this.queryControl.value ?? '').trim();
    if (value.length < 2 || this.isLoading()) return;

    this.isLoading.set(true);
    this.errored.set(false);
    this.results.set([]);
    this.answer.set(null);

    // Handle each mode in its own subscribe to avoid a union-of-Observables type.
    if (this.mode() === 'search') {
      this.aiService.semanticSearch(value, 15)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: res => { this.results.set(res); this.finishRun(); },
          error: () => this.failRun()
        });
    } else {
      this.aiService.ask(value, 6)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: res => { this.answer.set(res); this.finishRun(); },
          error: () => this.failRun()
        });
    }
  }

  private finishRun(): void {
    this.hasRun.set(true);
    this.isLoading.set(false);
  }

  private failRun(): void {
    this.errored.set(true);
    this.hasRun.set(true);
    this.isLoading.set(false);
  }

  /** Cosine score → friendly 0–100 relevance. */
  relevance(score: number): number {
    return Math.max(0, Math.min(100, Math.round(score * 100)));
  }
}
