import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  output,
  signal,
} from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { toSignal } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { RouterLink } from '@angular/router';
import { startWith } from 'rxjs';
import { RegisterCharacterPayload, isCipherComplete } from '../../../models/auth.models';
import { RegisterUserRequest, UserResponse } from '../../../models/user.models';
import { AuthService } from '../../../services/auth.service';
import { PixelIconComponent } from '../../shared/pixel-icon/pixel-icon.component';
import { RetroButtonComponent } from '../../shared/retro-button/retro-button.component';
import { CipherRequirementsComponent } from '../cipher-requirements/cipher-requirements.component';

const cipherIntegrityValidator: ValidatorFn = (
  control: AbstractControl<string>,
): ValidationErrors | null => {
  const value = control.value ?? '';
  if (!value) {
    return null;
  }
  return isCipherComplete(value) ? null : { cipherWeak: true };
};

const spellsMatchValidator: ValidatorFn = (group: AbstractControl): ValidationErrors | null => {
  const password = group.get('password')?.value ?? '';
  const confirmPassword = group.get('confirmPassword')?.value ?? '';
  if (!confirmPassword) {
    return null;
  }
  return password === confirmPassword ? null : { spellMismatch: true };
};

@Component({
  selector: 'app-register-card',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    PixelIconComponent,
    RetroButtonComponent,
    CipherRequirementsComponent,
  ],
  templateUrl: './register-card.component.html',
  styleUrl: './register-card.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'block',
  },
})
export class RegisterCardComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);

  protected readonly showPassword = signal<boolean>(false);
  protected readonly showConfirmPassword = signal<boolean>(false);
  protected readonly submitted = signal<boolean>(false);
  protected readonly isSubmitting = signal<boolean>(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly registeredUser = signal<UserResponse | null>(null);
  protected readonly previewBanner = signal<string | null>(null);

  protected readonly registerForm = this.fb.nonNullable.group(
    {
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      middleTitle: [''],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, cipherIntegrityValidator]],
      confirmPassword: ['', [Validators.required]],
      acceptedTerms: [false, [Validators.requiredTrue]],
    },
    { validators: [spellsMatchValidator] },
  );

  protected readonly passwordValue = toSignal(
    this.registerForm.controls.password.valueChanges.pipe(
      startWith(this.registerForm.controls.password.value),
    ),
    { initialValue: '' },
  );

  protected readonly confirmPasswordValue = toSignal(
    this.registerForm.controls.confirmPassword.valueChanges.pipe(
      startWith(this.registerForm.controls.confirmPassword.value),
    ),
    { initialValue: '' },
  );

  protected readonly matchState = computed<'idle' | 'matched' | 'mismatch'>(() => {
    const confirm = this.confirmPasswordValue();
    const password = this.passwordValue();
    if (!confirm) {
      return 'idle';
    }
    return password === confirm && password.length > 0 ? 'matched' : 'mismatch';
  });

  readonly characterForged = output<RegisterCharacterPayload>();
  readonly githubQuickSpawn = output<void>();

  protected togglePasswordVisibility(): void {
    this.showPassword.update((visible) => !visible);
  }

  protected toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword.update((visible) => !visible);
  }

  protected onGithubQuickSpawn(): void {
    this.previewBanner.set(
      'GitHub Guild Quick Spawn preview active — OAuth backend not connected yet.',
    );
    this.githubQuickSpawn.emit();
  }

  protected onSubmit(): void {
    this.submitted.set(true);

    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      this.previewBanner.set(null);
      this.errorMessage.set(null);
      return;
    }

    const raw = this.registerForm.getRawValue();
    const payload: RegisterCharacterPayload = {
      firstName: raw.firstName.trim(),
      middleTitle: raw.middleTitle.trim(),
      email: raw.email.trim(),
      password: raw.password,
      acceptedTerms: raw.acceptedTerms,
    };

    const request: RegisterUserRequest = {
      firstName: payload.firstName,
      middleName: payload.middleTitle ? payload.middleTitle : null,
      email: payload.email,
      username: null,
      password: payload.password,
      confirmPassword: payload.password,
      acceptTerms: payload.acceptedTerms,
    };

    this.isSubmitting.set(true);
    this.errorMessage.set(null);
    this.previewBanner.set(null);

    this.authService.register(request).subscribe({
      next: (user) => {
        this.isSubmitting.set(false);
        this.registeredUser.set(user);
        const fullName = user.middleName ? `${user.firstName} ${user.middleName}` : user.firstName;
        this.previewBanner.set(
          `✓ CHARACTER "${fullName.toUpperCase()}" FORGED · LEVEL ${user.currentLevel} READY`,
        );
        this.characterForged.emit(payload);
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting.set(false);
        const detail =
          err.error?.detail ||
          err.error?.title ||
          (err.status === 0
            ? 'Cannot connect to Orion API backend. Check network or server status.'
            : 'Error forging character. Please check your runes and try again.');
        this.errorMessage.set(detail);
      },
    });
  }

  protected hasError(
    field: 'firstName' | 'email' | 'password' | 'confirmPassword' | 'acceptedTerms',
  ): boolean {
    const control = this.registerForm.controls[field];
    const invalid =
      field === 'confirmPassword'
        ? control.invalid || this.registerForm.hasError('spellMismatch')
        : control.invalid;
    return invalid && (control.touched || this.submitted());
  }
}
