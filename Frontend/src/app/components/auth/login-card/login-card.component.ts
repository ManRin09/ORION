import { ChangeDetectionStrategy, Component, inject, input, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { LoginCredentials } from '../../../models/auth.models';
import { PixelIconComponent } from '../../shared/pixel-icon/pixel-icon.component';
import { RetroButtonComponent } from '../../shared/retro-button/retro-button.component';

@Component({
  selector: 'app-login-card',
  imports: [ReactiveFormsModule, RouterLink, PixelIconComponent, RetroButtonComponent],
  templateUrl: './login-card.component.html',
  styleUrl: './login-card.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'block',
  },
})
export class LoginCardComponent {
  private readonly fb = inject(FormBuilder);

  protected readonly showPassword = signal<boolean>(false);
  protected readonly submitted = signal<boolean>(false);

  protected readonly loginForm = this.fb.nonNullable.group({
    identifier: ['', [Validators.required, Validators.minLength(3)]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    rememberTerminal: [true],
  });

  readonly submitting = input<boolean>(false);

  readonly loginSubmitted = output<LoginCredentials>();
  readonly githubLoginRequested = output<void>();
  readonly forgotScrollRequested = output<void>();
  readonly validationFailed = output<void>();

  protected togglePasswordVisibility(): void {
    this.showPassword.update((visible) => !visible);
  }

  protected onSubmit(): void {
    this.submitted.set(true);

    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      this.validationFailed.emit();
      return;
    }

    const { identifier, password, rememberTerminal } = this.loginForm.getRawValue();
    this.loginSubmitted.emit({
      identifier: identifier.trim(),
      password,
      rememberTerminal,
    });
  }

  protected showFieldError(field: 'identifier' | 'password'): boolean {
    const control = this.loginForm.controls[field];
    return control.invalid && (control.touched || this.submitted());
  }
}
