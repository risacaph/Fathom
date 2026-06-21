import {Injectable} from '@angular/core';
import {Series} from '../_models/series';
import {SeriesMetadata} from '../_models/metadata/series-metadata';

export type CitationFormat = 'apa' | 'mla' | 'bibtex';

/**
 * Builds best-effort citations from a Series and its metadata. Intended for the Research Papers / Book
 * library types. Uses series name (title), writers (authors), release year, first publisher, and first web link.
 * This is a client-side approximation; a richer, server-side version using DOI / venue metadata is planned in
 * docs/academic-features.md once a dedicated academic-metadata schema lands.
 */
@Injectable({providedIn: 'root'})
export class CitationService {

  build(series: Series | null | undefined, metadata: SeriesMetadata | null | undefined, format: CitationFormat): string {
    const title = (series?.name ?? '').trim();
    const authors = (metadata?.writers ?? []).map(p => (p?.name ?? '').trim()).filter(n => n.length > 0);
    const year = metadata?.releaseYear && metadata.releaseYear > 0 ? metadata.releaseYear : undefined;
    const publisher = (metadata?.publishers ?? []).map(p => (p?.name ?? '').trim()).find(n => n.length > 0);
    const url = this.firstLink(metadata?.webLinks);

    switch (format) {
      case 'apa': return this.toApa(title, authors, year, publisher, url);
      case 'mla': return this.toMla(title, authors, year, publisher, url);
      case 'bibtex': return this.toBibtex(title, authors, year, publisher, url);
    }
  }

  private firstLink(webLinks: string | null | undefined): string | undefined {
    if (!webLinks) return undefined;
    const first = webLinks.split(',').map(s => s.trim()).find(s => s.length > 0);
    return first || undefined;
  }

  private toApa(title: string, authors: string[], year?: number, publisher?: string, url?: string): string {
    const authorPart = authors.length ? this.joinApaAuthors(authors) + ' ' : '';
    const yearPart = `(${year ?? 'n.d.'}). `;
    const titlePart = title ? `${title}. ` : '';
    const pubPart = publisher ? `${publisher}. ` : '';
    return `${authorPart}${yearPart}${titlePart}${pubPart}${url ?? ''}`.trim();
  }

  private toMla(title: string, authors: string[], year?: number, publisher?: string, url?: string): string {
    const authorPart = authors.length ? `${authors.join(', ')}. ` : '';
    const titlePart = title ? `"${title}." ` : '';
    const pubPart = publisher ? `${publisher}, ` : '';
    const yearPart = year ? `${year}. ` : '';
    const urlPart = url ? `${url}.` : '';
    return `${authorPart}${titlePart}${pubPart}${yearPart}${urlPart}`.trim();
  }

  private toBibtex(title: string, authors: string[], year?: number, publisher?: string, url?: string): string {
    const surname = authors.length ? authors[0].split(' ').filter(Boolean).pop()! : 'reference';
    const key = `${surname}${year ?? ''}`.replace(/[^a-zA-Z0-9]/g, '').toLowerCase() || 'reference';
    const lines: string[] = [`@misc{${key},`];
    if (title) lines.push(`  title        = {${title}},`);
    if (authors.length) lines.push(`  author       = {${authors.join(' and ')}},`);
    if (year) lines.push(`  year         = {${year}},`);
    if (publisher) lines.push(`  publisher    = {${publisher}},`);
    if (url) lines.push(`  howpublished = {\\url{${url}}},`);
    lines.push('}');
    return lines.join('\n');
  }

  private joinApaAuthors(authors: string[]): string {
    if (authors.length === 1) return authors[0].endsWith('.') ? authors[0] : authors[0] + '.';
    return authors.slice(0, -1).join(', ') + ', & ' + authors[authors.length - 1] + '.';
  }
}
