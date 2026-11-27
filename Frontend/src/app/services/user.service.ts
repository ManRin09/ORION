import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  PagedResponse,
  RegisterUserRequest,
  UpdateUserProfileRequest,
  UserResponse,
} from '../models/user.models';
import { RegisterCharacterPayload } from '../models/auth.models';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/users`;

  /**
   * Registers a new adventurer ("Spawn into Orion").
   */
  register(request: RegisterUserRequest): Observable<UserResponse> {
    return this.http.post<UserResponse>(`${this.baseUrl}/register`, request);
  }

  /**
   * Adapter helper that maps frontend's RegisterCharacterPayload to backend's RegisterUserRequest.
   */
  registerFromCharacterPayload(payload: RegisterCharacterPayload): Observable<UserResponse> {
    const request: RegisterUserRequest = {
      firstName: payload.firstName,
      middleName: payload.middleTitle ? payload.middleTitle : null,
      email: payload.email,
      username: null,
      password: payload.password,
      confirmPassword: payload.password,
      acceptTerms: payload.acceptedTerms,
    };

    return this.register(request);
  }

  /**
   * Retrieves an adventurer by unique identifier (GUID).
   */
  getById(id: string): Observable<UserResponse> {
    return this.http.get<UserResponse>(`${this.baseUrl}/${id}`);
  }

  /**
   * Lists active adventurers with pagination.
   */
  getPaged(pageNumber: number = 1, pageSize: number = 10): Observable<PagedResponse<UserResponse>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedResponse<UserResponse>>(this.baseUrl, { params });
  }

  /**
   * Updates editable profile fields of an active adventurer.
   */
  updateProfile(id: string, request: UpdateUserProfileRequest): Observable<UserResponse> {
    return this.http.put<UserResponse>(`${this.baseUrl}/${id}/profile`, request);
  }

  /**
   * Soft-deletes (deactivates) an adventurer account.
   */
  softDelete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
