import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { FooterComponent } from '../../components/landing/footer/footer.component';
import { NavbarComponent } from '../../components/landing/navbar/navbar.component';
import { PixelIconComponent } from '../../components/shared/pixel-icon/pixel-icon.component';
import { RetroButtonComponent } from '../../components/shared/retro-button/retro-button.component';
import { UpdateUserProfileRequest, UserResponse } from '../../models/user.models';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-adventurers',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    NavbarComponent,
    FooterComponent,
    PixelIconComponent,
    RetroButtonComponent,
  ],
  templateUrl: './adventurers.component.html',
  styleUrl: './adventurers.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdventurersComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly fb = inject(FormBuilder);

  protected readonly adventurers = signal<readonly UserResponse[]>([]);
  protected readonly totalCount = signal<number>(0);
  protected readonly pageNumber = signal<number>(1);
  protected readonly pageSize = signal<number>(8);
  protected readonly totalPages = signal<number>(1);

  protected readonly isLoading = signal<boolean>(false);
  protected readonly isSaving = signal<boolean>(false);
  protected readonly deletingUserId = signal<string | null>(null);

  protected readonly errorMessage = signal<string | null>(null);
  protected readonly successNotice = signal<string | null>(null);

  protected readonly inspectedUser = signal<UserResponse | null>(null);
  protected readonly editingUser = signal<UserResponse | null>(null);

  protected readonly editForm = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    middleName: [''],
    username: [''],
    avatarUrl: [''],
  });

  ngOnInit(): void {
    this.loadAdventurers();
  }

  protected loadAdventurers(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.authService.getPaged(this.pageNumber(), this.pageSize()).subscribe({
      next: (response) => {
        this.isLoading.set(false);
        this.adventurers.set(response.items);
        this.totalCount.set(response.totalCount);
        this.totalPages.set(response.totalPages);
      },
      error: (err: HttpErrorResponse) => {
        this.isLoading.set(false);
        this.errorMessage.set(
          err.error?.detail ||
            err.error?.title ||
            'Unable to retrieve guild roster from Orion API.',
        );
      },
    });
  }

  protected inspectUser(id: string): void {
    this.isLoading.set(true);
    this.authService.getById(id).subscribe({
      next: (user) => {
        this.isLoading.set(false);
        this.inspectedUser.set(user);
      },
      error: (err: HttpErrorResponse) => {
        this.isLoading.set(false);
        this.errorMessage.set(err.error?.detail || 'Adventurer record could not be loaded.');
      },
    });
  }

  protected closeInspect(): void {
    this.inspectedUser.set(null);
  }

  protected openEdit(user: UserResponse): void {
    this.editingUser.set(user);
    this.editForm.setValue({
      firstName: user.firstName,
      middleName: user.middleName ?? '',
      username: user.username,
      avatarUrl: user.avatarUrl ?? '',
    });
  }

  protected cancelEdit(): void {
    this.editingUser.set(null);
    this.editForm.reset();
  }

  protected saveEditProfile(): void {
    const user = this.editingUser();
    if (!user) {
      return;
    }

    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }

    const raw = this.editForm.getRawValue();
    const payload: UpdateUserProfileRequest = {
      firstName: raw.firstName.trim(),
      middleName: raw.middleName.trim() || null,
      username: raw.username.trim() || null,
      avatarUrl: raw.avatarUrl.trim() || null,
    };

    this.isSaving.set(true);
    this.errorMessage.set(null);

    this.authService.updateProfile(user.id, payload).subscribe({
      next: (updated) => {
        this.isSaving.set(false);
        this.editingUser.set(null);
        this.successNotice.set(`Adventurer @${updated.username} profile updated successfully!`);
        this.loadAdventurers();
        if (this.inspectedUser()?.id === updated.id) {
          this.inspectedUser.set(updated);
        }
      },
      error: (err: HttpErrorResponse) => {
        this.isSaving.set(false);
        this.errorMessage.set(
          err.error?.detail || err.error?.title || 'Failed to update adventurer profile.',
        );
      },
    });
  }

  protected deactivateUser(user: UserResponse): void {
    if (!confirm(`Are you sure you want to soft-delete / deactivate @${user.username}?`)) {
      return;
    }

    this.deletingUserId.set(user.id);
    this.errorMessage.set(null);

    this.authService.softDelete(user.id).subscribe({
      next: () => {
        this.deletingUserId.set(null);
        this.successNotice.set(`Adventurer @${user.username} was deactivated.`);
        if (this.inspectedUser()?.id === user.id) {
          this.inspectedUser.set(null);
        }
        this.loadAdventurers();
      },
      error: (err: HttpErrorResponse) => {
        this.deletingUserId.set(null);
        this.errorMessage.set(
          err.error?.detail || err.error?.title || 'Failed to deactivate adventurer.',
        );
      },
    });
  }

  protected prevPage(): void {
    if (this.pageNumber() > 1) {
      this.pageNumber.update((p) => p - 1);
      this.loadAdventurers();
    }
  }

  protected nextPage(): void {
    if (this.pageNumber() < this.totalPages()) {
      this.pageNumber.update((p) => p + 1);
      this.loadAdventurers();
    }
  }
}
