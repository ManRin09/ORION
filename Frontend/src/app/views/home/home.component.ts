import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FooterComponent } from '../../components/landing/footer/footer.component';
import { GuildInventoryComponent } from '../../components/landing/guild-inventory/guild-inventory.component';
import { HeroSectionComponent } from '../../components/landing/hero-section/hero-section.component';
import { NavbarComponent } from '../../components/landing/navbar/navbar.component';
import { QuestMapComponent } from '../../components/landing/quest-map/quest-map.component';
import { QuestingComparisonComponent } from '../../components/landing/questing-comparison/questing-comparison.component';
import { SavePointCtaComponent } from '../../components/landing/save-point-cta/save-point-cta.component';

@Component({
  selector: 'app-home',
  imports: [
    NavbarComponent,
    HeroSectionComponent,
    QuestMapComponent,
    GuildInventoryComponent,
    QuestingComparisonComponent,
    SavePointCtaComponent,
    FooterComponent,
  ],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent {}
