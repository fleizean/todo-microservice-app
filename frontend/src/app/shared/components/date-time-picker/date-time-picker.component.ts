import { Component, Input, Output, EventEmitter, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-date-time-picker',
  standalone: true,
  imports: [CommonModule, FormsModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DateTimePickerComponent),
      multi: true
    }
  ],
  template: `
    <div class="relative">
      <label *ngIf="label" [for]="inputId" class="text-foreground mb-2 block text-sm font-medium">
        <span [innerHTML]="label"></span>
      </label>
      
      <div class="relative flex gap-2">
        <!-- Date Input -->
        <div class="flex-1 relative">
          <input
            [id]="inputId + '_date'"
            type="date"
            [value]="dateValue"
            (change)="onDateChange($event)"
            [placeholder]="datePlaceholder"
            [min]="minDate"
            [max]="maxDate"
            class="border-muted bg-background text-foreground w-full rounded-lg border px-3 py-2 pr-10 text-sm focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary"
          />
          <div class="absolute inset-y-0 right-0 flex items-center pr-3 pointer-events-none">
            <svg class="h-4 w-4 text-muted-foreground" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"></path>
            </svg>
          </div>
        </div>
        
        <!-- Time Input -->
        <div class="flex-1 relative" *ngIf="includeTime">
          <input
            [id]="inputId + '_time'"
            type="time"
            [value]="timeValue"
            (change)="onTimeChange($event)"
            [placeholder]="timePlaceholder"
            class="border-muted bg-background text-foreground w-full rounded-lg border px-3 py-2 pr-10 text-sm focus:outline-none focus:ring-2 focus:ring-primary focus:border-primary"
          />
          <div class="absolute inset-y-0 right-0 flex items-center pr-3 pointer-events-none">
            <svg class="h-4 w-4 text-muted-foreground" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"></path>
            </svg>
          </div>
        </div>
      </div>
      
      <!-- Quick Date Shortcuts -->
      <div class="mt-2 flex flex-wrap gap-1" *ngIf="showQuickActions">
        <button
          type="button"
          (click)="setQuickDate('today')"
          class="bg-muted text-muted-foreground hover:text-foreground hover:bg-primary/10 rounded-md px-2 py-1 text-xs font-medium transition-colors"
        >
          Today
        </button>
        <button
          type="button"
          (click)="setQuickDate('tomorrow')"
          class="bg-muted text-muted-foreground hover:text-foreground hover:bg-primary/10 rounded-md px-2 py-1 text-xs font-medium transition-colors"
        >
          Tomorrow
        </button>
        <button
          type="button"
          (click)="setQuickDate('nextWeek')"
          class="bg-muted text-muted-foreground hover:text-foreground hover:bg-primary/10 rounded-md px-2 py-1 text-xs font-medium transition-colors"
        >
          Next Week
        </button>
        <button
          type="button"
          (click)="clearDate()"
          class="bg-muted text-muted-foreground hover:text-destructive hover:bg-destructive/10 rounded-md px-2 py-1 text-xs font-medium transition-colors"
        >
          Clear
        </button>
      </div>
      
      <!-- Helper Text -->
      <p *ngIf="helperText" class="text-muted-foreground mt-1 text-xs">
        {{ helperText }}
      </p>
    </div>
  `,
})
export class DateTimePickerComponent implements ControlValueAccessor {
  @Input() label: string = '';
  @Input() inputId: string = '';
  @Input() datePlaceholder: string = 'Select date';
  @Input() timePlaceholder: string = 'Select time';
  @Input() includeTime: boolean = true;
  @Input() showQuickActions: boolean = true;
  @Input() helperText: string = '';
  @Input() minDate: string = '';
  @Input() maxDate: string = '';

  @Output() dateTimeChange = new EventEmitter<string | null>();

  private _value: string | null = null;
  dateValue: string = '';
  timeValue: string = '';

  private onChange = (value: string | null) => {};
  private onTouched = () => {};

  get value(): string | null {
    return this._value;
  }

  set value(val: string | null) {
    this._value = val;
    this.updateDateTimeValues(val);
    this.onChange(val);
    this.onTouched();
  }

  writeValue(value: string | null): void {
    this.value = value;
  }

  registerOnChange(fn: (value: string | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  onDateChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.dateValue = input.value;
    this.updateCombinedValue();
  }

  onTimeChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.timeValue = input.value;
    this.updateCombinedValue();
  }

  private updateCombinedValue(): void {
    if (this.dateValue) {
      let combinedValue = this.dateValue;
      if (this.includeTime && this.timeValue) {
        combinedValue += `T${this.timeValue}`;
      } else if (this.includeTime) {
        combinedValue += 'T00:00';
      }
      this.value = combinedValue;
    } else {
      this.value = null;
    }
    this.dateTimeChange.emit(this.value);
  }

  private updateDateTimeValues(value: string | null): void {
    if (value) {
      const date = new Date(value);
      if (!isNaN(date.getTime())) {
        this.dateValue = date.toISOString().split('T')[0];
        this.timeValue = date.toTimeString().slice(0, 5);
      }
    } else {
      this.dateValue = '';
      this.timeValue = '';
    }
  }

  setQuickDate(type: string): void {
    const now = new Date();
    let targetDate: Date;

    switch (type) {
      case 'today':
        targetDate = now;
        break;
      case 'tomorrow':
        targetDate = new Date(now);
        targetDate.setDate(now.getDate() + 1);
        break;
      case 'nextWeek':
        targetDate = new Date(now);
        targetDate.setDate(now.getDate() + 7);
        break;
      default:
        return;
    }

    this.dateValue = targetDate.toISOString().split('T')[0];
    if (this.includeTime) {
      this.timeValue = '09:00'; // Default to 9 AM
    }
    this.updateCombinedValue();
  }

  clearDate(): void {
    this.dateValue = '';
    this.timeValue = '';
    this.value = null;
    this.dateTimeChange.emit(null);
  }
}