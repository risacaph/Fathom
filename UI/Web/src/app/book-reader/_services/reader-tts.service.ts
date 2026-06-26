import { Injectable, signal } from '@angular/core';

export type TtsState = 'stopped' | 'playing' | 'paused';

/**
 * "Read Aloud" — wraps the browser Web Speech API (speechSynthesis) to read book text out loud.
 * Fully client-side (works offline, no backend). Long text is split into sentence-sized utterances
 * so pause/resume work and very long pages don't get truncated by the speech engine.
 */
@Injectable({ providedIn: 'root' })
export class ReaderTtsService {
  private readonly synth: SpeechSynthesis | null =
    typeof window !== 'undefined' && 'speechSynthesis' in window ? window.speechSynthesis : null;

  private queue: string[] = [];
  private index = 0;
  private onComplete?: () => void;

  /** Reactive playback state, for binding the Read Aloud button. */
  readonly state = signal<TtsState>('stopped');

  /** True when the browser supports speech synthesis. */
  readonly supported = !!this.synth;

  /** Speech rate (0.1–10, 1 = normal) and pitch (0–2, 1 = normal). */
  rate = 1.0;
  pitch = 1.0;
  voice: SpeechSynthesisVoice | null = null;

  /** Available system voices (populated asynchronously by the engine). */
  readonly voices = signal<SpeechSynthesisVoice[]>([]);

  constructor() {
    if (this.synth) {
      const load = () => this.voices.set(this.synth!.getVoices());
      load();
      this.synth.addEventListener('voiceschanged', load);
    }
  }

  getVoices(): SpeechSynthesisVoice[] {
    return this.synth?.getVoices() ?? [];
  }

  setRate(rate: number): void {
    this.rate = Math.min(4, Math.max(0.5, rate || 1));
  }

  setVoiceByName(name: string): void {
    this.voice = this.getVoices().find(v => v.name === name) ?? null;
  }

  /**
   * Speak the given text. <paramref name="onComplete"/> fires only when the whole text finishes
   * naturally (not when stopped), letting the caller auto-advance to the next page.
   */
  speak(text: string, onComplete?: () => void): void {
    if (!this.synth) return;
    this.stop();

    const chunks = this.chunk(text);
    if (chunks.length === 0) {
      onComplete?.();
      return;
    }

    this.queue = chunks;
    this.index = 0;
    this.onComplete = onComplete;
    this.state.set('playing');
    this.speakNext();
  }

  pause(): void {
    if (this.synth && this.state() === 'playing') {
      this.synth.pause();
      this.state.set('paused');
    }
  }

  resume(): void {
    if (this.synth && this.state() === 'paused') {
      this.synth.resume();
      this.state.set('playing');
    }
  }

  stop(): void {
    if (!this.synth) return;
    // Set state first so the in-flight utterance's onend/onerror (fired by cancel) won't auto-advance.
    this.state.set('stopped');
    this.queue = [];
    this.index = 0;
    this.onComplete = undefined;
    this.synth.cancel();
  }

  private speakNext(): void {
    if (!this.synth) return;
    if (this.index >= this.queue.length) {
      this.state.set('stopped');
      const cb = this.onComplete;
      this.onComplete = undefined;
      cb?.();
      return;
    }

    const utterance = new SpeechSynthesisUtterance(this.queue[this.index]);
    utterance.rate = this.rate;
    utterance.pitch = this.pitch;
    if (this.voice) utterance.voice = this.voice;

    const advance = () => {
      if (this.state() === 'playing') {
        this.index++;
        this.speakNext();
      }
    };
    utterance.onend = advance;
    utterance.onerror = advance;

    this.synth.speak(utterance);
  }

  /** Split into sentence-ish chunks, capped in length so engines don't choke on huge utterances. */
  private chunk(text: string): string[] {
    const clean = (text ?? '').replace(/\s+/g, ' ').trim();
    if (!clean) return [];

    const sentences = clean.match(/[^.!?。！？]+[.!?。！？]*\s*/g) ?? [clean];
    const out: string[] = [];
    let buffer = '';
    for (const sentence of sentences) {
      if (buffer && (buffer + sentence).length > 240) {
        out.push(buffer.trim());
        buffer = '';
      }
      buffer += sentence;
    }
    if (buffer.trim()) out.push(buffer.trim());
    return out;
  }
}
