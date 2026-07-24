import type {
  ContentContact,
  ContentLink,
  ContentSection,
} from "@/components/content/ContentPage";
import type { Language } from "@/lib/i18n";

export type FooterPageContent = {
  title: string;
  lead: string;
  updated?: string;
  sections: ContentSection[];
  contacts?: ContentContact[];
  links?: ContentLink[];
};

const UPDATED_EN = "Last updated: 24 July 2026";
const UPDATED_HY = "Վերջին թարմացում՝ 24 հուլիսի 2026";

const howToOrder: Record<Language, FooterPageContent> = {
  en: {
    title: "How to order",
    lead: "Buy authentic Armenian products online and receive them anywhere we deliver. Follow the steps below from browsing to tracking.",
    updated: UPDATED_EN,
    sections: [
      {
        title: "1. Create an account (recommended)",
        paragraphs: [
          "You can browse without signing in. To place an order, save addresses, and track deliveries, register or sign in from your account. Your order history stays available across devices once you are signed in.",
        ],
      },
      {
        title: "2. Browse the catalog",
        paragraphs: [
          "Open Products or Categories, or use search to find makers, foods, crafts, beauty, and more. Open a product page to read the description, ingredients or materials, usage notes, and available variants.",
        ],
      },
      {
        title: "3. Add items to your cart",
        paragraphs: [
          "Select the variant and quantity, then add to cart. You can continue shopping and adjust quantities in the cart before checkout. Stock availability is shown on the product page.",
        ],
      },
      {
        title: "4. Checkout and pay",
        paragraphs: [
          "Go to checkout, enter your delivery details, and review the order summary. Shipping is estimated after you provide your destination address. Pay securely by card through Stripe. You will see confirmation once payment succeeds.",
        ],
      },
      {
        title: "5. Track your order",
        paragraphs: [
          "Open Orders in your account to follow status updates from confirmation through preparation, warehouse handling, shipping, and delivery. If something looks wrong, contact customer support with your order number.",
        ],
      },
    ],
    links: [
      { href: "/products", label: "Browse products" },
      { href: "/cart", label: "View cart" },
      { href: "/account", label: "My account" },
    ],
  },
  hy: {
    title: "Ինչպես պատվիրել",
    lead: "Գնեք հայկական իսկական ապրանքներ առցանց և ստացեք դրանք այնտեղ, որտեղ մենք առաքում ենք։ Հետևեք քայլերին՝ դիտումից մինչև հետևում։",
    updated: UPDATED_HY,
    sections: [
      {
        title: "1. Ստեղծեք հաշիվ (խորհուրդ է տրվում)",
        paragraphs: [
          "Կարող եք դիտել առանց մուտքի։ Պատվեր տեղադրելու, հասցեներ պահելու և առաքումներին հետևելու համար գրանցվեք կամ մուտք գործեք։ Մուտք գործելուց հետո պատվերների պատմությունը հասանելի է տարբեր սարքերից։",
        ],
      },
      {
        title: "2. Դիտեք կատալոգը",
        paragraphs: [
          "Բացեք Ապրանքներ կամ Կատեգորիաներ էջը, կամ օգտագործեք որոնումը՝ գտնելու արտադրողներ, սնունդ, արհեստ, գեղեցկություն և այլն։ Ապրանքի էջում կարդացեք նկարագրությունը, բաղադրությունը կամ նյութերը, օգտագործման նշումները և հասանելի տարբերակները։",
        ],
      },
      {
        title: "3. Ավելացրեք զամբյուղ",
        paragraphs: [
          "Ընտրեք տարբերակը և քանակը, ապա ավելացրեք զամբյուղ։ Կարող եք շարունակել գնումները և փոխել քանակները մինչև վճարումը։ Պաշարի առկայությունը երևում է ապրանքի էջում։",
        ],
      },
      {
        title: "4. Վճարեք պատվերը",
        paragraphs: [
          "Անցեք վճարման էջ, մուտքագրեք առաքման տվյալները և ստուգեք ամփոփումը։ Առաքման գնահատումը հայտնվում է հասցեն լրացնելուց հետո։ Վճարեք անվտանգ քարտով՝ Stripe-ի միջոցով։ Հաջող վճարումից հետո կտեսնեք հաստատումը։",
        ],
      },
      {
        title: "5. Հետևեք պատվերին",
        paragraphs: [
          "Բացեք Պատվերներ բաժինը ձեր հաշվում՝ տեսնելու կարգավիճակը հաստատումից մինչև պատրաստում, պահեստ, առաքում և ստացում։ Խնդրի դեպքում գրեք աջակցությանը՝ նշելով պատվերի համարը։",
        ],
      },
    ],
    links: [
      { href: "/products", label: "Դիտել ապրանքները" },
      { href: "/cart", label: "Զամբյուղ" },
      { href: "/account", label: "Իմ հաշիվը" },
    ],
  },
};

const deliveryPayment: Record<Language, FooterPageContent> = {
  en: {
    title: "Delivery & payment",
    lead: "BuyFromArmenia ships authentic products from Armenia to customers worldwide. Shipping costs and options depend on your destination and order.",
    updated: UPDATED_EN,
    sections: [
      {
        title: "Where we deliver",
        paragraphs: [
          "We offer international delivery from Armenia. Availability and carriers can vary by country. If a destination cannot be served for a given order, you will be informed before payment is completed or contacted afterward with alternatives.",
        ],
      },
      {
        title: "How shipping is calculated",
        paragraphs: [
          "Shipping is estimated at checkout after you enter your delivery address. The quote reflects destination, package characteristics, and the rates configured for the platform. We do not publish a single worldwide price list because costs differ by country and shipment.",
          "In some cases shipping may be adjusted after warehouse weighing or packing when the final package differs from the estimate. If an adjustment is required, we will communicate it clearly.",
        ],
      },
      {
        title: "Fulfilment and transit",
        paragraphs: [
          "Sellers prepare ordered items. Where applicable, packages may be consolidated before international dispatch. Transit time starts after the shipment is handed to the carrier and depends on destination, customs, and carrier service.",
          "You can follow progress from your order page. Tracking details are shown when the carrier provides them.",
        ],
      },
      {
        title: "Customs, duties, and restricted goods",
        paragraphs: [
          "International shipments may be subject to import duties, taxes, or customs inspection in the destination country. Unless stated otherwise at checkout, such charges are the buyer’s responsibility and are not included in the product or shipping price shown on BuyFromArmenia.",
          "Some products (for example alcohol or perishable foods) may be restricted or unavailable for certain destinations. If an item cannot be shipped to your address, we will help resolve the order.",
        ],
      },
      {
        title: "Payment methods",
        paragraphs: [
          "We accept major credit and debit cards. Payments are processed by Stripe. BuyFromArmenia does not store your full card number on our servers.",
          "Orders are confirmed after successful payment authorization. Failed or cancelled payments do not create a fulfilled order. Refunds for approved returns or cancellations are issued to the original payment method when possible.",
        ],
      },
      {
        title: "Currency and receipts",
        paragraphs: [
          "Prices are displayed in the currency shown on the product and checkout pages. Your bank or card issuer may apply its own conversion fees for foreign transactions. Order and payment references are available in your account after checkout.",
        ],
      },
    ],
    links: [
      { href: "/checkout", label: "Go to checkout" },
      { href: "/how-to-order", label: "How to order" },
      { href: "/contacts", label: "Contact support" },
    ],
  },
  hy: {
    title: "Առաքում և վճարում",
    lead: "BuyFromArmenia-ն հայկական իսկական ապրանքները առաքում է Հայաստանից հաճախորդներին ամբողջ աշխարհում։ Առաքման արժեքը և տարբերակները կախված են հասցեից և պատվերից։",
    updated: UPDATED_HY,
    sections: [
      {
        title: "Ուր ենք առաքում",
        paragraphs: [
          "Մենք առաջարկում ենք միջազգային առաքում Հայաստանից։ Հասանելիությունը և փոխադրողները կարող են տարբերվել ըստ երկրի։ Եթե որոշ հասցե հնարավոր չէ սպասարկել, կտեղեկացնենք վճարումից առաջ կամ կառաջարկենք այլ լուծում։",
        ],
      },
      {
        title: "Ինչպես է հաշվարկվում առաքումը",
        paragraphs: [
          "Առաքման գնահատումը հայտնվում է վճարման էջում՝ հասցեն մուտքագրելուց հետո։ Գինը կախված է նպատակակետից, փաթեթից և հարթակի սակագներից։ Մեկ համաշխարհային գնացուցակ չենք հրապարակում, քանի որ արժեքը տարբերվում է երկրից երկիր։",
          "Երբեմն առաքման վճարը կարող է ճշգրտվել պահեստում կշռելուց կամ փաթեթավորումից հետո, եթե վերջնական փաթեթը տարբերվում է գնահատումից։ Ճշգրտման դեպքում կտեղեկացնենք հստակ։",
        ],
      },
      {
        title: "Կատարում և ճանապարհ",
        paragraphs: [
          "Վաճառողները պատրաստում են պատվիրված ապրանքները։ Անհրաժեշտության դեպքում փաթեթները կարող են համախմբվել միջազգային առաքումից առաջ։ Ճանապարհորդության ժամանակը սկսվում է փոխադրողին հանձնելուց հետո և կախված է նպատակակետից, մաքսայինից և ծառայությունից։",
          "Ընթացքը կարող եք տեսնել պատվերի էջում։ Հետևման տվյալները ցուցադրվում են, երբ փոխադրողը դրանք տրամադրում է։",
        ],
      },
      {
        title: "Մաքսային, տուրքեր և սահմանափակումներ",
        paragraphs: [
          "Միջազգային առաքումները կարող են ենթարկվել ներմուծման տուրքերի, հարկերի կամ մաքսային ստուգման։ Եթե վճարման էջում այլ բան նշված չէ, այդ ծախսերը գնորդի պատասխանատվությունն են և ներառված չեն ապրանքի կամ առաքման գնի մեջ։",
          "Որոշ ապրանքներ (օրինակ՝ ալկոհոլ կամ շուտ փչացող սնունդ) կարող են սահմանափակված կամ անհասանելի լինել որոշ երկրների համար։ Եթե ապրանքը հնարավոր չէ առաքել ձեր հասցեով, կօգնենք լուծել պատվերը։",
        ],
      },
      {
        title: "Վճարման եղանակներ",
        paragraphs: [
          "Ընդունում ենք հիմնական վարկային և դեբետային քարտերը։ Վճարումները մշակում է Stripe։ BuyFromArmenia-ն չի պահում քարտի ամբողջական համարը։",
          "Պատվերը հաստատվում է հաջող վճարման թույլտվությունից հետո։ Չհաջողված կամ չեղարկված վճարումը չի ստեղծում կատարված պատվեր։ Հաստատված վերադարձների կամ չեղարկումների գումարը վերադարձվում է սկզբնական վճարման եղանակով՝ հնարավորության դեպքում։",
        ],
      },
      {
        title: "Արժույթ և անդորրագրեր",
        paragraphs: [
          "Գները ցուցադրվում են ապրանքի և վճարման էջերում նշված արժույթով։ Ձեր բանկը կարող է կիրառել սեփական փոխարկման վճարներ։ Պատվերի և վճարման հղումները հասանելի են հաշվում՝ վճարումից հետո։",
        ],
      },
    ],
    links: [
      { href: "/checkout", label: "Անցնել վճարմանը" },
      { href: "/how-to-order", label: "Ինչպես պատվիրել" },
      { href: "/contacts", label: "Կապ աջակցության հետ" },
    ],
  },
};

const returnsPage: Record<Language, FooterPageContent> = {
  en: {
    title: "Returns policy",
    lead: "We want you to be satisfied with your order. If something is wrong or you need to return an eligible item, follow this policy.",
    updated: UPDATED_EN,
    sections: [
      {
        title: "Return window",
        paragraphs: [
          "You may submit a return request within 14 days of delivery for eligible items. Requests submitted after this window may be declined unless the item is defective, damaged in transit, or materially different from the listing.",
        ],
      },
      {
        title: "Eligible returns",
        paragraphs: [
          "Most unused items in original packaging may be considered for return. Please include a clear reason and photos when the issue is damage, defect, or a wrong item.",
        ],
        items: [
          "Wrong item received",
          "Item damaged in transit",
          "Manufacturing defect",
          "Change of mind for eligible non-perishable goods (subject to inspection)",
        ],
      },
      {
        title: "Non-returnable or limited items",
        paragraphs: [
          "For health, safety, and quality reasons, some products cannot be returned once opened or after the window closes.",
        ],
        items: [
          "Perishable foods and opened consumables",
          "Personal-care items that have been opened or used",
          "Custom or made-to-order items, unless defective",
          "Items returned incomplete, used beyond inspection, or without original packaging when packaging was part of the product",
        ],
      },
      {
        title: "How to request a return",
        items: [
          "Sign in and open the order in your account.",
          "Submit a return request with the reason and supporting details.",
          "Wait for our review. We may approve, request more information, or decline with an explanation.",
          "If approved, follow the return shipping instructions we provide.",
          "After we receive and inspect the item, eligible refunds are issued to the original payment method.",
        ],
      },
      {
        title: "Refunds and shipping costs",
        paragraphs: [
          "Approved refunds cover the returned product price. Original outbound shipping is refunded when the return is due to our error, a defect, damage in transit, or a wrong item. For change-of-mind returns, outbound shipping is usually non-refundable, and return shipping may be your responsibility unless we state otherwise.",
          "Refund timing depends on inspection and your payment provider. Stripe refunds typically appear on your statement within several business days after we process them.",
        ],
      },
      {
        title: "Damaged or missing packages",
        paragraphs: [
          "If a package arrives damaged or items are missing, contact support within 48 hours of delivery with photos of the packaging and contents. Do not discard packaging until the case is reviewed.",
        ],
      },
    ],
    links: [
      { href: "/orders", label: "View my orders" },
      { href: "/contacts", label: "Contact support" },
      { href: "/delivery-payment", label: "Delivery & payment" },
    ],
  },
  hy: {
    title: "Վերադարձի քաղաքականություն",
    lead: "Մենք ցանկանում ենք, որ գոհ լինեք պատվերից։ Եթե ինչ-որ բան սխալ է կամ պետք է վերադարձնել համապատասխան ապրանք, հետևեք այս քաղաքականությանը։",
    updated: UPDATED_HY,
    sections: [
      {
        title: "Վերադարձի ժամկետ",
        paragraphs: [
          "Համապատասխան ապրանքների համար վերադարձի հայտ կարող եք ներկայացնել առաքումից հետո 14 օրվա ընթացքում։ Այդ ժամկետից հետո հայտերը կարող են մերժվել, բացառությամբ թերության, փոխադրման վնասի կամ հայտարարությունից էական տարբերության։",
        ],
      },
      {
        title: "Ինչը կարելի է վերադարձնել",
        paragraphs: [
          "Չօգտագործված և օրիգինալ փաթեթավորմամբ ապրանքների մեծ մասը կարող է դիտարկվել վերադարձի համար։ Վնասի, թերության կամ սխալ ապրանքի դեպքում նշեք հստակ պատճառ և կցեք լուսանկարներ։",
        ],
        items: [
          "Ստացել եք սխալ ապրանք",
          "Ապրանքը վնասվել է փոխադրման ընթացքում",
          "Արտադրական թերություն",
          "Կարծիքի փոփոխություն՝ չփչացող համապատասխան ապրանքների համար (ստուգումից հետո)",
        ],
      },
      {
        title: "Չվերադարձվող կամ սահմանափակ ապրանքներ",
        paragraphs: [
          "Առողջության, անվտանգության և որակի պատճառով որոշ ապրանքներ չեն վերադարձվում բացելուց հետո կամ ժամկետը լրանալուց հետո։",
        ],
        items: [
          "Շուտ փչացող սնունդ և բացված սպառվող ապրանքներ",
          "Անձնական խնամքի բացված կամ օգտագործված ապրանքներ",
          "Անհատական կամ պատվերով պատրաստված ապրանքներ, բացառությամբ թերության",
          "Անավարտ, ստուգման սահմանից դուրս օգտագործված կամ առանց օրիգինալ փաթեթավորման վերադարձներ, երբ փաթեթավորումը ապրանքի մաս է",
        ],
      },
      {
        title: "Ինչպես հայտ ներկայացնել",
        items: [
          "Մուտք գործեք և բացեք պատվերը ձեր հաշվում։",
          "Ուղարկեք վերադարձի հայտ՝ պատճառով և մանրամասներով։",
          "Սպասեք ստուգմանը։ Մենք կարող ենք հաստատել, լրացուցիչ տեղեկություն խնդրել կամ մերժել՝ բացատրությամբ։",
          "Հաստատման դեպքում հետևեք վերադարձի առաքման հրահանգներին։",
          "Ապրանքը ստանալուց և ստուգելուց հետո համապատասխան գումարը վերադարձվում է սկզբնական վճարման եղանակով։",
        ],
      },
      {
        title: "Գումարի վերադարձ և առաքման ծախսեր",
        paragraphs: [
          "Հաստատված վերադարձը ներառում է ապրանքի գինը։ Սկզբնական առաքման վճարը վերադարձվում է մեր սխալի, թերության, փոխադրման վնասի կամ սխալ ապրանքի դեպքում։ Կարծիքի փոփոխության դեպքում առաքման վճարը սովորաբար չի վերադարձվում, իսկ վերադարձի առաքումը կարող է լինել ձեր պատասխանատվությունը, եթե այլ բան չենք նշել։",
          "Վերադարձի ժամկետը կախված է ստուգումից և վճարման մատակարարից։ Stripe վերադարձները սովորաբար երևում են քաղվածքում մի քանի աշխատանքային օրվա ընթացքում։",
        ],
      },
      {
        title: "Վնասված կամ թերի փաթեթներ",
        paragraphs: [
          "Եթե փաթեթը վնասված է կամ ապրանքներ պակասում են, գրեք աջակցությանը առաքումից հետո 48 ժամվա ընթացքում՝ փաթեթի և պարունակության լուսանկարներով։ Մի՛ դեն նետեք փաթեթավորումը մինչև գործի քննարկումը։",
        ],
      },
    ],
    links: [
      { href: "/orders", label: "Իմ պատվերները" },
      { href: "/contacts", label: "Կապ աջակցության հետ" },
      { href: "/delivery-payment", label: "Առաքում և վճարում" },
    ],
  },
};

const sellerTerms: Record<Language, FooterPageContent> = {
  en: {
    title: "Seller terms",
    lead: "These Seller Terms govern how businesses and makers sell products on the BuyFromArmenia marketplace. By applying to sell or using the partner portal, you agree to these terms.",
    updated: UPDATED_EN,
    sections: [
      {
        title: "1. Role of the platform",
        paragraphs: [
          "BuyFromArmenia operates an online marketplace. Sellers offer products; BuyFromArmenia provides storefront, checkout, payment processing coordination, customer support tooling, and logistics coordination according to the services enabled for your account.",
          "A contract of sale for each product is formed between the seller and the customer, except where BuyFromArmenia expressly acts as seller of record for a specific listing.",
        ],
      },
      {
        title: "2. Eligibility and onboarding",
        paragraphs: [
          "You must provide accurate company, contact, tax, banking, and compliance information. We may request documents before approval. Selling privileges begin only after we approve your seller account. We may refuse or revoke approval at our discretion if information is incomplete, misleading, or non-compliant.",
        ],
      },
      {
        title: "3. Product listings and authenticity",
        paragraphs: [
          "You may list only authentic products of Armenian origin (or otherwise expressly permitted by BuyFromArmenia) that you are legally entitled to sell. Listings must be accurate regarding description, ingredients or materials, pricing, variants, inventory, images, and shipping attributes.",
          "You are responsible for product safety, labelling, intellectual property permissions, and any regulatory requirements applicable to your goods.",
        ],
      },
      {
        title: "4. Orders and fulfilment",
        paragraphs: [
          "You must monitor the partner portal, confirm orders within the required timeframes, prepare and pack items securely, and follow warehouse or carrier handoff instructions. Failure to fulfil on time may result in cancellation, customer remedies, suspension, or removal of listings.",
        ],
      },
      {
        title: "5. Prohibited conduct and products",
        paragraphs: [
          "You must not list counterfeit, unsafe, illegal, misleading, or prohibited goods; manipulate reviews or rankings; abuse customer data; or interfere with marketplace operations. BuyFromArmenia may remove content, cancel orders, suspend accounts, or terminate access for breaches.",
        ],
      },
      {
        title: "6. Customer data and privacy",
        paragraphs: [
          "Customer personal data shared for fulfilment may be used only to complete the order and related support. You must protect that data and must not use it for unrelated marketing without a lawful basis and BuyFromArmenia’s written permission where required.",
        ],
      },
      {
        title: "7. Liability and indemnity",
        paragraphs: [
          "To the fullest extent permitted by law, BuyFromArmenia is not liable for indirect or consequential losses arising from your use of the marketplace. You agree to indemnify BuyFromArmenia against claims arising from your products, listings, fulfilment failures, or legal non-compliance.",
        ],
      },
      {
        title: "8. Changes and contact",
        paragraphs: [
          "We may update these Seller Terms by publishing a revised version on this page. Continued use of seller services after the update date constitutes acceptance of the revised terms. Questions: partners@buyfromarmenia.com.",
        ],
      },
    ],
    links: [
      { href: "/sell", label: "Apply to sell" },
      { href: "/seller-support", label: "Seller support" },
      { href: "/privacy", label: "Privacy policy" },
    ],
  },
  hy: {
    title: "Վաճառողի պայմաններ",
    lead: "Այս պայմանները կարգավորում են, թե ինչպես են բիզնեսներն ու արտադրողները վաճառում BuyFromArmenia շուկայում։ Դիմելով վաճառքի կամ օգտագործելով գործընկերային պորտալը՝ դուք համաձայնում եք այս պայմաններին։",
    updated: UPDATED_HY,
    sections: [
      {
        title: "1. Հարթակի դերը",
        paragraphs: [
          "BuyFromArmenia-ն առցանց շուկա է։ Վաճառողները առաջարկում են ապրանքներ, իսկ BuyFromArmenia-ն տրամադրում է խանութ, վճարում, աջակցության գործիքներ և լոգիստիկ համակարգում՝ ըստ ձեր հաշվի ծառայությունների։",
          "Յուրաքանչյուր ապրանքի վաճառքի պայմանագիրը կնքվում է վաճառողի և գնորդի միջև, բացառությամբ այն դեպքերի, երբ BuyFromArmenia-ն հստակ հանդես է գալիս որպես վաճառող կոնկրետ հայտարարության համար։",
        ],
      },
      {
        title: "2. Իրավասություն և գրանցում",
        paragraphs: [
          "Պետք է տրամադրեք ճշգրիտ ընկերության, կապի, հարկային, բանկային և համապատասխանության տվյալներ։ Հաստատումից առաջ կարող ենք փաստաթղթեր խնդրել։ Վաճառել կարող եք միայն հաշվի հաստատումից հետո։ Կարող ենք մերժել կամ չեղարկել հաստատումը, եթե տվյալները թերի են, մոլորեցնող կամ չեն համապատասխանում պահանջներին։",
        ],
      },
      {
        title: "3. Հայտարարություններ և իսկություն",
        paragraphs: [
          "Կարող եք տեղադրել միայն իսկական հայկական ծագման ապրանքներ (կամ BuyFromArmenia-ի կողմից հստակ թույլատրված այլ ապրանքներ), որոնք իրավունք ունեք վաճառելու։ Հայտարարությունները պետք է ճշգրիտ լինեն նկարագրության, բաղադրության/նյութերի, գնի, տարբերակների, պաշարի, նկարների և առաքման հատկանիշների առումով։",
          "Դուք պատասխանատու եք ապրանքի անվտանգության, մակնշման, մտավոր սեփականության թույլտվությունների և կիրառելի կարգավորումների համար։",
        ],
      },
      {
        title: "4. Պատվերներ և կատարում",
        paragraphs: [
          "Պետք է հետևեք գործընկերային պորտալին, հաստատեք պատվերները սահմանված ժամկետում, ապահով փաթեթավորեք և հետևեք պահեստ կամ փոխադրող հանձնելու հրահանգներին։ Ուշացումը կարող է հանգեցնել չեղարկման, գնորդի պահանջների, կասեցման կամ հայտարարությունների հեռացման։",
        ],
      },
      {
        title: "5. Արգելված վարք և ապրանքներ",
        paragraphs: [
          "Արգելվում է տեղադրել կեղծ, անվտանգ չհամարվող, անօրինական կամ մոլորեցնող ապրանքներ, մանիպուլյացիա անել գնահատականներով, չարաշահել գնորդի տվյալները կամ խանգարել շուկայի աշխատանքին։ Խախտման դեպքում կարող ենք հեռացնել բովանդակությունը, չեղարկել պատվերները, կասեցնել հաշիվը կամ փակել մուտքը։",
        ],
      },
      {
        title: "6. Գնորդի տվյալներ և գաղտնիություն",
        paragraphs: [
          "Կատարման համար տրամադրված անձնական տվյալները կարող եք օգտագործել միայն պատվերը և առնչվող աջակցությունը ավարտելու համար։ Պետք է պաշտպանեք տվյալները և չօգտագործեք դրանք չկապված մարքեթինգի համար առանց իրավական հիմքի և, անհրաժեշտության դեպքում, BuyFromArmenia-ի գրավոր թույլտվության։",
        ],
      },
      {
        title: "7. Պատասխանատվություն և փոխհատուցում",
        paragraphs: [
          "Օրենքով թույլատրելի առավելագույն չափով BuyFromArmenia-ն պատասխանատու չէ անուղղակի կամ հետևանքային վնասների համար, որոնք ծագում են շուկայի օգտագործումից։ Դուք համաձայնում եք փոխհատուցել BuyFromArmenia-ին ձեր ապրանքների, հայտարարությունների, կատարման խափանումների կամ իրավական չհամապատասխանության հետ կապված պահանջների դեպքում։",
        ],
      },
      {
        title: "8. Փոփոխություններ և կապ",
        paragraphs: [
          "Մենք կարող ենք թարմացնել այս պայմանները՝ նոր տարբերակը հրապարակելով այս էջում։ Թարմացումից հետո վաճառողի ծառայությունները շարունակելը նշանակում է ընդունում։ Հարցեր՝ partners@buyfromarmenia.com։",
        ],
      },
    ],
    links: [
      { href: "/sell", label: "Դիմել վաճառքի համար" },
      { href: "/seller-support", label: "Վաճառողի աջակցություն" },
      { href: "/privacy", label: "Գաղտնիության քաղաքականություն" },
    ],
  },
};

const sellerSupport: Record<Language, FooterPageContent> = {
  en: {
    title: "Seller support",
    lead: "Resources for makers and brands selling on BuyFromArmenia — from first application to everyday fulfilment.",
    updated: UPDATED_EN,
    sections: [
      {
        title: "Getting started",
        paragraphs: [
          "Apply on the Sell page with your company details. After approval, use the partner portal to manage products, documents, bank details, and orders. Keep your compliance information up to date.",
        ],
      },
      {
        title: "What we can help with",
        items: [
          "Application and onboarding status",
          "Catalog, inventory, and listing quality questions",
          "Order confirmation, packing, and warehouse handoff",
          "Document or bank detail updates",
        ],
      },
      {
        title: "Before you write to us",
        paragraphs: [
          "Include your legal/trading name, the email on your seller account, and order IDs when relevant. Screenshots of portal errors help us resolve issues faster. For urgent fulfilment blockers, mark the subject clearly.",
        ],
      },
      {
        title: "Response times",
        paragraphs: [
          "We aim to respond to partner emails within two business days. During peak seasons response times may be longer. Order-critical issues are prioritised.",
        ],
      },
    ],
    contacts: [
      {
        title: "Partner support",
        description: "Onboarding, portal, and order questions.",
        email: "partners@buyfromarmenia.com",
      },
    ],
    links: [
      { href: "/sell", label: "Apply to sell" },
      { href: "/seller-terms", label: "Seller terms" },
    ],
  },
  hy: {
    title: "Վաճառողի աջակցություն",
    lead: "Ռեսուրսներ BuyFromArmenia-ում վաճառող արտադրողների և բրենդների համար՝ առաջին դիմումից մինչև ամենօրյա կատարում։",
    updated: UPDATED_HY,
    sections: [
      {
        title: "Սկիզբ",
        paragraphs: [
          "Դիմեք Վաճառք էջում՝ ընկերության տվյալներով։ Հաստատումից հետո գործընկերային պորտալում կառավարեք ապրանքները, փաստաթղթերը, բանկային տվյալները և պատվերները։ Պահեք համապատասխանության տվյալները արդիական։",
        ],
      },
      {
        title: "Ինչով կարող ենք օգնել",
        items: [
          "Դիմումի և գրանցման կարգավիճակ",
          "Կատալոգ, պաշար և հայտարարությունների որակ",
          "Պատվերի հաստատում, փաթեթավորում և պահեստ հանձնում",
          "Փաստաթղթերի կամ բանկային տվյալների թարմացում",
        ],
      },
      {
        title: "Նախքան մեզ գրելը",
        paragraphs: [
          "Նշեք իրավական/առևտրային անունը, վաճառողի հաշվի էլ․ հասցեն և պատվերի ID-ները։ Պորտալի սխալների լուսանկարները արագացնում են լուծումը։ Շտապ խափանումների դեպքում հստակ նշեք թեման։",
        ],
      },
      {
        title: "Պատասխանի ժամկետներ",
        paragraphs: [
          "Ձգտում ենք պատասխանել գործընկերային նամակներին երկու աշխատանքային օրվա ընթացքում։ Բարձր բեռնվածության շրջանում ժամկետը կարող է երկարել։ Պատվերի շտապ խնդիրները առաջնահերթ են։",
        ],
      },
    ],
    contacts: [
      {
        title: "Գործընկերների աջակցություն",
        description: "Գրանցում, պորտալ և պատվերների հարցեր։",
        email: "partners@buyfromarmenia.com",
      },
    ],
    links: [
      { href: "/sell", label: "Դիմել վաճառքի համար" },
      { href: "/seller-terms", label: "Վաճառողի պայմաններ" },
    ],
  },
};

const about: Record<Language, FooterPageContent> = {
  en: {
    title: "About BuyFromArmenia",
    lead: "BuyFromArmenia is a marketplace for authentic Armenian products — made by local makers, discovered online, and delivered worldwide.",
    updated: UPDATED_EN,
    sections: [
      {
        title: "Our mission",
        paragraphs: [
          "We exist to make Armenia’s craft, food, beauty, and design accessible to people everywhere, while giving local producers a modern channel to reach customers beyond their home market.",
          "Authenticity matters. We work with sellers who represent real Armenian products and stories, not generic imports dressed as souvenirs.",
        ],
      },
      {
        title: "What we offer customers",
        paragraphs: [
          "A curated catalog, secure checkout, international shipping coordination, and order tracking in one place. You shop makers you can trust; we help the purchase travel from Armenia to your door.",
        ],
      },
      {
        title: "What we offer sellers",
        paragraphs: [
          "A partner portal for catalog and order management, onboarding and compliance tools, and access to customers who specifically seek Armenian goods. Sellers focus on making; we support the digital storefront and logistics coordination.",
        ],
      },
      {
        title: "How the marketplace works",
        paragraphs: [
          "Customers discover and buy on buyfromarmenia.com. Approved sellers fulfil orders. BuyFromArmenia coordinates payments via Stripe, shipping estimates, customer support, and returns handling according to our published policies.",
        ],
      },
    ],
    links: [
      { href: "/products", label: "Start shopping" },
      { href: "/sell", label: "Sell with us" },
      { href: "/contacts", label: "Contact us" },
    ],
  },
  hy: {
    title: "BuyFromArmenia-ի մասին",
    lead: "BuyFromArmenia-ն հայկական իսկական ապրանքների շուկա է՝ տեղական արտադրողներից, առցանց հայտնաբերվող և ամբողջ աշխարհ առաքվող։",
    updated: UPDATED_HY,
    sections: [
      {
        title: "Մեր առաքելությունը",
        paragraphs: [
          "Մենք գոյություն ունենք, որպեսզի Հայաստանի արհեստը, սնունդը, գեղեցկությունն ու դիզայնը հասանելի լինեն ամենուր, իսկ տեղական արտադրողները ժամանակակից ալիք ունենան՝ տնային շուկայից դուրս հաճախորդներ գտնելու համար։",
          "Իսկությունը կարևոր է։ Մենք աշխատում ենք վաճառողների հետ, որոնք ներկայացնում են իրական հայկական ապրանքներ և պատմություններ, ոչ թե ընդհանուր ներմուծում՝ որպես հուշանվեր։",
        ],
      },
      {
        title: "Ինչ ենք առաջարկում գնորդներին",
        paragraphs: [
          "Ընտրված կատալոգ, անվտանգ վճարում, միջազգային առաքման համակարգում և պատվերի հետևում մեկ տեղում։ Դուք գնում եք վստահելի արտադրողներից, մենք օգնում ենք գնումը Հայաստանից հասցնել ձեր դուռը։",
        ],
      },
      {
        title: "Ինչ ենք առաջարկում վաճառողներին",
        paragraphs: [
          "Գործընկերային պորտալ կատալոգի և պատվերների համար, գրանցման և համապատասխանության գործիքներ, և հասանելիություն այն հաճախորդներին, ովքեր փնտրում են հայկական ապրանքներ։ Վաճառողները կենտրոնանում են արտադրության վրա, մենք՝ թվային խանութի և լոգիստիկայի համակարգման վրա։",
        ],
      },
      {
        title: "Ինչպես է աշխատում շուկան",
        paragraphs: [
          "Գնորդները հայտնաբերում և գնում են buyfromarmenia.com-ում։ Հաստատված վաճառողները կատարում են պատվերները։ BuyFromArmenia-ն համակարգում է վճարումները Stripe-ով, առաքման գնահատումները, աջակցությունը և վերադարձները՝ ըստ հրապարակված քաղաքականությունների։",
        ],
      },
    ],
    links: [
      { href: "/products", label: "Սկսել գնումները" },
      { href: "/sell", label: "Վաճառել մեզ հետ" },
      { href: "/contacts", label: "Կապ մեզ հետ" },
    ],
  },
};

const contacts: Record<Language, FooterPageContent> = {
  en: {
    title: "Contact us",
    lead: "We are here to help with orders, deliveries, returns, and seller partnerships.",
    updated: UPDATED_EN,
    sections: [
      {
        title: "How to get a faster reply",
        paragraphs: [
          "Include your full name, order number (if any), and a clear description of the issue. For delivery problems, attach photos of the package and label. We aim to respond within two business days.",
        ],
      },
      {
        title: "Business hours",
        paragraphs: [
          "Support is handled on business days, Monday–Friday. Messages received on weekends or public holidays are reviewed on the next business day.",
        ],
      },
    ],
    contacts: [
      {
        title: "Customer support",
        description: "Orders, shipping, payments, and returns.",
        email: "support@buyfromarmenia.com",
      },
      {
        title: "Seller / partner support",
        description: "Onboarding, partner portal, and fulfilment.",
        email: "partners@buyfromarmenia.com",
      },
      {
        title: "General enquiries",
        description: "Press, partnerships, and other questions.",
        email: "hello@buyfromarmenia.com",
      },
    ],
    links: [
      { href: "/how-to-order", label: "How to order" },
      { href: "/returns", label: "Returns policy" },
      { href: "/sell", label: "Sell on the platform" },
    ],
  },
  hy: {
    title: "Կապ մեզ հետ",
    lead: "Մենք պատրաստ ենք օգնել պատվերների, առաքումների, վերադարձների և վաճառող գործընկերությունների հարցերում։",
    updated: UPDATED_HY,
    sections: [
      {
        title: "Ինչպես ստանալ ավելի արագ պատասխան",
        paragraphs: [
          "Նշեք ձեր անունը, պատվերի համարը (եթե կա) և խնդրի հստակ նկարագրությունը։ Առաքման խնդիրների դեպքում կցեք փաթեթի և պիտակի լուսանկարներ։ Ձգտում ենք պատասխանել երկու աշխատանքային օրվա ընթացքում։",
        ],
      },
      {
        title: "Աշխատանքային ժամեր",
        paragraphs: [
          "Աջակցությունը տրամադրվում է աշխատանքային օրերին՝ երկուշաբթի–ուրբաթ։ Շաբաթ/կիրակի կամ տոներին ստացված նամակները քննարկվում են հաջորդ աշխատանքային օրը։",
        ],
      },
    ],
    contacts: [
      {
        title: "Հաճախորդների աջակցություն",
        description: "Պատվերներ, առաքում, վճարումներ և վերադարձ։",
        email: "support@buyfromarmenia.com",
      },
      {
        title: "Վաճառող / գործընկեր աջակցություն",
        description: "Գրանցում, պորտալ և կատարում։",
        email: "partners@buyfromarmenia.com",
      },
      {
        title: "Ընդհանուր հարցումներ",
        description: "Մամուլ, գործընկերություններ և այլ հարցեր։",
        email: "hello@buyfromarmenia.com",
      },
    ],
    links: [
      { href: "/how-to-order", label: "Ինչպես պատվիրել" },
      { href: "/returns", label: "Վերադարձի քաղաքականություն" },
      { href: "/sell", label: "Վաճառել հարթակում" },
    ],
  },
};

const privacy: Record<Language, FooterPageContent> = {
  en: {
    title: "Privacy policy",
    lead: "This Privacy Policy explains how BuyFromArmenia collects, uses, shares, and protects personal data when you use buyfromarmenia.com and related services.",
    updated: UPDATED_EN,
    sections: [
      {
        title: "1. Who we are",
        paragraphs: [
          "BuyFromArmenia operates the online marketplace at buyfromarmenia.com. For privacy questions, contact support@buyfromarmenia.com.",
        ],
      },
      {
        title: "2. Data we collect",
        paragraphs: [
          "Account and profile data: name, email address, phone number, and password credentials (stored in hashed form).",
          "Order and delivery data: shipping addresses, order contents, payment references, and communication related to fulfilment or returns.",
          "Seller data: company details, contacts, tax identifiers, bank details, compliance documents, and portal activity needed to operate seller accounts.",
          "Technical data: IP address, device/browser information, and cookies or similar technologies required for sessions, security, cart, and checkout.",
        ],
      },
      {
        title: "3. Why we use your data",
        items: [
          "To create and manage your account",
          "To process orders, payments, shipping, and returns",
          "To provide customer and seller support",
          "To prevent fraud, abuse, and security incidents",
          "To improve the storefront and marketplace operations",
          "To meet legal and accounting obligations",
        ],
      },
      {
        title: "4. Legal bases",
        paragraphs: [
          "We process personal data where needed to perform a contract with you (for example placing and delivering an order), where we have a legitimate interest in operating and securing the marketplace, where you consent (for optional communications), or where processing is required by law.",
        ],
      },
      {
        title: "5. Payments",
        paragraphs: [
          "Card payments are processed by Stripe. BuyFromArmenia does not store full card numbers on its servers. Stripe’s processing of payment data is governed by Stripe’s own terms and privacy policy.",
        ],
      },
      {
        title: "6. Sharing",
        paragraphs: [
          "We share personal data only as needed with: payment processors; logistics and warehouse partners; sellers (to fulfil your order); IT and hosting providers; and professional advisors or authorities when required by law.",
          "We do not sell your personal data.",
        ],
      },
      {
        title: "7. International transfers",
        paragraphs: [
          "Because we ship and operate internationally, data may be processed in countries other than your own. We take reasonable steps to ensure appropriate safeguards with service providers.",
        ],
      },
      {
        title: "8. Retention",
        paragraphs: [
          "We keep personal data only as long as needed for the purposes above, including order history, dispute handling, legal retention, and security. When data is no longer required, we delete or anonymise it where feasible.",
        ],
      },
      {
        title: "9. Cookies",
        paragraphs: [
          "We use essential cookies and similar storage to keep you signed in, maintain cart and checkout state, and protect the service. Disabling essential cookies may prevent parts of the site from working.",
        ],
      },
      {
        title: "10. Your rights",
        paragraphs: [
          "Depending on applicable law, you may request access, correction, deletion, or restriction of your personal data, and object to certain processing. You can update many account details while signed in. To exercise privacy rights, email support@buyfromarmenia.com. We may need to verify your identity before responding.",
        ],
      },
      {
        title: "11. Children",
        paragraphs: [
          "The marketplace is intended for adults. We do not knowingly collect personal data from children. If you believe a child has provided data, contact us so we can delete it.",
        ],
      },
      {
        title: "12. Changes",
        paragraphs: [
          "We may update this Privacy Policy from time to time. The “Last updated” date at the top of this page will change when we publish revisions. Continued use of the service after an update means you acknowledge the revised policy.",
        ],
      },
    ],
    links: [
      { href: "/contacts", label: "Contact us" },
      { href: "/returns", label: "Returns policy" },
      { href: "/seller-terms", label: "Seller terms" },
    ],
  },
  hy: {
    title: "Գաղտնիության քաղաքականություն",
    lead: "Այս քաղաքականությունը բացատրում է, թե ինչպես է BuyFromArmenia-ն հավաքում, օգտագործում, կիսում և պաշտպանում անձնական տվյալները buyfromarmenia.com-ի և առնչվող ծառայությունների օգտագործման ժամանակ։",
    updated: UPDATED_HY,
    sections: [
      {
        title: "1. Ով ենք մենք",
        paragraphs: [
          "BuyFromArmenia-ն շահագործում է առցանց շուկան buyfromarmenia.com հասցեով։ Գաղտնիության հարցերով գրեք support@buyfromarmenia.com։",
        ],
      },
      {
        title: "2. Ինչ տվյալներ ենք հավաքում",
        paragraphs: [
          "Հաշվի տվյալներ՝ անուն, էլ․ հասցե, հեռախոս և գաղտնաբառ (պահվում է հեշավորված)։",
          "Պատվերի և առաքման տվյալներ՝ հասցեներ, պատվերի բովանդակություն, վճարման հղումներ և կատարման/վերադարձի հաղորդակցություն։",
          "Վաճառողի տվյալներ՝ ընկերության տվյալներ, կոնտակտներ, հարկային նույնացուցիչներ, բանկային տվյալներ, համապատասխանության փաստաթղթեր և պորտալի գործունեություն։",
          "Տեխնիկական տվյալներ՝ IP, սարքի/զննարկչի տեղեկություն և cookies՝ սեսիայի, անվտանգության, զամբյուղի և վճարման համար։",
        ],
      },
      {
        title: "3. Ինչու ենք օգտագործում",
        items: [
          "Հաշիվ ստեղծելու և կառավարելու համար",
          "Պատվերներ, վճարումներ, առաքում և վերադարձ մշակելու համար",
          "Հաճախորդների և վաճառողների աջակցության համար",
          "Խարդախությունն ու անվտանգության միջադեպերը կանխելու համար",
          "Խանութը և շուկայի աշխատանքը բարելավելու համար",
          "Իրավական և հաշվապահական պարտավորությունների համար",
        ],
      },
      {
        title: "4. Իրավական հիմքեր",
        paragraphs: [
          "Մենք մշակում ենք տվյալներ, երբ դա անհրաժեշտ է ձեզ հետ պայմանագիր կատարելու համար (օրինակ՝ պատվեր և առաքում), երբ ունենք օրինական շահ շուկան շահագործելու և պաշտպանելու համար, երբ կա ձեր համաձայնությունը (ընտրովի հաղորդակցությունների համար), կամ երբ պահանջվում է օրենքով։",
        ],
      },
      {
        title: "5. Վճարումներ",
        paragraphs: [
          "Քարտային վճարումները մշակում է Stripe։ BuyFromArmenia-ն չի պահում քարտի ամբողջական համարները։ Stripe-ի մշակումը կարգավորվում է Stripe-ի սեփական պայմաններով և գաղտնիության քաղաքականությամբ։",
        ],
      },
      {
        title: "6. Փոխանցում",
        paragraphs: [
          "Տվյալները կիսում ենք միայն անհրաժեշտության դեպքում՝ վճարման մշակողների, լոգիստիկ և պահեստային գործընկերների, վաճառողների (պատվերը կատարելու համար), IT/հոսթինգ մատակարարների և, օրենքով պահանջվելու դեպքում, խորհրդատուների կամ մարմինների հետ։",
          "Մենք չենք վաճառում ձեր անձնական տվյալները։",
        ],
      },
      {
        title: "7. Միջազգային փոխանցումներ",
        paragraphs: [
          "Քանի որ աշխատում և առաքում ենք միջազգայնորեն, տվյալները կարող են մշակվել ձեր երկրից տարբեր երկրներում։ Մենք ձեռնարկում ենք ողջամիտ միջոցներ՝ մատակարարների հետ համապատասխան պաշտպանություն ապահովելու համար։",
        ],
      },
      {
        title: "8. Պահպանում",
        paragraphs: [
          "Անձնական տվյալները պահում ենք միայն վերոնշյալ նպատակների համար անհրաժեշտ ժամանակով՝ ներառյալ պատվերների պատմությունը, վեճերի քննարկումը, իրավական պահպանումը և անվտանգությունը։ Երբ տվյալներն այլևս պետք չեն, ջնջում կամ անանունացնում ենք՝ հնարավորության դեպքում։",
        ],
      },
      {
        title: "9. Cookies",
        paragraphs: [
          "Օգտագործում ենք անհրաժեշտ cookies և նմանատիպ պահեստավորում՝ մուտքը, զամբյուղը, վճարումը և անվտանգությունը ապահովելու համար։ Անհրաժեշտ cookies-ի անջատումը կարող է խանգարել կայքի աշխատանքին։",
        ],
      },
      {
        title: "10. Ձեր իրավունքները",
        paragraphs: [
          "Կիրառելի օրենքից կախված՝ կարող եք պահանջել տվյալների մուտք, ուղղում, ջնջում կամ սահմանափակում, ինչպես նաև առարկել որոշ մշակումների դեմ։ Շատ տվյալներ կարող եք թարմացնել մուտք գործած ժամանակ։ Իրավունքներ իրականացնելու համար գրեք support@buyfromarmenia.com։ Պատասխանելուց առաջ կարող ենք ստուգել ինքնությունը։",
        ],
      },
      {
        title: "11. Երեխաներ",
        paragraphs: [
          "Շուկան նախատեսված է չափահասների համար։ Մենք գիտակցաբար չենք հավաքում երեխաների տվյալներ։ Եթե կարծում եք, որ երեխա է տվյալներ տրամադրել, կապվեք մեզ հետ՝ ջնջելու համար։",
        ],
      },
      {
        title: "12. Փոփոխություններ",
        paragraphs: [
          "Մենք կարող ենք թարմացնել այս քաղաքականությունը։ Էջի վերևում նշված «Վերջին թարմացում» ամսաթիվը կփոխվի նոր տարբերակ հրապարակելիս։ Թարմացումից հետո ծառայությունը շարունակելը նշանակում է, որ ծանոթ եք նոր քաղաքականությանը։",
        ],
      },
    ],
    links: [
      { href: "/contacts", label: "Կապ մեզ հետ" },
      { href: "/returns", label: "Վերադարձի քաղաքականություն" },
      { href: "/seller-terms", label: "Վաճառողի պայմաններ" },
    ],
  },
};

export const footerPages = {
  howToOrder,
  deliveryPayment,
  returns: returnsPage,
  sellerTerms,
  sellerSupport,
  about,
  contacts,
  privacy,
} as const;

export function getFooterPage(
  page: keyof typeof footerPages,
  language: Language,
): FooterPageContent {
  return footerPages[page][language];
}
