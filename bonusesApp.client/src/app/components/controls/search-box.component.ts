import { Component, ElementRef, HostListener, input, output, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
    selector: 'app-search-box',
    templateUrl: './search-box.component.html',
    styleUrl: './search-box.component.scss',
    standalone: true,
    imports: [FormsModule]
})
export class SearchBoxComponent {
  public placeholder = input('Search...');
  readonly searchChange = output<string>();
  readonly searchInput = viewChild.required<ElementRef>('searchInput');

  onValueChange(value: string) {
    setTimeout(() => this.searchChange.emit(value), 1000);
  }

  @HostListener('keydown.escape')
  clear() {
    const searchInput = this.searchInput();

    searchInput.nativeElement.value = '';
    this.onValueChange(searchInput.nativeElement.value);
  }
}
