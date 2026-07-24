"use client";

import Link from "next/link";
import { PublicSiteLayout } from "@/components/layout/PublicSiteLayout";

export type ContentSection = {
  title: string;
  paragraphs?: string[];
  items?: string[];
};

export type ContentLink = {
  href: string;
  label: string;
  external?: boolean;
};

export type ContentContact = {
  title: string;
  description: string;
  email: string;
};

type ContentPageProps = {
  title: string;
  lead?: string;
  updated?: string;
  sections?: ContentSection[];
  contacts?: ContentContact[];
  links?: ContentLink[];
  children?: React.ReactNode;
};

export function ContentPage({
  title,
  lead,
  updated,
  sections = [],
  contacts = [],
  links = [],
  children,
}: ContentPageProps) {
  return (
    <PublicSiteLayout>
      <article className="container content-page">
        <header className="content-page-header">
          <h1>{title}</h1>
          {lead ? <p className="content-page-lead">{lead}</p> : null}
          {updated ? <p className="content-page-updated">{updated}</p> : null}
        </header>

        {sections.map((section) => (
          <section key={section.title} className="content-page-section">
            <h2>{section.title}</h2>
            {section.paragraphs?.map((paragraph) => (
              <p key={paragraph}>{paragraph}</p>
            ))}
            {section.items && section.items.length > 0 ? (
              <ul>
                {section.items.map((item) => (
                  <li key={item}>{item}</li>
                ))}
              </ul>
            ) : null}
          </section>
        ))}

        {contacts.length > 0 ? (
          <section className="content-page-section">
            <ul className="content-page-contact-list">
              {contacts.map((contact) => (
                <li key={contact.email}>
                  <strong>{contact.title}</strong>
                  <span>{contact.description}</span>
                  <br />
                  <a href={`mailto:${contact.email}`}>{contact.email}</a>
                </li>
              ))}
            </ul>
          </section>
        ) : null}

        {links.length > 0 ? (
          <p className="content-page-links">
            {links.map((link) =>
              link.external ? (
                <a
                  key={`${link.href}-${link.label}`}
                  href={link.href}
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  {link.label}
                </a>
              ) : (
                <Link key={`${link.href}-${link.label}`} href={link.href}>
                  {link.label}
                </Link>
              ),
            )}
          </p>
        ) : null}

        {children}
      </article>
    </PublicSiteLayout>
  );
}
