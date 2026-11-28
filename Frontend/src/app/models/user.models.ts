/**
 * Model definitions for User Aggregate and CRUD operations matching Orion.Api (.NET 10).
 */

export interface UserResponse {
  readonly id: string;
  readonly firstName: string;
  readonly middleName: string | null;
  readonly email: string;
  readonly username: string;
  readonly avatarUrl: string | null;
  readonly authProvider: 'Local' | 'GitHub' | string;
  readonly role: 'User' | 'Admin' | string;
  readonly planType: 'Free' | 'Pro' | 'Family' | string;
  readonly totalXp: number;
  readonly currentLevel: number;
  readonly currentStreak: number;
  readonly longestStreak: number;
  readonly gems: number;
  readonly isActive: boolean;
  readonly acceptedTermsAt: string;
  readonly createdAt: string;
  readonly updatedAt: string;
}

export interface PagedResponse<T> {
  readonly items: readonly T[];
  readonly pageNumber: number;
  readonly pageSize: number;
  readonly totalCount: number;
  readonly totalPages: number;
}

export interface RegisterUserRequest {
  readonly firstName: string;
  readonly middleName?: string | null;
  readonly email: string;
  readonly username?: string | null;
  readonly password: string;
  readonly confirmPassword: string;
  readonly acceptTerms: boolean;
}

export interface UpdateUserProfileRequest {
  readonly firstName: string;
  readonly middleName?: string | null;
  readonly username?: string | null;
  readonly avatarUrl?: string | null;
}

export interface ApiProblemDetails {
  readonly type?: string;
  readonly title?: string;
  readonly status?: number;
  readonly detail?: string;
  readonly instance?: string;
  readonly errors?: Record<string, readonly string[]>;
}
