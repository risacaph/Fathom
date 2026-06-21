import {Component, inject, Input, TemplateRef} from '@angular/core';
import {NgbModal, NgbModalRef} from '@ng-bootstrap/ng-bootstrap';
import {ToastrService} from 'ngx-toastr';
import {Series} from '../../../_models/series';
import {SeriesMetadata} from '../../../_models/metadata/series-metadata';
import {CitationFormat, CitationService} from '../../../_services/citation.service';

/**
 * "Cite" button for a series. Generates APA / MLA / BibTeX citations client-side from existing metadata
 * (title, writers, release year, publisher, web link) and copies to clipboard. Useful for Research Papers libraries.
 */
@Component({
  selector: 'app-cite-button',
  standalone: true,
  imports: [],
  template: `
    <button class="btn btn-actions" title="Cite" aria-label="Cite" (click)="open(content)">
      <i class="fa-solid fa-quote-right" aria-hidden="true"></i>
    </button>
    <ng-template #content>
      <div class="modal-header">
        <h4 class="modal-title">Cite</h4>
        <button type="button" class="btn-close" aria-label="Close" (click)="modalRef?.dismiss()"></button>
      </div>
      <div class="modal-body">
        @for (f of formats; track f.id) {
          <div class="mb-3">
            <div class="d-flex justify-content-between align-items-center mb-1">
              <strong>{{f.label}}</strong>
              <button class="btn btn-sm btn-secondary" (click)="copy(f.id)">Copy</button>
            </div>
            <pre class="cite-pre">{{cite(f.id)}}</pre>
          </div>
        }
      </div>
    </ng-template>
  `,
  styles: [`
    .cite-pre {
      white-space: pre-wrap;
      word-break: break-word;
      background: var(--card-bg-color, rgba(0,0,0,.25));
      padding: .5rem .75rem;
      border-radius: .25rem;
      margin: 0;
      font-size: .85rem;
    }
  `]
})
export class CiteButtonComponent {
  @Input({required: true}) series!: Series;
  @Input() metadata: SeriesMetadata | null = null;

  private readonly modalService = inject(NgbModal);
  private readonly citationService = inject(CitationService);
  private readonly toastr = inject(ToastrService);

  protected modalRef?: NgbModalRef;

  protected readonly formats: {id: CitationFormat; label: string}[] = [
    {id: 'apa', label: 'APA'},
    {id: 'mla', label: 'MLA'},
    {id: 'bibtex', label: 'BibTeX'},
  ];

  open(content: TemplateRef<unknown>) {
    this.modalRef = this.modalService.open(content, {size: 'lg', scrollable: true});
  }

  cite(format: CitationFormat): string {
    return this.citationService.build(this.series, this.metadata, format);
  }

  copy(format: CitationFormat) {
    const text = this.cite(format);
    navigator.clipboard?.writeText(text).then(
      () => this.toastr.success('Citation copied'),
      () => this.toastr.error('Could not copy to clipboard')
    );
  }
}
