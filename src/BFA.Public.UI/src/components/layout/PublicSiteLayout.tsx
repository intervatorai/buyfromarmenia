import { Footer } from "./Footer";
import { Header } from "./Header";
import { Topbar } from "./Topbar";

export function PublicSiteLayout({ children }: { children: React.ReactNode }) {
  return (
    <>
      <Topbar />
      <Header />
      <main>{children}</main>
      <Footer />
    </>
  );
}
