import {ChangeDetectionStrategy, ChangeDetectorRef, Component, computed, DestroyRef, inject, OnInit, signal} from '@angular/core';
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {ToastrService} from 'ngx-toastr';
import {catchError, debounceTime, distinctUntilChanged, filter, of, switchMap, tap} from 'rxjs';
import {translate, TranslocoModule} from "@jsverse/transloco";
import {takeUntilDestroyed} from "@angular/core/rxjs-interop";
import {SettingItemComponent} from "../../settings/_components/setting-item/setting-item.component";
import {SettingSwitchComponent} from "../../settings/_components/setting-switch/setting-switch.component";
import {DefaultValuePipe} from "../../_pipes/default-value.pipe";
import {EnterBlurDirective} from "../../_directives/enter-blur.directive";
import {AiProviderConfig, AiService} from "../../_services/ai.service";

/**
 * Admin panel for the optional AI provider (F6 semantic search / RAG, F8 metadata enrichment).
 * Talks to api/ai/config; everything stays inert until enabled and configured.
 */
@Component({
  selector: 'app-manage-ai-settings',
  templateUrl: './manage-ai-settings.component.html',
  styleUrls: ['./manage-ai-settings.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, TranslocoModule, SettingItemComponent, SettingSwitchComponent, DefaultValuePipe, EnterBlurDirective]
})
export class ManageAiSettingsComponent implements OnInit {

  private readonly cdRef = inject(ChangeDetectorRef);
  private readonly aiService = inject(AiService);
  private readonly toastr = inject(ToastrService);
  private readonly destroyRef = inject(DestroyRef);

  config = signal<AiProviderConfig | undefined>(undefined);
  settingsForm: FormGroup = new FormGroup({});
  isReindexing = signal(false);

  /** Drives the status banner: 'active' (chat+embeddings), 'partial' (chat only), or 'off'. */
  readonly statusKind = computed(() => {
    const c = this.config();
    if (!c || !c.isConfigured) return 'off';
    return c.canEmbed ? 'active' : 'partial';
  });

  ngOnInit(): void {
    this.aiService.getConfig().subscribe(config => {
      this.config.set(config);
      this.settingsForm.addControl('enabled', new FormControl(config.enabled, []));
      this.settingsForm.addControl('apiBaseUrl', new FormControl(config.apiBaseUrl, [Validators.required]));
      this.settingsForm.addControl('apiKey', new FormControl(config.apiKey, []));
      this.settingsForm.addControl('chatModel', new FormControl(config.chatModel, [Validators.required]));
      this.settingsForm.addControl('embeddingModel', new FormControl(config.embeddingModel, []));
      this.settingsForm.addControl('temperature', new FormControl(config.temperature, [Validators.min(0), Validators.max(2)]));
      this.settingsForm.addControl('maxContextChars', new FormControl(config.maxContextChars, [Validators.min(500)]));

      // Auto-save as fields are edited (house convention), then refresh derived status from the response.
      this.settingsForm.valueChanges.pipe(
        distinctUntilChanged(),
        debounceTime(400),
        filter(_ => this.settingsForm.valid),
        takeUntilDestroyed(this.destroyRef),
        switchMap(_ => this.aiService.updateConfig(this.packData()).pipe(catchError(err => {
          console.error(err);
          this.toastr.error(translate('manage-ai-settings.save-error'));
          return of(null);
        }))),
        tap(saved => {
          if (!saved) return;
          this.config.set(saved);
          this.cdRef.markForCheck();
        })
      ).subscribe();

      this.cdRef.markForCheck();
    });
  }

  packData(): AiProviderConfig {
    const current = this.config()!;
    return {
      ...current,
      enabled: this.settingsForm.get('enabled')?.value,
      apiBaseUrl: this.settingsForm.get('apiBaseUrl')?.value,
      apiKey: this.settingsForm.get('apiKey')?.value,
      chatModel: this.settingsForm.get('chatModel')?.value,
      embeddingModel: this.settingsForm.get('embeddingModel')?.value,
      temperature: this.settingsForm.get('temperature')?.value,
      maxContextChars: this.settingsForm.get('maxContextChars')?.value,
    };
  }

  reindex(): void {
    this.isReindexing.set(true);
    this.aiService.reindexSemantic().subscribe({
      next: () => {
        this.toastr.success(translate('manage-ai-settings.reindex-started'));
        this.isReindexing.set(false);
        this.cdRef.markForCheck();
      },
      error: () => {
        this.isReindexing.set(false);
        this.cdRef.markForCheck();
      }
    });
  }
}
