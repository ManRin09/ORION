import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RegisterCardComponent } from '../../components/auth/register-card/register-card.component';
import { FooterComponent } from '../../components/landing/footer/footer.component';
import { NavbarComponent } from '../../components/landing/navbar/navbar.component';

@Component({
  selector: 'app-register',
  imports: [NavbarComponent, RegisterCardComponent, FooterComponent],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegisterComponent {}
