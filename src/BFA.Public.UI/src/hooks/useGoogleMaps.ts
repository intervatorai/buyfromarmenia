"use client";

import { useEffect, useState } from "react";
import { importLibrary, setOptions } from "@googlemaps/js-api-loader";
import { GOOGLE_MAPS_API_KEY } from "@/lib/google-maps";

let loadPromise: Promise<void> | null = null;
let isLoaded = false;
let optionsConfigured = false;

/** Loads Google Maps Places once (same idea as Zentbow, updated loader API). */
export function useGoogleMaps() {
  const [apiLoaded, setApiLoaded] = useState(isLoaded);
  const [apiError, setApiError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (!GOOGLE_MAPS_API_KEY) {
      setApiError("Google Maps API key is not configured");
      return;
    }

    if (isLoaded) {
      setApiLoaded(true);
      return;
    }

    if (loadPromise) {
      setIsLoading(true);
      void loadPromise
        .then(() => {
          setApiLoaded(true);
          setApiError(null);
        })
        .catch(() => {
          setApiError("Failed to load Google Maps API");
        })
        .finally(() => setIsLoading(false));
      return;
    }

    setIsLoading(true);

    if (!optionsConfigured) {
      setOptions({
        key: GOOGLE_MAPS_API_KEY,
        v: "weekly",
      });
      optionsConfigured = true;
    }

    loadPromise = Promise.all([importLibrary("places"), importLibrary("geocoding")])
      .then(() => {
        isLoaded = true;
        setApiLoaded(true);
        setApiError(null);
      })
      .catch(() => {
        setApiError("Failed to load Google Maps API");
        loadPromise = null;
      })
      .finally(() => setIsLoading(false));
  }, []);

  return { apiLoaded, apiError, isLoading, hasKey: Boolean(GOOGLE_MAPS_API_KEY) };
}
