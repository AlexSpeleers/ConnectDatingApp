import { Component, input, Input, Self } from '@angular/core';
import { Register } from '../../features/account/register/register';
import { ControlValueAccessor, FormControl, NgControl, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-password-input',
  imports: [ReactiveFormsModule],
  templateUrl: './password-input.html',
  styleUrl: './password-input.css',
})
export class PasswordInput implements ControlValueAccessor {
  @Input() register!: Register;
  public label = input<string>('');
  public type = input<string>('text');

  constructor(@Self() public ngControl: NgControl) {
    this.ngControl.valueAccessor = this;
  }
  writeValue(obj: any): void {}
  registerOnChange(fn: any): void {}
  registerOnTouched(fn: any): void {}

  protected get control(): FormControl {
    return this.ngControl.control as FormControl;
  }
}
