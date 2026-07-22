import { CategoriesSection } from "@/components/home/CategoriesSection";
import { HeroSection } from "@/components/home/HeroSection";
import { HowItWorksSection } from "@/components/home/HowItWorksSection";
import { ProductsSection } from "@/components/home/ProductsSection";
import { SellerSection } from "@/components/home/SellerSection";
import { Footer } from "@/components/layout/Footer";
import { Header } from "@/components/layout/Header";
import { Topbar } from "@/components/layout/Topbar";

export default function Home() {
  return (
    <>
      <Topbar />
      <Header />

      <main>
        <HeroSection />
        <CategoriesSection />
        <ProductsSection />
        <HowItWorksSection />
        <SellerSection />
      </main>

      <Footer />
    </>
  );
}
