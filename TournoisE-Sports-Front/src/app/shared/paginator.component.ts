import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-paginator',
  standalone: true,
  imports: [],
  template: `
    @if (totalPages > 1) {
    <div class="paginator">
      <button
        class="btn btn--ghost btn--sm"
        [disabled]="page === 1"
        (click)="pageChange.emit(page - 1)"
      >
        Précédent
      </button>
      <span class="page-info">Page {{ page }} sur {{ totalPages }}</span>
      <button
        class="btn btn--ghost btn--sm"
        [disabled]="page === totalPages"
        (click)="pageChange.emit(page + 1)"
      >
        Suivant
      </button>
    </div>
    }
  `,
  styles: [`
    .paginator {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 1rem;
      margin-top: 1.5rem;
      font-size: 0.9rem;
      color: var(--muted);
    }
    .page-info {
      font-weight: 600;
      color: var(--text);
    }
  `]
})
export class PaginatorComponent {
  @Input() page = 1;
  @Input() totalPages = 0;
  @Output() pageChange = new EventEmitter<number>();
}