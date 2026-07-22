"use client";

import { useLanguage } from "@/components/providers/LanguageProvider";

const steps = [
  { number: "01", icon: "🛍", titleKey: "step1Title" as const, descKey: "step1Desc" as const },
  { number: "02", icon: "□", titleKey: "step2Title" as const, descKey: "step2Desc" as const },
  { number: "03", icon: "🚚", titleKey: "step3Title" as const, descKey: "step3Desc" as const },
  { number: "04", icon: "☺", titleKey: "step4Title" as const, descKey: "step4Desc" as const },
] as const;

export function HowItWorksSection() {
  const { translate } = useLanguage();

  return (
    <section className="how container" id="how">
      <p className="eyebrow">{translate("simpleTransparent")}</p>
      <h2>{translate("howTitle")}</h2>

      <div className="steps">
        {steps.map((step) => (
          <article key={step.number} className="step">
            <span className="step-number">{step.number}</span>
            <div className="step-icon">{step.icon}</div>
            <h3>{translate(step.titleKey)}</h3>
            <p>{translate(step.descKey)}</p>
          </article>
        ))}
      </div>
    </section>
  );
}
