import { ChangeDetectionStrategy, Component, computed, signal } from '@angular/core';
import { BATTLE_CODE_LINES } from '../../../data/quest-landing.data';
import { CodeTokenTone } from '../../../models/quest-landing.models';
import { PixelIconComponent } from '../../shared/pixel-icon/pixel-icon.component';
import { RetroButtonComponent } from '../../shared/retro-button/retro-button.component';

const MAX_GOBLIN_HP = 240;
const INITIAL_MP = 85;
const MAX_MP = 100;
const SPELL_COST_MP = 15;
const ATTACK_POWER = 42;

@Component({
  selector: 'app-battle-arena',
  imports: [PixelIconComponent, RetroButtonComponent],
  templateUrl: './battle-arena.component.html',
  styleUrl: './battle-arena.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BattleArenaComponent {
  protected readonly codeLines = BATTLE_CODE_LINES;
  protected readonly maxHp = MAX_GOBLIN_HP;
  protected readonly maxMp = MAX_MP;
  protected readonly attackPower = ATTACK_POWER;

  protected readonly goblinHp = signal<number>(MAX_GOBLIN_HP);
  protected readonly playerMp = signal<number>(INITIAL_MP);
  protected readonly castCount = signal<number>(0);

  protected readonly isDefeated = computed(() => this.goblinHp() <= 0);
  protected readonly hasInteracted = computed(() => this.castCount() > 0);
  protected readonly canCast = computed(() => this.playerMp() >= SPELL_COST_MP && !this.isDefeated());

  protected readonly hpBarPercent = computed(() => {
    if (!this.hasInteracted()) {
      return 84;
    }
    return Math.max(0, Math.round((this.goblinHp() / MAX_GOBLIN_HP) * 100));
  });

  protected castRunCode(): void {
    if (this.isDefeated() || this.playerMp() < SPELL_COST_MP) {
      this.resetBattle();
      return;
    }

    this.castCount.update((count) => count + 1);
    this.playerMp.update((mp) => Math.max(0, mp - SPELL_COST_MP));
    this.goblinHp.update((hp) => Math.max(0, hp - ATTACK_POWER * 2));
  }

  protected resetBattle(): void {
    this.goblinHp.set(MAX_GOBLIN_HP);
    this.playerMp.set(INITIAL_MP);
    this.castCount.set(0);
  }

  protected tokenClass(tone: CodeTokenTone): string {
    switch (tone) {
      case 'comment-ice':
        return 'text-[#99cbff]';
      case 'comment-coral':
        return 'text-[#ffb4ab]';
      case 'comment-green':
        return 'text-[#63de86]';
      case 'keyword':
        return 'text-[#f0bf69]';
      case 'fn':
        return 'text-[#63de86]';
      case 'plain':
        return 'text-[#e1e1f7]';
    }
  }
}
