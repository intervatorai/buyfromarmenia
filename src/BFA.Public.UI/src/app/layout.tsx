import type { Metadata } from "next";
import { DM_Sans, Noto_Sans_Armenian, Playfair_Display } from "next/font/google";
import { AuthProvider } from "@/components/providers/AuthProvider";
import { LanguageProvider } from "@/components/providers/LanguageProvider";
import "./globals.css";
import "../styles/home.css";

const dmSans = DM_Sans({
  subsets: ["latin"],
  weight: ["400", "500", "600", "700"],
  variable: "--font-dm-sans",
});

const notoArmenian = Noto_Sans_Armenian({
  subsets: ["armenian"],
  weight: ["400", "500", "600", "700"],
  variable: "--font-noto-armenian",
});

const playfair = Playfair_Display({
  subsets: ["latin"],
  weight: ["600", "700"],
  variable: "--font-playfair",
});

export const metadata: Metadata = {
  title: "BuyFromArmenia",
  description: "Authentic Armenian products delivered worldwide",
  applicationName: "BuyFromArmenia",
  manifest: "/site.webmanifest",
  icons: {
    icon: [
      { url: "/icons/favicon-96x96.png", sizes: "96x96", type: "image/png" },
      { url: "/icons/web-app-manifest-192x192.png", sizes: "192x192", type: "image/png" },
      { url: "/icons/web-app-manifest-512x512.png", sizes: "512x512", type: "image/png" },
    ],
    apple: [{ url: "/icons/apple-touch-icon.png", sizes: "180x180", type: "image/png" }],
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html
      lang="en"
      data-lang="en"
      className={`${dmSans.variable} ${notoArmenian.variable} ${playfair.variable}`}
    >
      <body>
        <LanguageProvider>
          <AuthProvider>{children}</AuthProvider>
        </LanguageProvider>
      </body>
    </html>
  );
}
