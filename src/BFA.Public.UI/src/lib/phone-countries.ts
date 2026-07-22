export type PhoneCountry = {
  iso: string;
  name: string;
  dialCode: string;
};

/** Common destinations + Armenia first for BuyFromArmenia registration. */
export const PHONE_COUNTRIES: PhoneCountry[] = [
  { iso: "AM", name: "Armenia", dialCode: "+374" },
  { iso: "RU", name: "Russia", dialCode: "+7" },
  { iso: "US", name: "United States", dialCode: "+1" },
  { iso: "CA", name: "Canada", dialCode: "+1" },
  { iso: "GB", name: "United Kingdom", dialCode: "+44" },
  { iso: "DE", name: "Germany", dialCode: "+49" },
  { iso: "FR", name: "France", dialCode: "+33" },
  { iso: "IT", name: "Italy", dialCode: "+39" },
  { iso: "ES", name: "Spain", dialCode: "+34" },
  { iso: "NL", name: "Netherlands", dialCode: "+31" },
  { iso: "BE", name: "Belgium", dialCode: "+32" },
  { iso: "CH", name: "Switzerland", dialCode: "+41" },
  { iso: "AT", name: "Austria", dialCode: "+43" },
  { iso: "PL", name: "Poland", dialCode: "+48" },
  { iso: "SE", name: "Sweden", dialCode: "+46" },
  { iso: "NO", name: "Norway", dialCode: "+47" },
  { iso: "DK", name: "Denmark", dialCode: "+45" },
  { iso: "FI", name: "Finland", dialCode: "+358" },
  { iso: "GE", name: "Georgia", dialCode: "+995" },
  { iso: "AE", name: "United Arab Emirates", dialCode: "+971" },
  { iso: "AU", name: "Australia", dialCode: "+61" },
  { iso: "NZ", name: "New Zealand", dialCode: "+64" },
  { iso: "TR", name: "Turkey", dialCode: "+90" },
  { iso: "IL", name: "Israel", dialCode: "+972" },
  { iso: "IR", name: "Iran", dialCode: "+98" },
  { iso: "IN", name: "India", dialCode: "+91" },
  { iso: "CN", name: "China", dialCode: "+86" },
  { iso: "JP", name: "Japan", dialCode: "+81" },
  { iso: "KR", name: "South Korea", dialCode: "+82" },
  { iso: "BR", name: "Brazil", dialCode: "+55" },
  { iso: "MX", name: "Mexico", dialCode: "+52" },
  { iso: "UA", name: "Ukraine", dialCode: "+380" },
  { iso: "KZ", name: "Kazakhstan", dialCode: "+7" },
  { iso: "BY", name: "Belarus", dialCode: "+375" },
];

export const DEFAULT_PHONE_COUNTRY_ISO = "AM";

export function buildE164Phone(dialCode: string, localNumber: string): string | undefined {
  const digits = localNumber.replace(/[^\d]/g, "");
  if (!digits) {
    return undefined;
  }

  // Drop a leading 0 from national numbers (e.g. 091... → 91...)
  const national = digits.startsWith("0") ? digits.slice(1) : digits;
  if (!national) {
    return undefined;
  }

  return `${dialCode}${national}`;
}
