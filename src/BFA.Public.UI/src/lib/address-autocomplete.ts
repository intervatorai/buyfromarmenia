export type AddressSuggestion = {
  displayName: string;
  line1: string;
  city: string;
  region: string;
  postalCode: string;
  countryCode: string;
};

type NominatimResult = {
  display_name: string;
  address?: {
    house_number?: string;
    road?: string;
    pedestrian?: string;
    city?: string;
    town?: string;
    village?: string;
    municipality?: string;
    state?: string;
    region?: string;
    county?: string;
    postcode?: string;
    country_code?: string;
  };
};

function buildLine1(address: NominatimResult["address"]): string {
  if (!address) {
    return "";
  }
  const road = address.road || address.pedestrian || "";
  const number = address.house_number || "";
  return [number, road].filter(Boolean).join(" ").trim();
}

export function mapNominatimToSuggestion(item: NominatimResult): AddressSuggestion {
  const address = item.address ?? {};
  const city =
    address.city || address.town || address.village || address.municipality || "";
  const region = address.state || address.region || address.county || "";
  const countryCode = (address.country_code || "").toUpperCase();
  const line1 = buildLine1(address) || item.display_name.split(",")[0]?.trim() || "";

  return {
    displayName: item.display_name,
    line1,
    city,
    region,
    postalCode: address.postcode || "",
    countryCode,
  };
}

/** OpenStreetMap Nominatim search — used when Google Maps key is absent. */
export async function searchAddressSuggestions(
  query: string,
  countryCodes: string[],
): Promise<AddressSuggestion[]> {
  const trimmed = query.trim();
  if (trimmed.length < 3) {
    return [];
  }

  const params = new URLSearchParams({
    q: trimmed,
    format: "json",
    addressdetails: "1",
    limit: "6",
  });

  if (countryCodes.length > 0) {
    params.set(
      "countrycodes",
      countryCodes.map((code) => code.toLowerCase()).join(","),
    );
  }

  const response = await fetch(
    `https://nominatim.openstreetmap.org/search?${params.toString()}`,
    {
      headers: {
        Accept: "application/json",
      },
    },
  );

  if (!response.ok) {
    return [];
  }

  const data = (await response.json()) as NominatimResult[];
  return data.map(mapNominatimToSuggestion);
}

function componentByType(
  components: google.maps.GeocoderAddressComponent[] | undefined,
  type: string,
): google.maps.GeocoderAddressComponent | undefined {
  return components?.find((component) => component.types.includes(type));
}

/** Maps a Google Place / Geocoder result into form fields (Zentbow-style). */
export function parseGooglePlace(
  place: google.maps.places.PlaceResult | google.maps.GeocoderResult,
): AddressSuggestion | null {
  const components = place.address_components;
  if (!components?.length) {
    return null;
  }

  const streetNumber = componentByType(components, "street_number")?.long_name ?? "";
  const route = componentByType(components, "route")?.long_name ?? "";
  const locality = componentByType(components, "locality")?.long_name ?? "";
  const postalTown = componentByType(components, "postal_town")?.long_name ?? "";
  const sublocality =
    componentByType(components, "sublocality")?.long_name ||
    componentByType(components, "sublocality_level_1")?.long_name ||
    "";
  const neighborhood = componentByType(components, "neighborhood")?.long_name ?? "";
  const admin2 = componentByType(components, "administrative_area_level_2")?.long_name ?? "";
  const admin1 = componentByType(components, "administrative_area_level_1");
  const postalCode = componentByType(components, "postal_code")?.long_name ?? "";
  const country = componentByType(components, "country")?.short_name?.toUpperCase() ?? "";

  // Prefer state abbreviation for US/CA; full name elsewhere (e.g. Armenian marz).
  const region =
    country === "US" || country === "CA"
      ? admin1?.short_name || admin1?.long_name || ""
      : admin1?.long_name || admin1?.short_name || "";

  const city = locality || postalTown || sublocality || neighborhood || admin2 || "";
  const line1 = [streetNumber, route].filter(Boolean).join(" ").trim();
  const formatted =
    ("formatted_address" in place && place.formatted_address) ||
    ("name" in place && place.name) ||
    line1 ||
    "";

  return {
    displayName: formatted,
    line1: line1 || formatted.split(",")[0]?.trim() || "",
    city,
    region,
    postalCode,
    countryCode: country,
  };
}

export function geocodeAddressString(
  address: string,
  countryHint?: string,
): Promise<AddressSuggestion | null> {
  return new Promise((resolve) => {
    if (!window.google?.maps?.Geocoder) {
      resolve(null);
      return;
    }

    const geocoder = new google.maps.Geocoder();
    const request: google.maps.GeocoderRequest = { address };
    if (countryHint) {
      request.componentRestrictions = { country: countryHint.toLowerCase() };
    }

    geocoder.geocode(request, (results, status) => {
      if (status !== "OK" || !results?.[0]) {
        resolve(null);
        return;
      }
      resolve(parseGooglePlace(results[0]));
    });
  });
}
