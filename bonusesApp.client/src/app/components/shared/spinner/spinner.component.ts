import { Component, OnInit } from '@angular/core';
import { SpinnerService } from './spinner.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-spinner',
  templateUrl: './spinner.component.html',
  standalone: true,
  styleUrls: ['./spinner.component.css']
})
export class SpinnerComponent implements OnInit {
  isLoading$: Observable<boolean> | undefined;
  constructor(private spinnerService: SpinnerService) {}

  ngOnInit(): void {
    this.isLoading$ = this.spinnerService.isLoading$;
  }
}

