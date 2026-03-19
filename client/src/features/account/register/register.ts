import { Component, inject, output, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { RegisterCreds, User } from '../../../types/user';
import { AccountService } from '../../../core/services/account-service';
import { TextInput } from '../../../shared/text-input/text-input';
import { PasswordInput } from '../../../shared/password-input/password-input';
import { Router } from '@angular/router';
@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, TextInput, PasswordInput],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  protected creds: RegisterCreds = {} as RegisterCreds;
  protected cancelRegister = output<boolean>();
  private accountService: AccountService = inject(AccountService);
  private router = inject(Router);
  private formBuilder = inject(FormBuilder);
  protected credentialsForm: FormGroup;
  protected profileForm: FormGroup;
  protected currentStep = signal(1);
  protected showHide: boolean = false;
  protected validationErrors = signal<string[]>([]);

  public get ShowHide(): boolean {
    return this.showHide;
  }

  constructor() {
    this.credentialsForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      displayName: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
      confirmPassword: ['', [Validators.required, this.MatchValues('password')]],
    });

    this.credentialsForm.controls['password'].valueChanges.subscribe(() => {
      this.credentialsForm.controls['confirmPassword'].updateValueAndValidity();
    });

    this.profileForm = this.formBuilder.group({
      gender: ['male', Validators.required],
      dateOfBirth: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
    });
  }

  protected MatchValues(matchTo: string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const parent = control.parent;
      if (!parent) return null;

      const matchValue = parent.get(matchTo)?.value;
      return control.value === matchValue ? null : { passwordMissmatch: true };
    };
  }

  protected NextStep() {
    if (this.credentialsForm.valid) {
      this.currentStep.update((prevStep) => prevStep + 1);
    }
  }

  protected PrevStep() {
    this.currentStep.update((prevStep) => prevStep - 1);
  }

  protected GetMaxDate() {
    const today = new Date();
    today.setFullYear(today.getFullYear() - 18);
    return today.toISOString().split('T')[0];
  }

  protected Register() {
    if (this.profileForm.valid && this.credentialsForm.valid) {
      const formData = { ...this.credentialsForm.value, ...this.profileForm.value };
      this.accountService.Register(formData).subscribe({
        next: () => {
          this.router.navigateByUrl('/members');
        },
        error: (error) => {
          console.log(error);
          this.validationErrors.set(error);
        },
      });
    }
  }

  public ToggleShowHide() {
    this.showHide = !this.showHide;
  }

  protected Cancel() {
    this.cancelRegister.emit(false);
  }
}
