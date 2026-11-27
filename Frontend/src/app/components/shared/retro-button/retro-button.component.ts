import { NgTemplateOutlet } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';

export type RetroButtonVariant = 'primary' | 'secondary';
export type RetroButtonSize = 'sm' | 'md' | 'lg';

@Component({
  selector: 'app-retro-button',
  imports: [NgTemplateOutlet, RouterLink],
  template: `
    <ng-template #projectedContent>
      <ng-content />
    </ng-template>

    @if (routerLink(); as routeTarget) {
      <a
        [routerLink]="routeTarget"
        [fragment]="fragment()"
        [class]="buttonClasses()"
        (click)="pressed.emit()"
      >
        <ng-container *ngTemplateOutlet="projectedContent" />
      </a>
    } @else if (href(); as targetHref) {
      <a [href]="targetHref" [class]="buttonClasses()" (click)="pressed.emit()">
        <ng-container *ngTemplateOutlet="projectedContent" />
      </a>
    } @else {
      <button
        [type]="type()"
        [disabled]="disabled()"
        [class]="buttonClasses()"
        (click)="pressed.emit()"
      >
        <ng-container *ngTemplateOutlet="projectedContent" />
      </button>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'inline-block',
    '[class.w-full]': 'fullWidth()',
  },
})
export class RetroButtonComponent {
  readonly variant = input<RetroButtonVariant>('primary');
  readonly size = input<RetroButtonSize>('md');
  readonly href = input<string | null>(null);
  readonly routerLink = input<string | readonly unknown[] | null>(null);
  readonly fragment = input<string | undefined>(undefined);
  readonly type = input<'button' | 'submit'>('button');
  readonly fullWidth = input<boolean>(false);
  readonly disabled = input<boolean>(false);

  readonly pressed = output<void>();

  protected readonly buttonClasses = computed(() => {
    const base =
      'inline-flex items-center justify-center gap-2 font-pixel font-bold uppercase tracking-[0.06em] cursor-pointer select-none text-center leading-snug sm:whitespace-nowrap disabled:opacity-50 disabled:cursor-not-allowed';

    const variantClass =
      this.variant() === 'primary' ? 'pixel-btn-primary' : 'pixel-btn-secondary';

    const sizeClasses: Record<RetroButtonSize, string> = {
      sm: 'px-4 py-2 text-[11px]',
      md: 'px-4 sm:px-6 py-3.5 text-[11px] sm:text-[13px]',
      lg: 'px-4 sm:px-12 py-4 text-[11px] sm:text-[15px]',
    };

    const widthClass = this.fullWidth() ? 'w-full' : '';

    return `${base} ${variantClass} ${sizeClasses[this.size()]} ${widthClass}`.trim();
  });
}
