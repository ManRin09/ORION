export interface LoginCredentials {
  readonly identifier: string;
  readonly password: string;
  readonly rememberTerminal: boolean;
}

export type FamiliarTone = 'default' | 'success' | 'warning';

export interface FamiliarGuideNotice {
  readonly message: string;
  readonly tone: FamiliarTone;
}

export type PasswordRuneId = 'min-length' | 'special-char' | 'uppercase' | 'lowercase';

export interface PasswordRuneStatus {
  readonly id: PasswordRuneId;
  readonly label: string;
  readonly met: boolean;
}

export interface RegisterCharacterPayload {
  readonly firstName: string;
  readonly middleTitle: string;
  readonly email: string;
  readonly password: string;
  readonly acceptedTerms: boolean;
}

const SPECIAL_GLYPH_REGEX = /[!@#$%^&*(),.?":{}|<>_\-+=[\]\\/~`';]/;
const UPPERCASE_REGEX = /[A-Z]/;
const LOWERCASE_REGEX = /[a-z]/;

export function evaluatePasswordRunes(password: string): readonly PasswordRuneStatus[] {
  return [
    {
      id: 'min-length',
      label: 'MINIMUM 8 RUNES (8+ CHARS)',
      met: password.length >= 8,
    },
    {
      id: 'special-char',
      label: 'ANCIENT GLYPH (!@#$%^&*)',
      met: SPECIAL_GLYPH_REGEX.test(password),
    },
    {
      id: 'uppercase',
      label: 'ELDER CAPITAL RUNE (A-Z)',
      met: UPPERCASE_REGEX.test(password),
    },
    {
      id: 'lowercase',
      label: 'SWIFT MINOR RUNE (a-z)',
      met: LOWERCASE_REGEX.test(password),
    },
  ];
}

export function isCipherComplete(password: string): boolean {
  return evaluatePasswordRunes(password).every((rune) => rune.met);
}
